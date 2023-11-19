using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LayerTileSpriteController : MonoBehaviour
{
    Dictionary<LayerTile, GameObject> LayerTileGameObjectMap;
    Dictionary<string, Sprite> LayerTileSprites;

    private bool isUpdatingLayerTiles = false;

    public Material TileMaterial;

    World world
    {
        get { return WorldController.Instance.world; }
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadSprites();

        // Instantiate dictionary which tracks which gameobject is rendering which tile data.
        LayerTileGameObjectMap = new Dictionary<LayerTile, GameObject>();

        // Register the creation of LayerTile event
        world.cbLayerTileCreated += OnLayerTileCreated;

        // Go through any EXISTING furniture (i.e. from a save that was loaded OnEnable) and call the OnCreated event manually
        foreach (LayerTile layerTile in world.layerTiles)
        {
            OnLayerTileCreated(layerTile);
        }
    }

    void LoadSprites()
    {
        LayerTileSprites = new Dictionary<string, Sprite>();
        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/LayerTiles/"); // Adjust the path to your sprites
        foreach (Sprite sprite in sprites)
        {
            LayerTileSprites[sprite.name] = sprite;
        }
    }

    public void OnLayerTileCreated(LayerTile layerTile)
    {
        if (layerTile == null)
        {
            Debug.LogError("Null InstalledObject passed to OnFurnitureCreated");
            return;
        }

        GameObject layerTile_go = new GameObject();

        // add our tile/GO pair to the dictionary
        LayerTileGameObjectMap.Add(layerTile, layerTile_go);

        layerTile_go.name = layerTile.Type.ToString() + "_" + layerTile.Tile.X + "_" + layerTile.Tile.Z;
        layerTile_go.transform.position = new Vector3(layerTile.Tile.X, 0, layerTile.Tile.Z);
        layerTile_go.transform.rotation = Quaternion.Euler(90, 0, 0);
        layerTile_go.transform.SetParent(this.transform, true);

        SpriteRenderer sr = layerTile_go.AddComponent<SpriteRenderer>();
        sr.sprite = GetSpriteForTile(layerTile);
        sr.sortingLayerName = "Tiles";

        sr.material = TileMaterial;

        // Register to the changing and removal of LayerTile event
        layerTile.cbOnChanged += OnLayerTileChanged;
        layerTile.cbOnRemoved += OnLayerTileRemoved;
    }

    void OnLayerTileRemoved(LayerTile layerTile_data)
    {
        if (LayerTileGameObjectMap.ContainsKey(layerTile_data) == false)
        {
            Debug.LogError("OnFurnitureChanged -- trying to change the visuals for furniture not in our map.");
            return;
        }

        GameObject layerTile_go = LayerTileGameObjectMap[layerTile_data];
        Destroy(layerTile_go);
        LayerTileGameObjectMap.Remove(layerTile_data);

        foreach (LayerTile layerTile in LayerTileGameObjectMap.Keys)
        {
            layerTile.cbOnChanged(layerTile);
        }
    }

    void OnLayerTileChanged(LayerTile layerTile)
    {
        if (LayerTileGameObjectMap.ContainsKey(layerTile) == false)
        {
            Debug.LogError("OnLayerTileChanged -- trying to change the visuals for layer tile not in our map.");
            return;
        }

        if (isUpdatingLayerTiles)
        {
            return;
        }

        isUpdatingLayerTiles = true;

        GameObject layerTile_go = LayerTileGameObjectMap[layerTile];

        layerTile_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForTile(layerTile);
        layerTile_go.GetComponent<SpriteRenderer>().material = TileMaterial;

        isUpdatingLayerTiles = false;
    }

    public Sprite GetSpriteForTile(LayerTile layerTile)
    {
        string spriteName = layerTile.Type + "_";

        // Check for neighbours North, East, South, West, Northeast, Southeast, Southwest, Northwest
        int x = layerTile.Tile.X;
        int z = layerTile.Tile.Z;
        string suffix = string.Empty;

        suffix += GetSuffixForNeighbour(layerTile, x, z + 1, "N");
        suffix += GetSuffixForNeighbour(layerTile, x + 1, z, "E");
        suffix += GetSuffixForNeighbour(layerTile, x, z - 1, "S");
        suffix += GetSuffixForNeighbour(layerTile, x - 1, z, "W");

        // Now we check if we have the neighbours in the cardinal directions next to the respective diagonals
        // because pure diagonal checking would leave us with diagonal walls and stockpiles, which make no sense.
        suffix += GetSuffixForDiagonalNeighbour(suffix, "N", "E", layerTile, x + 1, z + 1);
        suffix += GetSuffixForDiagonalNeighbour(suffix, "S", "E", layerTile, x + 1, z - 1);
        suffix += GetSuffixForDiagonalNeighbour(suffix, "S", "W", layerTile, x - 1, z - 1);
        suffix += GetSuffixForDiagonalNeighbour(suffix, "N", "W", layerTile, x - 1, z + 1);

        // For example, if this object has all eight neighbours of
        // the same type, then the string will look like:
        //       Wall_NESWneseswnw

        //Debug.Log(spriteName + suffix);
        return LayerTileSprites[spriteName + suffix];

    }

    private string GetSuffixForNeighbour(LayerTile layerTile, int x, int z, string suffix)
    {
        Tile t = World.current.GetTileAt(x, z);
        if (t != null && t.LayerTile != null && t.LayerTile.LinksToNeighbour == layerTile.LinksToNeighbour && layerTile.Type == t.LayerTile.Type)
        {
            return suffix;
        }

        return string.Empty;
    }

    private string GetSuffixForDiagonalNeighbour(string suffix, string coord1, string coord2, LayerTile layerTile, int x, int z)
    {
        if (suffix.Contains(coord1) && suffix.Contains(coord2))
        {
            return GetSuffixForNeighbour(layerTile, x, z, coord1.ToLower() + coord2.ToLower());
        }

        return string.Empty;
    }
}
