using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialManager : MonoBehaviour
{
    public static MaterialManager Instance { get; protected set; }

    Dictionary<string, Material> Materials;

    private void OnEnable()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be two world controllers.");
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadMaterials();
    }

    void LoadMaterials()
    {
        Materials = new Dictionary<string, Material>();
        Material[] materials = Resources.LoadAll<Material>("Materials"); // Adjust the path to your sprites
        foreach (Material material in materials)
        {
            Materials[material.name] = material;
        }
    }

    public Material GetMaterial(string materialName)
    {
        return Materials[materialName];
    }
}
