public interface IFurniturePrototype
{
    string Type { get; }
    int PathfindingCost { get; }
    int Width { get; }
    int Height { get; }
    bool LinksToNeighbours { get; }
    bool EnclosesRooms { get; }
    bool Is3D { get; }
}
