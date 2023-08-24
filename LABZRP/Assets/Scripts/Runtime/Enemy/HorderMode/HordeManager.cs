using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Runtime.Challenges;
using Runtime.Enemy.ZombieCombat.EnemyStatus;
using Runtime.Enemy.ZombieCombat.ZombieBehaviour;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Enemy.HorderMode
{
    public class HordeManager : MonoBehaviourPunCallbacks
    {
[Header("Game Objects")]
        [SerializeField] private GameObject NormalZombiePrefab;
        [SerializeField] private GameObject[] SpecialZombiesPrefabs;
        [SerializeField] private GameObject[] spawnPoints;
        [SerializeField] private GameObject FinalBosses;

        [Header("UI Components")]
        [FormerlySerializedAs("HorderText")] [SerializeField] private TextMeshProUGUI HordeText;
        [SerializeField] private Camera mainCamera;

        [Header("Managers and Generators")]
        [SerializeField] private VendingMachineHorderGenerator Itemgenerator;
        [SerializeField] private MainGameManager GameManager;
        [SerializeField] private ChallengeManager challengeManager;

        [Header("Horde Settings")]
        [SerializeField] private float firstHordeStartTime = 20f;
        [SerializeField] private bool isIncrementalZombiesPerPlayer = true;
        [SerializeField] private bool haveBaseZombieLifeIncrement = true;
        [SerializeField] private float baseZombieLifeIncrement = 0.5f;
        [SerializeField] private float spawnTime = 2f;
        [SerializeField] private float spawnTimeDecrement = 0.2f;
        [SerializeField] private float timeBetweenHordes = 5f;
        [SerializeField] private int firstHordeZombies = 3;
        [SerializeField] private int hordeIncrement = 6;
        [SerializeField] private int firstSpecialZombieAppearHorde = 3;
        [SerializeField] private float specialZombiePercentage = 25;
        [SerializeField] private float specialZombiePercentageDecrement = 5;
        [SerializeField] private bool isLastHordeMode = false;
        [SerializeField] private float lastHorde = 15;
        [SerializeField] private float timeBetweenZombiesOnLastHorde = 5f;

        [Header("Special Events Settings")]
        [SerializeField] private List<GameObject> challengeMachines;
        [SerializeField] private bool haveChallenges = true;
        [SerializeField] private int challengeMachineHordeSpawn = 5;
        [SerializeField] private bool challengeMachinesChangePosition = true;
        [SerializeField] private int challengeMachineHordeRespawn = 5;
        [SerializeField] private bool willSpawnDifferentDifficulties = true;
        [SerializeField] private int mediumDifficultyHordeSpawn = 7;
        [SerializeField] private int hardDifficultyHordeSpawn = 9;

        [Header("Online Settings")]
        private bool _isOnline = false;
        private bool _isMasterClient = false;
        public string[] enemyPrefabNames;

        private int _currentHorde = 0;
        private float _currentZombieLife = 0;
        private int _currentHordeZombies = 0;
        private int _nextHorde = 1;
        private int _playersCount;
        private float _timeBetweenHordesUI;
        private float _currentFirstHordeStartTime;
        private float _killedZombiesInHorde = 0;
        private int _zombiesAlive = 0;
        private bool _isBossZombieAlive = false;
        private bool _isGameOver = false;
        private List<GameObject> _players = new List<GameObject>();

        private bool _isSpecialEvent = false;
        private bool _isExplosiveZombieEvent = false;
        private bool _isCoffeeMachineEvent = false;
        //=========================================================================================
        public void Start()
        {
           InitializeHorde();
        }
        
        private void InitializeHorde()
        {
            _isMasterClient = PhotonNetwork.IsMasterClient;
            _currentFirstHordeStartTime = firstHordeStartTime;
            mainCamera = GameManager.getMainCamera();
            UpdateHordeText($"The first horde will start in {_currentHorde} seconds");
            Itemgenerator = GetComponent<VendingMachineHorderGenerator>();
            SetupOnlineSettings();
            _currentHordeZombies = firstHordeZombies;
            if (haveBaseZombieLifeIncrement)
            {
                _currentZombieLife = NormalZombiePrefab.GetComponent<EnemyStatus>().getBaseLife();
            }

            if (!_isOnline)
            {
                StartCoroutine(HordeBreakManager());
            }
        }
        
        private void SetupOnlineSettings()
        {
            Itemgenerator.setIsOnline(_isOnline);
            if (_isMasterClient)
            {
                Itemgenerator.setIsMasterClient(true);
            }
        }
        
        
        public void SetIsOnline(bool isOnline)
        {
            _isOnline = isOnline;
        }
        
        private void UpdateHordeCountdown()
        {
            if (_timeBetweenHordesUI <= 0 && _currentFirstHordeStartTime <= 0)
            {
                return;
            }

            _timeBetweenHordesUI -= Time.deltaTime;
            _currentFirstHordeStartTime -= Time.deltaTime;
            string text = (_currentHorde == 0) 
                ? $"The first horde will start in {_currentFirstHordeStartTime:F0} seconds" 
                : $"Next Horde in: {_timeBetweenHordesUI:F0}";

            UpdateHordeText(text);
        }
        
        private void UpdateHordeText(string text)
        {
            if (_isOnline)
            {
                photonView.RPC("SetHordeText", RpcTarget.All, text);
            }
            else
            {
                HordeText.text = text;
            }
        }
    
        public void setIsMasterClient(bool isMasterClient)
        {
            _isMasterClient = isMasterClient;
        }

        private void Update()
        {
            if(!_isOnline || PhotonNetwork.IsMasterClient)
                UpdateHordeCountdown();
        }

        //Função que decrementa a quantidade de zumbis vivos
        public void DecrementZombiesAlive(GameObject zombie)
        {
            GameManager.removeEnemy(zombie);
            _zombiesAlive--;

            UpdateKilledZombiesAndUI();

            if (AllZombiesKilled())
            {
                PrepareForNextHorde();
                ExecuteChallenges();
            }

            HandleLastHordeMode();
        }

        private void UpdateKilledZombiesAndUI()
        {
            _killedZombiesInHorde++;

            string hordeText = $"Horde: {_currentHorde + 1} | Zombies: {_currentHordeZombies - _killedZombiesInHorde}";

            if (_isOnline && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("SetHordeText", RpcTarget.All, hordeText);
            }
            else if (!_isOnline)
            {
                HordeText.text = hordeText;
            }
        }

        private bool AllZombiesKilled()
        {
            return _killedZombiesInHorde == _currentHordeZombies;
        }

        private void PrepareForNextHorde()
        {
            _currentHorde++;
            if (_currentHorde == _nextHorde && _zombiesAlive <= 0)
            {
                _killedZombiesInHorde = 0;
                _nextHorde++;
                _currentHordeZombies += hordeIncrement;

                if (spawnTime > 0.4f)
                    spawnTime -= spawnTimeDecrement;

                GameManager.updateAmmoBoxPrices();
                Itemgenerator.setIsOnHordeCooldown(true, _currentHorde);
                StartCoroutine(HordeBreakManager());
                _timeBetweenHordesUI = timeBetweenHordes;
            }

            if (_currentHorde < firstSpecialZombieAppearHorde) return;
            
            specialZombiePercentage -= specialZombiePercentageDecrement;
            if (specialZombiePercentage < 10f)
                specialZombiePercentage = 10f;
        }


private void ExecuteChallenges()
{
    if (!haveChallenges) return;

    if (willSpawnDifferentDifficulties)
    {
        if (_currentHorde == mediumDifficultyHordeSpawn)
            challengeManager.addChalengeMachinesPrefab(challengeMachines[0]);
        if (_currentHorde == hardDifficultyHordeSpawn)
            challengeManager.addChalengeMachinesPrefab(challengeMachines[1]);
    }

    if (_currentHorde == challengeMachineHordeSpawn)
    {
        challengeManager.spawnChallengeMachines();
    }

    if (challengeMachinesChangePosition && _currentHorde % challengeMachineHordeRespawn == 0)
    {
        challengeManager.respawnChallengeMachines();
    }
}

private void HandleLastHordeMode()
{
    if (!isLastHordeMode) return;
    if (_currentHorde != lastHorde) return;

    _isBossZombieAlive = true;
    spawnTime = timeBetweenZombiesOnLastHorde;
    _currentHordeZombies = 200;
}


        //Função que incrementa a quantidade de zumbis vivos
        [PunRPC]
        public void IncrementZombiesAlive()
        {
            _zombiesAlive++;
        }

        
        [PunRPC]
        public void SetHordeText(string text)
        {
            HordeText.text = text;
        }

        private List<GameObject> VisibleSpawnPoints()
        {
            List<GameObject> possibleSpawnPoints = new List<GameObject>();
            foreach (GameObject spawnPoint in spawnPoints)
            {
                if (!IsVisibleByCamera(spawnPoint.transform, mainCamera))
                {
                    possibleSpawnPoints.Add(spawnPoint);
                }
            }

            return possibleSpawnPoints;
        }
        private void HandleItemStartHorde()
        {
            Itemgenerator.setIsOnHordeCooldown(false, _currentHorde);
            Itemgenerator.verifySpawnVendingMachine(_currentHorde + 1);
        }


        private bool RandomizeSpecialZombie()
        {
            if(_currentHorde < (firstSpecialZombieAppearHorde))
                return false;
            
            bool isSpecialZombie = RandomBoolWithPercentage(specialZombiePercentage);
            return isSpecialZombie;
        }

        private IEnumerator HandleOnlineZombieSpawn(string updateText)
        {
            List<GameObject> possibleSpawnPoints = VisibleSpawnPoints();
            if (!_isMasterClient)
                yield break;

            photonView.RPC("SetHordeText", RpcTarget.All, updateText);

            if (_isBossZombieAlive)
            {
                GameObject zombieBoss = PhotonNetwork.Instantiate("FinalBoss",
                    possibleSpawnPoints[0].transform.position,
                    possibleSpawnPoints[0].transform.rotation);
                int viewID = zombieBoss.GetComponent<PhotonView>().ViewID;
                GameManager.addEnemyOnline(viewID);
            }

            for (var i = 0; i < _currentHordeZombies; i++)
            {
                yield return new WaitForSeconds(spawnTime);
                if (_isGameOver)
                    break;
                possibleSpawnPoints = VisibleSpawnPoints();
                int spawnPointIndex = Random.Range(0, possibleSpawnPoints.Count);
                bool isSpecialZombie = RandomizeSpecialZombie();
                GameObject zombieGameObject;
                EnemyStatus zombieStatus =null;
                EnemyNavMeshFollow zombieNavMeshFollow = null;

                if (isSpecialZombie)
                {
                    int specialZombieIndex = Random.Range(0, enemyPrefabNames.Length);
                    zombieGameObject = PhotonNetwork.Instantiate(enemyPrefabNames[specialZombieIndex],
                        possibleSpawnPoints[spawnPointIndex].transform.position,
                        possibleSpawnPoints[spawnPointIndex].transform.rotation);
                    zombieNavMeshFollow = zombieGameObject.GetComponent<EnemyNavMeshFollow>();

                }
                else
                {
                    zombieGameObject = PhotonNetwork.Instantiate("NormalZombiePrefab",
                        possibleSpawnPoints[spawnPointIndex].transform.position,
                        possibleSpawnPoints[spawnPointIndex].transform.rotation);
                    if (haveBaseZombieLifeIncrement)
                    {
                        zombieNavMeshFollow = zombieGameObject.GetComponent<EnemyNavMeshFollow>();
                        zombieStatus = zombieGameObject.GetComponent<EnemyStatus>();
                        zombieStatus.SetTotalLife(_currentZombieLife);
                        zombieStatus.SetCurrentLife(_currentZombieLife);
                    }

                    if(_isExplosiveZombieEvent)
                        zombieStatus?.SetExplosiveZombieEvent(true);
                    
               
                    if(_isCoffeeMachineEvent)
                        zombieNavMeshFollow?.setCoffeeMachineEvent(true);

                        
                }

                if (zombieStatus is null || zombieNavMeshFollow is null || zombieGameObject is null) {yield break;}
                
                
                    zombieStatus.SetHordeManager(this);
                    foreach (var player in _players)
                    {
                        zombieNavMeshFollow.AddPlayer(player);
                    }
                
                    int photonViewId = zombieGameObject.GetComponent<PhotonView>().ViewID;
                    GameManager.addEnemyOnline(photonViewId);
                    photonView.RPC("IncrementZombiesAlive", RpcTarget.All);

            }
        }

        private IEnumerator HandleLocalZombieSpawn(string updateText)
        {
            HordeText.text = updateText;
            List<GameObject> possibleSpawnPoints = VisibleSpawnPoints();
            if(_isBossZombieAlive)
            {
                GameObject zombieBoss = Instantiate(FinalBosses, possibleSpawnPoints[0].transform.position,
                    possibleSpawnPoints[0].transform.rotation);
                GameManager.addEnemy(zombieBoss);
            }
            
            for (var i = 0; i < _currentHordeZombies; i++)
            {
                yield return new WaitForSeconds(spawnTime);
                if (_isGameOver)
                    break;
                possibleSpawnPoints = VisibleSpawnPoints();
                if (possibleSpawnPoints.Count == 0)
                    possibleSpawnPoints.AddRange(spawnPoints);
                

                int spawnPointIndex = Random.Range(0, possibleSpawnPoints.Count);
                bool isSpecialZombie = RandomizeSpecialZombie();
                GameObject zombieGameObject;
                EnemyStatus zombieStatus =null;
                EnemyNavMeshFollow zombieNavMeshFollow = null;
                if (isSpecialZombie)
                {
                    int specialZombieIndex = Random.Range(0, SpecialZombiesPrefabs.Length);
                    zombieGameObject = Instantiate(SpecialZombiesPrefabs[specialZombieIndex],
                        possibleSpawnPoints[spawnPointIndex].transform.position,
                        possibleSpawnPoints[spawnPointIndex].transform.rotation);
                    zombieStatus = zombieGameObject.GetComponent<EnemyStatus>();
                    zombieNavMeshFollow = zombieGameObject.GetComponent<EnemyNavMeshFollow>();

                }
                else
                {
                    zombieGameObject = Instantiate(NormalZombiePrefab,
                        possibleSpawnPoints[spawnPointIndex].transform.position,
                        possibleSpawnPoints[spawnPointIndex].transform.rotation);
                    if (haveBaseZombieLifeIncrement)
                    {
                        zombieNavMeshFollow = zombieGameObject.GetComponent<EnemyNavMeshFollow>();
                        zombieStatus = zombieGameObject.GetComponent<EnemyStatus>();
                        zombieStatus.SetTotalLife(_currentZombieLife);
                        zombieStatus.SetCurrentLife(_currentZombieLife);
                    }
                    if(_isExplosiveZombieEvent)
                        zombieStatus?.SetExplosiveZombieEvent(true);
                    if(_isCoffeeMachineEvent)
                        zombieNavMeshFollow?.setCoffeeMachineEvent(true);
                }

                if (zombieStatus is null || zombieNavMeshFollow is null || zombieGameObject is null) {yield break;}
                
                
                    zombieStatus.SetHordeManager(this);
                    foreach (var player in _players)
                    {
                        zombieNavMeshFollow.AddPlayer(player);
                    }
                    GameManager.addEnemy(zombieGameObject);
                    IncrementZombiesAlive();

            }
            
            
        }


        //Função que recebe como parametro o tempo que o zumbi irá aparecer e a quantidade de zumbis que irão aparecer e spawna o zumbi
        private void SpawnZombie()
        {
            HandleItemStartHorde();
            string text = $"Horde: {(_currentHorde+1):F0} | Zombies: {(_currentHordeZombies - _killedZombiesInHorde):F0}";
            StartCoroutine(_isOnline ? HandleOnlineZombieSpawn(text) : HandleLocalZombieSpawn(text));
        }
        
        private bool IsVisibleByCamera(Transform target, Camera cam)
        {
            Vector3 screenPoint = cam.WorldToViewportPoint(target.position);
            bool isVisible = screenPoint.x >= 0 && screenPoint.x <= 1 && screenPoint.y >= 0 && screenPoint.y <= 1 && screenPoint.z > 0;
            return isVisible;
        }


        public void HordeBreakManagerManualStart()
        {
            Debug.Log("ManualHordeBreakManager no horde manager");
            StartCoroutine(HordeBreakManager());
        }

        private IEnumerator HordeBreakManager()
        {
            if (_currentHorde == 0)
            {
                yield return new WaitForSeconds(firstHordeStartTime);
                Debug.Log("Passou do firstHordeStartTime");
                if (isIncrementalZombiesPerPlayer)
                {
                    Debug.Log("É incremental");
                    _playersCount = GameManager.getPlayersCount();
                    firstHordeZombies *= _playersCount;
                    hordeIncrement *= _playersCount;
                }
            }
            else
                yield return new WaitForSeconds(timeBetweenHordes);
            if (haveBaseZombieLifeIncrement && _currentHorde > 0)
            {
                _currentZombieLife += (_currentZombieLife * baseZombieLifeIncrement);
            }
            SpawnZombie();
        }
        
        public bool RandomBoolWithPercentage(float probability)
        {
            float randomValue = Random.Range(0f, 100f);
            return randomValue <= probability;
        }
        
        
        
        public void setSpecialEvent(bool isSpecialEvent)
        {
            this._isSpecialEvent = isSpecialEvent;
        }
        
        public void AddPlayer(GameObject player)
        {
            _players.Add(player);
        }

        public void setExplosiveZombieEvent(bool isExplosiveZombieEvent)
        {
            this._isExplosiveZombieEvent = isExplosiveZombieEvent;
            if (_zombiesAlive > 0)
            {
                foreach (var zombie in GameManager.getEnemies())
                {
                    EnemyStatus ZombieStatus = zombie.GetComponent<EnemyStatus>();
                    ZombieStatus.SetExplosiveZombieEvent(true);
                }
            }
        }

        public void setCoffeeMachineEvent(bool isCoffeeMachineEvent)
        {
                _isCoffeeMachineEvent = isCoffeeMachineEvent;
        }
        
        public void gameOver()
        {
            StopAllCoroutines();
            _isGameOver = true;
            HordeText.text = " ";
        }

        public int getCurrentHorde()
        {
            return _currentHorde;
        }

        public void updateHordeOnline()
        {
            photonView.RPC("updateHordeOnline", RpcTarget.All, _currentHorde);
        }
        
        [PunRPC]
        public void updateHordeOnline(int currentHorde)
        {
            _currentHorde = currentHorde;
        }
    }
}


    

