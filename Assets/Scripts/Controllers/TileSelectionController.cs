
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TileSelectionController : MonoBehaviour
{
    public List<TileObject> tileObjects;
    public GameObject content;
    public GameObject prefab;

    public BuildModeController bmc;

    public static Action<List<TileObject>> sendTileObjects;

    Dictionary<string, Sprite> tileSprites;

    void LoadSprites()
    {
        tileSprites = new Dictionary<string, Sprite>();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Tiles/"); // Adjust the path to your sprites
        foreach (Sprite sprite in sprites)
        {
            tileSprites[sprite.name] = sprite;
        }
    }

    private void Start()
    {
        LoadSprites();

        foreach (TileObject tileObj in tileObjects)
        {
            GameObject tileInv_go = Instantiate(prefab);
            tileInv_go.transform.SetParent(content.transform);
            tileInv_go.GetComponentInChildren<TextMeshProUGUI>().text = tileObj.name;
            //tileInv_go.findGetComponentInChildren<Image>().sprite = SpriteManager.current.GetSprite("Tiles", tileObj.FileName);
            tileInv_go.transform.Find("Image").GetComponent<Image>().sprite = tileSprites[tileObj.FileName];
        }

        sendTileObjects?.Invoke(tileObjects);
    }
}
