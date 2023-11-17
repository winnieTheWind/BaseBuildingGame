using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterSelectionController : MonoBehaviour
{
    public List<CharacterObject> characterObjects;
    public GameObject content;
    public GameObject prefab;

    public BuildModeController bmc;

    public static Action<List<CharacterObject>> sendCharacterObjects;

    private void Start()
    {

        foreach (CharacterObject furnInv in characterObjects)
        {
            GameObject furnInv_go = Instantiate(prefab);
            furnInv_go.transform.SetParent(content.transform);
            furnInv_go.GetComponentInChildren<TextMeshProUGUI>().text = furnInv.Name;
        }

        sendCharacterObjects?.Invoke(characterObjects);
    }
}
