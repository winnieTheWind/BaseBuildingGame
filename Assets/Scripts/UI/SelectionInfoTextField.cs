using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectionInfoTextField : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    MouseController mc;
    TextMeshProUGUI txt;
    // Start is called before the first frame update
    void Start()
    {
        mc = FindObjectOfType<MouseController>();
        txt = GetComponent<TextMeshProUGUI>(); 

    }

    // Update is called once per frame
    void Update()
    {
        if (mc.mySelection == null)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            return;
        }

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        ISelectableInterface actualSelection = mc.mySelection.stuffInTile[mc.mySelection.subSelection];
        txt.text = actualSelection.GetName() + 
            "\n" + actualSelection.GetDescription() + 
            "\n" + actualSelection.GetHitPointString() +
            "\n" + actualSelection.GetCharacterType();
    }
}
