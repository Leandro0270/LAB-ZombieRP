
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
using UnityEngine.SocialPlatforms;

 public class OnlinePlayerConfigurationManager : MonoBehaviourPunCallbacks
{
     private List<int> playersIndexReady = new List<int>();
     private bool isMasterClient = false;
     private string roomCode;
     private string clientPlayerName;
     private int clientPlayerIndex;
     [SerializeField] private TextMeshProUGUI roomCodeText;
     [SerializeField] private GameObject lobbyPanel;
     [SerializeField] private GameObject clientPanel;
     [SerializeField] OnlinePlayerSetupMenuController ClientPlayerSetupMenu;
     [SerializeField] private List<OnlineLobbyPlayersShower> lobbyPlayersShower;
     [SerializeField] private List<OnlineLobbyPlayersShower> availableLobbyPlayersShower = new List<OnlineLobbyPlayersShower>();
     List<OnlinePlayerConfiguration> playerConfigs = new List<OnlinePlayerConfiguration>();
     [SerializeField] private PhotonView photonView;
     [SerializeField] private List<Material> Skin;
     [SerializeField] private List<Material> Eyes;
     [SerializeField] private List<Material> tshirt;
     [SerializeField] private List<Material> pants;
     [SerializeField] private List<Material> Shoes;
     [SerializeField] private List<ScObPlayerStats> playerStats;
     private Player[] playersNaSala;
     private int readyCount = 0;
     private int playersCount = 0;
     
     
     private void Awake()
     {
         DontDestroyOnLoad(this);
         availableLobbyPlayersShower = lobbyPlayersShower;
         PhotonNetwork.AutomaticallySyncScene = true;
     }
    public override void OnJoinedRoom(){
    
        playersNaSala = PhotonNetwork.CurrentRoom.Players.Values.ToArray();
        isMasterClient = PhotonNetwork.IsMasterClient;
        roomCode = PhotonNetwork.CurrentRoom.Name;
        roomCodeText.text = roomCode;
        clientPlayerName = PhotonNetwork.LocalPlayer.NickName;
        clientPlayerIndex = PhotonNetwork.LocalPlayer.ActorNumber;
        if (!isMasterClient)
        {
            foreach (var verificacaoDePlayer in playersNaSala)
            {
                OnlinePlayerConfiguration OnlineConfigPlayer = HandlePlayerJoined(verificacaoDePlayer);
                if (OnlineConfigPlayer.isLocal == false)
                {
                    OnlineConfigPlayer.lobbyPlayersShower = availableLobbyPlayersShower[0];
                    lobbyPlayersShower[0].setPlayerIndex(OnlineConfigPlayer.PlayerIndex);
                    availableLobbyPlayersShower.RemoveAt(0);
                    playerConfigs.Add(OnlineConfigPlayer);
                    
                }
                else
                {
                    clientPlayerIndex = OnlineConfigPlayer.PlayerIndex;
                    clientPlayerName = OnlineConfigPlayer.playerName;
                    ClientPlayerSetupMenu.SetPlayerIndex(clientPlayerIndex, clientPlayerName);
                    playerConfigs.Add(OnlineConfigPlayer);
                    
                }
            }
            //Vai organizar a lista de playerconfigs com base no playerindex
            playerConfigs = playerConfigs.OrderBy(playerConfig => playerConfig.PlayerIndex).ToList();
        }
        else
        {
            OnlinePlayerConfiguration MasterLocalPlayer = HandlePlayerJoined(playersNaSala[0]);
            clientPlayerIndex = MasterLocalPlayer.PlayerIndex;
            clientPlayerName = MasterLocalPlayer.playerName;
            ClientPlayerSetupMenu.SetPlayerIndex(clientPlayerIndex, clientPlayerName);
            playerConfigs.Add(MasterLocalPlayer);
        }

    }
    
    
    public override void OnPlayerEnteredRoom(Player newPlayer)
     {
         if(isMasterClient){
             foreach (int readyPlayerIndex in playersIndexReady)
             {
                 ScObPlayerCustom playerCustom = playerConfigs[readyPlayerIndex].playerCustom;
                 int skinIndex = playerCustom.SkinIndex;
                 int eyesIndex = playerCustom.EyesIndex;
                 int tshirtIndex = playerCustom.tshirtIndex;
                 int pantsIndex = playerCustom.pantsIndex;
                 int shoesIndex = playerCustom.ShoesIndex;
                 photonView.RPC("SetPlayerSkin", RpcTarget.All, readyPlayerIndex, skinIndex, eyesIndex, tshirtIndex, pantsIndex, shoesIndex);
             }
             foreach (int readyPlayerIndex in playersIndexReady)
             {
                 int classIndex = playerConfigs[readyPlayerIndex].playerStats.classIndex;
                    photonView.RPC("SetScObPlayerStats", RpcTarget.All, readyPlayerIndex, classIndex);
             }
             
             foreach (var readyPlayers in playersIndexReady)
             {
                 photonView.RPC("ReadyPlayer", RpcTarget.All, readyPlayers);
             }
         }
         playersNaSala.Append(newPlayer);
         OnlinePlayerConfiguration OnlineConfigPlayer = HandlePlayerJoined(newPlayer);
         if (OnlineConfigPlayer != null)
         {
             OnlineConfigPlayer.lobbyPlayersShower = availableLobbyPlayersShower[0];
             lobbyPlayersShower[0].setPlayerIndex(OnlineConfigPlayer.PlayerIndex);
             availableLobbyPlayersShower.RemoveAt(0);
             playerConfigs.Add(OnlineConfigPlayer);

         }
     }
    
