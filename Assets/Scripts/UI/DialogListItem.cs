using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using TMPro.EditorUtilities;

public class DialogListItem : MonoBehaviour
{
    public TMP_InputField inputField;

    public void Clicked()
    {
        inputField.text = transform.GetComponentInChildren<TextMeshProUGUI>().text;
    }
}
