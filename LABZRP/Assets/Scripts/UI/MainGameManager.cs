using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Runtime.Câmera.MainCamera;
using Runtime.Challenges;
using Runtime.Enemy.HorderMode;
using Runtime.Enemy.ZombieCombat.EnemyStatus;
using Runtime.Enemy.ZombieCombat.ZombieBehaviour;
using Runtime.Player.Combat.Gun;
using Runtime.Player.Combat.PlayerStatus;
using UI;
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
    [SerializeField] private List<AmmoBox> ammoBoxes;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private HordeManager hordeManager;
    [SerializeField] private ChallengeManager challengeManager;
    [SerializeField] private HordeModeGameOverManager gameOverUI;
    [SerializeField] private GameObject playerConfigurationManager;
    [SerializeField] private GameObject onlinePlayerConfigurationManager;
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private PauseMenu pauseMenu;
    [SerializeField] private List<PlayerHeadUiHandler> availablePlayerHeads;
    private List<PlayerHeadUiHandler> _usedPlayerHeads;


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
            var photonViewID = player.GetComponent<PhotonView>().ViewID;
            photonView.RPC("removeDownedPlayerRPC", RpcTarget.MasterClient, photonViewID);
            return;
            
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

    public PlayerHeadUiHandler getPlayerHeadUiHandler()
    {
        _usedPlayerHeads ??= new List<PlayerHeadUiHandler>();
        if (availablePlayerHeads[0] == null)
        {
            Debug.Log("Null");
            return null;
        }
        PlayerHeadUiHandler newPlayerHead = availablePlayerHeads[0];
        availablePlayerHeads.Remove(newPlayerHead);
        _usedPlayerHeads.Add(newPlayerHead);
        return newPlayerHead;
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
        hordeManager.AddPlayer(player);
        player.GetComponent<WeaponSystem>().set_challengeManager(challengeManager);
    }
    
    public void addEnemy(GameObject enemy)
    {
        enemies.Add(enemy);
    }
    
    public void addEnemyOnline(int PhotonViewID)
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
            enemy.GetComponent<EnemyStatus>().KillEnemy();
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
        Debug.Log("StartGameHorde no mainGameManager");
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
            player.GetComponent<PlayerStats>().TakeDamage(100000, false);
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
            enemy.GetComponent<EnemyNavMeshFollow>().setNearPlayerDestination();
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
            enemy.GetComponent<EnemyNavMeshFollow>().setCoffeeMachineEvent(false);
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
                zombie.GetComponent<EnemyStatus>().GameIsOver();
            }
            gameOverUI.gameOver();
        }
    }

    [PunRPC]
    public void updateAmmoBoxPricesRPC()
    {
        foreach (AmmoBox currentAmmoBox in ammoBoxes)
        {
            currentAmmoBox.UpdatePrice();
        }
    }
    public void updateAmmoBoxPrices()
    {
        if(isOnline)
            photonView.RPC("updateAmmoBoxPricesRPC", RpcTarget.All);
        else
        {
            foreach (AmmoBox currentAmmoBox in ammoBoxes)
            {
                currentAmmoBox.UpdatePrice();
            }
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
                zombie.GetComponent<EnemyStatus>().GameIsOver();
            }
        }
        gameOverUI.gameOver();
    }

    public void SetPlayerConfigurationManager(GameObject playerConfigurationManager)
    {
        this.playerConfigurationManager = playerConfigurationManager;
    }
    
    public GameObject getPlayerConfigurationManager()
    {
        if (isOnline)
            return onlinePlayerConfigurationManager;
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
    }



}
