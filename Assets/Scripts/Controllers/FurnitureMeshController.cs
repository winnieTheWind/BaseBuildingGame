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

        world.RegisterFurnitureCreated(OnFurnitureCreated);

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

            furn_go.name = furn.objectType.ToString() + "_" + furn.tile.X + "_" + furn.tile.Z;
            furn_go.transform.position = new Vector3(furn.tile.X + ((furn.Width)/2f), 0, furn.tile.Z + ((furn.Height) / 2f));
            furn_go.transform.SetParent(this.transform, true);
            // Get MeshRenderer component to the GameObject
            MeshRenderer meshRenderer = furn_go.GetComponent<MeshRenderer>();
            meshRenderer.sortingLayerName = "Furniture";

            // Optionally, you might want to set the material of the MeshRenderer
            meshRenderer.material = WallMaterial;

        } else
        {
            furn_go = new GameObject();
            // add our tile/GO pair to the dictionary
            furnitureGameObjectMap.Add(furn, furn_go);

            furn_go.name = furn.objectType.ToString() + "_" + furn.tile.X + "_" + furn.tile.Z;
            furn_go.transform.position = new Vector3(furn.tile.X + ((furn.Width - 1) / 2f), 0, furn.tile.Z + ((furn.Height - 1) / 2f));
            furn_go.transform.rotation = Quaternion.Euler(90, 0, 0);
            furn_go.transform.SetParent(this.transform, true);
            //furn_go.GetComponentInChildren<SpriteRenderer>().sortingLayerName = "Furniture";

            SpriteRenderer sr = furn_go.AddComponent<SpriteRenderer>();
            sr.sprite = GetSpriteForFurniture(furn);
            sr.sortingLayerName = "Furniture";
            sr.color = furn.tint;

           
        }

        // Register our callback so that our GameObject gets updated whenever
        // the object's into changes.
        furn.RegisterOnChangedCallback(OnFurnitureChanged);
        furn.RegisterOnRemovedCallback(OnFurnitureRemoved);


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

            GameObject newFurn_go = Instantiate(GetGameObjectForFurniture(furn), new Vector3(furn.tile.X + ((furn.Width) / 2f), 0, furn.tile.Z + ((furn.Height) / 2f)) , Quaternion.identity); 
            newFurn_go.name = updatedGameObjectName + "_" + furn.tile.X + "_" + furn.tile.Z;
            newFurn_go.transform.SetParent(this.transform, true);
            MeshRenderer meshRenderer = newFurn_go.GetComponent<MeshRenderer>();
            meshRenderer.material = WallMaterial;
            meshRenderer.material.color = new Color(1f, 1f, 1f, 1f);

            //This hardcoding is not ideal!
            if (furn.objectType == "Door")
            {
                newFurn_go.transform.position = new Vector3(furn.tile.X + ((furn.Width - 1) / 2f), 0, furn.tile.Z + ((furn.Height - 1) / 2f));

                // By default, the door mesh is meant for walls to the east and west
                // check to see if we actually have a wall north/south, and if so
                // then rotate this GO by 90 degrees

                Tile northTile = world.GetTileAt(furn.tile.X, furn.tile.Z + 1);
                Tile southTile = world.GetTileAt(furn.tile.X, furn.tile.Z - 1);

                if (northTile != null && southTile != null && northTile.furniture != null && southTile.furniture != null &&
                    northTile.furniture.objectType.Contains("Wall") && southTile.furniture.objectType.Contains("Wall"))
                {
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
            furn_go.GetComponent<SpriteRenderer>().color = furn.tint;
            isUpdatingFurniture = false;
        }
    }

    public Sprite GetSpriteForFurniture(Furniture furn)
    {
        string spriteName = furn.objectType;

        if (furn.linksToNeighbour == false)
        {

            // If this is a DOOR, let's check OPENNESS and update the sprite.
            // FIXME: All this hardcoding needs to be generalized later.
            if (furn.objectType == "Door")
            {
                if (furn.GetParameter("openness") < 0.1f)
                {
                    // Door is closed
                    spriteName = "Door";
                }
                else if (furn.GetParameter("openness") < 0.5f)
                {
                    // Door is a bit open
                    spriteName = "Door_openness_1";
                }
                else if (furn.GetParameter("openness") < 0.9f)
                {
                    // Door is a lot open
                    spriteName = "Door_openness_2";
                }
                else
                {
                    // Door is a fully open
                    spriteName = "Door_openness_3";
                }
                //Debug.Log(spriteName);
            }

            /*if(furnitureSprites.ContainsKey(spriteName) == false) {
				Debug.Log("furnitureSprites has no definition for: " + spriteName);
				return null;
			}
*/

            return SpriteManager.current.GetSprite("Furniture", spriteName); // furnitureSprites[spriteName];
        }

        // Otherwise, the sprite name is more complicated.

        spriteName = furn.objectType + "_";

        // Check for neighbours North, East, South, West

        int x = furn.tile.X;
        int y = furn.tile.Z;

        Tile t;

        t = world.GetTileAt(x, y + 1);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
        {
            spriteName += "N";
        }
        t = world.GetTileAt(x + 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
        {
            spriteName += "E";
        }
        t = world.GetTileAt(x, y - 1);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
        {
            spriteName += "S";
        }
        t = world.GetTileAt(x - 1, y);
        if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
        {
            spriteName += "W";
        }

        // For example, if this object has all four neighbours of
        // the same type, then the string will look like:
        //       Wall_NESW

        /*		if(furnitureSprites.ContainsKey(spriteName) == false) {
                    Debug.LogError("GetSpriteForInstalledObject -- No sprites with name: " + spriteName);
                    return null;
                }
        */

        return SpriteManager.current.GetSprite("Furniture", spriteName); //furnitureSprites[spriteName];

    }


    public Sprite GetSpriteForFurniture(string objectType)
    {
        Sprite s = SpriteManager.current.GetSprite("Furniture", objectType);

        if (s == null)
        {
            s = SpriteManager.current.GetSprite("Furniture", objectType + "_");
        }

        return s;
    }
    public GameObject GetGameObjectForFurniture(Furniture furn)
    {
        string gameObjectName = furn.objectType;

        if (furn.linksToNeighbour == false)
        {
            // if this is a door, lets check openness and update the mesh.
            // FIXME: all this hardcording needs to be generalized
            if (furn.objectType == "Door")
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

        if (furn.linksToNeighbour == true)
        {
            // Otherwise the gameobject name is more complicated

            gameObjectName = furn.objectType + "_";

            // Check for neighbours north east south west

            int x = furn.tile.X;
            int z = furn.tile.Z;

            Tile t;
            t = world.GetTileAt(x, z + 1);
            if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
            {
                gameObjectName += "N";
            }
            t = world.GetTileAt(x + 1, z);
            if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
            {
                gameObjectName += "E";
            }
            t = world.GetTileAt(x, z - 1);
            if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
            {
                gameObjectName += "S";
            }
            t = world.GetTileAt(x - 1, z);
            if (t != null && t.furniture != null && t.furniture.objectType == furn.objectType)
            {
                gameObjectName += "W";
            }
        }

        // For example, if this object has all four neighbours of the same type,
        // then the string will look like:
        // Wall_NSWE

        if (furnitureMeshes.ContainsKey(gameObjectName) == false)
        {
            Debug.LogError("GetGameObjectFurniture -- No gameobject with name: " + gameObjectName);
            return furnitureMeshes[gameObjectName];
        }

        return furnitureMeshes[gameObjectName];
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
}