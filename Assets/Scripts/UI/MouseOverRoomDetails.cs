using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class MouseOverRoomDetails : MonoBehaviour
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

        if (t == null || t.Room == null)
        {
            myText.text = "";
            return;
        }

        string s = "";

        foreach (string g in t.Room.GetGasNames())
        {
            s += g + ": " + t.Room.GetGasAmount(g) + " (" + (t.Room.GetGasPercentage(g) * 100) + "%) ";
        }

        myText.text = s;
    }
}
