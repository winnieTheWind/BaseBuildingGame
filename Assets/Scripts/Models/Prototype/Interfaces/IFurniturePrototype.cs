
public interface IFurniturePrototype : IPrototype
{
    new string Type { get; }
    int PathfindingCost { get; }
    int Width { get; }
    int Height { get; }
    bool LinksToNeighbours { get; }
    bool EnclosesRooms { get; }
    new bool Is3D { get; }
}
