using System;
using UnityEngine;

[Serializable]
public class CharacterPrototype : ICharacterPrototype
{
    [SerializeField]
    private string type; // Unity will serialize this field
    [SerializeField]
    private int width;
    [SerializeField]
    private int height;
    [SerializeField]
    private bool is3D;

    // These properties exposes the serialized field.
    public string Type { get { return type; } }
    public int Width { get { return width; } } // This property exposes the serialized field.
    public int Height { get { return height; } } // This property exposes the serialized field.
    public bool Is3D { get { return is3D; } }
}
