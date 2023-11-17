using TMPro;
using UnityEngine;

public class MouseOverRoomIndexText : MonoBehaviour
{

    TextMeshProUGUI myText;
    MouseController mouseController;
    // Start is called before the first frame update
    void Start()
    {
        myText = GetComponent<TextMeshProUGUI>();

        if (myText == null)
        {
            Debug.LogError("MouseOverRoomIndex: No 'TextMeshProUGUI' UI Component on this object");
            this.enabled = false;
            return;
        }

        mouseController = GameObject.FindObjectOfType<MouseController>();
        if (mouseController == null)
        {
            Debug.LogError("How do we not have an instance of mouse controller.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        Tile t = mouseController.GetMouseOverTile();

        if (myText != null && t != null)
        {
            myText.text = "Room Index: " + World.current.rooms.IndexOf(t.Room).ToString();
        }
    }
}
