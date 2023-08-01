
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
     [SerializeField] private GameObject mainCanvas;
     private List<int> AvailablesPlayersIndex = new List<int>(){1,2,3};
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
     [SerializeField] private GameObject LoadPanel;
     private int localPlayerIndex;
     private bool hideClientPanel = false;
     private Player[] playersNaSala;
     private bool isReplay = true;
     private int readyCount = 0;
     private int playersCount = 0;
     private bool gameStarted = false;
     private bool initializedConfigs = false;
     private bool isRestatedLobby = false;
     private int[] playersOnLobbyByActorNumber = new int[4];

     
     
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
             playersCount = PhotonNetwork.CurrentRoom.PlayerCount;
             roomCode = PhotonNetwork.CurrentRoom.Name;
             roomCodeText.text = "Código da sala: " + roomCode;
             if (isMasterClient)
             {
                 roomCodeText.text += "\n Você é o host da sala";
                 int[] availablesPlayersIndexArray = AvailablesPlayersIndex.ToArray();
                 initializeJoinedPlayer();
                 Array.Sort(playersNaSala, (x, y) => x.ActorNumber.CompareTo(y.ActorNumber));
                 
                 for(int i=0 ; i < playersNaSala.Length; i++)
                 {
                     playersOnLobbyByActorNumber[i] = playersNaSala[i].ActorNumber;
                 }
                 int[]playersOnLobbyByActorNumberArray = playersOnLobbyByActorNumber.ToArray();
                 photonView.RPC("UpdateAvailablesPlayerIndex", RpcTarget.All, availablesPlayersIndexArray, playersOnLobbyByActorNumberArray, readyCount);
                 
             }
         }

     }
     

     [PunRPC]
     public void UpdateAvailablesPlayerIndex(int[] availablesPlayersIndexArray, int[] playersOnLobbyByActorNumber)
     {
         AvailablesPlayersIndex = new List<int>(availablesPlayersIndexArray);
         this.playersOnLobbyByActorNumber = playersOnLobbyByActorNumber;
         if (isReplay)
         {
             for(int i = 0; i< playersNaSala.Length; i++)
             {
                 if (playersOnLobbyByActorNumber[i] != PhotonNetwork.LocalPlayer.ActorNumber)
                 {
                     AvailablesPlayersIndex[i] = -1;
                 }
             }
         }
         if(!initializedConfigs)
             initializeJoinedPlayer();
     }
     
     
     [PunRPC]
     public void VerifyPlayerIndexAvailables()
     {
         //Organiza a lista de availablesPlayersIndex em ordem crescente
         AvailablesPlayersIndex.Sort();

         // Converta a lista para um array antes de enviar
         int[] avaiablesPlayersIndexArray = AvailablesPlayersIndex.ToArray();
    
         photonView.RPC("UpdateAvailablesPlayerIndex", RpcTarget.All, avaiablesPlayersIndexArray, playersOnLobbyByActorNumber,readyCount);
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
         if (isMasterClient)
             initializeJoinedPlayer();
         
         
         
     }

     public void initializeJoinedPlayer()
     {
         initializedConfigs = true;
         int newPlayerIndex = -1;
         if (!isMasterClient)
         {
             foreach (int initializedPlayerIndex in AvailablesPlayersIndex)
             {
                 if (initializedPlayerIndex != -1)
                 {
                     newPlayerIndex = initializedPlayerIndex;
                     break;
                 }
             }

             if (newPlayerIndex != -1)
             {
                 for(int i = 0; i < playersOnLobbyByActorNumber.Length; i++)
                 {
                     if (AvailablesPlayersIndex[i] == newPlayerIndex)
                     {
                         AvailablesPlayersIndex[i] = -1;
                         break;
                     }
                 }
             }
         }
         else{
             newPlayerIndex = 0;
             playersOnLobbyByActorNumber[0] = 0;
         }

         playersNaSala = PhotonNetwork.CurrentRoom.Players.Values.ToArray();
         isMasterClient = PhotonNetwork.IsMasterClient;
         if(!isMasterClient)
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
                 if (OnlineConfigPlayer != null)
                 {
                     if (OnlineConfigPlayer.isLocal == false)
                     {
                         OnlineConfigPlayer.lobbyPlayersShower = availableLobbyPlayersShower[0];
                         lobbyPlayersShower[0].setPlayerIndex(OnlineConfigPlayer.PlayerIndex, verificacaoDePlayer.NickName);
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
                 else
                 {
                     Debug.Log("Error! OnlineCOnfigPlayer is null");
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
                 AvailablesPlayersIndex[playerToRemove.PlayerIndex] = playerToRemove.PlayerIndex;
                 AvailablesPlayersIndex.Sort();
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
        playersNaSala.Append(newPlayer);
         int newPlayerIndex = 0;
         foreach (int indexPlayer in AvailablesPlayersIndex)
         {
             if(indexPlayer != -1)
             {
                 Debug.Log("Entrou no if");
                 Debug.Log(newPlayer.ActorNumber + " vai entrar no index " + indexPlayer);
                 newPlayerIndex = indexPlayer;
                 break;
             }
         }
         playersOnLobbyByActorNumber[newPlayerIndex] = newPlayer.ActorNumber;
         playersCount++;
         readyCountText.text = readyCount + "/" + playersCount + " Ready";
         if (isMasterClient)
         {
             AvailablesPlayersIndex.Sort();
             int[] avaiablesPlayersIndexArray = AvailablesPlayersIndex.ToArray();
             photonView.RPC("UpdateAvailablesPlayerIndex", RpcTarget.All, avaiablesPlayersIndexArray,
                 playersOnLobbyByActorNumber);
             
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

         OnlinePlayerConfiguration OnlineConfigPlayer = HandlePlayerJoined(newPlayer);
         if (OnlineConfigPlayer != null)
         {
             for(int i = 0; i < AvailablesPlayersIndex.Count; i++)
             {
                 if(AvailablesPlayersIndex[i] == newPlayerIndex)
                 {
                     AvailablesPlayersIndex[i] = -1;
                     break;
                 }
             }
             OnlineConfigPlayer.lobbyPlayersShower = availableLobbyPlayersShower[0];
             lobbyPlayersShower[0].setPlayerIndex(OnlineConfigPlayer.PlayerIndex, newPlayer.NickName);
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

     [PunRPC]
     public void LoadingForAllPlayers()
     {
         mainCanvas.SetActive(false);
         LoadPanel.SetActive(true);
     }
     public void LoadLevel()
     {
         photonView.RPC("LoadingForAllPlayers", RpcTarget.All);
         PhotonNetwork.LoadLevel("SampleScene");
         StartGameButton.interactable = false;
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
         {
             Debug.Log("Adicionou o masterClient");
             config = new OnlinePlayerConfiguration(verificacaoDePlayer, 0);
         }
         else if(verificacaoDePlayer.ActorNumber == PhotonNetwork.LocalPlayer.ActorNumber)
         {
             config = new OnlinePlayerConfiguration(verificacaoDePlayer, clientPlayerIndex);
         }
         else
         {
             Debug.Log("Não é o masterClient nem o local player");
             for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
             { 
                 if(playersOnLobbyByActorNumber[i] == verificacaoDePlayer.ActorNumber)
                 {
                     config = new OnlinePlayerConfiguration(verificacaoDePlayer, i);
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

    public int[] getPlayersOnLobbyByActorNumber()
    {
        return playersOnLobbyByActorNumber;
    }
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
