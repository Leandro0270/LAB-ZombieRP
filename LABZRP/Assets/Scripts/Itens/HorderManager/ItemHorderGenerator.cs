using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class ItemHorderGenerator : MonoBehaviour
{
    public GameObject item;
    public int playersCount = 0;
    public List<ScObItem> specsItems;
    private GameObject[] SpawnPoints;

    private void Start()
    {
        SpawnPoints = GameObject.FindGameObjectsWithTag("ItemSpawnPoints");
    }

    public void addPlayer(GameObject player)
    {
        playersCount++;
    }
    
    public void removePlayer(GameObject player)
    {
        playersCount--;
    }
    
    public void GenerateItem()
    {
        if (playersCount > 0)
        {
            for (int i = 0; i < playersCount; i++)
            {
                int randomSpawn = Random.Range(0, SpawnPoints.Length);
                int randomItens = Random.Range(0, specsItems.Count);
                Debug.Log("Spawnou");
                GameObject SpawnItem = Instantiate(item, SpawnPoints[randomSpawn].transform.position, SpawnPoints[randomSpawn].transform.rotation);
                SpawnItem.GetComponent<Item>().setItem(specsItems[randomItens]);
            }
       
        }
    }
}
