using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MainGameManager : MonoBehaviour
{
    private List<GameObject> players;
    private List<GameObject> enemies;
    private List<GameObject> itens;
    
    
    
    
    public void Start()
    {
        players = new List<GameObject>();
        enemies = new List<GameObject>();
        itens = new List<GameObject>();
    }
    
    public void addItem(GameObject item)
    {
        itens.Add(item);
    }

    public Boolean verifyItemDistance(Transform Spawnpoint)
    {
        //for each de todos os itens verificando a distancia do parametro de transform passado e caso não esteja no raio de 5 metros retorna true
        foreach (GameObject item in itens)
        {
            if (Vector3.Distance(item.transform.position, Spawnpoint.position) > 5)
            {
                return true;
            }
        }

        return false;
    }


    public int getCountItens()
    {
        return itens.Count;
    }
    public void addPlayer(GameObject player)
    {
        players.Add(player);
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
}
