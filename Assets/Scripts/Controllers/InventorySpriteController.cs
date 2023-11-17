using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InventorySpriteController : MonoBehaviour
{
    public GameObject inventoryUIPrefab;

    Dictionary<Inventory, GameObject> inventoryGameObjectMap;

    Dictionary<string, Sprite> inventorySprites;

    public Material InventoryMaterial;

    World world
    {
        get { return WorldController.Instance.world; }
    }

    // Use this for initialization
    void Start()
    {
        LoadSprites();

        // Instantiate our dictionary that tracks which GameObject is rendering which Tile data.
        inventoryGameObjectMap = new Dictionary<Inventory, GameObject>();

        // Register our callback so that our GameObject gets updated whenever
        // the tile's type changes.
        world.RegisterInventoryCreated(OnInventoryCreated);

        // Check for pre-existing inventory, which won't do the callback.
        foreach (string objectType in world.inventoryManager.inventories.Keys)
        {
            foreach (Inventory inv in world.inventoryManager.inventories[objectType])
            {
                OnInventoryCreated(inv);
            }
        }


        //c.SetDestination( world.GetTileAt( world.Width/2 + 5, world.Height/2 ) );
    }

    void LoadSprites()
    {
        inventorySprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Inventory/");

        //Debug.Log("LOADED RESOURCE:");
        foreach (Sprite s in sprites)
        {
            //Debug.Log(s);
            inventorySprites[s.name] = s;
        }
    }

    public void OnInventoryCreated(Inventory inv)
    {
        //Debug.Log("OnInventoryCreated");
        // Create a visual GameObject linked to this data.

        // FIXME: Does not consider multi-tile objects nor rotated objects

        // This creates a new GameObject and adds it to our scene.
        GameObject inv_go = new GameObject();

        // Add our tile/GO pair to the dictionary.
        inventoryGameObjectMap.Add(inv, inv_go);

        inv_go.name = inv.objectType;
        inv_go.transform.position = new Vector3(inv.tile.X, 0.4f, inv.tile.Z);
        inv_go.transform.SetParent(this.transform, true);

        SpriteRenderer sr = inv_go.AddComponent<SpriteRenderer>();
        sr.sprite = inventorySprites[inv.objectType];

        // Inventory Sprites are set on the same layer as the Character Sprites,
        // so that the depth of the sprites are handled correctly..
        sr.sortingLayerName = "Characters";

        // Character and Inventory Material uses a shader..
        // Im unsure if the shader is actually doing the work..
        sr.material = InventoryMaterial;

        inv_go.layer = 8;

        inv_go.AddComponent<BillboardController>();

        if (inv.maxStackSize > 1)
        {
            // This is a stackable object, so lets add a InventoryUI component
            // which is text that shows the current stack size.
            GameObject ui_go = Instantiate(inventoryUIPrefab);
            // Inventory text are set on the same layer as the Character Sprites,
            // so that the depth of the sprites are handled correctly..
            ui_go.GetComponent<Canvas>().sortingLayerName = "Characters";
            ui_go.transform.SetParent(inv_go.transform);
            ui_go.transform.localPosition = Vector3.zero;

            ui_go.GetComponentInChildren<TextMeshProUGUI>().text = inv.stackSize.ToString();
            ui_go.layer = 7;
        }

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        // FIXME: Add on changed callbacks
        inv.RegisterChangedCallback( OnInventoryChanged );

    }

    void OnInventoryChanged(Inventory inv)
    {
        // FIXME:  Still needs to work!  And get called!

        //Debug.Log("OnFurnitureChanged");
        // Make sure the furniture's graphics are correct.

        if (inventoryGameObjectMap.ContainsKey(inv) == false)
        {
            Debug.LogError("OnCharacterChanged -- trying to change visuals for inventory not in our map.");
            return;
        }

        GameObject inv_go = inventoryGameObjectMap[inv];
        inv_go.name = inv.objectType;
        inv_go.transform.position = new Vector3(inv.tile.X, 0.4f, inv.tile.Z);
        inv_go.transform.SetParent(this.transform, true);
        inv_go.layer = 8;


        if (inv.stackSize > 0)
        {
            TextMeshProUGUI text = inv_go.GetComponentInChildren<TextMeshProUGUI>();
            // FIXME: if maxStackSize changed to/from 1, then we either need to create or destroy the text.
            if (text != null)
            {
                text.text = inv.stackSize.ToString();
            }
        } else
        {
            // this stack has gone to zero, so remove the sprite.
            Destroy(inv_go);
            inventoryGameObjectMap.Remove(inv);
            inv.UnregisterChangedCallback( OnInventoryChanged );
        }
     

    }



}
