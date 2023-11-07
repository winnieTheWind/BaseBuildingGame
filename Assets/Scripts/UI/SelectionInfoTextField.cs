using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SelectionInfoTextField : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    MouseController mc;
    TextMeshProUGUI txt;

    void Start()
    {
        mc = FindObjectOfType<MouseController>();
        txt = GetComponent<TextMeshProUGUI>();

    }

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
