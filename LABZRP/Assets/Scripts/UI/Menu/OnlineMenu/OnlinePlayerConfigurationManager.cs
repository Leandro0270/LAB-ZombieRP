
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
using UnityEngine.UI;

public class OnlinePlayerConfigurationManager : MonoBehaviourPunCallbacks
{
     private List<int> playersIndexReady = new List<int>();
     private bool isMasterClient = false;
     private string roomCode;
     private string clientPlayerName;
     private int clientPlayerIndex;
     private List<int> AvaiablesPlayersIndex = new List<int>(){1,2,3};
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
     public static OnlinePlayerConfigurationManager Instance;
     [SerializeField] private GameObject disconnectWindow;
     [SerializeField] private Button firstSelectDisconectWindow;
     [SerializeField] private Button quitButton;
     [SerializeField] private Button StartGameButton;
     [SerializeField] private TextMeshProUGUI WaitingForHostText;
     [SerializeField] private TextMeshProUGUI readyCountText;
     private int localPlayerIndex;
     private bool hideClientPanel = false;
     private Player[] playersNaSala;
     private bool isReplay = true;
     private int readyCount = 0;
     private int playersCount = 0;
     private bool gameStarted = false;
     private bool initializedConfigs = false;
     private bool isRestatedLobby = false;
     private int[] playersOnLobbyByActorNumber = new int[3];

     
     
     private void Start()
     {
         if (Instance != null)
         {
             Destroy(gameObject);
             return;
         }
         Instance = this;
         DontDestroyOnLoad(this);
         availableLobbyPlayersShower = lobbyPlayersShower;
         PhotonNetwork.AutomaticallySyncScene = true;
         if (isReplay)
         {
             playersNaSala = PhotonNetwork.CurrentRoom.Players.Values.ToArray();
             isMasterClient = PhotonNetwork.IsMasterClient;
             roomCode = PhotonNetwork.CurrentRoom.Name;
             roomCodeText.text = "Código da sala: " + roomCode;
             if (isMasterClient)
             {
                 roomCodeText.text += "\n Você é o host da sala";
                 //for para adicionar os playersActorNumber na lista excluindo o master client
                    for (int i = 0; i < playersNaSala.Length; i++)
                    {
                        if (playersNaSala[i].ActorNumber != PhotonNetwork.LocalPlayer.ActorNumber)
                        {
                            playersOnLobbyByActorNumber[i-1] = playersNaSala[i].ActorNumber;
                        }
                    }
             }

             clientPlayerName = PhotonNetwork.LocalPlayer.NickName;
             if (PhotonNetwork.LocalPlayer.ActorNumber > playersNaSala.Length && !isMasterClient)
             {
                 photonView.RPC("VerifyPlayerIndexAvailables", RpcTarget.MasterClient);
             }
             else initializeJoinedPlayer();
         }


     }

     public void ResetGame()
     {
         AvaiablesPlayersIndex = new List<int>(){1,2,3};
         playersIndexReady.Clear();
         playerConfigs.Clear();
         availableLobbyPlayersShower = lobbyPlayersShower;
         readyCount = 0;
         playersCount = 0;
     }

     [PunRPC]
     public void UpdateAvaiablesPlayerIndex(int[] avaiablesPlayersIndexArray, int[] playersOnLobbyByActorNumber, int readyCount)
     {
         AvaiablesPlayersIndex = new List<int>(avaiablesPlayersIndexArray);
         this.playersOnLobbyByActorNumber = playersOnLobbyByActorNumber;
         this.readyCount = readyCount;
         if(!initializedConfigs)
             initializeJoinedPlayer();
     }

