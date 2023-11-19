using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.U2D;

public class TileSpriteController : MonoBehaviour
{
    public Material TileMaterial;
    public Material WallMaterial;
    public GameObject BitmaskText;

    Dictionary<Tile, GameObject> tileGameObjectMap;
    Dictionary<string, Sprite> tileSprites;

    World world
    {
        get { return WorldController.Instance.world; }
    }

    void LoadSprites()
    {
        tileSprites = new Dictionary<string, Sprite>();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Tiles/"); // Adjust the path to your sprites
        foreach (Sprite sprite in sprites)
        {
            tileSprites[sprite.name] = sprite;
        }
    }

    void Start()
    {
        LoadSprites();

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
                tile_go.transform.SetParent(this.transform, true);

                // Add a sprite renderer
                SpriteRenderer sr = tile_go.AddComponent<SpriteRenderer>();
                sr.material = TileMaterial;
                sr.sprite = tileSprites["Grass"];
                sr.material.SetTexture("Grass", tileSprites["Grass"].texture);

                sr.sortingLayerName = "Tiles";

                // Add a collider
                BoxCollider collider = tile_go.AddComponent<BoxCollider>();
                collider.size = new Vector3(1, 1, 1); // Set the size to match your tiles

                OnTileChanged(tile_data);
            }
        }

        // Register to the changing of tile event
        world.cbTileChanged += OnTileChanged;
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
        tile_go.GetComponent<SpriteRenderer>().sprite = tileSprites[tile_data.Type.ToString()];
    }
}


