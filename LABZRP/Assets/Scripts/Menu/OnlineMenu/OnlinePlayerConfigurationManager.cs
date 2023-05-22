
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
    [SerializeField] private PhotonView photonView;
    
    [SerializeField] private List<Material> Skin;
    [SerializeField] private List<Material> Eyes;
    [SerializeField] private List<Material> tshirt;
    [SerializeField] private List<Material> pants;
    [SerializeField] private List<Material> Shoes;

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

    [PunRPC]
    public void SetPlayerName(int index, string name)
    {
        foreach (OnlinePlayerConfiguration configs in playerConfigs)
        {
            if (configs.PlayerIndex == index)
            {
                if (configs.isLocal)
                {
                    ClientPlayerSetupMenu.setPlayerName(name);
                }
                else
                {
                    configs.lobbyPlayersShower.setPlayerName(name);
                }
            }
        }
    }

    [PunRPC]
    public void SetPlayerSkin(int index, int skinIndex, int eyesIndex, int tshirtIndex, int pantsIndex, int shoesIndex)
    {
        ScObPlayerCustom playerCustom = ScriptableObject.CreateInstance<ScObPlayerCustom>();
        playerCustom.Skin = Skin[skinIndex];
        playerCustom.SkinIndex = skinIndex;
        playerCustom.Eyes = Eyes[eyesIndex];
        playerCustom.EyesIndex = eyesIndex;
        playerCustom.tshirt = tshirt[tshirtIndex];
        playerCustom.tshirtIndex = tshirtIndex;
        playerCustom.pants = pants[pantsIndex];
        playerCustom.pantsIndex = pantsIndex;
        playerCustom.Shoes = Shoes[shoesIndex];
        playerCustom.ShoesIndex = shoesIndex;
        foreach (OnlinePlayerConfiguration configs in playerConfigs)
        {
            if (configs.PlayerIndex == index)
            {
                configs.playerCustom = playerCustom;
                if(!configs.isLocal){
                    configs.lobbyPlayersShower.setPlayerCustom(playerCustom);
                }
            }
        }
        
    }
    
    public void PunSetPlayerSkin(int index, ScObPlayerCustom playerCustom)
    {
        int skinIndex = playerCustom.SkinIndex;
        int eyesIndex = playerCustom.EyesIndex;
        int tshirtIndex = playerCustom.tshirtIndex;
        int pantsIndex = playerCustom.pantsIndex;
        int shoesIndex = playerCustom.ShoesIndex;
        photonView.RPC("SetPlayerSkin", RpcTarget.All, index, skinIndex, eyesIndex, tshirtIndex, pantsIndex, shoesIndex);
    }
    public void PunSetPlayerName(int index, string name)
    {
        photonView.RPC("SetPlayerName", RpcTarget.All, index, name);
    }

    public List<OnlinePlayerConfiguration> GetPlayerConfigs()
    {
        return playerConfigs;
    }

    public void PunReadyPlayer(int index)
    {
        photonView.RPC("ReadyPlayer", RpcTarget.All, index);
    }
    [PunRPC]
    public void ReadyPlayer(int index)
    {
        foreach (OnlinePlayerConfiguration configs in playerConfigs)
        {
            if (configs.PlayerIndex == index)
            {
                if (configs.isLocal)
                {
                    lobbyPanel.SetActive(true);
                    ClientPlayerSetupMenu.transform.SetParent(lobbyPanel.transform);
                }else
                {
                    configs.lobbyPlayersShower.setIsReady(true);   
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
    
    public void PunCancelReadyPlayer(int index)
    {
        photonView.RPC("CancelReadyPlayer", RpcTarget.All, index);
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
                if (OnlineConfigPlayer.player == PhotonNetwork.LocalPlayer)
                {
                    OnlineConfigPlayer.isLocal = true;
                    ClientPlayerSetupMenu.SetPlayerIndex(OnlineConfigPlayer.PlayerIndex);
                    playerConfigs.Add(OnlineConfigPlayer);


                }
                else
                {
                    OnlineConfigPlayer.isLocal = false;
                    OnlineConfigPlayer.lobbyPlayersShower = availableLobbyPlayersShower[0];
                    lobbyPlayersShower[0].setPlayerIndex(OnlineConfigPlayer.PlayerIndex);
                    Debug.Log(OnlineConfigPlayer.lobbyPlayersShower);
                    availableLobbyPlayersShower.RemoveAt(0);
                    playerConfigs.Add(OnlineConfigPlayer);

                }

                foreach (Player player in playersNaSala)
                {
                    if (OnlineConfigPlayer.player == player)
                    {
                        PunSetPlayerName(OnlineConfigPlayer.PlayerIndex, player.NickName);
                    }
                }
            }
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        
        playersNaSala.Append(newPlayer);
        OnlinePlayerConfiguration OnlineConfigPlayer = HandlePlayerJoined(newPlayer);
        if (OnlineConfigPlayer != null)
        {
            OnlineConfigPlayer.isLocal = false;
            OnlineConfigPlayer.lobbyPlayersShower = availableLobbyPlayersShower[0];
            lobbyPlayersShower[0].setPlayerIndex(OnlineConfigPlayer.PlayerIndex);
            Debug.Log(OnlineConfigPlayer.lobbyPlayersShower);
            PunSetPlayerName(OnlineConfigPlayer.PlayerIndex, newPlayer.NickName);
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
