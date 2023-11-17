using UnityEngine;
using System;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using System.Collections.Generic;
using MoonSharp.Interpreter;

public enum TileType { 

    Empty, Grass, Slab1, Slab2, 
    Slab3, Slab4, Road1, Road2, 
    Road3, Road4, Road5, Stone_Panel, 
    Wood_Panel, Dirt, Stockpile
};

public enum ENTERABILITY { Yes, Never, Soon };

[MoonSharpUserData]
public class Tile : IXmlSerializable, ISelectableInterface
{
    TileType _type = TileType.Empty;

    public TileType Type { 
        get { 
            return _type; 
        }
        set
        {
            TileType oldType = _type;
            _type = value;
            // Call the callback and let things know we've changed
            if (cbTileChanged != null && oldType != _type)
            {
                cbTileChanged(this);
            }
        }
    }

    public LayerTile LayerTile
    {
        get; protected set;
    }

    public Room Room;

    public Inventory Inventory;
    public Furniture Furniture
    {
        get; protected set;
    }

    public Furniture Ceiling
    {
        get; protected set;
    }

    public Furniture TableFurniture
    {
        get; protected set;
    }

    public List<Furniture> FurnitureItems;

    public List<Character> Characters;

    public Job PendingFurnitureJob;
    public Job PendingTileJob;

    Action<Tile> cbTileChanged;

    public int X { get; protected set; }
    public int Z { get; protected set; }

    public bool IsUsingBitmaskText = false;

    private float _movementCost = 1f; // Default value

    public float MovementCost
    {
        get
        {
            if (Type == TileType.Empty)
                return 0;   // 0 is unwalkable

            if (Furniture == null)
                return _movementCost;

            if (Furniture.ObjectType == "Mining_Drone_Station")
            {
                if (Furniture.GetJobSpotTile() == this)
                    return 1;
                if (Furniture.GetSpawnSpotTile() == this)
                    return 1;
            }

            return _movementCost * Furniture.MovementCost;

        }
        set
        {
            _movementCost = value;
        }
    }



    public Tile(int x, int z) 
    {
        this.X = x;
        this.Z = z;
        Characters = new List<Character>();
        FurnitureItems = new List<Furniture>();

    }

