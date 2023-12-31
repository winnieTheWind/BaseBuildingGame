using UnityEngine;
using TMPro;

public class MouseOverFurnitureTypeText : MonoBehaviour
{

    // Every frame, this script checks to see which tile
    // is under the mouse and then updates the GetComponent<Text>.text
    // parameter of the object it is attached to.

    TextMeshProUGUI myText;
    MouseController mouseController;

    // Use this for initialization
    void Start()
    {
        myText = GetComponent<TextMeshProUGUI>();

        if (myText == null)
        {
            Debug.LogError("MouseOverTileTypeText: No 'Text' UI component on this object.");
            this.enabled = false;
            return;
        }

        mouseController = GameObject.FindObjectOfType<MouseController>();
        if (mouseController == null)
        {
            Debug.LogError("How do we not have an instance of mouse controller?");
            return;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Tile t = mouseController.GetMouseOverTile();

        string s = "NULL";

        if (t != null && t.Furniture != null && t.Furniture.ObjectType != "Ceiling")
        {
            s = t.Furniture.ObjectType;
        }
        else
        {
            s = "N/A";
        }

        myText.text = "Furniture: " + s;
    }
}