     [PunRPC]
     public void VerifyPlayerIndexAvailables()
     {
         //Organiza a lista de availablesPlayersIndex em ordem crescente
         AvaiablesPlayersIndex.Sort();

         // Converta a lista para um array antes de enviar
         int[] avaiablesPlayersIndexArray = AvaiablesPlayersIndex.ToArray();
    
         photonView.RPC("UpdateAvaiablesPlayerIndex", RpcTarget.All, avaiablesPlayersIndexArray, playersOnLobbyByActorNumber,readyCount);
     }
     public override void OnJoinedRoom(){ 
         if(initializedConfigs)
             return;
         isReplay = false;
         playersNaSala = PhotonNetwork.CurrentRoom.Players.Values.ToArray();
         playersCount = playersNaSala.Length;
         isMasterClient = PhotonNetwork.IsMasterClient;
         roomCode = PhotonNetwork.CurrentRoom.Name;
         roomCodeText.text = "Código da sala: " + roomCode;
         if(isMasterClient)
             roomCodeText.text += "\n Você é o host da sala";
         clientPlayerName = PhotonNetwork.LocalPlayer.NickName;
         if (PhotonNetwork.LocalPlayer.ActorNumber > playersNaSala.Length && !isMasterClient)
         {
             photonView.RPC("VerifyPlayerIndexAvailables", RpcTarget.MasterClient);
         }
         else initializeJoinedPlayer();
     }

     public void initializeJoinedPlayer()
     {
         initializedConfigs = true;
         int newPlayerIndex = AvaiablesPlayersIndex[0];
         if(!isMasterClient)
            AvaiablesPlayersIndex.RemoveAt(0);
         playersNaSala = PhotonNetwork.CurrentRoom.Players.Values.ToArray();
        isMasterClient = PhotonNetwork.IsMasterClient;
        clientPlayerIndex = newPlayerIndex;
        if (isMasterClient && !isReplay)
        {
            OnlinePlayerConfiguration MasterLocalPlayer = HandlePlayerJoined(playersNaSala[0]);
            clientPlayerIndex = MasterLocalPlayer.PlayerIndex;
            clientPlayerName = MasterLocalPlayer.playerName;
            ClientPlayerSetupMenu.SetPlayerIndex(clientPlayerIndex, clientPlayerName);
            playerConfigs.Add(MasterLocalPlayer);
        }
        else
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
        readyCountText.text = readyCount + "/" + playersCount + " Ready";
     }


     public void showDisconnectWindow()
     {
         disconnectWindow.SetActive(true);
         firstSelectDisconectWindow.Select();
         quitButton.gameObject.SetActive(false);

         //if clientpanel is active, disable it
         if (clientPanel.activeSelf)
         {
             hideClientPanel = true;
             clientPanel.SetActive(false);
         }
         else
                lobbyPanel.SetActive(false);
     }
     
     public void hideDisconnectWindow()
     {
         disconnectWindow.SetActive(false);
         quitButton.gameObject.SetActive(true);
         quitButton.Select();
         if (hideClientPanel)
             clientPanel.SetActive(true);
         else
             lobbyPanel.SetActive(true);
            

     }


     public override void OnDisconnected(DisconnectCause cause)
     {
         Debug.Log(cause);
         SceneManager.LoadScene("MainMenu");
         Destroy(gameObject);

     }
     public override void OnLeftRoom()
     {
         // find gameobjct with tag GameManager
         GameObject gameManager = GameObject.FindGameObjectWithTag("GameManager");
         if(gameManager != null)
            gameManager.GetComponent<MainGameManager>().removeDisconnectedPlayer();
         PhotonNetwork.Disconnect();
     }
     public void DisconnectButton()
     {
         PhotonNetwork.LeaveRoom();
     }
    
     
     public override void OnPlayerLeftRoom(Player otherPlayer)
     {
         OnlinePlayerConfiguration playerToRemove = playerConfigs.Find(config => Equals(config.player, otherPlayer));
             if (playerToRemove != null)
             {
                 if (playerToRemove.isMasterClient)
                 {
                     Debug.Log("Master client se desconectou, retornando ao menu");
                     PhotonNetwork.LeaveRoom();
                     PhotonNetwork.Disconnect();
                     SceneManager.LoadScene("MainMenu");
                     Destroy(gameObject);
                 }
                 Debug.Log("O player de playerActorNumber " + playerToRemove.player.ActorNumber + " se desconectou");
                 availableLobbyPlayersShower.Add(playerToRemove.lobbyPlayersShower);
                 playerToRemove.lobbyPlayersShower.removePlayer();
                 playersCount--;
                 if(playerToRemove.isReady)
                     readyCount--;
                 readyCountText.text = readyCount + "/" + playersCount + " Ready";
                 if (isMasterClient && playerConfigs.Count == 1)
                 {
                     StartGameButton.interactable = false;
                 }
                 AvaiablesPlayersIndex.Add(playerToRemove.PlayerIndex);
                 AvaiablesPlayersIndex.Sort();
                 playersOnLobbyByActorNumber[playerToRemove.PlayerIndex] = -1;
                 playerConfigs.Remove(playerToRemove);
             }
     }
     
