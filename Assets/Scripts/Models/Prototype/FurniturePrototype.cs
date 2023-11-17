using System;
using UnityEngine;

[Serializable]
public class FurniturePrototype : IFurniturePrototype
{
    [SerializeField]
    private string type; // Unity will serialize this field
    [SerializeField]
    private int pathfindingCost;
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    [SerializeField]
    private bool linksToNeighbours;
    [SerializeField]
    private bool enclosesRooms;
    [SerializeField]
    private bool is3D;
    [SerializeField]
    private float floorHeight;

    // These properties exposes the serialized field.
    public string Type { get { return type; } }
    public int PathfindingCost { get { return pathfindingCost; } }
    public int Width { get { return width; } } // This property exposes the serialized field.
    public int Height { get { return height; } } // This property exposes the serialized field.
    public bool LinksToNeighbours { get { return linksToNeighbours; } } // This property exposes the serialized field.
    public bool EnclosesRooms { get { return enclosesRooms; } } // This property exposes the serialized field.
    public bool Is3D { get { return is3D; } }
    public float FloorHeight { get { return floorHeight; } }

}
