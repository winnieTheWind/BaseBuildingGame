using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FurnitureButton : MonoBehaviour
{
    public FurnitureObject furn;

    List<FurnitureObject> furnitureObjects;

    private BuildModeController bmc;

    private void Start()
    {
        bmc = GameObject.FindObjectOfType<BuildModeController>();
    }

    private void OnEnable()
    {
        FurnitureSelectionController.sendFurnitureObjects += RegisterSendFurnitureObjects;
    }

    private void OnDisable()
    {
        FurnitureSelectionController.sendFurnitureObjects -= RegisterSendFurnitureObjects;
    }

    void RegisterSendFurnitureObjects(List<FurnitureObject> _furnitureObjects)
    {
        furnitureObjects = new List<FurnitureObject>();
        furnitureObjects = _furnitureObjects;
    }

    public void Clicked()
    {
        string s = transform.GetComponentInChildren<TextMeshProUGUI>().text;
        for (int i = 0; i < furnitureObjects.Count; i++)
        {
            if (furnitureObjects[i].Name == s)
            {
                bmc.SetMode_BuildFurniture(furnitureObjects[i].Name);
            }
        }
    }

}
