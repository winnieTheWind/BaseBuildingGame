
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using MoonSharp.Interpreter;

[MoonSharpUserData]

public class World : IXmlSerializable
{
    // A two-dimensional array to hold our tile data.
    Tile[,] tiles;

    public List<Character> characters;
    public List<Furniture> furnitures;
    public List<LayerTile> layerTiles;
    public List<Room> rooms;
    public List<Inventory> inventories;
    public InventoryManager inventoryManager;

    // The pathfinding graph used to navigate our world map.
    public Path_TileGraph tileGraph;

    public Dictionary<string, Furniture> furniturePrototypes;
    public Dictionary<string, Job> furnitureJobPrototypes;
    public Dictionary<string, LayerTile> layerTilePrototypes;
    public Dictionary<string, Job> tileJobPrototypes;

    // The tile width of the world.
    public int Width { get; protected set; }

    // The tile height of the world
    public int Height { get; protected set; }

    Action<Furniture> cbFurnitureCreated;
    Action<Character> cbCharacterCreated;
    Action<Inventory> cbInventoryCreated;
    Action<LayerTile> cbLayerTileCreated;

    Action<Tile> cbTileChanged;

    Action<LayerTile> cbLayerTileRemoved;

    // TODO: Most likely this will be replaced with a dedicated
    // class for managing job queues (plural!) that might also
    // be semi-static or self initializing or some damn thing.
    // For now, this is just a PUBLIC member of World
    public JobQueue jobQueue;

    private float CameraPosX;
    private float CameraPosZ;

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

        // Make one character
        CreateCharacter(GetTileAt(Width / 2, Height / 2));

