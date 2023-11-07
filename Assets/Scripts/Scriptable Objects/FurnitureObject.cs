using UnityEngine;

public abstract class FurnitureObject : ScriptableObject
{
    public string Name;
    public string FileName;
    [TextArea(15, 20)]
    public string Description;
}
