using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MainGameManager : MonoBehaviour
{
   
    private List<GameObject> players;
    private List<GameObject> alivePlayers;
    private List<GameObject> downedPlayers;
    private List<GameObject> enemies;
    private List<GameObject> itens;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private HordeManager hordeManager;
    [SerializeField] private ChallengeManager challengeManager;
    [SerializeField] private HordeModeGameOverManager gameOverUI;
    [SerializeField] private GameObject playerConfigurationManager;


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


    public int getCountItens()
    {
        return itens.Count;
    }
    
    public void removeDownedPlayer(GameObject player)
    {
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
    
    public void killAllEnemies()
    {
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<EnemyStatus>().killEnemy();
        }
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
        gameOverUI.gameObject.SetActive(true);
        hordeManager.gameOver();
        foreach (var zombie in enemies)
        {
            zombie.GetComponent<EnemyStatus>().gameIsOver();
        }
        gameOverUI.gameOver();
    }
    
    public void setPlayerConfigurationManager(GameObject playerConfigurationManager)
    {
        this.playerConfigurationManager = playerConfigurationManager;
    }
    
    public GameObject getPlayerConfigurationManager()
    {
        return playerConfigurationManager;
    }
    
}
