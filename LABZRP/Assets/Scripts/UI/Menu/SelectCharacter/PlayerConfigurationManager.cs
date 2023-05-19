using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerConfigurationManager : MonoBehaviour
{
    private List<PlayerConfiguration> playerConfigs;
    private int readyCount = 0;

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
        Debug.Log("Player " + index + " pronto");
        playerConfigs[index].isReady = true;
        readyCount++;
        if (readyCount == playerConfigs.Count)
        {
            Debug.Log("carregando fase");
            SceneManager.LoadScene("SampleScene");
        }
    }

    public void HandlePlayerJoined(PlayerInput pi)
    {
        Debug.Log("Player Joined" + pi.playerIndex);
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