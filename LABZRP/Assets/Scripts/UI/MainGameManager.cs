using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Random = UnityEngine.Random;

public class MainGameManager : MonoBehaviourPunCallbacks
{
    private bool isMasterClient = false;
    private bool isOnline = false;
    private List<GameObject> players;
    private List<GameObject> alivePlayers;
    private List<GameObject> downedPlayers;
    private List<GameObject> enemies;
    private List<GameObject> itens;
    [SerializeField] private PhotonView photonView;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private HordeManager hordeManager;
    [SerializeField] private ChallengeManager challengeManager;
    [SerializeField] private HordeModeGameOverManager gameOverUI;
    [SerializeField] private GameObject playerConfigurationManager;
    [SerializeField] private GameObject onlinePlayerConfigurationManager;
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private PauseMenu pauseMenu;


    public bool _killAllPlayers = false;
    
    
    
   
    public void Start()
    {
        players = new List<GameObject>();
        alivePlayers = new List<GameObject>();
        downedPlayers = new List<GameObject>();
        enemies = new List<GameObject>();
        itens = new List<GameObject>();
    }

    private void Update()
    {
        if(_killAllPlayers){
            killAllPlayers();
            _killAllPlayers = false;
        }
    }

    public void addItem(GameObject item)
    {
        itens.Add(item);
    }
    public PauseMenu getPauseMenu()
    {
        return pauseMenu;
    }

    public int getPlayersCount()
    {
        return players.Count;
    }
    public int getCountItens()
    {
        return itens.Count;
    }
    [PunRPC]
    public void removeDownedPlayerRPC(int photonViewID)
    {
        GameObject player = PhotonView.Find(photonViewID).gameObject;
        removeDownedPlayer(player);
    }
    public void removeDownedPlayer(GameObject player)
    {
        if (isOnline && !isMasterClient)
        {
            int photonViewID = player.GetComponent<PhotonView>().ViewID;
            photonView.RPC("removeDownedPlayerRPC", RpcTarget.MasterClient, photonViewID);
            
        }
        if (alivePlayers.Contains(player))
        {
            alivePlayers.Remove(player);
            downedPlayers.Add(player);
            if (downedPlayers.Count == players.Count)
            {
                
                StartCoroutine(gameOver());
            }
        }
        
        
    }
    public Camera getMiniMapCamera()
    {
        return minimapCamera;
    }
    
    public void addDownedPlayer(GameObject player)
    {
        alivePlayers.Add(player);
        downedPlayers.Remove(player);
    }
    public void addPlayer(GameObject player)
    {
        players.Add(player);
        alivePlayers.Add(player);
        player.GetComponent<WeaponSystem>().set_challengeManager(challengeManager);
    }
    
    public void addEnemy(GameObject enemy)
    {
        enemies.Add(enemy);
    }
    
    public void addEnemyOnOnline(int PhotonViewID)
    {
        photonView.RPC("addEnemyOnOnlineRPC", RpcTarget.All, PhotonViewID);
    }
    
    [PunRPC]
    public void addEnemyOnOnlineRPC(int PhotonViewID)
    {
        GameObject enemy = PhotonView.Find(PhotonViewID).gameObject;
        enemies.Add(enemy);
    }
    
    public void killAllEnemies()
    {
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<EnemyStatus>().killEnemy();
        }
    }

    public void removeDisconnectedPlayer()
    {
        foreach (GameObject player in players)
        {
            if (player.GetComponent<PhotonView>().IsMine)
            {
                photonView.RPC("removePlayer", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);
                break;
            }
        }
    }

    public void StartGameHorde()
    {
        hordeManager.HordeBreakManagerManualStart();
    }
    [PunRPC]
    public void removePlayer(int photonViewID)
    {
        GameObject player = PhotonView.Find(photonViewID).gameObject;
        if(player == null){
            return;
        }
        players.Remove(player);
        alivePlayers.Remove(player);
        downedPlayers.Remove(player);
    }
    
    public void setIsMasterClient(bool isMasterClient)
    {
        this.isMasterClient = isMasterClient;
    }
    
    public void killAllPlayers()
    {
        foreach (GameObject player in players)
        {
            player.GetComponent<PlayerStats>().takeDamage(100000);
        }
    }

    public GameObject getRandomPlayer()
    {
        int randomPlayer = Random.Range(0, players.Count);
        return players[randomPlayer];
    }
    
    
    public void removeItem(GameObject item)
    {
        itens.Remove(item);
    }
    
    
    public void removeEnemy(GameObject enemy)
    {
        enemies.Remove(enemy);
    }
    
    
    public Camera getMainCamera()
    {
        return mainCamera;
    }
    
    public List<GameObject> getPlayers()
    {
        return players;
    }
    
    public List<GameObject> getAlivePlayers()
    {
        return alivePlayers;
    }
    
    public List<GameObject> getEnemies()
    {
        return enemies;
    }
    
    public List<GameObject> getDownedPlayers()
    {
        return downedPlayers;

    }
    
    public void resetZombiesTarget()
    {
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<EnemyFollow>().setNearPlayerDestination();
        }
    }
    public HordeManager getHordeManager()
    {
        return hordeManager;
    }

    public void cancelCoffeeMachineEvent()
    {
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<EnemyFollow>().setCoffeeMachineEvent(false);
        }
        resetZombiesTarget();
    }
    
    private IEnumerator gameOver()
    {
        yield return new WaitForSeconds(2);
        
        if (isOnline)
        {
            photonView.RPC("showGameOverUI", RpcTarget.All);
        }
        else
        {
            gameOverUI.gameObject.SetActive(true);
            hordeManager.gameOver();
            foreach (var zombie in enemies)
            {
                zombie.GetComponent<EnemyStatus>().gameIsOver();
            }
            gameOverUI.gameOver();
        }
    }

    
    [PunRPC]
    public void showGameOverUI()
    {
        gameOverUI.gameObject.SetActive(true);
        gameOverUI.setIsOnline(true);
        hordeManager.gameOver();
        if (PhotonNetwork.IsMasterClient)
        {
            hordeManager.updateHordeOnline();
            foreach (var zombie in enemies)
            {
                zombie.GetComponent<EnemyStatus>().gameIsOver();
            }
        }
        gameOverUI.gameOver();
    }

    public void setPlayerConfigurationManager(GameObject playerConfigurationManager)
    {
        this.playerConfigurationManager = playerConfigurationManager;
    }
    
    public GameObject getPlayerConfigurationManager()
    {
        if (isOnline)
            return onlinePlayerConfigurationManager;
        else
            return playerConfigurationManager;
        
    }
    
    public void setOnlinePlayerConfigurationManager(GameObject onlinePlayerConfigurationManager)
    {
        this.onlinePlayerConfigurationManager = onlinePlayerConfigurationManager;
    }

    public ChallengeManager getChallengeManager()
    {
        return challengeManager;
    }
    public GameObject getOnlinePlayerConfigurationManager()
    {
        return onlinePlayerConfigurationManager;
    }
    
    public void setIsOnline(bool isOnline)
    {
        this.isOnline = isOnline;
        pauseMenu.setIsOnline(isOnline);
        GameObject mainCameraObject = mainCamera.gameObject;
        mainCameraObject.GetComponent<CameraMovement>().setIsOnline(isOnline);
    }



}
