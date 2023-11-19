
using System;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Serialization;
using MoonSharp.Interpreter;

// Inventory are things that are lying on the floor/stockpile, like a bunch of metal bars
// or potentially a non-installed copy of furniture (e.g. a cabinet still in the box from Ikea)

[MoonSharpUserData]
public class Inventory : IXmlSerializable, ISelectableInterface
{
    public string objectType = "Steel_Plate";
    public int maxStackSize = 50;

    protected int _stackSize = 1;
    public int stackSize
    {
        get { return _stackSize; }
        set
        {
            if (_stackSize != value)
            {
                _stackSize = value;

                cbInventoryChanged?.Invoke(this);
            }
        }
    }

    public Action<Inventory> cbInventoryChanged;

    public Tile tile;
    public Character character;

    public Inventory()
    {

    }

    public Inventory(string objectType, int maxStackSize, int stackSize)
    {
        this.objectType = objectType;
        this.maxStackSize = maxStackSize;
        this.stackSize = stackSize;
    }

    protected Inventory(Inventory other)
    {
        objectType = other.objectType;
        maxStackSize = other.maxStackSize;
        stackSize = other.stackSize;
    }

    public virtual Inventory Clone()
    {
        return new Inventory(this);
    }

    public void RegisterChangedCallback(Action<Inventory> callback)
    {
        cbInventoryChanged += callback;
    }

    public void UnregisterChangedCallback(Action<Inventory> callback)
    {
        cbInventoryChanged -= callback;
    }

    // IXmlSerializable implementation
    public XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(XmlReader reader)
    {
        // Deserialize the Inventory from XML.
        objectType = reader.GetAttribute("objectType");
        maxStackSize = int.Parse(reader.GetAttribute("maxStackSize"));

        // You might want to use int.TryParse for better error handling
        int stackSizeValue;
        if (int.TryParse(reader.GetAttribute("stackSize"), out stackSizeValue))
        {
            stackSize = stackSizeValue;
        }

        // Read any additional details here...
    }

    public void WriteXml(XmlWriter writer)
    {
        // Serialize the Inventory to XML.
        writer.WriteAttributeString("objectType", objectType);
        writer.WriteAttributeString("maxStackSize", maxStackSize.ToString());
        writer.WriteAttributeString("stackSize", stackSize.ToString());

        // Write any additional details here...
    }

    #region ISelectableInterface implementation
    public string GetName()
    {
        return this.objectType;
    }
    public string GetDescription()
    {
        return "A stack of inventory."; // TODO: Add "Description" property and matching XML field.
    }
    public string GetHitPointString()
    {
        return ""; // TODO: Does inventory have hitpoints? How does it get destroyed?
    }
    public string GetCharacterType()
    {
        return null; // TODO: Does inventory have hitpoints? How does it get destroyed?
    }
    #endregion

}
