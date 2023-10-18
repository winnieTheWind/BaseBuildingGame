using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UserModeButton : MonoBehaviour
{
    public TextMeshProUGUI gameStateText;

    World world;

    private void Start()
    {
        world = WorldController.Instance.world;
        UpdateGameStateText(); // Update the UI text after changing the game state

    }

    public void ToggleGameState()
    {
        switch (world.currentUserState)
        {
            case World.UserState.FREE_EDIT:
                world.currentUserState = World.UserState.GAME;
                break;
            case World.UserState.GAME:
                world.currentUserState = World.UserState.FREE_EDIT;
                break;
            case World.UserState.CUTSCENE:
                world.currentUserState = World.UserState.FREE_EDIT; // Or choose the next appropriate state
                break;
        }
        UpdateGameStateText(); // Update the UI text after changing the game state

    }

    void UpdateGameStateText()
    {
        gameStateText.text = world.currentUserState.ToString(); // Update TMP text with current state
    }


    public void Clicked()
    {
        ToggleGameState();
    }
}
