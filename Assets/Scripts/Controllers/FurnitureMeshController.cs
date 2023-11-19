using UnityEngine;
using System.Collections.Generic;

public class FurnitureMeshController : MonoBehaviour
{
    public Material WallMaterial;
    public Material OxygenGeneratorMaterial;

    Dictionary<Furniture, GameObject> furnitureGameObjectMap;
    Dictionary<string, GameObject> furnitureMeshes;
    Dictionary<string, Sprite> furnitureSprites;

    private bool isUpdatingFurniture = false;

    World world
    {
        get { return WorldController.Instance.world; }
    }

    void Start()
    {
        LoadMeshes();

        // Instantiate dictionary which tracks which gameobject is rendering which tile data.
        furnitureGameObjectMap = new Dictionary<Furniture, GameObject>();

        // Register the creation of furniture event
        world.cbFurnitureCreated += OnFurnitureCreated;

        // Go through any EXISTING furniture (i.e. from a save that was loaded OnEnable) and call the OnCreated event manually
        foreach (Furniture furn in world.furnitures)
        {
            OnFurnitureCreated(furn);
        }
    }

    void LoadMeshes()
    {
        furnitureMeshes = new Dictionary<string, GameObject>();
        furnitureSprites = new Dictionary<string, Sprite>();

        Sprite[] sprites = Resources.LoadAll<Sprite>("Sprites/Furniture/"); // Adjust the path to your sprites
        foreach (Sprite sprite in sprites)
        {
            furnitureSprites[sprite.name] = sprite;
        }

        GameObject[] furniture = Resources.LoadAll<GameObject>("Models/Furniture/");
        foreach (GameObject g in furniture)
        {
            furnitureMeshes[g.name] = g;
        }
    }

    public void OnFurnitureCreated(Furniture furn)
    {
        if (furn == null)
        {
            Debug.LogError("Null InstalledObject passed to OnFurnitureCreated");
            return;
        }

        GameObject furn_go;

        // Create a Visual GameObject linked to this data.
        if (furn.Is3D)
        {
            furn_go = Instantiate(GetGameObjectForFurniture(furn), Vector3.zero, Quaternion.identity);

            // add our tile/GO pair to the dictionary
            furnitureGameObjectMap.Add(furn, furn_go);

            furn_go.name = furn.ObjectType.ToString() + "_" + furn.Tile.X + "_" + furn.Tile.Z;
            furn_go.transform.position = new Vector3(furn.Tile.X + ((furn.Width-1)/2f), furn.FloorHeight, furn.Tile.Z + ((furn.Height - 1) / 2f));
            furn_go.transform.SetParent(this.transform, true);

            // Get MeshRenderer component to the GameObject
            MeshRenderer meshRenderer = furn_go.GetComponent<MeshRenderer>();
            meshRenderer.sortingLayerName = "Furniture";

            // Optionally, you might want to set the material of the MeshRenderer
            meshRenderer.material = MaterialManager.Instance.GetMaterial(furn.ObjectType + "Material");

        } else
        {
            furn_go = new GameObject();
            // add our tile/GO pair to the dictionary
            furnitureGameObjectMap.Add(furn, furn_go);

            furn_go.name = furn.ObjectType.ToString() + "_" + furn.Tile.X + "_" + furn.Tile.Z;
            furn_go.transform.position = new Vector3(furn.Tile.X, 0, furn.Tile.Z);
            furn_go.transform.rotation = Quaternion.Euler(90, 0, 0);
            furn_go.transform.SetParent(this.transform, true);
            //furn_go.GetComponentInChildren<SpriteRenderer>().sortingLayerName = "Furniture";

            SpriteRenderer sr = furn_go.AddComponent<SpriteRenderer>();
            sr.sprite = GetSpriteForFurniture(furn);
            sr.sortingLayerName = "FurnitureTiles";
            sr.color = furn.Tint;

        }

        // Register the changing and removal of furniture event
        furn.cbOnChanged += OnFurnitureChanged;
        furn.cbOnRemoved += OnFurnitureRemoved;

    }

    void OnFurnitureRemoved(Furniture furn)
    {
        if (furnitureGameObjectMap.ContainsKey(furn) == false)
        {
            Debug.LogError("OnFurnitureChanged -- trying to change the visuals for furniture not in our map.");
            return;
        }

        GameObject furn_go = furnitureGameObjectMap[furn];
        Destroy(furn_go);
        furnitureGameObjectMap.Remove(furn);
    }

