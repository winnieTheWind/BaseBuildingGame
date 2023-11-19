
using System;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;

[MoonSharpUserData]
public class Furniture : IXmlSerializable, ISelectableInterface
{
    // Custom parameters for this particular piece of furniture (e.g sofa)
    // We are using a dictionary because later, custom lua function will be
    // able to use whatever parameters the use/modder would like.
    // Basically the lua code will bind to this dictionary.
    protected Dictionary<string, float> furnParameters;
    // These actions are called every update, they get passed the furniture they belong to..
    protected Action<Furniture, float> updateActions;

    public Func<Furniture, ENTERABILITY> IsEnterable;

    List<Job> jobs;

    public Vector3 JobSpotOffset = Vector3.zero;
    public Vector3 JobSpawnSpotOffset = Vector3.zero;

    public void Update(float deltaTime)
    {
        if (updateActions != null)
        {
            updateActions(this, deltaTime);
        }
    }

    // This represents the BASE tile of the object -- but in practice, large objects may actually occupy
    // multile tiles.
    public Tile Tile
    {
        get; protected set;
    }

    // This "objectType" will be queried by the visual system to know what sprite to render for this object
    public string ObjectType
    {
        get; protected set;
    }

    // This is a multipler. So a value of "2" here, means you move twice as slowly (i.e. at half speed)
    // Tile types and other environmental effects may be combined.
    // For example, a "rough" tile (cost of 2) with a table (cost of 3) that is on fire (cost of 3)
    // would have a total movement cost of (2+3+3 = 8), so you'd move through this tile at 1/8th normal speed.
    // SPECIAL: If movementCost = 0, then this tile is impassible. (e.g. a wall).
    public float MovementCost { get; protected set; }

    public bool RoomEnclosure;

    // For example, a sofa might be 3x2 (actual graphics only appear to cover the 3x1 area, but the extra row is for leg room.)
    public int Width { get; protected set; }
    public int Height { get; protected set; }

    public float FloorHeight { get; protected set; }

    public Color32 Tint = new Color32(255, 255, 255, 255);

    public bool LinksToNeighbour
    {
        get; protected set;
    }

    public EdgeOrientation edgeOrientation { get; set; }

    // TODO: Implement Larger Objects
    // TODO: Implement Object Rotation

    public Action<Furniture> cbOnChanged;
    public Action<Furniture> cbOnCeilingFurnitureChanged;
    public Action<Furniture> cbOnRemoved;

    Func<Tile, bool> funcPositionValidation;

    public bool Is3D = false;

    public int Bitmask { get; set; }

    public bool UsingFurniture = false;

    public Queue<Character> CharacterQueue;

    // Using Prototype Design Pattern

    // Empty constructor is used for serialization
    public Furniture() {
        furnParameters = new Dictionary<string, float>();
        jobs = new List<Job>();
        CharacterQueue = new Queue<Character>();
    }

    // Copy constructor -- dont call this directly unless
    // we never do any sub-classing, instead use Clone().
    // which is more virtual.

    // Creates a new Furniture object as a copy of an existing one.
    // This is particularly useful when you want to create a new instance
    // that initially shares the same properties as an existing instance.

    // The advantage is that you don't need to set all the properties again;
    // they are copied from the prototype. This is particularly useful if
    // the initialization process is complex or resource-intensive.

    protected Furniture(Furniture other)
    {
        this.ObjectType = other.ObjectType;
        this.MovementCost = other.MovementCost;
        this.RoomEnclosure = other.RoomEnclosure;
        this.Width = other.Width;
        this.Height = other.Height;
        this.Tint = other.Tint;
        this.LinksToNeighbour = other.LinksToNeighbour;
        this.JobSpotOffset = other.JobSpotOffset;
        this.JobSpawnSpotOffset = other.JobSpawnSpotOffset;
        this.FloorHeight = other.FloorHeight;

        this.Is3D = other.Is3D;

        this.furnParameters = new Dictionary<string, float>(other.furnParameters);
        jobs = new List<Job>();

        if (other.updateActions != null)
            this.updateActions = (Action<Furniture, float>)other.updateActions.Clone();

        if (other.funcPositionValidation != null)
            this.funcPositionValidation = (Func<Tile, bool>)other.funcPositionValidation.Clone();

        this.IsEnterable = other.IsEnterable;
    }

    // Make a copy of the current furniture. Sub-classed should
    // override this Clone() if a different (sub-classed) copy
    // constructor should be run.
    virtual public Furniture Clone()
    {
        return new Furniture(this);
    }

    // This is basically used by our object factory to create the prototypical object
    // Note that it DOESN'T ask for a tile.

    // Create furniture from parameters -- this will probably only ever be used for prototypes
    public Furniture(string objectType, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeighbour = false, bool roomEnclosure = false, bool is3D = false, float floorHeight = 0)
    {
        Furniture obj = new Furniture();

        this.ObjectType = objectType;
        this.MovementCost = movementCost;
        this.RoomEnclosure = roomEnclosure;
        this.Width = width;
        this.Height = height;
        this.LinksToNeighbour = linksToNeighbour;
        this.Is3D = is3D;
        this.FloorHeight = floorHeight;
        // Assign the method directly to the delegate
        this.funcPositionValidation = this.DEFAULT__IsValidPosition;

        furnParameters = new Dictionary<string, float>();
    }

