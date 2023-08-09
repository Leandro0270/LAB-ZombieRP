using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public class SpawnPlayerSetupMenu : MonoBehaviour
{
    public PlayerInput input;
    private void Awake()
    {
        PlayerConfigurationManager PCM = FindObjectOfType<PlayerConfigurationManager>();
        if (PCM != null)
        {
            PlayerSetupMenuController menu;
            menu = PCM.setPlayerSetupMenuController(input);
            input.uiInputModule = menu.GetInputSystemUIInputModule(input);
            menu.SetPlayerIndex(input.playerIndex);
            PCM.deletePlayerSetupMenuController();
        }
    }
}
