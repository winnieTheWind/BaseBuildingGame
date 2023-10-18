using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryTileController : MonoBehaviour
{
    public List<TileObject> tileObjects;
    public GameObject content;
    public GameObject prefab;

    public BuildModeController bmc;

    public string fileName = "";

    public static Action<List<TileObject>> sendTileObjects;

    private void Start()
    {
        foreach (TileObject tileObj in tileObjects)
        {
            fileName = tileObj.FileName;

            GameObject tileInv_go = Instantiate(prefab);
            tileInv_go.transform.SetParent(content.transform);
            tileInv_go.GetComponentInChildren<TextMeshProUGUI>().text = tileObj.name;
            //tileInv_go.findGetComponentInChildren<Image>().sprite = SpriteManager.current.GetSprite("Tiles", tileObj.FileName);
            tileInv_go.transform.Find("ImagePlaceholder").GetComponent<Image>().sprite = SpriteManager.current.GetSprite("Tiles", tileObj.FileName); ;
        }

        sendTileObjects?.Invoke(tileObjects);
    }
}
