using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMenuController : MonoBehaviour
{
    public GameObject TileView;
    public GameObject FurnitureView;
    public GameObject EquipmentView;

    public void ClickedTileButton()
    {
        TileView.SetActive(true);
        //FurnitureView.SetActive(false);
        //EquipmentView.SetActive(false);

    }

    public void ClickedFurnitureButton()
    {
        //TileView.SetActive(false);
        //FurnitureView.SetActive(true);
        //EquipmentView.SetActive(false);


    }

    public void ClickedEquipmentButton()
    {
        //TileView.SetActive(false);
        //FurnitureView.SetActive(false);
        //EquipmentView.SetActive(true);
    }

    public void ClickedCloseButton()
    {
        TileView.SetActive(false);
        FurnitureView.SetActive(false);
        EquipmentView.SetActive(false);
    }
}
