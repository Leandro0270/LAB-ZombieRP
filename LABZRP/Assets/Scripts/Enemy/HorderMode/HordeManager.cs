using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
    [SerializeField] private ItemHorderGenerator Itemgenerator;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private MainGameManager GameManager;
    //Horde Parameters==========================================================
    [SerializeField] private float spawnTime = 2f;
    [SerializeField] private float spawnTimeDecrement = 0.2f;
    [SerializeField] private float timeBetweenHordes = 5f;
    [SerializeField] private int firstHorde = 3;
    [SerializeField] private int hordeIncrement = 6;
    [SerializeField] private float specialZombiePercentage = 25;
    [SerializeField] private float specialZombiePercentageDecrement = 5;
    [SerializeField] private float lastHorde = 15;
    [SerializeField] private float timeBetweenZombiesOnLastHorde = 5f;
    //Intern Variables=================================================================
    private int currentHordeZombies = 0;
    private int currentHorde = 0;
    private int nextHorde = 1;
    private int zombiesAlive = 0;
    private float timeBetweenHordesUI;
    private bool isBossZombieAlive = false;
    
    public void Start()
    { mainCamera = GameManager.getMainCamera();
        HorderText.text = "Prepare for the First Horder";
        Itemgenerator = GetComponent<ItemHorderGenerator>();
        currentHordeZombies = firstHorde;
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
        public void decrementZombiesAlive()
        {
            zombiesAlive--;
            HorderText.text = "Horder: " + (currentHorde + 1) + "\n Zombies: " + zombiesAlive;
            if (zombiesAlive == 0)
            {
                currentHorde++;
                if (currentHorde == nextHorde && zombiesAlive <= 0)
                {
                    nextHorde++;
                    currentHordeZombies += hordeIncrement;
                    if (spawnTime > 0.4f)
                        spawnTime -= spawnTimeDecrement;
                    StartCoroutine(HorderBreakManager());
                    timeBetweenHordesUI = timeBetweenHordes;
                }
                
                if(currentHorde == lastHorde)
                {
                    isBossZombieAlive = true;
                    spawnTime = timeBetweenZombiesOnLastHorde;
                    currentHordeZombies = 200;

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
            List<GameObject> visibleSpawnPoints = new List<GameObject>();

            foreach (GameObject spawnPoint in spawnPoints)
            {
                if (!IsVisibleByCamera(spawnPoint.transform, mainCamera))
                {
                    visibleSpawnPoints.Add(spawnPoint);
                }
            }

            Itemgenerator.GenerateItem();
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

                if (isSpecialZombie && currentHorde > 3)
                {
                    int specialZombieIndex = Random.Range(0, SpecialZombiesPrefabs.Length);
                    GameObject zombie = Instantiate(SpecialZombiesPrefabs[specialZombieIndex], visibleSpawnPoints[spawnPointIndex].transform.position,
                        visibleSpawnPoints[spawnPointIndex].transform.rotation);
                    GameManager.addEnemy(zombie);
                }else{
                    GameObject zombie = Instantiate(NormalZombiePrefab, visibleSpawnPoints[spawnPointIndex].transform.position,
                        visibleSpawnPoints[spawnPointIndex].transform.rotation);
                    GameManager.addEnemy(zombie);
                }
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

    }

