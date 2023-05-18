
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class OnlinePlayerConfigurationManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI roomCodeText;
    [SerializeField] private GameObject lobbyPanel;
    [SerializeField] private GameObject clientPanel;
    [SerializeField] PlayerSetupMenuController ClientPlayerSetupMenu;
    [SerializeField] private List<OnlineLobbyPlayersShower> lobbyPlayersShower;
    private List<OnlineLobbyPlayersShower> availableLobbyPlayersShower = new List<OnlineLobbyPlayersShower>();
    private List<OnlinePlayerConfiguration> playerConfigs;
    private Player[] playersNaSala;
    private int readyCount = 0;
    private int playersCount = 0;
    

    public static OnlinePlayerConfigurationManager Instance { get; private set; }

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
            availableLobbyPlayersShower = lobbyPlayersShower;
            playerConfigs = new List<OnlinePlayerConfiguration>();
        }
    }

    public void SetScObPlayerStats(int index, ScObPlayerStats stats)
    {
        playerConfigs[index].playerStats = stats;
    }
    
    public void CancelReadyPlayer(int index)
    {
        playerConfigs[index].isReady = false;
        readyCount--;
    }
    
    public void SetPlayerName(int index, string name)
    {
        playerConfigs[index].playerStats.name = name;
    }
    public void SetPlayerSkin(int index, ScObPlayerCustom playerCustom)
    {
        playerConfigs[index].playerCustom = playerCustom;
    }

    public List<OnlinePlayerConfiguration> GetPlayerConfigs()
    {
        return playerConfigs;
    }
    
    [PunRPC]
    public void ReadyPlayer(int index)
    {
        Debug.Log("Chamou o RPC");
        foreach (var configs in playerConfigs)
        {
            if (configs.PlayerIndex == index)
            {
                if (configs.isLocal)
                {
                    Debug.Log("Entrou na condição de local");
                    lobbyPanel.SetActive(true);
                    ClientPlayerSetupMenu.transform.SetParent(lobbyPanel.transform);
                }else
                {
                    Debug.Log("Entrou na condição de não local");
                    playerConfigs[index].lobbyPlayersShower.setIsReady(true);   
                }
                configs.isReady = true;
                readyCount++;
                if (readyCount == playerConfigs.Count)
                {
                    SceneManager.LoadScene("SampleScene");
                }
                
            }
        }
    }


    public OnlinePlayerConfiguration HandlePlayerJoined(Player player)
    {
        if (playerConfigs.Count >= 4)
        {
            Debug.Log("Max players reached");
            return null;
        }

        for (int i = 0; i < playerConfigs.Count; i++)
        {
            if (Equals(playerConfigs[i].player, player))
                return null;
        }
        
        var config = new OnlinePlayerConfiguration(player);
        config.player = player;
        config.PlayerIndex = player.ActorNumber - 1;
        playerConfigs.Add(config);
        return config;
    }
 

    public override void OnJoinedRoom()
    {
        roomCodeText.text = "Room Code: " + PhotonNetwork.CurrentRoom.Name;
        playersNaSala = PhotonNetwork.CurrentRoom.Players.Values.ToArray();
        foreach (var CurrentPlayer in playersNaSala)
        {
            OnlinePlayerConfiguration OnlineConfigPlayer = HandlePlayerJoined(CurrentPlayer);
            if (OnlineConfigPlayer != null)
            {
                if (playersNaSala[0] == OnlineConfigPlayer.player)
                {
                    OnlineConfigPlayer.isLocal = true;
                    ClientPlayerSetupMenu.SetPlayerIndex(OnlineConfigPlayer.PlayerIndex);
                }
                else
                {
                    OnlineConfigPlayer.isLocal = false;
                    OnlineConfigPlayer.lobbyPlayersShower = availableLobbyPlayersShower[0];
                    lobbyPlayersShower[0].setPlayerIndex(OnlineConfigPlayer.PlayerIndex);
                    availableLobbyPlayersShower.RemoveAt(0);
                }
            }
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        OnlinePlayerConfiguration OnlineConfigPlayer = HandlePlayerJoined(newPlayer);
        if (OnlineConfigPlayer != null)
        {
            OnlineConfigPlayer.lobbyPlayersShower = availableLobbyPlayersShower[0];
            lobbyPlayersShower[0].setPlayerIndex(OnlineConfigPlayer.PlayerIndex);
            availableLobbyPlayersShower.RemoveAt(0);
        }
    }

    public class OnlinePlayerConfiguration
    {
        public OnlinePlayerConfiguration(Player player)
        {
            PlayerIndex = player.ActorNumber;
        }
    
        public Player player { get; set; }
        public int PlayerIndex { get; set; }
        
        public bool isLocal { get; set; }

        public OnlineLobbyPlayersShower lobbyPlayersShower { get; set; }
        public bool isReady { get; set; }
        public ScObPlayerStats playerStats { get; set; }
    
        public ScObPlayerCustom playerCustom { get; set; }
    }

}
