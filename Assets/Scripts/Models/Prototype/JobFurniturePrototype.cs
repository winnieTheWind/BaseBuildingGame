using System;
using System.Runtime.CompilerServices;
using UnityEngine;

[Serializable]
public class JobFurniturePrototype : IJobFurniturePrototype
{
    [SerializeField]
    private string type; // Unity will serialize this field
    [SerializeField]
    private int jobTime;
    [SerializeField]
    private string materialForBuild;
    [SerializeField]
    private bool is3D;
    [SerializeField]
    private bool jobRepeats;

    // These properties exposes the serialized field.
    public string Type { get { return type; } }
    public int JobTime { get { return jobTime; } }
    public string MaterialForBuild { get { return materialForBuild; } }
    public bool Is3D { get { return is3D; } }
    public bool JobRepeats { get { return jobRepeats; } }



}
