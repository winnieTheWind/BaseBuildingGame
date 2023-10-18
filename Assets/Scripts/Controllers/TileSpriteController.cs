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
                tile_go.transform.rotation = Quaternion.Euler(90, 0, 0);
                tile_go.transform.localScale = new Vector3(2, 2, 2);
                tile_go.transform.SetParent(this.transform, true);

                Vector3 tilePosition = new Vector3(tile_data.X + 0.5f, 0, tile_data.Z + 0.5f); // Adjusting for the tile's center
                tile_go.transform.position = tilePosition;


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
                tile_data.RegisterTileChanged(OnTileChanged);
            }
        }

        world.RegisterTileChanged(OnTileChanged);
    }

public void OnTileChanged(Tile tile_data)
    {
        if (!tileGameObjectMap.TryGetValue(tile_data, out var tile_go))
        {
            Debug.LogError("OnTileChanged - Invalid tile data!");
            return;
        }

        if (tile_go == null)
        {
            Debug.LogError("OnTileChanged - Missing game object!");
            return;
        }

        // Set the appropriate sprite based on the tile type.
        tile_go.GetComponent<SpriteRenderer>().sprite = SpriteManager.current.GetSprite("Tiles", tile_data.Type.ToString());
        tile_go.GetComponent<SpriteRenderer>().material.color = tile_data.TileColor;
    }

}

