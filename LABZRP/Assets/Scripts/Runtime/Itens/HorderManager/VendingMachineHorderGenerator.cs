using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class VendingMachineHorderGenerator : MonoBehaviour
{
    
    private int playersCount = 0;
    [SerializeField] private GameObject VendingMachinePrefab;
    [SerializeField] private int VendingMachinesRespawnHound = 3;
    [SerializeField] private Transform[] VendingMachinesSpawnPoints;
    [SerializeField] private int VendingMachinesPerPlayer = 3; 
    [SerializeField] private Camera mainCamera;
    [SerializeField] private MainGameManager mainGameManager;
    private List<GameObject> spawnedVendingMachines = new List<GameObject>();

    public void addPlayer(GameObject player)
    {
        playersCount++;
    }
    
    public void removePlayer(GameObject player)
    {
        playersCount--;
    }

    public void verifySpawnVendingMachine(int atualHorde)
    {
        if (atualHorde % VendingMachinesRespawnHound == 0 || atualHorde == 1)
        {
            if (spawnedVendingMachines.Count > 0)
            {
                foreach (GameObject vendingMachine in spawnedVendingMachines)
                {
                    Destroy(vendingMachine.gameObject);
                }
            }
            List<Transform> NotVisibleSpawnPoints = new List<Transform>();
            foreach (Transform spawnPoint in VendingMachinesSpawnPoints)
            {
                    if (!IsVisibleByCamera(spawnPoint))
                    {
                        NotVisibleSpawnPoints.Add(spawnPoint);
                    }
            }

            for (int i = 0; i < playersCount * VendingMachinesPerPlayer; i++)
            {

                    int randomSpawnPoint = Random.Range(0, NotVisibleSpawnPoints.Count);
                    GameObject NewVendingMachine = Instantiate(VendingMachinePrefab,
                        NotVisibleSpawnPoints[randomSpawnPoint].transform.position,
                        NotVisibleSpawnPoints[randomSpawnPoint].transform.rotation);
                    spawnedVendingMachines.Add(NewVendingMachine);
                    NotVisibleSpawnPoints.RemoveAt(randomSpawnPoint);
            }
        }
    }
    


    private bool IsVisibleByCamera(Transform target)
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(target.position);
        bool isVisible = screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1 && screenPoint.z > 0;
        return isVisible;
    }
    
    
    public void setIsOnHorderCooldown(bool isOnHorderCooldown)
    {
        foreach (var vendingMachine in spawnedVendingMachines)
        {
            if(vendingMachine != null)
                vendingMachine.GetComponent<VendingMachine>().setIsOnHorderCooldown(isOnHorderCooldown);
        }
    }
    

}
