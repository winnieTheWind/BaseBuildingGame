using UnityEngine;
using TMPro;

public class MouseOverTileTypeText : MonoBehaviour
{

    // Every frame, this script checks to see which tile
    // is under the mouse and then updates the GetComponent<Text>.text
    // parameter of the object it is attached to.

    TextMeshProUGUI myText;

    MouseController mouseController;

    public TextMeshProUGUI costText;

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

        if (t != null)
        {
            if (t.furniture != null && t.furniture.objectType == "Stockpile")
            {
                myText.text = "Tile Type: " + t.Type.ToString() + " " + t.X.ToString() + " " + t.Z.ToString() + " " + t.furniture.Bitmask;

            } else
            {
                myText.text = "Tile Type: " + t.Type.ToString() + " " + t.X.ToString() + " " + t.Z.ToString() + " " + t.movementCost;

            }
        }
        else
        {
            myText.text = "Tile Type: N/A";
        }
    }
}