    void OnFurnitureChanged(Furniture furn)
    {
        if (furnitureGameObjectMap.ContainsKey(furn) == false)
        {
            Debug.LogError("OnFurnitureChanged -- trying to change the visuals for furniture not in our map.");
            return;
        }

        if (isUpdatingFurniture)
        {
            return;
        }

        isUpdatingFurniture = true;

        GameObject oldFurn_go = furnitureGameObjectMap[furn];

        if (furn.Is3D)
        {
            // Destroy the old GameObject
            Destroy(oldFurn_go);
            furnitureGameObjectMap.Remove(furn);

            // Get updated game object name and rotation for the new state of the furniture

            string updatedGameObjectName = GetGameObjectForFurniture(furn).ToString();  // Assuming you have such a function

            GameObject newFurn_go = Instantiate(GetGameObjectForFurniture(furn), new Vector3(furn.Tile.X, furn.FloorHeight, furn.Tile.Z), Quaternion.identity);
            newFurn_go.name = updatedGameObjectName + "_" + furn.Tile.X + "_" + furn.Tile.Z;
            newFurn_go.transform.SetParent(this.transform, true);

            MeshRenderer meshRenderer = newFurn_go.GetComponent<MeshRenderer>();
            meshRenderer.material = MaterialManager.Instance.GetMaterial(furn.ObjectType + "Material");

            //This hardcoding is not ideal!
            if (furn.ObjectType == "Door")
            {
                // By default, the door mesh is meant for walls to the east and west
                // check to see if we actually have a wall north/south, and if so
                // then rotate this GO by 90 degrees

                Tile northTile = world.GetTileAt(furn.Tile.X, furn.Tile.Z + 1);
                Tile southTile = world.GetTileAt(furn.Tile.X, furn.Tile.Z - 1);

                if (northTile != null && southTile != null && northTile.Furniture != null && southTile.Furniture != null &&
                    northTile.Furniture.ObjectType == "Wall" && southTile.Furniture.ObjectType == "Wall")
                {
                    //furn_go.transform.rotation = Quaternion.Euler(-90, 0, 90);
                    newFurn_go.transform.rotation = Quaternion.Euler(0, 90, 0);
                }
            }

            // Update the map to point to the new GameObject
            furnitureGameObjectMap[furn] = newFurn_go;

            isUpdatingFurniture = false;
        }
        else
        {
            if (furnitureGameObjectMap.ContainsKey(furn) == false)
            {
                Debug.LogError("OnFurnitureChanged -- trying to change visuals for furniture not in our map.");
                return;
            }

            GameObject furn_go = furnitureGameObjectMap[furn];

            furn_go.GetComponent<SpriteRenderer>().sprite = GetSpriteForFurniture(furn);
            furn_go.GetComponent<SpriteRenderer>().color = furn.Tint;

            isUpdatingFurniture = false;
        }
    }

    public Sprite GetSpriteForFurniture(Furniture furniture)
    {
        string spriteName = furniture.ObjectType + "_";

        // Check for neighbours North, East, South, West, Northeast, Southeast, Southwest, Northwest
        int x = furniture.Tile.X;
        int z = furniture.Tile.Z;
        string suffix = string.Empty;

        suffix += GetSuffixForNeighbour(furniture, x, z + 1, "N");
        suffix += GetSuffixForNeighbour(furniture, x + 1, z, "E");
        suffix += GetSuffixForNeighbour(furniture, x, z - 1, "S");
        suffix += GetSuffixForNeighbour(furniture, x - 1, z, "W");

        // Now we check if we have the neighbours in the cardinal directions next to the respective diagonals
        // because pure diagonal checking would leave us with diagonal walls and stockpiles, which make no sense.
        suffix += GetSuffixForDiagonalNeighbour(suffix, "N", "E", furniture, x + 1, z + 1);
        suffix += GetSuffixForDiagonalNeighbour(suffix, "S", "E", furniture, x + 1, z - 1);
        suffix += GetSuffixForDiagonalNeighbour(suffix, "S", "W", furniture, x - 1, z - 1);
        suffix += GetSuffixForDiagonalNeighbour(suffix, "N", "W", furniture, x - 1, z + 1);

        // For example, if this object has all eight neighbours of
        // the same type, then the string will look like:
        //       Wall_NESWneseswnw

        return furnitureSprites[spriteName + suffix];
    }

