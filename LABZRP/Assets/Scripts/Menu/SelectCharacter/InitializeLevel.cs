using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;


public class InitializeLevel : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform[] playerSpawns;
    [SerializeField] private MainGameManager mainGameManager;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private PhotonView photonView;
    private OnlinePlayerConfigurationManager.OnlinePlayerConfiguration[] pc;
    private List<GameObject> players = new List<GameObject>();
    void Start()
    {
        //Verifica se há um instancia de PlayerConfigurationManager
        if (PlayerConfigurationManager.Instance == null)
        {
            Debug.Log("ONLINE");
            mainGameManager.setIsOnline(true);
            mainGameManager.setOnlinePlayerConfigurationManager(OnlinePlayerConfigurationManager.Instance.gameObject);
            pc = OnlinePlayerConfigurationManager.Instance.GetPlayerConfigs().ToArray();
            if (PhotonNetwork.IsMasterClient)
            {
                for (int i = 0; i < pc.Length; i++)
                {
                    var player = PhotonNetwork.Instantiate("OnlinePlayerPrefab", playerSpawns[i].position,
                        playerSpawns[i].rotation);
                    int photonViewID = player.GetComponent<PhotonView>().ViewID;
                    photonView.RPC("setConfigsToplayer", RpcTarget.All, i, photonViewID);
                }
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
        //Procura o gameobject que possui o photonId
        GameObject player = PhotonView.Find(photonId).gameObject;
        player.GetComponent<PlayerInputHandler>().InitializeOnlinePlayer(pc[index]);
    }
    
    
    
    
    
}
