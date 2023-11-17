using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterButton : MonoBehaviour
{
    public CharacterObject furn;

    List<CharacterObject> charObjects;

    private BuildModeController bmc;

    private void Start()
    {
        bmc = GameObject.FindObjectOfType<BuildModeController>();
    }

    private void OnEnable()
    {
        CharacterSelectionController.sendCharacterObjects += RegisterSendCharacterObjects;
    }

    private void OnDisable()
    {
        CharacterSelectionController.sendCharacterObjects -= RegisterSendCharacterObjects;
    }

    void RegisterSendCharacterObjects(List<CharacterObject> _charObjects)
    {
        charObjects = new List<CharacterObject>();
        charObjects = _charObjects;
    }

    public void Clicked()
    {
        string s = transform.GetComponentInChildren<TextMeshProUGUI>().text;
        for (int i = 0; i < charObjects.Count; i++)
        {
            if (charObjects[i].Name == s)
            {
                bmc.SetMode_BuildCharacter(charObjects[i].Name);
            }
        }
    }

}
