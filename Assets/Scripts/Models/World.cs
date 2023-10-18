//=======================================================================
// Copyright Martin "quill18" Glaude 2015.
//		http://quill18.com
//=======================================================================

using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;
using MoonSharp.Interpreter;



[MoonSharpUserData]

public class World : IXmlSerializable
{
    // I'm adding this user state here to keep track
    // of the kind of states the user can be in.
    // Eventually I want to lock the FREE_EDIT mode so
    // that non-developers dont have access to that mode..
    // Future modes? (FREE_BUILD maybe?)


        // A two-dimensional array to hold our tile data.
        Tile[,] tiles;
    public List<Character> characters;
    public List<Furniture> furnitures;
    public List<Room> rooms;
    public InventoryManager inventoryManager;

    // The manager that handles chunks of the map
    public TilemapChunkManager tilemapChunkManager;

    // The pathfinding graph used to navigate our world map.
    public Path_TileGraph tileGraph;

    public Dictionary<string, Furniture> furniturePrototypes;
    public Dictionary<string, Job> furnitureJobPrototypes;
    public Dictionary<string, Character> characterPrototypes;

    // The tile width of the world.
    public int Width { get; protected set; }

    // The tile height of the world
    public int Height { get; protected set; }

    Action<Furniture> cbFurnitureCreated;
    Action<Character> cbCharacterCreated;
    Action<Inventory> cbInventoryCreated;
    Action<Tile> cbTileChanged;
    // TODO: Most likely this will be replaced with a dedicated
    // class for managing job queues (plural!) that might also
    // be semi-static or self initializing or some damn thing.
    // For now, this is just a PUBLIC member of World
    public JobQueue jobQueue;

    static public World current { get; protected set; }

    public enum UserState
    {
        FREE_EDIT,
        CUTSCENE,
        GAME
    }

    public UserState currentUserState;

    public World(int width, int height)
    {
        // Creates an empty world.
        SetupWorld(width, height);

        currentUserState = UserState.GAME;
    }

    public World() {}

    public Room GetOutsideRoom()
    {
        return rooms[0];
    }

    public int GetRoomID(Room r)
    {
        return rooms.IndexOf(r);
    }

    public Room GetRoomFromID(int i)
    {
        if (i < 0 || i > rooms.Count - 1)
            return null;

        return rooms[i];
    }

    public void AddRoom(Room r)
    {
        rooms.Add(r);
    }

    public void DeleteRoom(Room r)
    {
        if (r == GetOutsideRoom())
        {
            Debug.LogError("Tried to delete the outside room.");
            return;
        }

        // Remove this room from our rooms list.
        rooms.Remove(r);

        // All tiles that belonged to this room should be re-assigned to
        // the outside.
        r.ReturnTilesToOutsideRoom();
    }

