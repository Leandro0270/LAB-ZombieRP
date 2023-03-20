using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HordeManager : MonoBehaviour
{
    public float timeBetweenHordes = 5f;
    public int firstHorde = 3;
    public int hordeIncrement = 6;
    private int currentHordeZombies = 0;
    private int currentHorde = 0;
    private int nextHorde = 1;
    public float spawnTime = 2f;
    public float spawnTimeDecrement = 0.2f;
    private int zombiesAlive = 0;
    private float timeBetweenHordesUI;
    private MainGameManager GameManager;
    public GameObject zombiePrefab;
    private GameObject[] spawnPoints;
    private ItemHorderGenerator Itemgenerator;
    [SerializeField] private TextMeshProUGUI HorderText;
    public void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<MainGameManager>();
        HorderText.text = "Prepare for the First Horder";
        Itemgenerator = GetComponent<ItemHorderGenerator>();
        currentHordeZombies = firstHorde;
        //Pega os objetos que possuem a tag SpawnPoint
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        StartCoroutine(HorderBreakManager());
        
    }

    void Update()
    {
        if (currentHorde == nextHorde && zombiesAlive == 0)
        {
            nextHorde++;
            currentHordeZombies += hordeIncrement;
            if (spawnTime > 0.4f)
                spawnTime -= spawnTimeDecrement;
            StartCoroutine(HorderBreakManager());
            timeBetweenHordesUI = timeBetweenHordes;
        }

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

            Itemgenerator.GenerateItem();
            HorderText.text = "Horder: " + (currentHorde + 1) + "\n Zombies: " + currentHordeZombies;

            //inicia um loop que ira rodar conforme a variavel spawnCount, e ira rodar conforme o tempo que foi passado na variavel spawnTime
            for (int i = 0; i < currentHordeZombies; i++)
            {
                yield return new WaitForSeconds(spawnTime);
                //Pega um numero aleatorio entre 0 e o tamanho do array de spawnPoints
                int spawnPointIndex = Random.Range(0, spawnPoints.Length);
                //Instancia o prefab do zumbi na posição do spawnPoint
                GameObject zombie = Instantiate(zombiePrefab, spawnPoints[spawnPointIndex].transform.position,
                    spawnPoints[spawnPointIndex].transform.rotation);
                GameManager.addEnemy(zombie);
                //Incrementa a quantidade de zumbis vivos
                incrementZombiesAlive();


            }


        }

        //Função que adiciona um tempo entre as chamadas de spawnDeZumbis
        IEnumerator HorderBreakManager()
        {
            yield return new WaitForSeconds(timeBetweenHordes);
            StartCoroutine(SpawnZombie());
        }


    }

