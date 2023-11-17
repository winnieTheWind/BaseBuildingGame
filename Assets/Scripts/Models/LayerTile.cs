using MoonSharp.Interpreter;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using System;

[MoonSharpUserData]
public class LayerTile : IXmlSerializable, ISelectableInterface
{
    protected Dictionary<string, float> LayerTileParameters;

    // This represents the BASE tile of the object -- but in practice, large objects may actually occupy
    // multile tiles.
    public Tile Tile
    {
        get; protected set;
    }

    // This "objectType" will be queried by the visual system to know what sprite to render for this object
    public string Type
    {
        get; protected set;
    }

    public bool LinksToNeighbour
    {
        get; protected set;
    }

    public int Width { get; protected set; }
    public int Height { get; protected set; }

    public Action<LayerTile> cbOnChanged;
    public Action<LayerTile> cbOnRemoved;

    Func<Tile, bool> funcPositionValidation;

    public LayerTile()
    {
        LayerTileParameters = new Dictionary<string, float>();
    }

    protected LayerTile(LayerTile other)
    {
        this.Type = other.Type;
        this.LinksToNeighbour = other.LinksToNeighbour;
        this.Width = other.Width;
        this.Height = other.Height;

        this.LayerTileParameters = new Dictionary<string, float>(other.LayerTileParameters);

        if (other.funcPositionValidation != null)
            this.funcPositionValidation = (Func<Tile, bool>)other.funcPositionValidation.Clone();
    }

    virtual public LayerTile Clone()
    {
        return new LayerTile(this);
    }

    public LayerTile(int width, int height, string type, bool linksToNeighbour = false)
    {
        LayerTile obj = new LayerTile();

        this.Type = type;
        this.LinksToNeighbour = linksToNeighbour;
        this.Width = width;
        this.Height = height;

        // Assign the method directly to the delegate
        this.funcPositionValidation = this.DEFAULT__IsValidPosition;

        LayerTileParameters = new Dictionary<string, float>();

    }

    static public LayerTile PlaceInstance(LayerTile proto, Tile tile)
    {
        LayerTile obj = proto.Clone();
        obj.Tile = tile;

        // FIXME: This assumes we are 1x1!
        if (tile.PlaceLayerTile(obj) == false)
        {
            return null;
        }

        if (obj.LinksToNeighbour)
        {
            Tile t;
            int x = tile.X;
            int z = tile.Z;

            t = World.current.GetTileAt(x, z + 1); // Northern neighbour
            if (t != null && t.LayerTile != null && t.LayerTile.cbOnChanged != null && t.LayerTile.Type == obj.Type)
            {
                t.LayerTile.cbOnChanged(t.LayerTile);
            }
            t = World.current.GetTileAt(x - 1, z); // Western neighbour
            if (t != null && t.LayerTile != null && t.LayerTile.cbOnChanged != null && t.LayerTile.Type == obj.Type)
            {
                t.LayerTile.cbOnChanged(t.LayerTile);
            }
            t = World.current.GetTileAt(x + 1, z); // Eastern neighbour
            if (t != null && t.LayerTile != null && t.LayerTile.cbOnChanged != null && t.LayerTile.Type == obj.Type)
            {
                t.LayerTile.cbOnChanged(t.LayerTile);
            }
            t = World.current.GetTileAt(x, z - 1); // Southern neighbour

            if (t != null && t.LayerTile != null && t.LayerTile.cbOnChanged != null && t.LayerTile.Type == obj.Type)
            {
                t.LayerTile.cbOnChanged(t.LayerTile);
            }
            t = World.current.GetTileAt(x + 1, z + 1); // NE neighbour
            if (t != null && t.LayerTile != null && t.LayerTile.cbOnChanged != null && t.LayerTile.Type == obj.Type)
            {
                t.LayerTile.cbOnChanged(t.LayerTile);
            }
            t = World.current.GetTileAt(x - 1, z + 1); // NW neighbour
            if (t != null && t.LayerTile != null && t.LayerTile.cbOnChanged != null && t.LayerTile.Type == obj.Type)
            {
                t.LayerTile.cbOnChanged(t.LayerTile);
            }
            t = World.current.GetTileAt(x + 1, z - 1); // SE neighbour
            if (t != null && t.LayerTile != null && t.LayerTile.cbOnChanged != null && t.LayerTile.Type == obj.Type)
            {
                t.LayerTile.cbOnChanged(t.LayerTile);
            }
            t = World.current.GetTileAt(x - 1, z - 1); // SW neighbour
            if (t != null && t.LayerTile != null && t.LayerTile.cbOnChanged != null && t.LayerTile.Type == obj.Type)
            {
                t.LayerTile.cbOnChanged(t.LayerTile);
            }
        }
        return obj;
    }

    public void Deconstruct()
    {
        Tile.UnplaceLayerTile();

        if (cbOnRemoved != null)
        {
            cbOnRemoved(this);
        }
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
        for (int x_off = t.X; x_off < (t.X + Width); x_off++)
        {
            for (int z_off = t.Z; z_off < (t.Z + Height); z_off++)
            {
                Tile t2 = World.current.GetTileAt(x_off, z_off);

                    // For other furniture, the normal rules apply.
                    if (t2.LayerTile != null)
                    {
                        return false;
                    }
            }
        }

        return true; // Or whatever logic you need here
    }

    public void RegisterOnChangedCallback(Action<LayerTile> callbackFunc)
    {
        cbOnChanged += callbackFunc;
    }

    public void UnregisterOnChangedCallback(Action<LayerTile> callbackFunc)
    {
        cbOnChanged -= callbackFunc;
    }

    public void RegisterOnRemovedCallback(Action<LayerTile> callbackFunc)
    {
        cbOnRemoved += callbackFunc;
    }

    public void UnregisterOnRemovedCallback(Action<LayerTile> callbackFunc)
    {
        cbOnRemoved -= callbackFunc;
    }

    public XmlSchema GetSchema()
    {
        return null;
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttributeString("X", Tile.X.ToString());
        writer.WriteAttributeString("Z", Tile.Z.ToString());
        writer.WriteAttributeString("type", Type.ToString());

        foreach (string k in LayerTileParameters.Keys)
        {
            writer.WriteStartElement("Param");
            writer.WriteAttributeString("name", k);
            writer.WriteAttributeString("value", LayerTileParameters[k].ToString());
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
                LayerTileParameters[k] = v;
            } while (reader.ReadToNextSibling("Param"));
        }
    }

    #region ISelectableInterface implementation
    public string GetName()
    {
        return this.Type;
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