    private string GetSuffixForNeighbour(Furniture furn, int x, int z, string suffix)
    {
        if (furn.ObjectType != "Ceiling")
        {
            Tile t = World.current.GetTileAt(x, z);
            if (t != null && t.Furniture != null && t.Furniture.LinksToNeighbour == furn.LinksToNeighbour)
            {
                return suffix;
            }
        } else
        {
            Tile t = World.current.GetTileAt(x, z);
            if (t != null && t.Ceiling != null && t.Ceiling.LinksToNeighbour == furn.LinksToNeighbour)
            {
                return suffix;
            }
        }
        return string.Empty;
    }

    private string GetSuffixForDiagonalNeighbour(string suffix, string coord1, string coord2, Furniture furn, int x, int z)
    {
        if (suffix.Contains(coord1) && suffix.Contains(coord2))
        {
            return GetSuffixForNeighbour(furn, x, z, coord1.ToLower() + coord2.ToLower());
        }

        return string.Empty;
    }

    public GameObject GetGameObjectForFurniture(Furniture furn)
    {
        string gameObjectName = furn.ObjectType;

        if (furn.LinksToNeighbour == false)
        {
            // if this is a door, lets check openness and update the mesh.
            // FIXME: all this hardcording needs to be generalized
            if (furn.ObjectType == "Door")
            {
                if (furn.GetParameter("openness") < 0.1f)
                {
                    // Door is closed
                    gameObjectName = "Door";
                }
                else if (furn.GetParameter("openness") < 0.5f)
                {
                    // Door is a bit open
                    gameObjectName = "Door_openness_1";
                }
                else if (furn.GetParameter("openness") < 0.9f)
                {
                    // Door is a lot open
                    gameObjectName = "Door_openness_2";
                }
                else
                {
                    // Door is fully open
                    gameObjectName = "Door_openness_3";
                }
            }
            return furnitureMeshes[gameObjectName];
        }

        // Otherwise the gameobject name is more complicated
        gameObjectName = furn.ObjectType + "_";

        // Check for neighbours North, East, South, West, Northeast, Southeast, Southwest, Northwest
        int x = furn.Tile.X;
        int z = furn.Tile.Z;
        string suffix = string.Empty;

        suffix += GetSuffixForNeighbour(furn, x, z + 1, "N");
        suffix += GetSuffixForNeighbour(furn, x + 1, z, "E");
        suffix += GetSuffixForNeighbour(furn, x, z - 1, "S");
        suffix += GetSuffixForNeighbour(furn, x - 1, z, "W");

        // Now we check if we have the neighbours in the cardinal directions next to the respective diagonals
        // because pure diagonal checking would leave us with diagonal walls and stockpiles, which make no sense.
        suffix += GetSuffixForDiagonalNeighbour(suffix, "N", "E", furn, x + 1, z + 1);
        suffix += GetSuffixForDiagonalNeighbour(suffix, "S", "E", furn, x + 1, z - 1);
        suffix += GetSuffixForDiagonalNeighbour(suffix, "S", "W", furn, x - 1, z - 1);
        suffix += GetSuffixForDiagonalNeighbour(suffix, "N", "W", furn, x - 1, z + 1);

        // For example, if this object has all eight neighbours of
        // the same type, then the string will look like:
        //       Wall_NESWneseswnw

        if (furnitureMeshes.ContainsKey(gameObjectName + suffix) == false)
        {
            Debug.LogError("GetGameObjectFurniture -- No gameobject with name: " + gameObjectName + suffix);
            return furnitureMeshes[gameObjectName + suffix];
        }

        return furnitureMeshes[gameObjectName + suffix];
    }
    
    public GameObject GetGameObjectForFurniture(string objectType)
    {
        if (furnitureMeshes.ContainsKey(objectType))
        {
            return furnitureMeshes[objectType];
        }

        if (furnitureMeshes.ContainsKey(objectType + "_"))
        {
            return furnitureMeshes[objectType + "_"];
        }

        Debug.LogError("GetGameObjectForFurniture -- No mesh with name: " + "meshName");
        return null;
    }

    public Sprite GetSpriteForFurniture(string objectType)
    {
        if (furnitureSprites.ContainsKey(objectType))
        {
            return furnitureSprites[objectType];
        }

        if (furnitureSprites.ContainsKey(objectType + "_"))
        {
            return furnitureSprites[objectType + "_"];
        }

        Debug.LogError("GetSpriteForFurniture -- No sprites with name: " + objectType);
        return null;
    }
}