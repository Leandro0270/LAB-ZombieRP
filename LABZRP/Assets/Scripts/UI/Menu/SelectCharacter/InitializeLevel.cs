using System;
using System.Collections.Generic;
using Photon.Pun;
using Runtime.Player.Inputs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Menu.SelectCharacter
{
    public class InitializeLevel : MonoBehaviourPunCallbacks
    {
        private OnlinePlayerConfigurationManager _onlinePlayerConfigurationManager;
        private PlayerConfigurationManager _playerConfigurationManager;
        [SerializeField] private GameObject waitingForPlayersPanel;
        [SerializeField] private Transform[] playerSpawns;
        [SerializeField] private MainGameManager mainGameManager;
        [SerializeField] private GameObject playerPrefab;
        private List<OnlinePlayerConfiguration> _onlinePlayerConfigs = new List<OnlinePlayerConfiguration>();
        private List<PlayerConfiguration> _localPlayerConfigs = new List<PlayerConfiguration>();
        private List<PlayerInputHandler> _players = new List<PlayerInputHandler>();
        private int _photonViewID;
        private int _playerReady = 0;
        private bool _isOnline;
        //private bool _startedGame;

        private void Awake()
        {
            if (PhotonNetwork.IsConnected)
            {        
                GameObject gameObjectOnlinePlayerConfigurationManager = GameObject.Find("OnlinePlayerConfigurationManager");
                _onlinePlayerConfigurationManager = gameObjectOnlinePlayerConfigurationManager.GetComponent<OnlinePlayerConfigurationManager>();
                _onlinePlayerConfigs = _onlinePlayerConfigurationManager.GetPlayerConfigs();
                _isOnline = true;
            }
            else
            {
                GameObject gameObjectPlayerConfigurationManager = GameObject.Find("PlayerConfigurationManager");
                _playerConfigurationManager = gameObjectPlayerConfigurationManager.GetComponent<PlayerConfigurationManager>();
                _localPlayerConfigs = _playerConfigurationManager.GetPlayerConfigs();
            }
            SceneManager.sceneLoaded += OnSceneLoaded;


        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_isOnline)
            {
                mainGameManager.setIsOnline(true);
                mainGameManager.setOnlinePlayerConfigurationManager(_onlinePlayerConfigurationManager.gameObject);
                _onlinePlayerConfigs = _onlinePlayerConfigurationManager.GetPlayerConfigs();
                mainGameManager.getHordeManager().SetIsOnline(true);
                mainGameManager.getChallengeManager().setIsOnline(true);
                photonView.RPC("SceneLoaded", RpcTarget.MasterClient);
            }
            else
            {
                waitingForPlayersPanel.SetActive(false);
                mainGameManager.SetPlayerConfigurationManager(_playerConfigurationManager.gameObject);
                Debug.Log(_localPlayerConfigs.Count);
                for (int index = 0; index < _localPlayerConfigs.Count; index++)
                {
                    Debug.Log(index);
                    GameObject player = Instantiate(playerPrefab, playerSpawns[index].position,
                        playerSpawns[index].rotation,
                        gameObject.transform);
                    PlayerInputHandler playerInputHandler = player.GetComponent<PlayerInputHandler>();
                    _players.Add(playerInputHandler);
                    playerInputHandler.InitializePlayer(_localPlayerConfigs[index]);
                }
            }
        }


        [PunRPC]
        public void SetConfigsToPlayer(int index, int photonId)
        {
            GameObject player = PhotonView.Find(photonId).gameObject;
            PlayerInputHandler playerInputHandler = player.GetComponent<PlayerInputHandler>();
            _players.Add(playerInputHandler);
            playerInputHandler.InitializeOnlinePlayer(_onlinePlayerConfigs[index]);
        }

        [PunRPC]
        public void instantiatePlayer()
        {
            //_startedGame = true;
            int playerindex = -1;

            foreach (OnlinePlayerConfiguration NewPlayerConfiguration in _onlinePlayerConfigs)
            {
                if (NewPlayerConfiguration.player == PhotonNetwork.LocalPlayer)
                {
                    playerindex = NewPlayerConfiguration.PlayerIndex;
                    break;
                }
            }

            GameObject player = PhotonNetwork.Instantiate("OnlinePlayerPrefab", playerSpawns[playerindex].position,
                playerSpawns[playerindex].rotation);
            _photonViewID = player.GetComponent<PhotonView>().ViewID;
            photonView.RPC("SetConfigsToPlayer", RpcTarget.All, playerindex, _photonViewID);
        }

        [PunRPC]
        public void SceneLoaded()
        {
            mainGameManager.setIsMasterClient(true);
            mainGameManager.getHordeManager().setIsMasterClient(true);
            _playerReady++;
            if (_playerReady != _onlinePlayerConfigs.Count) return;
        
            mainGameManager.StartGameHorde();
            photonView.RPC("instantiatePlayer", RpcTarget.All);
        }
    
        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    
    
    
    
    
    
    
    
    
    
    
    }
}
