
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

        if (playerConfigs[index].isLocal)
        {
            Debug.Log("Entrou na condição de local");
            lobbyPanel.SetActive(true);
            ClientPlayerSetupMenu.transform.SetParent(lobbyPanel.transform);
        }
        else
        {
            Debug.Log("Entrou na condição de não local");
         playerConfigs[index].lobbyPlayersShower.setIsReady(true);   
        }
        playerConfigs[index].isReady = true;
        readyCount++;
        if (readyCount == playerConfigs.Count)
        {
            SceneManager.LoadScene("SampleScene");
        }
    }


    public OnlinePlayerConfiguration HandlePlayerJoined(Player player)
    {
        if (playerConfigs.Any(p => p.PlayerIndex == player.ActorNumber - 1))
            return null;
        
        var config = new OnlinePlayerConfiguration(player);
        config.PlayerIndex = player.ActorNumber - 1;
        playerConfigs.Add(config);
        return config;

    }


    public override void OnJoinedRoom()
    {
        roomCodeText.text = "CÓDIGO DA SALA: "+ PhotonNetwork.CurrentRoom.Name;

            foreach (Player player in PhotonNetwork.CurrentRoom.Players.Values)
            {
                if (!player.IsLocal)
                {
                    OnlinePlayerConfiguration config = HandlePlayerJoined(player);
                    if(config != null){
                        config.lobbyPlayersShower = availableLobbyPlayersShower[0];
                        lobbyPlayersShower[0].setPlayerIndex(config.PlayerIndex);
                        availableLobbyPlayersShower.RemoveAt(0);

                    }
                }
                else
                {
                    OnlinePlayerConfiguration clientConfig = HandlePlayerJoined(player);
                    if (clientConfig != null)
                    {
                        clientConfig.isLocal = true;
                        ClientPlayerSetupMenu.SetPlayerIndex(clientConfig.PlayerIndex);
                    }
                }
            }
            
            
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        
        OnlinePlayerConfiguration config = HandlePlayerJoined(newPlayer);
        if(config != null){
            lobbyPlayersShower[0].setPlayerIndex(config.PlayerIndex);
            availableLobbyPlayersShower.RemoveAt(0);

        }
        
        
    }

    public class OnlinePlayerConfiguration
    {
        public OnlinePlayerConfiguration(Player player)
        {
            PlayerIndex = player.ActorNumber;
        }
    
        public Player Player { get; set; }
        public int PlayerIndex { get; set; }
        
        public bool isLocal { get; set; }

        public OnlineLobbyPlayersShower lobbyPlayersShower { get; set; }
        public bool isReady { get; set; }
        public ScObPlayerStats playerStats { get; set; }
    
        public ScObPlayerCustom playerCustom { get; set; }
    }

}
