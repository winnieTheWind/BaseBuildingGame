using UnityEngine;
using System.Collections.Generic;

public class TileSpriteController : MonoBehaviour
{
    public Material TileMaterial;

    public Material WallMaterial;

    Dictionary<Tile, GameObject> tileGameObjectMap;

    public InventoryTileController inventoryTileController;

    World world
    {
        get { return WorldController.Instance.world; }
    }

    public string TileTypeFileName = "";

    void Start()
    {
        // Instantiate dictionary which tracks which gameobject is rendering which tile data.
        tileGameObjectMap = new Dictionary<Tile, GameObject>();

        // Create a gameobject for each of our tiles
        for (int x = 0; x < world.Width; x++)
        {
            for (int z = 0; z < world.Height; z++)
            {
                Tile tile_data = world.GetTileAt(x, z);

                GameObject tile_go = new GameObject();

                // Add out tile / gameobject to dictionary
                tileGameObjectMap.Add(tile_data, tile_go);

                tile_go.name = "Tile_" + x + "_" + z;
                tile_go.transform.position = new Vector3(tile_data.X, 0, tile_data.Z);
                tile_go.transform.rotation = Quaternion.Euler(90, 0, 0);
                tile_go.transform.localScale = new Vector3(2, 2, 2);
                tile_go.transform.SetParent(this.transform, true);

                // Add a sprite renderer
                SpriteRenderer sr = tile_go.AddComponent<SpriteRenderer>();
                sr.material = TileMaterial;
                sr.material.color = tile_data.TileColor;

                Sprite spr = SpriteManager.current.GetSprite("Tiles", "Empty");

                sr.sprite = spr;
                //sr.transform.localScale = new Vector3(1, 0, 1);
                sr.material.SetTexture("Empty", SpriteManager.current.GetSprite("Tiles", "Empty").texture);

                // Add a collider
                BoxCollider collider = tile_go.AddComponent<BoxCollider>();
                collider.size = new Vector3(1, 1, 1); // Set the size to match your tiles

                OnTileChanged(tile_data);
            }
        }

        world.RegisterTileChanged(OnTileChanged);
    }

    void OnTileChanged(Tile tile_data)
    {
        if (tileGameObjectMap.ContainsKey(tile_data) == false)
        {
            Debug.LogError("tileGameObjectMap doesnt contain the tile_data");
            return;
        }

        GameObject tile_go = tileGameObjectMap[tile_data];

        if (tile_go == null)
        {
            Debug.LogError("tileGameObjectMap returned game object is null");
            return;
        }

        LoadSprite(tile_go, tile_data, TileType.Empty);
        LoadSprite(tile_go, tile_data, TileType.Wood_Panel);
        LoadSprite(tile_go, tile_data, TileType.Stone_Panel);
        LoadSprite(tile_go, tile_data, TileType.Grass);
        LoadSprite(tile_go, tile_data, TileType.Concrete_Slab);
        LoadSprite(tile_go, tile_data, TileType.Concrete_Slab2);
        LoadSprite(tile_go, tile_data, TileType.Clean_Concrete_Slab);
        LoadSprite(tile_go, tile_data, TileType.Cracked_Slab);
        LoadSprite(tile_go, tile_data, TileType.Road1);
        LoadSprite(tile_go, tile_data, TileType.Road2);
        LoadSprite(tile_go, tile_data, TileType.Road3);
        LoadSprite(tile_go, tile_data, TileType.Road4);
        LoadSprite(tile_go, tile_data, TileType.Road5);








    }

    private void LoadSprite(GameObject tile_go, Tile tile_data, TileType type)
    {
        if (tile_data.Type == type)
        {
            tile_go.GetComponent<SpriteRenderer>().material = TileMaterial;
            tile_go.GetComponent<SpriteRenderer>().sprite = SpriteManager.current.GetSprite("Tiles", type.ToString());
            tile_go.transform.localScale = new Vector3(2, 2, 2);
            SpriteRenderer sr = tile_go.GetComponent<SpriteRenderer>();
            sr.material.color = tile_data.TileColor;
        }

    }
}

