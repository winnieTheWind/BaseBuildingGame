using UnityEngine;
using TMPro;

public class DialogListItem : MonoBehaviour
{
    public TMP_InputField inputField;

    public void Clicked()
    {
        inputField.text = transform.GetComponentInChildren<TextMeshProUGUI>().text;
    }
}
