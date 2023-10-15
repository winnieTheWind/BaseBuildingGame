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
    //protected Action<Furniture, float> updateActions;
    protected List<string> updateActions;

    protected string isEnterableAction;

    List<Job> jobs;

    public Vector3 jobSpotOffset = Vector3.zero;

    // If the job causes some kind of object to be spawned where will it appear?
    public Vector3 jobSpawnSpotOffset = Vector3.zero;

    public void Update(float deltaTime)
    {
        if (updateActions != null)
        {
            //updateActions(this, deltaTime);
            FurnitureActions.CallFunctionsWithFurniture(updateActions.ToArray(), this, deltaTime);

        }
    }

    public ENTERABILITY IsEnterable()
    {
        if (isEnterableAction == null || isEnterableAction.Length == 0)
        {
            return ENTERABILITY.Yes;
        }

        //FurnitureActions.CallFunctionsWithFurniture(isEnterableActions.ToArray(), this);
        DynValue ret = FurnitureActions.CallFunction(isEnterableAction, this);

        return (ENTERABILITY)ret.Number;
    }
    // This represents the BASE tile of the object -- but in practice, large objects may actually occupy
    // multile tiles.
    public Tile tile
    {
        get; protected set;
    }

    // This "objectType" will be queried by the visual system to know what sprite to render for this object
    public string objectType
    {
        get; protected set;
    }

    private string _Name = null;
    public string Name
    {
        get
        {
            if (_Name == null || _Name.Length == 0)
            {
                return objectType;
            }
            return _Name;
        }
        set
        {
            _Name = value;
        }
    }

    // This is a multipler. So a value of "2" here, means you move twice as slowly (i.e. at half speed)
    // Tile types and other environmental effects may be combined.
    // For example, a "rough" tile (cost of 2) with a table (cost of 3) that is on fire (cost of 3)
    // would have a total movement cost of (2+3+3 = 8), so you'd move through this tile at 1/8th normal speed.
    // SPECIAL: If movementCost = 0, then this tile is impassible. (e.g. a wall).
    public float movementCost { get; protected set; }

    public bool roomEnclosure { get; protected set; }

    // For example, a sofa might be 3x2 (actual graphics only appear to cover the 3x1 area, but the extra row is for leg room.)
    public int Width { get; protected set; }
    public int Height { get; protected set; }

    public Color tint = Color.white;

    public bool linksToNeighbour
    {
        get; protected set;
    }

    public bool Is3D { get; protected set; }

    public Action<Furniture> cbOnChanged;
    public Action<Furniture> cbOnRemoved;

    Func<Tile, bool> funcPositionValidation;

    // Empty constructor is used for serialization
    public Furniture() {
        updateActions = new List<string>();
        furnParameters = new Dictionary<string, float>();
        jobs = new List<Job>();
        // Assign the method directly to the delegate
        this.funcPositionValidation = this.DEFAULT__IsValidPosition;
        this.Height = 1;
        this.Width = 1;
        this.Is3D = false;
    }

    // Copy Constructor -- don't call this directly, unless we never
    // do ANY sub-classing. Instead use Clone(), which is more virtual.
    protected Furniture(Furniture other)
    {
        this.objectType = other.objectType;
        this.Name = other.Name;
        this.Is3D = other.Is3D;
        this.movementCost = other.movementCost;
        this.roomEnclosure = other.roomEnclosure;
        this.Width = other.Width;
        this.Height = other.Height;
        this.tint = other.tint;
        this.linksToNeighbour = other.linksToNeighbour;

        this.jobSpotOffset = other.jobSpotOffset;
        this.jobSpawnSpotOffset = other.jobSpawnSpotOffset;

        this.furnParameters = new Dictionary<string, float>(other.furnParameters);
        jobs = new List<Job>();

        if (other.updateActions != null)
            this.updateActions = new List<string>(other.updateActions);

        this.isEnterableAction = other.isEnterableAction;

        if (other.funcPositionValidation != null)
            this.funcPositionValidation = (Func<Tile, bool>)other.funcPositionValidation.Clone();

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
    //public Furniture(string objectType, bool is3D = false, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeighbour = false, bool roomEnclosure = false)
    //{
    //    Furniture obj = new Furniture();

    //    this.objectType = objectType;
    //    this.is3D = is3D;
    //    this.movementCost = movementCost;
    //    this.roomEnclosure = roomEnclosure;
    //    this.Width = width;
    //    this.Height = height;
    //    this.linksToNeighbour = linksToNeighbour;

    //    furnParameters = new Dictionary<string, float>();
    //}

    static public Furniture PlaceInstance(Furniture proto, Tile tile)
    {
        if (proto.funcPositionValidation(tile) == false)
        {
            return null;
        }
        Furniture obj = proto.Clone();
        //Furniture obj = new Furniture(proto);
        obj.tile = tile;

        // FIXME: This assumes we are 1x1!
        if (tile.PlaceFurniture(obj) == false)
        {
            return null;
        }

        if (obj.linksToNeighbour)
        {
            // This type of furniture links itself to its neighbours,
            // so we should inform our neighbours that they have a new
            // buddy.  Just trigger their OnChangedCallback.

            Tile t;
            int x = tile.X;
            int z = tile.Z;

            t = World.current.GetTileAt(x, z + 1);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                // We have a Northern Neighbour with the same object type as us, so
                // tell it that it has changed by firing is callback.
                t.furniture.cbOnChanged(t.furniture);
            }
            t = World.current.GetTileAt(x + 1, z);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = World.current.GetTileAt(x, z - 1);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = World.current.GetTileAt(x - 1, z);
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
        }

        return obj;
    }

    public void RegisterOnChangedCallback(Action<Furniture> callbackFunc)
    {
        cbOnChanged += callbackFunc;
    }

    public void UnregisterOnChangedCallback(Action<Furniture> callbackFunc)
    {
        cbOnChanged -= callbackFunc;
    }

    public void RegisterOnRemovedCallback(Action<Furniture> callbackFunc)
    {
        cbOnRemoved += callbackFunc;
    }

    public void UnregisterOnRemovedCallback(Action<Furniture> callbackFunc)
    {
        cbOnRemoved -= callbackFunc;
    }

    public bool IsValidPosition(Tile t)
    {
        return funcPositionValidation(t);
    }

    // This part should remain unchanged
    // FIXME: These functions should never be called directly,
    // This will be replaced by validation checks fed to use fro lua files
    // that will be customizable for each of piece of furniture.
    // For ex. a dor might be specific that it needs two walls to connect to.

    protected bool DEFAULT__IsValidPosition(Tile t)
    {
        List<TileType> allowedTileTypes = new List<TileType>
    {
        TileType.Grass,
        TileType.Stone_Panel,
        TileType.Wood_Panel,
        TileType.Concrete_Slab,
        TileType.Concrete_Slab2,
        TileType.Clean_Concrete_Slab,
        TileType.Cracked_Slab,
        TileType.Road1,
        TileType.Road2,
        TileType.Road3,
        TileType.Road4,
        TileType.Road5,


        // Add more allowed tile types as needed
    };

        for (int x_off = t.X; x_off < (t.X + Width); x_off++)
        {
            for (int z_off = t.Z; z_off < (t.Z + Height); z_off++)
            {
                Tile t2 = World.current.GetTileAt(x_off, z_off);

                if (!allowedTileTypes.Contains(t2.Type))
                {
                    return false;
                }

                // Make sure tile doesnt already have furniture
                if (t2.furniture != null)
                {
                    return false;
                }
            }
        }

        return true; // Or whatever logic you need here
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", tile.X.ToString());
        writer.WriteAttributeString("Z", tile.Z.ToString());
        writer.WriteAttributeString("objectType", objectType.ToString());

        foreach (string k in furnParameters.Keys)
        {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("name", k);
            writer.WriteAttributeString("value", furnParameters[k].ToString());
            writer.WriteEndElement();
        }
    }


    public void ReadXmlPrototype(XmlReader reader_parent)
    {
        //Debug.Log("ReadXmlPrototype");

        objectType = reader_parent.GetAttribute("objectType");

        XmlReader reader = reader_parent.ReadSubtree();

        while (reader.Read())
        {
            switch (reader.Name)
            {
                case "Is3D":
                    reader.Read();
                    Is3D = reader.ReadContentAsBoolean();
                    break;
                case "Name":
                    reader.Read();
                    Name = reader.ReadContentAsString();
                    break;
                case "MovementCost":
                    reader.Read();
                    movementCost = reader.ReadContentAsFloat();
                    break;
                case "Width":
                    reader.Read();
                    Width = reader.ReadContentAsInt();
                    break;
                case "Height":
                    reader.Read();
                    Height = reader.ReadContentAsInt();
                    break;
                case "LinksToNeighbours":
                    reader.Read();
                    linksToNeighbour = reader.ReadContentAsBoolean();
                    break;
                case "EnclosesRooms":
                    reader.Read();
                    roomEnclosure = reader.ReadContentAsBoolean();
                    break;
                case "BuildingJob":
                    float jobTime = float.Parse(reader.GetAttribute("jobTime"));

                    List<Inventory> invs = new List<Inventory>();

                    XmlReader invs_reader = reader.ReadSubtree();

                    while (invs_reader.Read())
                    {
                        if (invs_reader.Name == "Inventory")
                        {
                            // Found an inventory requirement, so add it to the list!
                            invs.Add(new Inventory(
                                invs_reader.GetAttribute("objectType"),
                                int.Parse(invs_reader.GetAttribute("amount")),
                                0
                            ));
                        }
                    }
                    Job j = new Job(null,
                        objectType,
                        FurnitureActions.JobComplete_FurnitureBuilding, jobTime,
                        invs.ToArray()
                    );

                    World.current.SetFurnitureJobPrototype(j, this);

                    break;
                case "OnUpdate":
                    string functionName = reader.GetAttribute("FunctionName");
                    RegisterUpdateAction(functionName);
                    break;
                case "IsEnterable":
                    isEnterableAction = reader.GetAttribute("FunctionName");
                    break;
                case "JobSpotOffset":
                    jobSpotOffset = new Vector3(
                        int.Parse(reader.GetAttribute("X")), 
                        0, 
                        int.Parse(reader.GetAttribute("Z"))
                    );
                    
                    break;
                case "JobSpawnSpotOffset":
                    jobSpawnSpotOffset = new Vector3(
                        int.Parse(reader.GetAttribute("X")),
                        0,
                        int.Parse(reader.GetAttribute("Z"))
                    );
                    break;
                case "Params":
                    ReadXmlParams(reader);  // Read in the Param tag
                    break;
            }
        }
    }

    public void ReadXml(XmlReader reader)
    {
        // X, Y, and objectType have already been set, and we should already
        // be assigned to a tile.  So just read extra data.

        //movementCost = int.Parse( reader.GetAttribute("movementCost") );

        ReadXmlParams(reader);
    }

    public void ReadXmlParams(XmlReader reader)
    {
        // X, Y, and objectType have already been set, and we should already
        // be assigned to a tile.  So just read extra data.

        //movementCost = int.Parse( reader.GetAttribute("movementCost") );

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



    public float GetParameter(string key, float default_value)
    {
        if (furnParameters.ContainsKey(key) == false)
        {
            return default_value;
        }

        return furnParameters[key];
    }

    public float GetParameter(string key)
    {
        return GetParameter(key, 0);
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

    /// <summary>
    /// Registers a function that will be called every Update.
    /// (Later this implementation might change a bit as we support LUA.)
    /// </summary>
    public void RegisterUpdateAction(string luaFunctionName)
    {
        updateActions.Add(luaFunctionName);
    }

    public void UnregisterUpdateAction(string luaFunctionName)
    {
        updateActions.Remove(luaFunctionName);

    }

    public int JobCount()
    {
        return jobs.Count;
    }

    public void AddJob(Job j)
    {
        j.furniture = this;
        jobs.Add(j);
        j.RegisterJobStoppedCallback(OnJobStopped);
        World.current.jobQueue.Enqueue(j);
    }

    void OnJobStopped(Job j)
    {
        RemoveJob(j); 
    }

    protected void RemoveJob(Job j)
    {
        j.UnregisterJobStoppedCallback(OnJobStopped);
        jobs.Remove(j);
        j.furniture = null;
    }

    protected void ClearJobs()
    {
        Job[] jobs_Array = jobs.ToArray();

        foreach (Job j in jobs_Array)
        {
            RemoveJob(j);
        }
    }

    public void CancelJobs()
    {
        Job[] jobs_Array = jobs.ToArray();

        foreach (Job j in jobs_Array)
        {
            j.CancelJob();
        }
    }

    public bool IsStockpile()
    {
        return objectType == "Stockpile";
    }

    public void Deconstruct()
    {
        tile.UnplaceFurniture();

        if (cbOnRemoved != null)
        {
            cbOnRemoved(this);
        }

        if (roomEnclosure)
        {
            Room.DoRoomFloodFill(this.tile, false);
        }

        World.current.InvalidateTileGraph();

        // At this point no data structures should be pointing to us, so we
        // should get garbage-collected.

    }
    public Tile GetJobSpotTile()
    {
        return World.current.GetTileAt(tile.X + (int)jobSpotOffset.x, tile.Z + (int)jobSpotOffset.z);
    }

    public Tile GetSpawnSpotTile ()
    {
        return World.current.GetTileAt(tile.X + (int)jobSpawnSpotOffset.x, tile.Z + (int)jobSpawnSpotOffset.z);
    }

    #region ISelectableInterface implementation
    public string GetName()
    {
        return this.Name;
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