    void SetupWorld(int width, int height)
    {

        jobQueue = new JobQueue();

        // Set the current world to be this world.
        // TODO: Do we need to do any cleanup of the old world?
        current = this;

        Width = width;
        Height = height;

        tiles = new Tile[Width, Height];

        rooms = new List<Room>();
        rooms.Add(new Room()); // Create the outside?

        tilemapChunkManager = new TilemapChunkManager(Width, Height, 4, 4);

        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                tiles[x, z] = new Tile(x, z, Color.white);
                tiles[x, z].RegisterTileTypeChangedCallback(OnTileChanged);
                tiles[x, z].room = GetOutsideRoom(); // Rooms 0 is always going to be outside, and that is our default room

                Tile tile = tiles[x, z];

                //// Calculate which chunk this tile belongs to
                int chunkColumn = x / tilemapChunkManager.ChunkWidth;
                int chunkRow = z / tilemapChunkManager.ChunkHeight;

                //// Get the corresponding chunk from the manager and add the tile to it
                Chunk chunk = tilemapChunkManager.GetChunkAtGridPosition(chunkColumn, chunkRow); // This is correct.

                if (chunk != null)
                {
                    chunk.AddTile(tile);
                    tile.chunk = chunk;
                }
                else
                {
                    Debug.LogError("SetupWorld: No chunk available for the calculated position");
                }
            }
        }

        //Debug.Log("GenerateChunks -- TileCount for one chunk -- " + tilemapChunkManager.Chunks[0].tiles.Count);
        //Color[] colors = new Color[] { Color.red, Color.blue, Color.green, Color.cyan, Color.grey, Color.yellow };
        //foreach (var item in tilemapChunkManager.Chunks)
        //{
        //    item.ChangeTilesColor(colors[UnityEngine.Random.Range(0, colors.Length)]);
        //}

        CreateFurniturePrototypes();
        CreateCharacterPrototypes();

        characters = new List<Character>();
        furnitures = new List<Furniture>();
        inventoryManager = new InventoryManager();
    }

    public void Update(float deltaTime)
    {
        foreach (Character c in characters)
        {
            c.Update(deltaTime);
        }

        foreach (Furniture f in furnitures)
        {
            f.Update(deltaTime);
        }

    }

    public Character CreateCharacter(Tile t)
    {
        //Debug.Log("CreateCharacter");
        Character c = new Character(t);

        characters.Add(c);

        if (cbCharacterCreated != null)
            cbCharacterCreated(c);

        return c;
    }

    public void SetFurnitureJobPrototype(Job j, Furniture f)
    {
        furnitureJobPrototypes[f.objectType] = j;
    }

    void LoadFurnitureLua()
    {
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "LUA");
        filePath = System.IO.Path.Combine(filePath, "Furniture.lua");
        string myLuaCode = System.IO.File.ReadAllText(filePath);

        //Debug.Log("My LUA Code");
        //Debug.Log(myLuaCode);

        // Instantiate the singleton
        new FurnitureActions(myLuaCode);

    }

    void CreateCharacterPrototypes()
    {
        characterPrototypes = new Dictionary<string, Character>();


        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
        filePath = System.IO.Path.Combine(filePath, "Character.xml");
        string characterXmlText = System.IO.File.ReadAllText(filePath);

        XmlTextReader reader = new XmlTextReader(new StringReader(characterXmlText));

        int charCount = 0;
        if (reader.ReadToDescendant("Characters"))
        {
            if (reader.ReadToDescendant("Character"))
            {
                do
                {
                    charCount++;

                    Character c = new Character();
                    c.ReadXmlPrototype(reader);

                    characterPrototypes[c.objectType] = c;



                } while (reader.ReadToNextSibling("Character"));
            }
            else
            {
                Debug.LogError("The Character prototype definition file doesn't have any 'Character' elements.");
            }
        }
        else
        {
            Debug.LogError("Did not find a 'Characters' element in the prototype definition file.");
        }
    }

    void CreateFurniturePrototypes()
    {
        LoadFurnitureLua();

        furniturePrototypes = new Dictionary<string, Furniture>();
        furnitureJobPrototypes = new Dictionary<string, Job>();

        // READ FURNITURE PROTOTYPE XML FILE HERE
        // TODO:  Probably we should be getting past a StreamIO handle or the raw
        // text here, rather than opening the file ourselves.

        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "Data");
        filePath = System.IO.Path.Combine(filePath, "Furniture.xml");
        string furnitureXmlText = System.IO.File.ReadAllText(filePath);

        XmlTextReader reader = new XmlTextReader(new StringReader(furnitureXmlText));

        int furnCount = 0;
        if (reader.ReadToDescendant("Furnitures"))
        {
            if (reader.ReadToDescendant("Furniture"))
            {
                do
                {
                    furnCount++;

                    Furniture furn = new Furniture();
                    furn.ReadXmlPrototype(reader);

                    furniturePrototypes[furn.objectType] = furn;



                } while (reader.ReadToNextSibling("Furniture"));
            }
            else
            {
                Debug.LogError("The furniture prototype definition file doesn't have any 'Furniture' elements.");
            }
        }
        else
        {
            Debug.LogError("Did not find a 'Furnitures' element in the prototype definition file.");
        }

        //Debug.Log("Furniture prototypes read: " + furnCount.ToString());

        // This bit will come from parsing a LUA file later, but for now we still need to
        // implement furniture behaviour directly in C# code.
        //furniturePrototypes["Door"].RegisterUpdateAction( FurnitureActions.Door_UpdateAction );
        //furniturePrototypes["Door"].IsEnterable = FurnitureActions.Door_IsEnterable;

    }


    /*	void CreateFurniturePrototypes() {
            // This will be replaced by a function that reads all of our furniture data
            // from a text file in the future.

            furniturePrototypes = new Dictionary<string, Furniture>();
            furnitureJobPrototypes = new Dictionary<string, Job>();

            furniturePrototypes.Add("furn_SteelWall", 
                new Furniture(
                    "furn_SteelWall",
                    0,	// Impassable
                    1,  // Width
                    1,  // Height
                    true, // Links to neighbours and "sort of" becomes part of a large object
                    true  // Enclose rooms
                )
            );
            furniturePrototypes["furn_SteelWall"].Name = "Basic Wall";
            furnitureJobPrototypes.Add("furn_SteelWall",
                new Job( null, 
                    "furn_SteelWall", 
                    FurnitureActions.JobComplete_FurnitureBuilding, 1f, 
                    new Inventory[]{ new Inventory("Steel Plate", 5, 0) } 
                )
            );

            furniturePrototypes.Add("Door", 
                new Furniture(
                    "Door",
                    1,	// Door pathfinding cost
                    1,  // Width
                    1,  // Height
                    false, // Links to neighbours and "sort of" becomes part of a large object
                    true  // Enclose rooms
                )
            );

            // What if the object behaviours were scriptable? And therefore were part of the text file
            // we are reading in now?

            furniturePrototypes["Door"].SetParameter("openness", 0);
            furniturePrototypes["Door"].SetParameter("is_opening", 0);
            furniturePrototypes["Door"].RegisterUpdateAction( FurnitureActions.Door_UpdateAction );

            furniturePrototypes["Door"].IsEnterable = FurnitureActions.Door_IsEnterable;


            furniturePrototypes.Add("Stockpile", 
                new Furniture(
                    "Stockpile",
                    1,	// Impassable
                    1,  // Width
                    1,  // Height
                    true, // Links to neighbours and "sort of" becomes part of a large object
                    false  // Enclose rooms
                )
            );
            furniturePrototypes["Stockpile"].RegisterUpdateAction( FurnitureActions.Stockpile_UpdateAction );
            furniturePrototypes["Stockpile"].tint = new Color32( 186, 31, 31, 255 );
            furnitureJobPrototypes.Add("Stockpile",
                new Job( 
                    null, 
                    "Stockpile", 
                    FurnitureActions.JobComplete_FurnitureBuilding,
                    -1,
                    null
                )
            );



            furniturePrototypes.Add("Oxygen Generator", 
                new Furniture(
                    "Oxygen Generator",
                    10,	// Door pathfinding cost
                    2,  // Width
                    2,  // Height
                    false, // Links to neighbours and "sort of" becomes part of a large object
                    false  // Enclose rooms
                )
            );
            furniturePrototypes["Oxygen Generator"].RegisterUpdateAction( FurnitureActions.OxygenGenerator_UpdateAction );



            furniturePrototypes.Add("Mining Drone Station", 
                new Furniture(
                    "Mining Drone Station",
                    1,	// Pathfinding cost
                    3,  // Width			
                    3,  // Height		// TODO: In the future, the mining drone station will be a 3x2 object with an offset work spot
                    false, // Links to neighbours and "sort of" becomes part of a large object
                    false  // Enclose rooms
                )
            );
            furniturePrototypes["Mining Drone Station"].jobSpotOffset = new Vector2( 1, 0 );

            furniturePrototypes["Mining Drone Station"].RegisterUpdateAction( FurnitureActions.MiningDroneStation_UpdateAction );



        }
    */

    /// <summary>
    /// A function for testing out the system
    /// </summary>
    public void RandomizeTiles()
    {
        Debug.Log("RandomizeTiles");
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {

                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    tiles[x, z].Type = TileType.Empty;
                }
                else
                {
                    tiles[x, z].Type = TileType.Grass;
                }

            }
        }
    }

    public void SetupPathfindingExample()
    {
        Debug.Log("SetupPathfindingExample");

        // Make a set of floors/walls to test pathfinding with.

        int l = Width / 2 - 5;
        int b = Height / 2 - 5;

        for (int x = l - 5; x < l + 15; x++)
        {
            for (int z = b - 5; z < b + 15; z++)
            {
                tiles[x, z].Type = TileType.Grass;


                if (x == l || x == (l + 9) || z == b || z == (b + 9))
                {
                    if (x != (l + 9) && z != (b + 4))
                    {
                        PlaceFurniture("furn_SteelWall", tiles[x, z]);
                    }
                }



            }
        }

    }

    /// <summary>
    /// Gets the tile data at x and y.
    /// </summary>
    /// <returns>The <see cref="Tile"/>.</returns>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public Tile GetTileAt(int x, int z)
    {
        if (x >= Width || x < 0 || z >= Height || z < 0)
        {
            //Debug.LogError("Tile ("+x+","+y+") is out of range.");
            return null;
        }
        return tiles[x, z];
    }


    public Furniture PlaceFurniture(string objectType, Tile t, bool doRoomFloodFill = true)
    {
        //Debug.Log("PlaceInstalledObject");
        // TODO: This function assumes 1x1 tiles -- change this later!

        if (furniturePrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("furniturePrototypes doesn't contain a proto for key: " + objectType);
            return null;
        }

        Furniture furn = Furniture.PlaceInstance(furniturePrototypes[objectType], t);

        if (furn == null)
        {
            // Failed to place object -- most likely there was already something there.
            return null;
        }

        furn.RegisterOnRemovedCallback(OnFurnitureRemoved);
        furnitures.Add(furn);

        // Do we need to recalculate our rooms?
        if (doRoomFloodFill && furn.roomEnclosure)
        {
            Room.DoRoomFloodFill(furn.tile);
        }

        if (cbFurnitureCreated != null)
        {
            cbFurnitureCreated(furn);

            if (furn.movementCost != 1)
            {
                // Since tiles return movement cost as their base cost multiplied
                // buy the furniture's movement cost, a furniture movement cost
                // of exactly 1 doesn't impact our pathfinding system, so we can
                // occasionally avoid invalidating pathfinding graphs
                InvalidateTileGraph();  // Reset the pathfinding system
            }
        }

        return furn;
    }

    public void RegisterFurnitureCreated(Action<Furniture> callbackfunc)
    {
        cbFurnitureCreated += callbackfunc;
    }

    public void UnregisterFurnitureCreated(Action<Furniture> callbackfunc)
    {
        cbFurnitureCreated -= callbackfunc;
    }

    public void RegisterCharacterCreated(Action<Character> callbackfunc)
    {
        cbCharacterCreated += callbackfunc;
    }

    public void UnregisterCharacterCreated(Action<Character> callbackfunc)
    {
        cbCharacterCreated -= callbackfunc;
    }

    public void RegisterInventoryCreated(Action<Inventory> callbackfunc)
    {
        cbInventoryCreated += callbackfunc;
    }

    public void UnregisterInventoryCreated(Action<Inventory> callbackfunc)
    {
        cbInventoryCreated -= callbackfunc;
    }

    public void RegisterTileChanged(Action<Tile> callbackfunc)
    {
        cbTileChanged += callbackfunc;
    }

    public void UnregisterTileChanged(Action<Tile> callbackfunc)
    {
        cbTileChanged -= callbackfunc;
    }

    // Gets called whenever ANY tile changes
    void OnTileChanged(Tile t)
    {
        if (cbTileChanged == null)
            return;

        cbTileChanged(t);

        

        InvalidateTileGraph();

    }

    // This should be called whenever a change to the world
    // means that our old pathfinding info is invalid.
    public void InvalidateTileGraph()
    {
        tileGraph = null;
    }


    public bool IsFurniturePlacementValid(string furnitureType, Tile t)
    {
        return furniturePrototypes[furnitureType].IsValidPosition(t);
    }

    public bool IsCharacterPlacementValid(string characterType, Tile t)
    {
        return characterPrototypes[characterType].IsValidPosition(t);
    }

    public Furniture GetFurniturePrototype(string objectType)
    {
        if (furniturePrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("No furniture with type: " + objectType);
            return null;
        }

        return furniturePrototypes[objectType];
    }

    public Character GetCharacterPrototype(string objectType)
    {
        if (characterPrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("No furniture with type: " + objectType);
            return null;
        }

        return characterPrototypes[objectType];
    }

    //////////////////////////////////////////////////////////////////////////////////////
    /// 
    /// 						SAVING & LOADING
    /// 
    //////////////////////////////////////////////////////////////////////////////////////

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        // Save info here
        writer.WriteAttributeString("Width", Width.ToString());
        writer.WriteAttributeString("Height", Height.ToString());

        writer.WriteStartElement("Rooms");
        foreach (Room r in rooms)
        {

            if (GetOutsideRoom() == r)
                continue;   // Skip the outside room. Alternatively, should SetupWorld be changed to not create one?

            writer.WriteStartElement("Room");
            r.WriteXml(writer);
            writer.WriteEndElement();
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Tiles");
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                if (tiles[x, z].Type != TileType.Empty)
                {
                    writer.WriteStartElement("Tile");
                    tiles[x, z].WriteXml(writer);
                    writer.WriteEndElement();
                }
            }
        }
        writer.WriteEndElement();

        writer.WriteStartElement("Furnitures");
        foreach (Furniture furn in furnitures)
        {
            writer.WriteStartElement("Furniture");
            furn.WriteXml(writer);
            writer.WriteEndElement();

        }
        writer.WriteEndElement();

        writer.WriteStartElement("Characters");
        foreach (Character c in characters)
        {
            writer.WriteStartElement("Character");
            c.WriteXml(writer);
            writer.WriteEndElement();

        }
        writer.WriteEndElement();

        /*		writer.WriteStartElement("Width");
                writer.WriteValue(Width);
                writer.WriteEndElement();
        */

        //Debug.Log(writer.ToString());

    }

    public void ReadXml(XmlReader reader)
    {
        // Load info here

        Width = int.Parse(reader.GetAttribute("Width"));
        Height = int.Parse(reader.GetAttribute("Height"));

        SetupWorld(Width, Height);

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Rooms":
                    ReadXml_Rooms(reader);
                    break;
                case "Tiles":
                    ReadXml_Tiles(reader);
                    break;
                case "Furnitures":
                    ReadXml_Furnitures(reader);
                    break;
                case "Characters":
                    ReadXml_Characters(reader);
                    break;
            }
        }

        // DEBUGGING ONLY!  REMOVE ME LATER!
        // Create an Inventory Item
        Inventory inv = new Inventory("Steel_Plate", 50, 50);
        Tile t = GetTileAt(Width / 2, Height / 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null)
        {
            cbInventoryCreated(t.inventory);
        }

        inv = new Inventory("Steel_Plate", 50, 4);
        t = GetTileAt(Width / 2 + 2, Height / 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null)
        {
            cbInventoryCreated(t.inventory);
        }

        inv = new Inventory("Steel_Plate", 50, 3);
        t = GetTileAt(Width / 2 + 1, Height / 2 + 2);
        inventoryManager.PlaceInventory(t, inv);
        if (cbInventoryCreated != null)
        {
            cbInventoryCreated(t.inventory);
        }
    }

    void ReadXml_Tiles(XmlReader reader)
    {
        // We are in the "Tiles" element, so read elements until
        // we run out of "Tile" nodes.

        if (reader.ReadToDescendant("Tile"))
        {
            // We have at least one tile, so do something with it.

            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int z = int.Parse(reader.GetAttribute("Z"));
                tiles[x, z].ReadXml(reader);
            } while (reader.ReadToNextSibling("Tile"));

        }

    }

    void ReadXml_Furnitures(XmlReader reader)
    {
        if (reader.ReadToDescendant("Furniture"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int z = int.Parse(reader.GetAttribute("Z"));

                Furniture furn = PlaceFurniture(reader.GetAttribute("objectType"), tiles[x, z], false);
                furn.ReadXml(reader);
            } while (reader.ReadToNextSibling("Furniture"));

            /*			We don't need to do a flood fill on load, because we're getting room info
                        from the save file

                        foreach(Furniture furn in furnitures) {
                            Room.DoRoomFloodFill( furn.tile, true );
                        }
            */
        }

    }

    void ReadXml_Rooms(XmlReader reader)
    {
        if (reader.ReadToDescendant("Room"))
        {
            do
            {
                /*int x = int.Parse( reader.GetAttribute("X") );
				int y = int.Parse( reader.GetAttribute("Y") );

				Furniture furn = PlaceFurniture( reader.GetAttribute("objectType"), tiles[x,y], false );*/

                //furn.ReadXml(reader);

                Room r = new Room();
                rooms.Add(r);
                r.ReadXml(reader);
            } while (reader.ReadToNextSibling("Room"));

        }

    }



    void ReadXml_Characters(XmlReader reader)
    {
        if (reader.ReadToDescendant("Character"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int z = int.Parse(reader.GetAttribute("Z"));

                Character c = CreateCharacter(tiles[x, z]);
                c.ReadXml(reader);
            } while (reader.ReadToNextSibling("Character"));
        }
    }

    public void OnInventoryCreated(Inventory inv)
    {
        if (cbInventoryCreated != null)
            cbInventoryCreated(inv);
    }

    public void OnFurnitureRemoved(Furniture furn)
    {
        furnitures.Remove(furn);
    }
}
