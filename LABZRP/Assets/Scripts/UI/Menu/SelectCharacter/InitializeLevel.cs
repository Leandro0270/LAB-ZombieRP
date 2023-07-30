using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializeLevel : MonoBehaviourPunCallbacks
{
    private OnlinePlayerConfigurationManager onlinePlayerConfigurationManager;
    [SerializeField] private GameObject waitingForPlayersPanel;
    [SerializeField] private Transform[] playerSpawns;
    [SerializeField] private MainGameManager mainGameManager;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private PhotonView photonView;
    private int[] playersOnLobbyByActorNumber;
    private List<OnlinePlayerConfiguration> pc;
    private List<GameObject> players = new List<GameObject>();
    int photonViewID;
    private int playersReady = 1;
    private bool startedGame = false;

    private void Awake()
    {
        GameObject gameObjectonlinePlayerConfigurationManager = GameObject.FindGameObjectWithTag("OnlinePlayerConfigurationManager");
        if (gameObjectonlinePlayerConfigurationManager != null)
        {
            onlinePlayerConfigurationManager = gameObjectonlinePlayerConfigurationManager.GetComponent<OnlinePlayerConfigurationManager>();
        }
        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    private void Update()
    {
        if (startedGame && waitingForPlayersPanel.activeSelf)
        {
            waitingForPlayersPanel.SetActive(false);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Verifica se há um instancia de PlayerConfigurationManager
        if (PlayerConfigurationManager.Instance == null)
        {
            Debug.Log("ONLINE");
            mainGameManager.setIsOnline(true);
            mainGameManager.setOnlinePlayerConfigurationManager(onlinePlayerConfigurationManager.gameObject);
            pc = onlinePlayerConfigurationManager.GetPlayerConfigs();
            mainGameManager.getHordeManager().setIsOnline(true);
            mainGameManager.getChallengeManager().setIsOnline(true);
            playersOnLobbyByActorNumber = onlinePlayerConfigurationManager.getPlayersOnLobbyByActorNumber();
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                photonView.RPC("sceneLoaded", RpcTarget.MasterClient);
            }
            else
            {
                photonView.RPC("sceneLoaded", RpcTarget.MasterClient);
                mainGameManager.setIsMasterClient(true);
                mainGameManager.getHordeManager().setIsMasterClient(true);

            }
        }
        else
        {
            waitingForPlayersPanel.SetActive(false);
            Debug.Log("LOCAL");
            var playerConfigs = PlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();
            for (int i = 0; i < playerConfigs.Length; i++)
            {
                var player = Instantiate(playerPrefab, playerSpawns[i].position, playerSpawns[i].rotation,
                    gameObject.transform);
                player.GetComponent<PlayerInputHandler>().InitializePlayer(playerConfigs[i]);
            }
            mainGameManager.setPlayerConfigurationManager(PlayerConfigurationManager.Instance.gameObject);

        }
    }


    [PunRPC]
    public void setConfigsToplayer(int index, int photonId)
    {
        GameObject player = PhotonView.Find(photonId).gameObject;
        mainGameManager.addPlayer(player);
        players.Add(player);
        player.GetComponent<PlayerInputHandler>().InitializeOnlinePlayer(pc[index]);
    }

    [PunRPC]
    public void instantiatePlayer()
    {
        startedGame = true;
        int playerindex = -1;

        foreach (OnlinePlayerConfiguration NewPlayerConfiguration in pc)
        {
            if (NewPlayerConfiguration.player == PhotonNetwork.LocalPlayer)
            {
                playerindex = NewPlayerConfiguration.PlayerIndex;
                break;
            }
        }
        

        Debug.Log("Chamou para o player" + (playerindex));
        GameObject player = PhotonNetwork.Instantiate("OnlinePlayerPrefab", playerSpawns[playerindex].position,
            playerSpawns[playerindex].rotation);
        photonViewID = player.GetComponent<PhotonView>().ViewID;
        photonView.RPC("setConfigsToplayer", RpcTarget.All, playerindex, photonViewID);
    }

    [PunRPC]
    public void sceneLoaded()
    {
        playersReady++;
        if (playersReady == pc.Count)
        {
            photonView.RPC("instantiatePlayer", RpcTarget.All);
        }
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    
    
    
    
    
    
    
    
    
    
}