        currentUserState = UserState.GAME;
    }

    public World() {}

    public Room GetOutsideRoom()
    {
        return rooms[0];
    }

    public void AddRoom(Room r)
    {
        rooms.Add(r);
    }

    public void DeleteRoom(Room r)
    {
        if (r == GetOutsideRoom())
        {
            Debug.LogError("Tried to delete the outside room!");
        }

        // removes this rooms from our rooms list
        rooms.Remove(r);

        // all tiles that belonged to this room should be reassigned to the outside.
        r.ReturnTilesToOutsideRoom();
    }

    void SetupWorld(int width, int height)
    {
        jobQueue = new JobQueue();

        // set the current world to be this world
        // todo: do we need to do any cleanup?
        current = this;

        Width = width;
        Height = height;

        tiles = new Tile[Width, Height];

        rooms = new List<Room>();
        rooms.Add(new Room(this)); // Create the outside?

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                tiles[x, y] = new Tile(x, y);
                tiles[x, y].RegisterTileTypeChangedCallback(OnTileChanged);
                tiles[x, y].room = GetOutsideRoom();
            }
        }

        //Debug.Log("World created with " + (Width * Height) + " tiles.");

        CreateFurniturePrototypes();

        characters = new List<Character>();
        furnitures = new List<Furniture>();
        inventoryManager = new InventoryManager();
        layerTiles = new List<LayerTile>();
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
        Character c = new Character(t);
        characters.Add(c);

        if (cbCharacterCreated != null)
            cbCharacterCreated(c);

        return c;
    }

    void CreateFurniturePrototypes()
    {
        furniturePrototypes = new Dictionary<string, Furniture>();
        furnitureJobPrototypes = new Dictionary<string, Job>();
        layerTilePrototypes = new Dictionary<string, LayerTile>();

        layerTilePrototypes.Add("Dirt", new LayerTile(1, 1, "Dirt", true));
        layerTilePrototypes.Add("Stockpile", new LayerTile(1, 1, "Stockpile", true));

        var furniturePrototypesTest = PrototypeFactory.GetPrototypes<FurniturePrototype>("furniture_prototypes");

        foreach (var kvp in furniturePrototypesTest)
        {
            var proto = kvp.Value;

            // Create the Furniture instance using the prototype data.
            furniturePrototypes.Add(proto.Type,
                new Furniture(
                    proto.Type,
                    proto.PathfindingCost, // This will be in FurniturePrototype, replace with appropriate property
                    proto.Width,
                    proto.Height,
                    proto.LinksToNeighbours,
                    proto.EnclosesRooms,
                    proto.Is3D
            ));
        }

        furnitureJobPrototypes.Add("Wall",
            new Job(null,
                "Wall",
                FurnitureActions.JobComplete_FurnitureBuilding, 1f,
                new Inventory[] { new Inventory("Steel_Plate", 5, 0) },
                false
            )
        );

        furnitureJobPrototypes.Add("Desk",
            new Job(null,
                "Desk",
                FurnitureActions.JobComplete_FurnitureBuilding, 1f,
                null,
                false
            )
        );

        furnitureJobPrototypes.Add("Round_Table",
            new Job(null,
                "Round_Table",
                FurnitureActions.JobComplete_FurnitureBuilding, 1f,
                null,
                false
            )
        );

        furniturePrototypes["Door"].SetParameter("openness", 0);
        furniturePrototypes["Door"].SetParameter("is_opening", 0);
        furniturePrototypes["Door"].RegisterUpdateAction(FurnitureActions.Door_UpdateAction);

        furniturePrototypes["Door"].IsEnterable = FurnitureActions.Door_IsEnterable;

        furniturePrototypes["Stockpile"].RegisterUpdateAction(FurnitureActions.Stockpile_UpdateAction);
        furniturePrototypes["Stockpile"].tint = new Color32(186, 31, 31, 255);
        furnitureJobPrototypes.Add("Stockpile",
            new Job(
                null,
                "Stockpile",
                FurnitureActions.JobComplete_FurnitureBuilding,
                -1,
                null,
                false,
                false
            )
        );

        furniturePrototypes["Oxygen_Generator"].RegisterUpdateAction(FurnitureActions.OxygenGenerator_UpdateAction);

        furniturePrototypes["Mining_Drone_Station"].jobSpotOffset = new Vector3(1, 0, 0);
        furniturePrototypes["Mining_Drone_Station"].RegisterUpdateAction(FurnitureActions.MiningDroneStation_UpdateAction);
    }

    /// A function for testing out the system
    public void RandomizeTiles()
    {
        Debug.Log("RandomizeTiles");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (UnityEngine.Random.Range(0, 2) == 0)
                {
                    tiles[x, y].Type = TileType.Empty;
                }
                else
                {
                    tiles[x, y].Type = TileType.Grass;
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
            for (int y = b - 5; y < b + 15; y++)
            {
                tiles[x, y].Type = TileType.Grass;
                if (x == l || x == (l + 9) || y == b || y == (b + 9))
                {
                    if (x != (l + 9) && y != (b + 4))
                    {
                        PlaceFurniture("Wall", tiles[x, y]);
                    }
                }
            }
        }
    }
    
    public Tile GetTileAt(int x, int z)
    {
        if (x >= Width || x < 0 || z >= Height || z < 0)
        {
            return null;
        }
        return tiles[x, z];
    }

    public Furniture PlaceFurniture(string objectType, Tile t, bool doRoomFloodFill = true)
    {
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
            Room.DoRoomFloodFill(furn.tile, false);
        }

        if (cbFurnitureCreated != null)
        {
            cbFurnitureCreated(furn);
            if (furn.movementCost != 1)
            {
                // since tiles return movement cost as their base cost multiplied
                // by the furnitures movement cose, a furniture movement cost
                // of exactly 1 doesnt impact oour pathfinding system so we can
                // occasionally avoid invalidating pathfinding graphs..f
                InvalidateTileGraph(); // Reset the pathfinding system
            }
        }

        return furn;
    }

    public LayerTile PlaceLayerTile(string type, Tile t)
    {
        // TODO: This function assumes 1x1 tiles -- change this later!
        if (layerTilePrototypes.ContainsKey(type) == false)
        {
            Debug.LogError("layerTilePrototypes doesn't contain a proto for key: " + type);
            return null;
        }

        LayerTile layerTile = LayerTile.PlaceInstance(layerTilePrototypes[type], t);

        if (layerTile == null)
        {
            // Failed to place layer tile -- most likely there was already something there.
            return null;
        }

        layerTile.RegisterOnRemovedCallback(OnLayerTileRemoved);
        layerTiles.Add(layerTile);

        if (cbLayerTileCreated != null)
        {
            cbLayerTileCreated(layerTile);
        }

        return layerTile;
    }


    public void RegisterLayerTileCreated(Action<LayerTile> callbackfunc)
    {
        cbLayerTileCreated += callbackfunc;
    }

    public void UnregisterLayerTileCreated(Action<LayerTile> callbackfunc)
    {
        cbLayerTileCreated -= callbackfunc;
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

    public void RegisterLayerTileRemoved(Action<LayerTile> callbackfunc)
    {
        cbLayerTileRemoved += callbackfunc;
    }

    public void UnregisterLayerTileRemoved(Action<LayerTile> callbackfunc)
    {
        cbLayerTileRemoved -= callbackfunc;
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

    public Furniture GetFurniturePrototype(string objectType)
    {
        if (furniturePrototypes.ContainsKey(objectType) == false)
        {
            Debug.LogError("No furniture with type: " + objectType);
            return null;
        }

        return furniturePrototypes[objectType];
    }

    public LayerTile GetLayerTilePrototype(string type)
    {
        if (layerTilePrototypes.ContainsKey(type) == false)
        {
            Debug.LogError("No layer tile with type: " + type);
            return null;
        }

        return layerTilePrototypes[type];
    }


    //////////////////////////////////////////////////////////////////////////////////////
    /// 						SAVING & LOADING
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

        writer.WriteAttributeString("CameraPosX", Camera.main.transform.position.x.ToString());
        writer.WriteAttributeString("CameraPosZ", Camera.main.transform.position.z.ToString());

        writer.WriteStartElement("Tiles");
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (tiles[x, y].Type != TileType.Empty)
                {
                    writer.WriteStartElement("Tile");
                    tiles[x, y].WriteXml(writer);
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

        writer.WriteStartElement("LayerTiles");
        foreach (LayerTile layerTile in layerTiles)
        {
            writer.WriteStartElement("LayerTile");
            layerTile.WriteXml(writer);
            writer.WriteEndElement();

        }
        writer.WriteEndElement();

        writer.WriteStartElement("Inventories");
        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                if (tiles[x, z].inventory != null) // Assuming tiles have an 'Inventory' property
                {
                    writer.WriteStartElement("Inventory");

                    // Serialize necessary Tile info (like position) if needed
                    writer.WriteAttributeString("X", x.ToString());
                    writer.WriteAttributeString("Z", z.ToString());

                    // Serialize the actual Inventory data
                    tiles[x, z].inventory.WriteXml(writer);

                    writer.WriteEndElement();
                }
            }
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
    }

    public void ReadXml(XmlReader reader)
    {
        // Load info here
        Width = int.Parse(reader.GetAttribute("Width"));
        Height = int.Parse(reader.GetAttribute("Height"));

        SetupWorld(Width, Height);

        // Set the camera to the saved positions
        Camera.main.transform.position = new Vector3(float.Parse(reader.GetAttribute("CameraPosX")), 9, float.Parse(reader.GetAttribute("CameraPosZ")));

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Tiles":
                    ReadXml_Tiles(reader);
                    break;
                case "Furnitures":
                    ReadXml_Furnitures(reader);
                    break;
                case "Characters":
                    ReadXml_Characters(reader);
                    break;
                case "Inventories":
                    ReadXml_Inventories(reader);
                    break;
                case "LayerTiles":
                    ReadXml_LayerTiles(reader);
                    break;
            }
        }

        // DEBUGGING ONLY!  REMOVE ME LATER!
        // Create an Inventory Item
        //CreateInventoryItem(0, 0, "Steel_Plate", 150, 150);
        //CreateInventoryItem(2, 0, "Steel_Plate", 150, 150);
        //CreateInventoryItem(1, 2, "Steel_Plate", 150, 150);
        //CreateInventoryItem(2, 2, "Steel_Plate", 150, 150);
        //CreateInventoryItem(3, 2, "Steel_Plate", 150, 150);
        //CreateInventoryItem(4, 2, "Steel_Plate", 150, 150);
       
    }

    void CreateInventoryItem(int x, int z, string key, int maxSize, int currentSize)
    {
        Inventory inv = new Inventory(key, maxSize, currentSize);

        Tile t = GetTileAt(Width / 2 + x, Height / 2 + z);
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
            do {
                //We have at least one tile, so do something with it.
                int x = int.Parse(reader.GetAttribute("X"));
                int z = int.Parse(reader.GetAttribute("Z"));
                tiles[x, z].ReadXml(reader);
            } while (reader.ReadToNextSibling("Tile"));
        }
    }

    void ReadXml_LayerTiles(XmlReader reader)
    {
        if (reader.ReadToDescendant("LayerTile"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int z = int.Parse(reader.GetAttribute("Z"));

                LayerTile layerTile = PlaceLayerTile(reader.GetAttribute("type"), tiles[x, z]);
                layerTile.ReadXml(reader);
            } while (reader.ReadToNextSibling("LayerTile"));
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

            foreach (Furniture furn in furnitures)
            {
                Room.DoRoomFloodFill(furn.tile, true);
            }
        }
    }
    void ReadXml_Inventories(XmlReader reader)
    {
        // We are in the "Inventories" element, so read elements until
        // we run out of "Inventory" nodes.
        if (reader.ReadToDescendant("Inventory"))
        {
            do
            {
                int x = int.Parse(reader.GetAttribute("X"));
                int z = int.Parse(reader.GetAttribute("Z")); // Instead of "Y"

                // Here you need to determine what information is stored for each Inventory in your XML.
                // For instance, you likely store the 'objectType', 'maxStackSize', and 'stackSize'.
                string objectType = reader.GetAttribute("objectType");
                int maxStackSize = int.Parse(reader.GetAttribute("maxStackSize"));
                int stackSize = int.Parse(reader.GetAttribute("stackSize"));

                // Create a new Inventory object with the data extracted from the XML.
                Inventory inv = new Inventory(objectType, maxStackSize, stackSize);

                // Now, place the inventory onto the tile. This assumes you have a method for this purpose.
                Tile tile = GetTileAt(x, z);
                inventoryManager.PlaceInventory(tile, inv);  // This is assuming you have a method like this.

                // You may want to trigger any callbacks or updates here as well.
                if (cbInventoryCreated != null)
                {
                    cbInventoryCreated(inv);
                }

            } while (reader.ReadToNextSibling("Inventory"));
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
        {
            cbInventoryCreated(inv);
        }
    }

    public void OnFurnitureRemoved(Furniture furn)
    {
        furnitures.Remove(furn);
    }

    public void OnLayerTileRemoved(LayerTile layerTile)
    {

        cbLayerTileRemoved(null);
        layerTiles.Remove(layerTile);
    }
}
