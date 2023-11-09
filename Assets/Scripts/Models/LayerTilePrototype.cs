using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LayerTilePrototype : IFurniturePrototype
{
    public string Type { get; set; }
    public int PathfindingCost { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool LinksToNeighbours { get; set; }
    public bool EnclosesRooms { get; set; }
    public bool Is3D { get; set; }
    
}
