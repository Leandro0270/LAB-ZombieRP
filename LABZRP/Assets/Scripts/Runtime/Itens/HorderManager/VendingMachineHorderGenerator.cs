using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Photon.Pun;
using UnityEngine.Serialization;

public class VendingMachineHorderGenerator : MonoBehaviourPunCallbacks
{
    
    private int playersCount = 0;
    [SerializeField] private GameObject VendingMachinePrefab;
    [FormerlySerializedAs("VendingMachinesRespawnHound")] [SerializeField] private int VendingMachinesRespawnRound = 3;
    [SerializeField] private Transform[] VendingMachinesSpawnPoints;
    [SerializeField] private int VendingMachinesPerPlayer = 2; 
    [SerializeField] private Camera mainCamera;
    [SerializeField] private MainGameManager mainGameManager;
    private int maxVendingMachines = 1;
    private List<GameObject> spawnedVendingMachines = new List<GameObject>();
    private bool isOnline = false;
    private int currentVendingMachineToSpawn = 0;
    private bool isMasterClient = false;


    private void Start()
    {
        maxVendingMachines = VendingMachinesSpawnPoints.Length;
    }

    public void addPlayer(GameObject player)
    {
        playersCount++;
        currentVendingMachineToSpawn += VendingMachinesPerPlayer;
        if(currentVendingMachineToSpawn > maxVendingMachines)
            currentVendingMachineToSpawn = maxVendingMachines;
    }
    
    public void setIsOnline(bool isOnline)
    {
        this.isOnline = isOnline;
    }
    
    public void setIsMasterClient(bool isMasterClient)
    {
        this.isMasterClient = isMasterClient;
    }
    
    public void removePlayer(GameObject player)
    {
        playersCount--;
    }

    public void verifySpawnVendingMachine(int atualHorde)
    {
        if (!isOnline || PhotonNetwork.IsMasterClient)
        {

            if (atualHorde % VendingMachinesRespawnRound == 0 || atualHorde == 1)
            {
                if (spawnedVendingMachines.Count > 0)
                {
                    foreach (GameObject vendingMachine in spawnedVendingMachines)
                    {
                        if (isOnline)
                            PhotonNetwork.Destroy(vendingMachine.gameObject);
                        
                        else
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
                
                for (int i = 0; i < currentVendingMachineToSpawn; i++)
                {

                    int randomSpawnPoint = Random.Range(0, NotVisibleSpawnPoints.Count);
                    GameObject NewVendingMachine;

                    if (isOnline)
                    {
                        NewVendingMachine = PhotonNetwork.Instantiate("VendingMachinePrefab",
                            NotVisibleSpawnPoints[randomSpawnPoint].transform.position,
                            NotVisibleSpawnPoints[randomSpawnPoint].transform.rotation);
                    }
                    else
                    {
                        NewVendingMachine = Instantiate(VendingMachinePrefab,
                            NotVisibleSpawnPoints[randomSpawnPoint].transform.position,
                            NotVisibleSpawnPoints[randomSpawnPoint].transform.rotation);
                    }

                    if (NewVendingMachine != null)
                    {
                        NewVendingMachine.GetComponent<VendingMachine>().setIsMasterClient(true);
                        spawnedVendingMachines.Add(NewVendingMachine);
                        NotVisibleSpawnPoints.RemoveAt(randomSpawnPoint);
                    }
                }
            }
        }
    }
    


    private bool IsVisibleByCamera(Transform target)
    {
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(target.position);
        bool isVisible = screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1 && screenPoint.z > 0;
        return isVisible;
    }
    
    
    public void setIsOnHordeCooldown(bool isOnHorderCooldown, int currentHorde)
    {
        foreach (var vendingMachine in spawnedVendingMachines)
        {
                if (vendingMachine != null)
                    vendingMachine.GetComponent<VendingMachine>().setIsOnHorderCooldown(isOnHorderCooldown, currentHorde);
        }
    }
    
    
}
