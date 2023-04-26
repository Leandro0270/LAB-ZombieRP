using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class HordeManager : MonoBehaviour
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
    //Special events Variables==========================================================
    private bool isSpecialEvent = false;
    private bool isExplosiveZombieEvent = false;
    //=================================================================
    public void Start()
    { mainCamera = GameManager.getMainCamera();
        HorderText.text = "Prepare for the First Horder";
        Itemgenerator = GetComponent<VendingMachineHorderGenerator>();
        currentHordeZombies = firstHordeZombies;
        if(haveBaseZombieLifeIncrement)
            currentZombieLife = NormalZombiePrefab.GetComponent<EnemyStatus>().get_life();
        //Pega os objetos que possuem a tag SpawnPoint
        StartCoroutine(HorderBreakManager());
        
    }

    void Update()
    {
        if (timeBetweenHordesUI > 0)
        {
            timeBetweenHordesUI -= Time.deltaTime;
            //Vai printar o tempo que falta para a proxima horder formatado com no máximo 2 casas decimais
            HorderText.text = "Next Horder in: " + timeBetweenHordesUI.ToString("F2");

        }
}

    //Função que decrementa a quantidade de zumbis vivos
        public void decrementZombiesAlive(GameObject zombie)
        {
            zombiesAlive--;
            killedZombiesInHorde++;
            HorderText.text = "Horder: " + (currentHorde + 1) + "\n Zombies: " + (currentHordeZombies - killedZombiesInHorde);
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

        //Função que recebe como parametro o tempo que o zumbi irá aparecer e a quantidade de zumbis que irão aparecer e spawna o zumbi
        IEnumerator SpawnZombie()
        {
            Itemgenerator.setIsOnHorderCooldown(false);
            Itemgenerator.verifySpawnVendingMachine(currentHorde+1);
            List<GameObject> visibleSpawnPoints = new List<GameObject>();

            foreach (GameObject spawnPoint in spawnPoints)
            {
                if (!IsVisibleByCamera(spawnPoint.transform, mainCamera))
                {
                    visibleSpawnPoints.Add(spawnPoint);
                }
            }

            if (haveBaseZombieLifeIncrement && currentHorde > 0)
            {
                currentZombieLife += (currentZombieLife* baseZombieLifeIncrement);
            }
            
            HorderText.text = "Horder: " + (currentHorde + 1) + "\n Zombies: " + currentHordeZombies;
            if(isBossZombieAlive)
            {
                GameObject bossZombie = Instantiate(FinalBosses, visibleSpawnPoints[0].transform.position,
                    visibleSpawnPoints[0].transform.rotation);
                GameManager.addEnemy(bossZombie);
            }
            //inicia um loop que ira rodar conforme a variavel spawnCount, e ira rodar conforme o tempo que foi passado na variavel spawnTime
            for (int i = 0; i < currentHordeZombies; i++)
            {
                yield return new WaitForSeconds(spawnTime);
                specialZombiePercentage -= specialZombiePercentageDecrement;
                if(specialZombiePercentage < 10f)
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
                    zombie = Instantiate(SpecialZombiesPrefabs[specialZombieIndex], visibleSpawnPoints[spawnPointIndex].transform.position,
                        visibleSpawnPoints[spawnPointIndex].transform.rotation);
                }else{
                    zombie = Instantiate(NormalZombiePrefab, visibleSpawnPoints[spawnPointIndex].transform.position,
                        visibleSpawnPoints[spawnPointIndex].transform.rotation);
                    if (haveBaseZombieLifeIncrement)
                    {
                        EnemyStatus ZombieStatus = zombie.GetComponent<EnemyStatus>();
                        ZombieStatus.setTotalLife(currentZombieLife);
                        ZombieStatus.setCurrentLife(currentZombieLife);
                    }

                    

                }
                
                if (isExplosiveZombieEvent)
                { 
                    EnemyStatus ZombieStatus = zombie.GetComponent<EnemyStatus>();
                    ZombieStatus.setExplosiveZombieEvent(true);
                }
                GameManager.addEnemy(zombie);
                incrementZombiesAlive();
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

            {
                
            }
        }

    }

