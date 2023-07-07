using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;


public class InitializeLevel : MonoBehaviourPunCallbacks
{
    private OnlinePlayerConfigurationManager onlinePlayerConfigurationManager;
    [SerializeField] private Transform[] playerSpawns;
    [SerializeField] private MainGameManager mainGameManager;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private PhotonView photonView;
    private List<OnlinePlayerConfiguration> pc;
    private List<GameObject> players = new List<GameObject>();
    int photonViewID;
    private int playersReady = 1;

    private void Awake()
    {
        onlinePlayerConfigurationManager = GameObject.FindGameObjectWithTag("OnlinePlayerConfigurationManager").GetComponent<OnlinePlayerConfigurationManager>();
    }

    void Start()
    {
        //Verifica se há um instancia de PlayerConfigurationManager
        if (PlayerConfigurationManager.Instance == null)
        {
            Debug.Log("ONLINE");
            mainGameManager.setIsOnline(true);
            mainGameManager.setOnlinePlayerConfigurationManager(onlinePlayerConfigurationManager.gameObject);
            pc = onlinePlayerConfigurationManager.GetPlayerConfigs();
            mainGameManager.getHordeManager().setIsOnline(true);
            if (!PhotonNetwork.LocalPlayer.IsMasterClient)
            {
                photonView.RPC("sceneLoaded", RpcTarget.MasterClient);
            }
            else
            {
                photonView.RPC("sceneLoaded", RpcTarget.MasterClient);
                mainGameManager.setIsMasterClient(true);
                mainGameManager.getHordeManager().setSpecialEvent(true);
                mainGameManager.getHordeManager().setIsMasterClient(true);

            }
        }
        else
        {
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
        players.Add(player);
        player.GetComponent<PlayerInputHandler>().InitializeOnlinePlayer(pc[index]);
    }

    [PunRPC]
    public void instantiatePlayer(int playerIndex)
    {
        Debug.Log("Chamou para o player" + playerIndex);
        GameObject player = PhotonNetwork.InstantiateSceneObject("OnlinePlayerPrefab", playerSpawns[playerIndex].position,
            playerSpawns[playerIndex].rotation);
        photonViewID = player.GetComponent<PhotonView>().ViewID;
        photonView.RPC("setConfigsToplayer", RpcTarget.All, playerIndex, photonViewID);
    }

    [PunRPC]
    public void sceneLoaded()
    {
        playersReady++;
        if (playersReady == pc.Count)
        {
            for(int i = 0; i < pc.Count; i++)
                instantiatePlayer(i);
        }
    }
    
    
    
    
    
    
    
    
    
    
    
}
