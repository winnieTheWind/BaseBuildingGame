using UnityEngine;

public abstract class TileObject : ScriptableObject
{
    public string Name;
    public string FileName;
    [TextArea(15, 20)]
    public string Description;
    public TileType Type;
}
