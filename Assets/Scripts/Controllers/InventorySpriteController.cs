using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class InventorySpriteController : MonoBehaviour
{
    public GameObject inventoryUIPrefab;

    Dictionary<Inventory, GameObject> inventoryGameObjectMap;

    Dictionary<string, Sprite> inventorySprites;

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
        inv_go.transform.position = new Vector3(inv.tile.X + 0.5f, 0.4f, inv.tile.Z + 0.5f);
        inv_go.transform.localScale = new Vector3(2, 2, 2);
        inv_go.transform.SetParent(this.transform, true);

        SpriteRenderer sr = inv_go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteManager.current.GetSprite("Inventory", inv.objectType);
        sr.sortingLayerName = "Inventory";

        inv_go.AddComponent<BillboardController>();

        if (inv.maxStackSize > 1)
        {
            // This is a stackable object, so lets add a InventoryUI component
            // which is text that shows the current stack size.
            GameObject ui_go = Instantiate(inventoryUIPrefab);
            ui_go.transform.SetParent(inv_go.transform);
            ui_go.transform.localPosition = Vector3.zero;
            ui_go.GetComponentInChildren<TextMeshProUGUI>().text = inv.stackSize.ToString();
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
        inv_go.transform.position = new Vector3(inv.tile.X + 0.5f, 0.4f, inv.tile.Z + 0.5f);
        inv_go.transform.localScale = new Vector3(2, 2, 2);
        inv_go.transform.SetParent(this.transform, true);

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

// Ok I'm spawning the InventoryUIPrefab where the sprite is being spawned.
// However, I can't place tiles where the sprites are..
// Im guessing it has something to do with the raycast, maybe?
// I can normally place tiles and walls on the ground.
// if I only spawn the sprite, placing the tiles or walls works.
// Once I instantiate the InventoryUIPrefab, I then cant place tiles or walls in the vicinity
// of the text.
// The text itself is a child of a canvas object, and the canvas object is
// set to world space. The text itself is a TextMeshProGUI object.