using UnityEngine;

[CreateAssetMenu(fileName = "New Default Object", menuName = "Tile UI System/Items/Default")]
public class DefaultObject : TileObject
{
    public void Awake()
    {
        name = "Default";
    }
}