     public void cancelReadyPlayer(int playerIndex)
     {
         photonView.RPC("cancelReadyPlayer", RpcTarget.All, playerIndex);
     }
     
     
     [PunRPC]
     public void cancelReadyPlayerRPC(int playerIndex)
     {
         readyCount--;
         playersIndexReady.Remove(playerIndex);
         readyCountText.text = readyCount + "/" + playersCount + " Ready";
         if (!playerConfigs[playerIndex].player.IsLocal)
         {
             playerConfigs[playerIndex].isReady = false;
             playerConfigs[playerIndex].lobbyPlayersShower.setIsReady(false);
         }
         else
         {
             ClientPlayerSetupMenu.OnCancel();
             ClientPlayerSetupMenu.transform.SetParent(clientPanel.transform);
             lobbyPanel.SetActive(false);
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
         playersOnLobbyByActorNumber[AvaiablesPlayersIndex[0] - 1] = newPlayer.ActorNumber;
         playersCount++;
         readyCountText.text = readyCount + "/" + playersCount + " Ready";
         OnlinePlayerConfiguration OnlineConfigPlayer = HandlePlayerJoined(newPlayer);
         if (OnlineConfigPlayer != null)
         {
             AvaiablesPlayersIndex.RemoveAt(0);
             OnlineConfigPlayer.lobbyPlayersShower = availableLobbyPlayersShower[0];
             lobbyPlayersShower[0].setPlayerIndex(OnlineConfigPlayer.PlayerIndex);
             availableLobbyPlayersShower.RemoveAt(0);
             playerConfigs.Add(OnlineConfigPlayer);
             playerConfigs = playerConfigs.OrderBy(playerConfig => playerConfig.PlayerIndex).ToList();


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
                     if (isMasterClient)
                     {
                         StartGameButton.gameObject.SetActive(true);
                     }
                     else
                     {
                         WaitingForHostText.gameObject.SetActive(true);
                         WaitingForHostText.text = "Esperando os outros jogadores";
                     }
                 }else
                 {
                     configs.lobbyPlayersShower.setIsReady(true);   
                 }
                 if(configs.isReady)
                     return;
                 
                 configs.isReady = true;
                 readyCount++;
                 playersIndexReady.Add(index);
                 readyCountText.text = readyCount + "/" + playersCount + " Ready";
                 if (readyCount == playerConfigs.Count)
                 {
                     if (isMasterClient)
                     {
                         if(playerConfigs.Count == 1)
                             return;
                         StartGameButton.interactable = true;
                         StartGameButton.Select();
                     }
                     else
                     {
                         WaitingForHostText.text = "Esperando o host iniciar o jogo";
                     }
                 }
             }
         }
     }

     public void LoadLevel()
     {
         PhotonNetwork.LoadLevel("SampleScene");
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
         OnlinePlayerConfiguration config = null;
         if (verificacaoDePlayer.IsMasterClient)
             config = new OnlinePlayerConfiguration(verificacaoDePlayer, 0);
         else if (verificacaoDePlayer.IsLocal)
             config = new OnlinePlayerConfiguration(verificacaoDePlayer, clientPlayerIndex);
         else
         {
             for (int i = 0; i < playersOnLobbyByActorNumber.Length; i++)
             {
                 if(playersOnLobbyByActorNumber[i] == verificacaoDePlayer.ActorNumber){
                     config = new OnlinePlayerConfiguration(verificacaoDePlayer, AvaiablesPlayersIndex[i]);
                     break;
                 }
             }
             
         }
         
             
         
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

}
