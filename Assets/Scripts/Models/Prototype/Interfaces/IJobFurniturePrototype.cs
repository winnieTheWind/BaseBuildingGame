
public interface IJobFurniturePrototype : IPrototype
{
    int JobTime { get; }
    string MaterialForBuild { get; }
    bool JobRepeats { get; }

}
