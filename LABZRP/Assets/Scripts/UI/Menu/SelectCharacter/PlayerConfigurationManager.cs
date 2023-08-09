using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerConfigurationManager : MonoBehaviour
{

    [SerializeField] private Button returnButton;
    [SerializeField] private GameObject returnMenu;
    [SerializeField] private GameObject[] playerPrefabs;
    private List<PlayerConfiguration> playerConfigs;
    private int readyCount = 0;
    private int PlayerPausedIndex = -1;
    private PlayerSetupMenuController currentPlayerPausedMenu;
    private PlayerSetupMenuController returnButtonOwner;
    private bool showedReturnMenu = false;
    [SerializeField] private List<PlayerSetupMenuController> playerSetupsList;
    public static PlayerConfigurationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.Log("SINGLETON - Trying to create another instance of singleton");
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
            playerConfigs = new List<PlayerConfiguration>();
        }
    }

    public void SetScObPlayerStats(int index, ScObPlayerStats stats)
    {
        playerConfigs[index].playerStats = stats;
    }

    public void ReturnButton()
    {
        if (returnButtonOwner == null)
        {
            return;
        }
        int playerIndex = returnButtonOwner.GetPlayerIndex();
        MultiplayerEventSystem eventSystem = returnButtonOwner.GetEventSystem();
        GameObject cancelButton = returnButtonOwner.GetCancelButton();
        bool canShow = PlayerConfigurationManager.Instance.ShowReturnMenu(playerIndex, returnButtonOwner);
        if (canShow)
        {
            eventSystem.playerRoot = returnMenu;
            eventSystem.SetSelectedGameObject(cancelButton);
        }
    }

    public PlayerSetupMenuController setPlayerSetupMenuController(PlayerInput input)
    {
        if (input.currentControlScheme == "Keyboard")
        {
            returnButton.gameObject.transform.SetParent(playerSetupsList[0].transform);
            returnButtonOwner = playerSetupsList[0];
        }

        return playerSetupsList[0];
    }
    public void deletePlayerSetupMenuController()
    {
        playerSetupsList.RemoveAt(0);
    }
    public void SetPlayerName(int index, string name)
    {
        playerConfigs[index].playerStats.name = name;
    }
    public void SetPlayerSkin(int index, ScObPlayerCustom playerCustom)
    {
        playerConfigs[index].playerCustom = playerCustom;
    }

    public List<PlayerConfiguration> GetPlayerConfigs()
    {
        return playerConfigs;
    }
    public void ReadyPlayer(int index)
    {
        playerConfigs[index].isReady = true;
        readyCount++;
        if (readyCount == playerConfigs.Count)
        {
            loadScene("SampleScene");
        }
    }
    
    public void loadScene(string scene)
    {
        if (scene == "MainMenu")
        {
            SceneManager.LoadScene(scene);
            Destroy(gameObject);
        }
        else
            SceneManager.LoadScene(scene);
    }
    
    public void CancelReadyPlayer(int index)
    {
        playerConfigs[index].isReady = false;
        readyCount--;
    }

    public void HideReturnMenu()
    {
        returnMenu.SetActive(false);
        foreach (GameObject players in playerPrefabs)
        {
            players.SetActive(true);
        }
        showedReturnMenu = false;
        currentPlayerPausedMenu.SelectFirstClass();
        currentPlayerPausedMenu = null;
        PlayerPausedIndex = -1;
        

    }

    public bool ShowReturnMenu(int playerPausedIndex, PlayerSetupMenuController playerSetupMenuController)
    {
        if (playerPausedIndex == PlayerPausedIndex)
        {
            HideReturnMenu();
            return false;
        }
        returnMenu.SetActive(true);
        foreach (GameObject players in playerPrefabs)
        {
            players.SetActive(false);
        }
        if (PlayerPausedIndex == -1 && !showedReturnMenu)
        {
                PlayerPausedIndex = playerPausedIndex;
                showedReturnMenu = true;
                currentPlayerPausedMenu = playerSetupMenuController;
                return true;
            
        }

        return false;
    }
    
    public void HandlePlayerJoined(PlayerInput pi)
    {
        if (!playerConfigs.Any(p => p.PlayerIndex == pi.playerIndex))
        {
            pi.transform.SetParent(transform);
            playerConfigs.Add(new PlayerConfiguration(pi));
        }
    }

}

public class PlayerConfiguration
{
    public PlayerConfiguration(PlayerInput pi)
    {
        PlayerIndex = pi.playerIndex;
        Input = pi;
    }
    public PlayerInput Input { get; set; }
    
    public int PlayerIndex { get; set; }
    
    public bool isReady { get; set; }
    public ScObPlayerStats playerStats { get; set; }
    
    public ScObPlayerCustom playerCustom { get; set; }
}