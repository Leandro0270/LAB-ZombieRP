using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HordeManager : MonoBehaviourPunCallbacks
{
    //GameObjects===============================================================
    [SerializeField] private GameObject NormalZombiePrefab;
    [SerializeField] private GameObject[] SpecialZombiesPrefabs;
    [SerializeField] private GameObject[] spawnPoints;
    [SerializeField] private GameObject FinalBosses;
    
    //Components================================================================
    [SerializeField] private TextMeshProUGUI HorderText;
    [SerializeField] private VendingMachineHorderGenerator Itemgenerator;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private MainGameManager GameManager;
    //Horde Parameters==========================================================
    [SerializeField] private bool isHorderMode = true;
    [SerializeField] private bool haveBaseZombieLifeIncrement = true;
    [SerializeField] private float baseZombieLifeIncrement = 0.5f;
    [SerializeField] private float spawnTime = 2f;
    [SerializeField] private float spawnTimeDecrement = 0.2f;
    [SerializeField] private float timeBetweenHordes = 5f;
    [SerializeField] private int firstHordeZombies = 3;
    [SerializeField] private int hordeIncrement = 6;
    [SerializeField] private float specialZombiePercentage = 25;
    [SerializeField] private float specialZombiePercentageDecrement = 5;
    [SerializeField] private bool isLastHordeMode = false;
    [SerializeField] private float lastHorde = 15;
    [SerializeField] private float timeBetweenZombiesOnLastHorde = 5f;
    //Intern Variables=================================================================
    private float killedZombiesInHorde = 0;
    private float currentZombieLife = 0;
    private int currentHordeZombies = 0;
    private int currentHorde = 0;
    private int nextHorde = 1;
    private int zombiesAlive = 0;
    private float timeBetweenHordesUI;
    private bool isBossZombieAlive = false;
    private bool isGameOver = false;
    //Special events Variables==========================================================
    [SerializeField] private ChallengeManager challengeManager;
    [SerializeField] private List<GameObject> challengeMachines;
    [SerializeField] private bool haveChallenges = true;
    [SerializeField] private int challengeMachineHordeSpawn = 5;
    [SerializeField] private bool challengeMachinesChangePosition = true;
    [SerializeField] private int challengeMachineHordeRespawn = 5;
    [SerializeField] private bool willSpawnDifferentDifficulties = true;
    [SerializeField] private int mediumDifficultyHordeSpawn = 7;
    [SerializeField] private int hardDifficultyHordeSpawn = 9;
    private bool isSpecialEvent = false;
    private bool isExplosiveZombieEvent = false;
    private bool isCoffeeMachineEvent = false;
    //Other settings variables=================================================================
    private bool isOnline = false;
    private bool isMasterClient = false;
    public string[] enemyPrefabNames;
    [SerializeField] private PhotonView photonView;
    //=========================================================================================
    public void Start()
    
    { mainCamera = GameManager.getMainCamera();
        HorderText.text = "Prepare for the First Horder";
        Itemgenerator = GetComponent<VendingMachineHorderGenerator>();
        if (isOnline)
        {
            Itemgenerator.setIsOnline(true);
            if (isMasterClient)
            {
                Itemgenerator = GetComponent<VendingMachineHorderGenerator>();
                Itemgenerator.setIsMasterClient(true);
            }

        }
        currentHordeZombies = firstHordeZombies;
        if(haveBaseZombieLifeIncrement)
            currentZombieLife = NormalZombiePrefab.GetComponent<EnemyStatus>().get_life();
        if (isOnline)
        {
            if(isMasterClient)
                StartCoroutine(HorderBreakManager());
        }
        else
            StartCoroutine(HorderBreakManager());
        
        
    }
    public void setIsOnline(bool isOnline)
    {
        this.isOnline = isOnline;
    }
    
    public void setIsMasterClient(bool isMasterClient)
    {
        this.isMasterClient = isMasterClient;
    }
    void Update()
    {
        if (timeBetweenHordesUI > 0)
        {
            timeBetweenHordesUI -= Time.deltaTime;
            //Vai printar o tempo que falta para a proxima horder formatado com no máximo 2 casas decimais
            if (isOnline)
            {
                string text = "Next Horder in: " + timeBetweenHordesUI.ToString("F0");
                photonView.RPC("setHordeText", RpcTarget.All, text);

            }
            else
            {
                HorderText.text = "Next Horder in: " + timeBetweenHordesUI.ToString("F0");
            }
        }
}

    //Função que decrementa a quantidade de zumbis vivos
        public void decrementZombiesAlive(GameObject zombie)
        {
            GameManager.removeEnemy(zombie);
            zombiesAlive--;
            killedZombiesInHorde++;
            if (isOnline)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    string text = "Horder: " + (currentHorde + 1) + " | Zombies: " +
                                  (currentHordeZombies - killedZombiesInHorde);
                    photonView.RPC("setHordeText", RpcTarget.All, text);
                }
            }
            else
            {
                HorderText.text = "Horder: " + (currentHorde + 1) + " | Zombies: " + (currentHordeZombies - killedZombiesInHorde);
            }
            if (zombiesAlive == 0)
            {
                currentHorde++;
                if (currentHorde == nextHorde && zombiesAlive <= 0)
                {
                    killedZombiesInHorde = 0;
                    nextHorde++;
                    currentHordeZombies += hordeIncrement;
                    if (spawnTime > 0.4f)
                        spawnTime -= spawnTimeDecrement;
                    Itemgenerator.setIsOnHorderCooldown(true);
                    StartCoroutine(HorderBreakManager());
                    timeBetweenHordesUI = timeBetweenHordes;
                }
                if(haveChallenges){
                    if (willSpawnDifferentDifficulties)
                    {
                        if(currentHorde == mediumDifficultyHordeSpawn)
                            challengeManager.addChalengeMachinesPrefab(challengeMachines[0]);
                        if(currentHorde == hardDifficultyHordeSpawn)
                            challengeManager.addChalengeMachinesPrefab(challengeMachines[1]);
                    }
                    if(currentHorde == challengeMachineHordeSpawn)
                    {
                        challengeManager.spawnChallengeMachines();
                    }
                    
                    if(challengeMachinesChangePosition && currentHorde % challengeMachineHordeRespawn == 0)
                    {
                        challengeManager.respawnChallengeMachines();
                    }
                }

                if (isLastHordeMode)
                {
                    if (currentHorde == lastHorde)
                    {
                        isBossZombieAlive = true;
                        spawnTime = timeBetweenZombiesOnLastHorde;
                        currentHordeZombies = 200;

                    }
                }
            }
        }

        //Função que incrementa a quantidade de zumbis vivos
        public void incrementZombiesAlive()
        {
            zombiesAlive++;
        }

        
        [PunRPC]
        public void setHordeText(string text)
        {
            HorderText.text = text;
        }
        
        
        //Função que recebe como parametro o tempo que o zumbi irá aparecer e a quantidade de zumbis que irão aparecer e spawna o zumbi
        IEnumerator SpawnZombie()
        {
            Itemgenerator.setIsOnHorderCooldown(false);
            Itemgenerator.verifySpawnVendingMachine(currentHorde + 1);
            List<GameObject> visibleSpawnPoints = new List<GameObject>();

            foreach (GameObject spawnPoint in spawnPoints)
            {
                if (!IsVisibleByCamera(spawnPoint.transform, mainCamera))
                {
                    visibleSpawnPoints.Add(spawnPoint);
                }
            }
            
            if (isOnline)
            {
                string text = "Horder: " + (currentHorde + 1) + " | Zombies: " +
                              (currentHordeZombies - killedZombiesInHorde);
                photonView.RPC("setHordeText", RpcTarget.All, text);
                if (isBossZombieAlive)
                {
                    GameObject zombieBoss = PhotonNetwork.Instantiate("FinalBoss", visibleSpawnPoints[0].transform.position,
                        visibleSpawnPoints[0].transform.rotation);
                    int viewID = zombieBoss.GetComponent<PhotonView>().ViewID;
                    GameManager.addEnemyOnOnline(viewID);
                }
                
                for (int i = 0; i < currentHordeZombies; i++)
                {
                    yield return new WaitForSeconds(spawnTime);
                    if (isGameOver)
                        break;
                    specialZombiePercentage -= specialZombiePercentageDecrement;
                    if (specialZombiePercentage < 10f)
                        specialZombiePercentage = 10f;
                    bool isSpecialZombie = RandomBoolWithPercentage(specialZombiePercentage);
                    int spawnPointIndex = Random.Range(0, visibleSpawnPoints.Count);
                    if (IsVisibleByCamera(visibleSpawnPoints[spawnPointIndex].transform, mainCamera))
                    {
                        visibleSpawnPoints.Remove(visibleSpawnPoints[spawnPointIndex]);
                        spawnPointIndex = Random.Range(0, visibleSpawnPoints.Count);

                    }

                    GameObject zombie;
                    if (isSpecialZombie && currentHorde > 3)
                    {
                        int specialZombieIndex = Random.Range(0, SpecialZombiesPrefabs.Length);
                        zombie = PhotonNetwork.Instantiate(enemyPrefabNames[specialZombieIndex], visibleSpawnPoints[0].transform.position,
                            visibleSpawnPoints[0].transform.rotation);
                    }
                    else
                    {
                        zombie = PhotonNetwork.Instantiate("NormalZombiePrefab", visibleSpawnPoints[spawnPointIndex].transform.position,
                            visibleSpawnPoints[spawnPointIndex].transform.rotation);
                        if (haveBaseZombieLifeIncrement)
                        {
                            EnemyStatus ZombieStatus = zombie.GetComponent<EnemyStatus>();
                            ZombieStatus.setTotalLife(currentZombieLife);
                            ZombieStatus.setCurrentLife(currentZombieLife);
                        }

                        if (isCoffeeMachineEvent)
                        {
                            EnemyFollow ZombieFollow = zombie.GetComponent<EnemyFollow>();
                            ZombieFollow.setCoffeeMachineEvent(true);

                        }
                    }

                    if (isExplosiveZombieEvent)
                    {
                        EnemyStatus ZombieStatus = zombie.GetComponent<EnemyStatus>();
                        ZombieStatus.setExplosiveZombieEvent(true);
                    }

                    zombie.GetComponent<EnemyStatus>().setHordeManager(this);
                    int photonViewId = zombie.GetComponent<PhotonView>().ViewID;
                    GameManager.addEnemyOnOnline(photonViewId);
                    incrementZombiesAlive();
                }
                
            }
            else
            {

                if (haveBaseZombieLifeIncrement && currentHorde > 0)
                {
                    currentZombieLife += (currentZombieLife * baseZombieLifeIncrement);
                }

                HorderText.text = "Horder: " + (currentHorde + 1) + " | Zombies: " +
                                  (currentHordeZombies - killedZombiesInHorde);
                if (isBossZombieAlive)
                {
                    GameObject bossZombie = Instantiate(FinalBosses, visibleSpawnPoints[0].transform.position,
                        visibleSpawnPoints[0].transform.rotation);
                    GameManager.addEnemy(bossZombie);
                }

                //inicia um loop que ira rodar conforme a variavel spawnCount, e ira rodar conforme o tempo que foi passado na variavel spawnTime
                for (int i = 0; i < currentHordeZombies; i++)
                {
                    yield return new WaitForSeconds(spawnTime);
                    if (isGameOver)
                        break;
                    specialZombiePercentage -= specialZombiePercentageDecrement;
                    if (specialZombiePercentage < 10f)
                        specialZombiePercentage = 10f;
                    bool isSpecialZombie = RandomBoolWithPercentage(specialZombiePercentage);
                    int spawnPointIndex = Random.Range(0, visibleSpawnPoints.Count);
                    if (IsVisibleByCamera(visibleSpawnPoints[spawnPointIndex].transform, mainCamera))
                    {
                        visibleSpawnPoints.Remove(visibleSpawnPoints[spawnPointIndex]);
                        spawnPointIndex = Random.Range(0, visibleSpawnPoints.Count);

                    }

                    GameObject zombie;
                    if (isSpecialZombie && currentHorde > 3)
                    {
                        int specialZombieIndex = Random.Range(0, SpecialZombiesPrefabs.Length);
                        zombie = Instantiate(SpecialZombiesPrefabs[specialZombieIndex],
                            visibleSpawnPoints[spawnPointIndex].transform.position,
                            visibleSpawnPoints[spawnPointIndex].transform.rotation);
                    }
                    else
                    {
                        zombie = Instantiate(NormalZombiePrefab, visibleSpawnPoints[spawnPointIndex].transform.position,
                            visibleSpawnPoints[spawnPointIndex].transform.rotation);
                        if (haveBaseZombieLifeIncrement)
                        {
                            EnemyStatus ZombieStatus = zombie.GetComponent<EnemyStatus>();
                            ZombieStatus.setTotalLife(currentZombieLife);
                            ZombieStatus.setCurrentLife(currentZombieLife);
                        }

                        if (isCoffeeMachineEvent)
                        {
                            EnemyFollow ZombieFollow = zombie.GetComponent<EnemyFollow>();
                            ZombieFollow.setCoffeeMachineEvent(true);

                        }
                    }

                    if (isExplosiveZombieEvent)
                    {
                        EnemyStatus ZombieStatus = zombie.GetComponent<EnemyStatus>();
                        ZombieStatus.setExplosiveZombieEvent(true);
                    }

                    zombie.GetComponent<EnemyStatus>().setHordeManager(this);
                    GameManager.addEnemy(zombie);
                    incrementZombiesAlive();
                }

            }
        }
        
        private bool IsVisibleByCamera(Transform target, Camera cam)
        {
            Vector3 screenPoint = cam.WorldToViewportPoint(target.position);
            bool isVisible = screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1 && screenPoint.z > 0;
            return isVisible;
        }

        //Função que adiciona um tempo entre as chamadas de spawnDeZumbis
        IEnumerator HorderBreakManager()
        {
            yield return new WaitForSeconds(timeBetweenHordes);
            StartCoroutine(SpawnZombie());
        }

        
        public bool RandomBoolWithPercentage(float probability)
        {
            float randomValue = Random.Range(0f, 100f);
            return randomValue <= probability;
        }
        
        
        
        public void setSpecialEvent(bool isSpecialEvent)
        {
            this.isSpecialEvent = isSpecialEvent;
        }

        public void setExplosiveZombieEvent(bool isExplosiveZombieEvent)
        {
            this.isExplosiveZombieEvent = isExplosiveZombieEvent;
            if (zombiesAlive > 0)
            {
                foreach (var zombie in GameManager.getEnemies())
                {
                    EnemyStatus ZombieStatus = zombie.GetComponent<EnemyStatus>();
                    ZombieStatus.setExplosiveZombieEvent(true);
                }
            }
        }

        public void setCoffeeMachineEvent(bool isCoffeeMachineEvent)
            {
                if (isCoffeeMachineEvent)
                {
                    this.isCoffeeMachineEvent = isCoffeeMachineEvent;
                }
            }
        
        public void gameOver()
        {
            StopAllCoroutines();
            isGameOver = true;
            HorderText.text = " ";
        }

        public int getCurrentHorde()
        {
            return currentHorde;
        }
}


    

