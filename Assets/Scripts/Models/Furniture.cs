
// InstalledObjects are objects that are installed to a tile, like: Doors, Walls, Tables etc

using System;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using UnityEngine;
using MoonSharp.Interpreter;
using Unity.VisualScripting;
using System.Net.Security;

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

    public Vector3 jobSpotOffset = Vector3.zero;
    public Vector3 jobSpawnSpotOffset = Vector3.zero;


    public void Update(float deltaTime)
    {
        if (updateActions != null)
        {
            updateActions(this, deltaTime);
        }
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

    // This is a multipler. So a value of "2" here, means you move twice as slowly (i.e. at half speed)
    // Tile types and other environmental effects may be combined.
    // For example, a "rough" tile (cost of 2) with a table (cost of 3) that is on fire (cost of 3)
    // would have a total movement cost of (2+3+3 = 8), so you'd move through this tile at 1/8th normal speed.
    // SPECIAL: If movementCost = 0, then this tile is impassible. (e.g. a wall).
    public float movementCost { get; protected set; }

    public bool roomEnclosure;

    // For example, a sofa might be 3x2 (actual graphics only appear to cover the 3x1 area, but the extra row is for leg room.)
    public int Width { get; protected set; }
    public int Height { get; protected set; }

    public Color32 tint = new Color32(255, 255, 255, 255);

    public bool linksToNeighbour
    {
        get; protected set;
    }

    // TODO: Implement Larger Objects
    // TODO: Implement Object Rotation

    public Action<Furniture> cbOnChanged;
    public Action<Furniture> cbOnRemoved;

    Func<Tile, bool> funcPositionValidation;

    public bool Is3D = false;

    public int Bitmask { get; set; }

    // Empty constructor is used for serialization
    public Furniture() {
        furnParameters = new Dictionary<string, float>();
        jobs = new List<Job>();
    }

    // Copy constructor -- dont call this directly unless
    // we never do any sub-classing, instead use Clone().
    // which is more virtual.
    protected Furniture(Furniture other)
    {
        this.objectType = other.objectType;
        this.movementCost = other.movementCost;
        this.roomEnclosure = other.roomEnclosure;
        this.Width = other.Width;
        this.Height = other.Height;
        this.tint = other.tint;
        this.linksToNeighbour = other.linksToNeighbour;
        this.jobSpotOffset = other.jobSpotOffset;
        this.jobSpawnSpotOffset = other.jobSpawnSpotOffset;

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
    public Furniture(string objectType, float movementCost = 1f, int width = 1, int height = 1, bool linksToNeighbour = false, bool roomEnclosure = false, bool is3D = false)
    {
        Furniture obj = new Furniture();

        this.objectType = objectType;
        this.movementCost = movementCost;
        this.roomEnclosure = roomEnclosure;
        this.Width = width;
        this.Height = height;
        this.linksToNeighbour = linksToNeighbour;
        this.Is3D = is3D;
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
        //Furniture obj = new Furniture(proto);
        obj.tile = tile;

        // FIXME: This assumes we are 1x1!
        if (tile.PlaceFurniture(obj) == false)
        {
            return null;
        }

        if (obj.linksToNeighbour)
        {
            Tile t;
            int x = tile.X;
            int z = tile.Z;

            t = World.current.GetTileAt(x, z + 1); // Northern neighbour
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = World.current.GetTileAt(x - 1, z); // Western neighbour
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = World.current.GetTileAt(x + 1, z); // Eastern neighbour
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = World.current.GetTileAt(x, z - 1); // Southern neighbour

            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = World.current.GetTileAt(x + 1, z + 1); // NE neighbour
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = World.current.GetTileAt(x - 1, z + 1); // NW neighbour
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = World.current.GetTileAt(x + 1, z - 1); // SE neighbour
            if (t != null && t.furniture != null && t.furniture.cbOnChanged != null && t.furniture.objectType == obj.objectType)
            {
                t.furniture.cbOnChanged(t.furniture);
            }
            t = World.current.GetTileAt(x - 1, z - 1); // SW neighbour
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
        // If placing a CashRegister, we have different rules.
        bool isPlacingCashRegister = this.objectType == "CashRegister";

        for (int x_off = t.X; x_off < (t.X + Width); x_off++)
        {
            for (int z_off = t.Z; z_off < (t.Z + Height); z_off++)
            {
                Tile t2 = World.current.GetTileAt(x_off, z_off);

                // If we're placing a CashRegister, the tile must have a Desk.
                if (isPlacingCashRegister)
                {
                    if (t2.furniture == null || t2.furniture.objectType != "Desk")
                    {
                        // There's no Desk under this tile, so we can't place a CashRegister here.
                        return false;
                    }
                }
                else
                {
                    // For other furniture, the normal rules apply.
                    if (!allowedTileTypes.Contains(t2.Type) || t2.furniture != null)
                    {
                        return false;
                    }
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
        //writer.WriteAttributeString("movementCost", movementCost.ToString());

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
        //objectType = reader.GetAttribute("objectType");
        //movementCost = int.Parse(reader.GetAttribute("movementCost"));

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

    /// <summary>
    /// Gets the custom furniture parameter from a string key.
    /// </summary>
    /// <returns>The parameter value (float).</returns>
    /// <param name="key">Key string.</param>
    /// <param name="default_value">Default value.</param>
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

    /// <summary>
    /// Registers a function that will be called every Update.
    /// (Later this implementation might change a bit as we support LUA.)
    /// </summary>
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
        return objectType == "Stockpile";
    }

    public void Deconstruct()
    {
        UnityEngine.Debug.Log("Deconstruct");

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

    public Tile GetSpawnSpotTile()
    {
        // TODO: Allow us to customize this
        return World.current.GetTileAt(tile.X + (int)jobSpawnSpotOffset.x, tile.Z + (int)jobSpawnSpotOffset.z);
    }

    #region ISelectableInterface implementation
    public string GetName()
    {
        return this.objectType;
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