    static public Furniture PlaceInstance(Furniture proto, Tile tile)
    {
        if (proto.funcPositionValidation(tile) == false)
        {
            return null;
        }
        Furniture obj = proto.Clone();
        obj.Tile = tile;

        // Place the furniture
        if (tile.PlaceFurniture(obj) == false)
        {
            return null; // Failed to place the furniture
        }

        if (obj.LinksToNeighbour)
        {
            Tile t;
            int x = tile.X;
            int z = tile.Z;

            // This needs to be worked on to be tidier, too much repetition.

            // Northern neighbour
            t = World.current.GetTileAt(x, z + 1);
            if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.cbOnChanged(t.Furniture);
            }
            // Western neighbour
            t = World.current.GetTileAt(x - 1, z);
            if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.cbOnChanged(t.Furniture);
            }
            // Eastern neighbour
            t = World.current.GetTileAt(x + 1, z);
            if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.cbOnChanged(t.Furniture);
            }
            // Southern neighbour
            t = World.current.GetTileAt(x, z - 1);
            if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.cbOnChanged(t.Furniture);
            }
            // NE neighbour
            t = World.current.GetTileAt(x + 1, z + 1);
            if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.cbOnChanged(t.Furniture);
            }
            // NW neighbour
            t = World.current.GetTileAt(x - 1, z + 1);
            if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.cbOnChanged(t.Furniture);
            }
            // SE neighbour
            t = World.current.GetTileAt(x + 1, z - 1);
            if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.cbOnChanged(t.Furniture);
            }
            // SW neighbour
            t = World.current.GetTileAt(x - 1, z - 1);
            if (t != null && t.Furniture != null && t.Furniture.cbOnChanged != null && t.Furniture.ObjectType == obj.ObjectType)
            {
                t.Furniture.cbOnChanged(t.Furniture);
            }

            // Ceiling objects only
            // Northern neighbour
            t = World.current.GetTileAt(x, z + 1);
            if (t != null && t.Ceiling != null && t.Ceiling.cbOnChanged != null && t.Ceiling.ObjectType == "Ceiling")
            {
                t.Ceiling.cbOnChanged(t.Ceiling);
            }
            // Western neighbour
            t = World.current.GetTileAt(x - 1, z);
            if (t != null && t.Ceiling != null && t.Ceiling.cbOnChanged != null && t.Ceiling.ObjectType == "Ceiling")
            {
                t.Ceiling.cbOnChanged(t.Ceiling);
            }
            // Eastern neighbour
            t = World.current.GetTileAt(x + 1, z);
            if (t != null && t.Ceiling != null && t.Ceiling.cbOnChanged != null && t.Ceiling.ObjectType == "Ceiling")
            {
                t.Ceiling.cbOnChanged(t.Ceiling);
            }
            // Southern neighbour
            t = World.current.GetTileAt(x, z - 1);
            if (t != null && t.Ceiling != null && t.Ceiling.cbOnChanged != null && t.Ceiling.ObjectType == "Ceiling")
            {
                t.Ceiling.cbOnChanged(t.Ceiling);
            }
            // NE neighbour
            t = World.current.GetTileAt(x + 1, z + 1);
            if (t != null && t.Ceiling != null && t.Ceiling.cbOnChanged != null && t.Ceiling.ObjectType == "Ceiling")
            {
                t.Ceiling.cbOnChanged(t.Ceiling);
            }
            // NW neighbour
            t = World.current.GetTileAt(x - 1, z + 1);
            if (t != null && t.Ceiling != null && t.Ceiling.cbOnChanged != null && t.Ceiling.ObjectType == "Ceiling")
            {
                t.Ceiling.cbOnChanged(t.Ceiling);
            }
            // SE neighbour
            t = World.current.GetTileAt(x + 1, z - 1);
            if (t != null && t.Ceiling != null && t.Ceiling.cbOnChanged != null && t.Ceiling.ObjectType == "Ceiling")
            {
                t.Ceiling.cbOnChanged(t.Ceiling);
            }
            // SW neighbour
            t = World.current.GetTileAt(x - 1, z - 1);
            if (t != null && t.Ceiling != null && t.Ceiling.cbOnChanged != null && t.Ceiling.ObjectType == "Ceiling")
            {
                t.Ceiling.cbOnChanged(t.Ceiling);
            }
        }

        return obj;
    }

    public bool IsValidPosition(Tile t)
    {
        return funcPositionValidation(t);
    }

    // This part should remain unchanged
    // This function should never be called directly,

    protected bool DEFAULT__IsValidPosition(Tile t)
    {
        List<TileType> allowedTileTypes = new List<TileType>
        {
            TileType.Grass,
            TileType.Stone_Panel,
            TileType.Wood_Panel,
            TileType.Slab1,
            TileType.Slab2,
            TileType.Slab3,
            TileType.Slab4,
            TileType.Road1,
            TileType.Road2,
            TileType.Road3,
            TileType.Road4,
            TileType.Road5,
            // Add more allowed tile types as needed
        };

        return HandleFurnitureValidPosition(t, this.ObjectType, allowedTileTypes); // Or whatever logic you need here
    }

    bool HandleFurnitureValidPosition(Tile t, string objectType, List<TileType> allowedTileTypes)
    {
        for (int x_off = t.X; x_off < (t.X + Width); x_off++)
        {
            for (int z_off = t.Z; z_off < (t.Z + Height); z_off++)
            {
                Tile t2 = World.current.GetTileAt(x_off, z_off);

                // If we're placing a CashRegister, the tile must have a Desk.
                if (objectType == "Ceiling")
                {
                    if (t2.Ceiling != null)
                    {
                        return false;
                    }

                    // For other furniture, the normal rules apply.
                    if (!allowedTileTypes.Contains(t2.Type))
                    {
                        return false;
                    }
                }
                else if (objectType == "CashRegister")
                {
                    if (t2.Furniture == null)
                    {
                        return false;
                    }

                    if (t2.Furniture.ObjectType != "Desk")
                    {
                        return false;
                    }
                    // For other furniture, the normal rules apply.
                    if (!allowedTileTypes.Contains(t2.Type))
                    {
                        return false;
                    }
                }
                else
                {
                    // For other furniture, the normal rules apply.
                    if (!allowedTileTypes.Contains(t2.Type) || t2.Furniture != null)
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", Tile.X.ToString());
        writer.WriteAttributeString("Z", Tile.Z.ToString());
        writer.WriteAttributeString("objectType", ObjectType.ToString());

        foreach (string k in furnParameters.Keys)
        {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("name", k);
            writer.WriteAttributeString("value", furnParameters[k].ToString());
            writer.WriteEndElement();
        }
    }

    public void ReadXml(XmlReader reader)
    {
        if (reader.ReadToDescendant("Param"))
        {
            do
            {
                string k = reader.GetAttribute("name");
                float v = float.Parse(reader.GetAttribute("value"));
                furnParameters[k] = v;
            } while (reader.ReadToNextSibling("Param"));
        }
    }

    public float GetParameter(string key, float default_value = 0)
    {
        if (furnParameters.ContainsKey(key) == false)
        {
            return default_value;
        }

        return furnParameters[key];
    }

    public void SetParameter(string key, float value)
    {
        furnParameters[key] = value;
    }

    public void ChangeParameter(string key, float value)
    {
        if (furnParameters.ContainsKey(key) == false)
        {
            furnParameters[key] = value;
        }

        furnParameters[key] += value;
    }

    public void RegisterUpdateAction(Action<Furniture, float> a)
    {
        updateActions += a;
    }

    public void UnregisterUpdateAction(Action<Furniture, float> a)
    {
        updateActions -= a;
    }

    public int JobCount()
    {
        return jobs.Count;
    }

    public void AddJob(Job j)
    {
        j.furniture = this;
        jobs.Add(j);

        // Register stopping of job event
        j.cbJobStopped += OnJobStopped;

        World.current.jobQueue.Enqueue(j);
    }

    void OnJobStopped(Job j)
    {
        RemoveJob(j);
    }

    protected void RemoveJob(Job j)
    {
        // Unregister stopping of job event
        j.cbJobStopped -= OnJobStopped;

        jobs.Remove(j);
        j.furniture = null;

    }

    protected void ClearJobs()
    {
        Job[] jobs_array = jobs.ToArray();
        foreach (Job j in jobs_array)
        {
            RemoveJob(j);
        }
    }

    public void CancelJobs()
    {
        Job[] jobs_array = jobs.ToArray();
        foreach (Job j in jobs_array)
        {
            j.CancelJob();
        }
    }

    public bool IsStockpile()
    {
        return ObjectType == "Stockpile";
    }

    public void Deconstruct()
    {
        Tile.UnplaceFurniture();

        if (cbOnRemoved != null)
        {
            cbOnRemoved(this);
        }

        if (RoomEnclosure)
        {
            Room.DoRoomFloodFill(this.Tile, false);
        }

        World.current.InvalidateTileGraph();

        // At this point no data structures should be pointing to us, so we
        // should get garbage-collected.

    }
    public Tile GetJobSpotTile()
    {
        return World.current.GetTileAt(Tile.X + (int)JobSpotOffset.x, Tile.Z + (int)JobSpotOffset.z);
    }

    public Tile GetSpawnSpotTile()
    {
        // TODO: Allow us to customize this
        return World.current.GetTileAt(Tile.X + (int)JobSpawnSpotOffset.x, Tile.Z + (int)JobSpawnSpotOffset.z);
    }

    #region ISelectableInterface implementation
    public string GetName()
    {
        return this.ObjectType;
    }
    public string GetDescription()
    {
        return "This is a piece of Furniture."; // TODO: Add "Description" property and matching XML field.
    }
    public string GetHitPointString()
    {
        return "18/18"; // TODO: Add a hitpoint system to... well.. everything.
    }
    public string GetCharacterType()
    {
        return null; // TODO: Add a hitpoint system to... well.. everything.
    }
    #endregion
}
