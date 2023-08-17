using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Runtime.Player.Inputs;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializeLevel : MonoBehaviourPunCallbacks
{
    private OnlinePlayerConfigurationManager onlinePlayerConfigurationManager;
    [SerializeField] private GameObject waitingForPlayersPanel;
    [SerializeField] private Transform[] playerSpawns;
    [SerializeField] private MainGameManager mainGameManager;
    [SerializeField] private GameObject playerPrefab;
    private List<OnlinePlayerConfiguration> pc;
    private List<GameObject> players = new List<GameObject>();
    int photonViewID;
    private int playersReady = 1;
    private bool startedGame;

    private void Awake()
    {
        GameObject gameObjectonlinePlayerConfigurationManager = GameObject.Find("OnlinePlayerConfigurationManager");
        if (gameObjectonlinePlayerConfigurationManager != null)
        {
            onlinePlayerConfigurationManager = gameObjectonlinePlayerConfigurationManager.GetComponent<OnlinePlayerConfigurationManager>();
        }
        SceneManager.sceneLoaded += OnSceneLoaded;

    }

    private void Update()
    {
        if ((startedGame && waitingForPlayersPanel.activeSelf) || !PhotonNetwork.IsConnected)
            waitingForPlayersPanel.SetActive(false);
        
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Verifica se há um instancia de PlayerConfigurationManager
        if (PlayerConfigurationManager.Instance == null)
        {
            mainGameManager.setIsOnline(true);
            mainGameManager.setOnlinePlayerConfigurationManager(onlinePlayerConfigurationManager.gameObject);
            pc = onlinePlayerConfigurationManager.GetPlayerConfigs();
            mainGameManager.getHordeManager().setIsOnline(true);
            mainGameManager.getChallengeManager().setIsOnline(true);
            photonView.RPC("SceneLoaded", RpcTarget.MasterClient);
        }
        else
        {
            waitingForPlayersPanel.SetActive(false);
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

        GameObject player = PhotonNetwork.Instantiate("OnlinePlayerPrefab", playerSpawns[playerindex].position,
            playerSpawns[playerindex].rotation);
        photonViewID = player.GetComponent<PhotonView>().ViewID;
        photonView.RPC("setConfigsToplayer", RpcTarget.All, playerindex, photonViewID);
    }

    [PunRPC]
    public void SceneLoaded()
    {
        mainGameManager.setIsMasterClient(true);
        mainGameManager.getHordeManager().setIsMasterClient(true);
        Debug.Log("SceneLoaded");
        playersReady++;
        if (playersReady == pc.Count)
        {
            Debug.Log("mainGameManager é nulo? "+(mainGameManager == null));
            mainGameManager.StartGameHorde();
            Debug.Log("Players todos ready" + (playersReady == pc.Count));
            photonView.RPC("instantiatePlayer", RpcTarget.All);
        }
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    
    
    
    
    
    
    
    
    
    
}