    //PUNRPC FUNCTIONS ============================================

    [PunRPC]
    public void SetScObPlayerStats(int index, int classIndex) {
        playerConfigs[index].playerStats = playerStats[classIndex];
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
                 if(configs.isReady)
                     return;
                 
                 configs.isReady = true;
                 readyCount++;
                 if (readyCount == playerConfigs.Count)
                 {
                     if(isMasterClient)
                         PhotonNetwork.LoadLevel("SampleScene");
                 }
                 
             }
         }
     }
     
     
    //PUNRPC CALLS=====================================
    public void PunSetPlayerSkin(int index, ScObPlayerCustom playerCustom)
     {
         int skinIndex = playerCustom.SkinIndex;
         int eyesIndex = playerCustom.EyesIndex;
         int tshirtIndex = playerCustom.tshirtIndex;
         int pantsIndex = playerCustom.pantsIndex;
         int shoesIndex = playerCustom.ShoesIndex;
         photonView.RPC("SetPlayerSkin", RpcTarget.All, index, skinIndex, eyesIndex, tshirtIndex, pantsIndex, shoesIndex);
     }
    
    private OnlinePlayerConfiguration HandlePlayerJoined(Player verificacaoDePlayer)
     {
         var config = new OnlinePlayerConfiguration(verificacaoDePlayer);
         return config;
     }
    
    public void PunSetPlayerStats(int index, int classIndex)
    {
        photonView.RPC("SetScObPlayerStats", RpcTarget.All, index, classIndex);
    }
    
    

//     
//

//     
//     public void CancelReadyPlayer(int index)
//     {
//         playerConfigs[index].isReady = false;
//         readyCount--;
//     }
//
//
//     
//     

//
     public List<OnlinePlayerConfiguration> GetPlayerConfigs()
     {
         return playerConfigs;
     }
//
     public void PunReadyPlayer(int index)
     {
         photonView.RPC("ReadyPlayer", RpcTarget.All, index);
     }
     
//     
//     public void PunCancelReadyPlayer(int index)
//     {
//         photonView.RPC("CancelReadyPlayer", RpcTarget.All, index);
//     }
//
//

//  
//
//    
//     }
//         
//     
// 
//
//     
//
}
