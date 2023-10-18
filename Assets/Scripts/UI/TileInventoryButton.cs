using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TileInventoryButton : MonoBehaviour
{
    public TileObject tile;

    List<TileObject> tileObjects;

    private BuildModeController bmc;

    private void OnEnable()
    {
        InventoryTileController.sendTileObjects += RegisterSendTileObjects;
        BuildModeController.sendBuildModeController += RegisterSendBuildModeController;
    }

    private void OnDisable()
    {
        InventoryTileController.sendTileObjects -= UnregisterSendTileObjects;
        BuildModeController.sendBuildModeController -= UnregisterSendBuildModeController;
    }

    void RegisterSendTileObjects(List<TileObject> _tileObjs)
    {
        tileObjects = new List<TileObject>();
        tileObjects = _tileObjs;
    }

    void UnregisterSendTileObjects(List<TileObject> _tileObjs)
    {
        tileObjects.Clear();
    }

    void RegisterSendBuildModeController(BuildModeController _bmc)
    {
        bmc = _bmc;
    }

    void UnregisterSendBuildModeController(BuildModeController _bmc)
    {
        bmc = null;
    }

    public void Clicked()
    {
        string s = transform.GetComponentInChildren<TextMeshProUGUI>().text;

        for (int i = 0; i < tileObjects.Count; i++)
        {
            if (tileObjects[i].Name == s)
            {
                bmc.SetMode_BuildFloor(tileObjects[i].Type);
            }
        }
    }

}
