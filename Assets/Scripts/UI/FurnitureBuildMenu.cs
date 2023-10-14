using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FurnitureBuildMenu : MonoBehaviour
{

    public GameObject buildFurnitureButtonPrefab;
    // Start is called before the first frame update
    void Start()
    {
        BuildModeController bmc = GameObject.FindObjectOfType<BuildModeController>();

        // Add a button for building each type of furniture
        foreach (string s in World.current.furniturePrototypes.Keys)
        {
            GameObject go = (GameObject)Instantiate(buildFurnitureButtonPrefab);
            go.transform.SetParent(transform);

            string objectId = s;
            string objectName = World.current.furniturePrototypes[s].Name;

            go.name = "Button - Build " + objectId;
            go.transform.GetComponentInChildren<TextMeshProUGUI>().text = "Build " + objectName;

            Button b = go.GetComponent<Button>();


            b.onClick.AddListener(delegate { bmc.SetMode_BuildFurniture(objectId); });
        }

        // Add a button for building each type of furniture
        foreach (string s in World.current.characterPrototypes.Keys)
        {
            GameObject go = (GameObject)Instantiate(buildFurnitureButtonPrefab);
            go.transform.SetParent(transform);

            string objectId = s;
            string objectName = World.current.characterPrototypes[s].Name;

            go.name = "Button - Build " + objectId;
            go.transform.GetComponentInChildren<TextMeshProUGUI>().text = "Build " + objectName;

            Button b = go.GetComponent<Button>();

            b.onClick.AddListener(delegate { bmc.SetMode_SpawnCharacter(objectId); });
        }

    }

}
