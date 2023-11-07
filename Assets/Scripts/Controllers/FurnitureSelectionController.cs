
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FurnitureSelectionController : MonoBehaviour
{
    public List<FurnitureObject> tileObjects;
    public GameObject content;
    public GameObject prefab;

    public BuildModeController bmc;

    public static Action<List<FurnitureObject>> sendFurnitureObjects;

    private void Start()
    {

        foreach (FurnitureObject furnInv in tileObjects)
        {
            GameObject furnInv_go = Instantiate(prefab);
            furnInv_go.transform.SetParent(content.transform);
            furnInv_go.GetComponentInChildren<TextMeshProUGUI>().text = furnInv.Name;
        }

        sendFurnitureObjects?.Invoke(tileObjects);
    }
}
