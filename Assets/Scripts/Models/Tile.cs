using UnityEngine;
using System;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Xml;
using MoonSharp.Interpreter;
using System.Collections.Generic;

public enum TileType { Empty, Grass, Concrete_Slab, Stone_Panel, Wood_Panel, 
    Clean_Concrete_Slab, Concrete_Slab2, Cracked_Slab, Road1, Road2, Road3, Road4, 
    Road5};

public enum ENTERABILITY { Yes, Never, Soon };

[MoonSharpUserData]
public class Tile : IXmlSerializable, ISelectableInterface
{
    TileType _type = TileType.Grass;

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

    public Room room;

    public List<Character> characters;

    public Inventory inventory;
    public Furniture furniture
    {
        get; protected set;
    }

    public Job pendingFurnitureJob;

    public int X { get; protected set; }
    public int Z { get; protected set; }

    public float movementCost
    {
        get
        {
            if (Type == TileType.Empty)
                return 0;   // 0 is unwalkable

            if (furniture == null)
                return 1;

            return 1 * furniture.movementCost;
        }
    }

    Action<Tile> cbTileChanged;

    public Color TileColor { get; set; }

    public Chunk chunk { get; set; }

    public Tile(int x, int z, Color color) 
    {
        this.X = x;
        this.Z = z;
        this.TileColor = color;
        characters = new List<Character>();
    }

    // You can also have a method to change the color of the tile.
    public void ChangeColor(Color newColor)
    {
        TileColor = newColor;
    }

    public void RegisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged += callback;
    }

    public void UnregisterTileTypeChangedCallback(Action<Tile> callback)
    {
        cbTileChanged -= callback;
    }

    public bool UnplaceFurniture()
    {
        // Just uninstalling. FIXME: What if we have a multi tile furniture?
        //furniture = null;

        if (furniture == null)
        {
            return false;
        }

        Furniture f = furniture;

        for (int x_off = X; x_off < (X + f.Width); x_off++)
        {
            for (int z_off = Z; z_off < (Z + f.Height); z_off++)
            {
                // At this point, everythings fine..
                Tile t = World.current.GetTileAt(x_off, z_off);
                t.furniture = null;
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
                // At this point, everythings fine..
                Tile t = World.current.GetTileAt(x_off, z_off);
                t.furniture = objInstance;
            }
        }

        return true;
    }

    public bool PlaceInventory(Inventory inv)
    {
        if (inv == null)
        {
            inventory = null;
            return true;
        }

        if (inventory != null)
        {
            // Theres already inventory here, maybe we can combine a stack?

            if (inventory.objectType != inv.objectType)
            {
                Debug.LogError("Trying to assign inventory to a tile that already has some of a different type.");
                return false;
            }

            int numToMove = inv.stackSize;
            if (inventory.stackSize + numToMove > inventory.maxStackSize)
            {
                numToMove = inventory.maxStackSize - inventory.stackSize;
            }
            inventory.stackSize += numToMove;
            inv.stackSize -= numToMove;

            return true;
        }
        // at this point, we know that our current inventor is actually null.
        // now can cant just do a direct assignment because the inventory manager needs
        // to know that the old stack is nw empty and 
        // has to be removed from the lists.

        inventory = inv.Clone();
        inventory.tile = this;
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

    public ENTERABILITY IsEnterable()
    {
        // This returns true if you can enter this tile right this moment.
        if (movementCost == 0)
            return ENTERABILITY.Never;

        // Check out furniture to see if it has a special block on enterability
        if (furniture != null)
        {
            return furniture.IsEnterable();
        }

        return ENTERABILITY.Yes;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", X.ToString());
        writer.WriteAttributeString("Z", Z.ToString());
        writer.WriteAttributeString("RoomID", room == null ? "-1" : room.ID.ToString());
        writer.WriteAttributeString("Type", ((int)Type).ToString());
    }


    public void ReadXml(XmlReader reader)
    {
        // X and Z have already been read/processed
        room = World.current.GetRoomFromID(int.Parse(reader.GetAttribute("RoomID")));

        Type = (TileType)int.Parse(reader.GetAttribute("Type"));
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
        return World.current.GetTileAt(X, Z - 1);
    }

    public Tile East()
    {
        return World.current.GetTileAt(X, Z + 1);
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