    public void RegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged += callback;
    }

    public void UnregisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged -= callback;
    }

    public bool UnplaceLayerTile()
    {
        // Just uninstalling. FIXME: What if we have a multi tile furniture?
        //furniture = null;

        if (LayerTile == null)
        {
            return false;
        }

        LayerTile l = LayerTile;

        for (int x_off = X; x_off < (X + l.Width); x_off++)
        {
            for (int z_off = Z; z_off < (Z + l.Height); z_off++)
            {
                // At this point, everythings fine..
                Tile t = World.current.GetTileAt(x_off, z_off);
                t.LayerTile = null;
            }
        }

        return true;
    }

    public bool UnplaceFurniture()
    {
        // Just uninstalling. FIXME: What if we have a multi tile furniture?
        //furniture = null;

        if (Furniture == null)
        {
            return false;
        }

        Furniture f = Furniture;

        for (int x_off = X; x_off < (X + f.Width); x_off++)
        {
            for (int z_off = Z; z_off < (Z + f.Height); z_off++)
            {
                // At this point, everythings fine..
                Tile t = World.current.GetTileAt(x_off, z_off);
                t.Furniture = null;
            }
        }

        return true;
    }

    public bool PlaceFurniture(Furniture objInstance) 
    {
        if (objInstance == null)
        {
            return UnplaceFurniture();
        }

        if (objInstance.IsValidPosition(this) == false)
        {
            Debug.LogError("Trying to assign a furniture to a tile that isnt valid.");
            return false;
        }

        for (int x_off = X; x_off < (X + objInstance.Width); x_off++)
        {
            for (int z_off = Z; z_off < (Z + objInstance.Height); z_off++)
            {
                Tile t = World.current.GetTileAt(x_off, z_off);

                if (objInstance.ObjectType == "Ceiling")
                {
                    t.Ceiling = objInstance;
                } 
                else if (objInstance.ObjectType == "CashRegister")
                {
                    t.TableFurniture = objInstance;
                } 
                else
                {
                    t.Furniture = objInstance;
                }
            }
        }

        return true;
    }

    public bool PlaceLayerTile(LayerTile objInstance)
    {
        if (objInstance == null)
        {
            return UnplaceLayerTile();
        }

        if (objInstance.IsValidPosition(this) == false)
        {
            Debug.LogError("Trying to assign a furniture to a tile that isnt valid.");
            return false;
        }

        for (int x_off = X; x_off < (X + objInstance.Width); x_off++)
        {
            for (int z_off = Z; z_off < (Z + objInstance.Height); z_off++)
            {
                // At this point, everythings fine..
                Tile t = World.current.GetTileAt(x_off, z_off);

                if (Furniture == null)
                {
                    t.LayerTile = objInstance;
                }
            }
        }

        return true;
    }

    public bool PlaceInventory(Inventory inv)
    {
        if (inv == null)
        {
            Inventory = null;
            return true;
        }

        if (Inventory != null)
        {
            // Theres already inventory here, maybe we can combine a stack?

            if (Inventory.objectType != inv.objectType)
            {
                Debug.LogError("Trying to assign inventory to a tile that already has some of a different type.");
                return false;
            }

            int numToMove = inv.stackSize;
            if (Inventory.stackSize + numToMove > Inventory.maxStackSize)
            {
                numToMove = Inventory.maxStackSize - Inventory.stackSize;
            }
            Inventory.stackSize += numToMove;
            inv.stackSize -= numToMove;

            return true;
        }
        // at this point, we know that our current inventor is actually null.
        // now can cant just do a direct assignment because the inventory manager needs
        // to know that the old stack is nw empty and 
        // has to be removed from the lists.

        Inventory = inv.Clone();
        Inventory.tile = this;
        inv.stackSize = 0;

        return true;
    }

    // Tells us if two tiles are adjacent
    public bool IsNeighbour(Tile tile, bool diagOkay = false)
    {
        // check to see if we have the difference of exactly one between the two
        // tile coordinates. Is so, then we are vertical or horizontal neighhours.
        return Mathf.Abs(this.X - tile.X) + Mathf.Abs(this.Z - tile.Z) == 1 ||
            (diagOkay && (Mathf.Abs(this.X - tile.X) == 1 && Mathf.Abs(this.Z - tile.Z) == 1)); // Check diag adjacency
    }

    public Tile[] GetNeighbours(bool diagOkay = false)
    {
        Tile[] ns;

        if (diagOkay == false)
        {
            ns = new Tile[4];// NESW
        } else
        {
            ns = new Tile[8]; // NESW NE NW SE SW
        }

        Tile n;

        n = World.current.GetTileAt(X, Z+1);
        ns[0] = n;
        n = World.current.GetTileAt(X+1, Z);
        ns[1] = n;
        n = World.current.GetTileAt(X, Z-1);
        ns[2] = n;
        n = World.current.GetTileAt(X-1, Z);
        ns[3] = n;

        if (diagOkay == true)
        {
            n = World.current.GetTileAt(X+1, Z+1);
            ns[4] = n;
            n = World.current.GetTileAt(X+1, Z-1);
            ns[5] = n;
            n = World.current.GetTileAt(X-1, Z-1);
            ns[6] = n;
            n = World.current.GetTileAt(X-1, Z+1);
            ns[7] = n;
        }

        return ns;
    }


    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Z", Z.ToString());
        writer.WriteAttributeString("Type", ((int)Type).ToString());
    }


    public void ReadXml(XmlReader reader)
    {
        Type = (TileType)int.Parse(reader.GetAttribute("Type"));
    }

    public ENTERABILITY IsEnterable()
    {
        // this returns true if you enter this tile right this moment.
        if (MovementCost == 0)
        {
            return ENTERABILITY.Never;
        }
        // check out firntiure to see if it has a special block on enterability
        if (Furniture != null && Furniture.IsEnterable != null)
        {
            return Furniture.IsEnterable(Furniture);
        }
        return ENTERABILITY.Yes;
    }

    public Tile North()
    {
        return World.current.GetTileAt(X, Z + 1);
    }

    public Tile South()
    {
        return World.current.GetTileAt(X, Z - 1);
    }

    public Tile West()
    {
        return World.current.GetTileAt(X - 1, Z);
    }

    public Tile East()
    {
        return World.current.GetTileAt(X + 1, Z);
    }


    #region ISelectableInterface implementation
    public string GetName()
    {
        return this._type.ToString();
    }
    public string GetDescription()
    {
        return "A tile."; // TODO: Add "Description" property and matching XML field.
    }
    public string GetHitPointString()
    {
        return ""; // TODO: Do tiles have hitpoints? How does it get destroyed? Obviously "empty" is indestructible.
    }
    public string GetCharacterType()
    {
        return null; // TODO: Does inventory have hitpoints? How does it get destroyed?
    }
    #endregion

}
