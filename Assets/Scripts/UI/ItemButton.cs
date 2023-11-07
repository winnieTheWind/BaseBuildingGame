using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemButton : MonoBehaviour
{
    public TileObject tile;

    List<TileObject> tileObjects;

    private BuildModeController bmc;

    private void Start()
    {
        bmc = GameObject.FindObjectOfType<BuildModeController>();
    }

    private void OnEnable()
    {
        TileSelectionController.sendTileObjects += RegisterSendTileObjects;
    }

    private void OnDisable()
    {
        TileSelectionController.sendTileObjects -= UnregisterSendTileObjects;
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

    public void Clicked()
    {
        string s = transform.GetComponentInChildren<TextMeshProUGUI>().text;

        for (int i = 0; i < tileObjects.Count; i++)
        {
            if (tileObjects[i].Name == s)
            {
                string name = tileObjects[i].Name;
                if (name == "Dirt" || name == "Stockpile")
                {
                    bmc.SetMode_BuildLayerFloor(tileObjects[i].Type);
                }
                else
                {
                    bmc.SetMode_BuildFloor(tileObjects[i].Type);
                }
                //For a piece of code, how can I make it handle more than one type, I want to also
                // check if its "Dirt" or "Stockpile"
            }
        }
    }

}
