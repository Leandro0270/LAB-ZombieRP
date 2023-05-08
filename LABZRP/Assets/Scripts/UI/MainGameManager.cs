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
    [SerializeField] private Camera miniMapCamera;
    [SerializeField] private HordeManager hordeManager;
    [SerializeField] private ChallengeManager challengeManager;
    
    
    
    
    public void Start()
    {
        players = new List<GameObject>();
        alivePlayers = new List<GameObject>();
        downedPlayers = new List<GameObject>();
        enemies = new List<GameObject>();
        itens = new List<GameObject>();
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
        alivePlayers.Remove(player);
        downedPlayers.Add(player);
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

    public void cancelCoffeeMachineEvent()
    {
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<EnemyFollow>().setCoffeeMachineEvent(false);
        }
        resetZombiesTarget();
    }

    public Camera getMiniMapCamera()
    {
        return miniMapCamera;
    }
    
}
