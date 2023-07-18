using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChallengeManager : MonoBehaviour
{

    private ScObChallengesSpecs activeChallenge; // O desafio ativo no momento.
    private bool challengeInProgress = false; // Flag para indicar se um desafio está em andamento.
    private bool challengeCompleted = false; // Flag para indicar se um desafio foi completado.
    private bool challengeFailed = false; // Flag para indicar se um desafio foi falhado.

    //ChallengeMachine
    [SerializeField] private int challengeMachinesToSpawn = 3;
    private List<ChallengeMachine> challengeMachines;
    private List<Transform> _possibleChallengeMachineSpawnPoints;
    private ChallengeMachine _startedChallengeMachine;
    [SerializeField] private List<GameObject> challengeMachinesPrefabs;
    [SerializeField] private Transform[] challengeMachineSpawnPoints;


    //GENERAL VARIABLES
    private bool _isOnline = false;
    private float challengeTime;
    private List<GameObject> players;
    private String challengeName;
    private String challengeDescription;
    private int challengeReward;
    private int challengeDifficulty;
    private float currentTimeToStartChallenge = 0;
    private float timeToStartChallenge = 5;
    private float currentChallengeTime = 0;
    [SerializeField] private float cooldownBetweenChallenges = 10f;
    private bool _respawnChallengeMachineWhenCompleted = false;
    
    
    //Other components
    [SerializeField] private PhotonView _photonView;
    [SerializeField] private MainGameManager mainGameManager;
    [SerializeField] private HordeManager hordeManager;


    
    
    
    //UI
    [SerializeField] private GameObject currentChallengeText;
    [SerializeField] private GameObject challengeNameUI;
    [SerializeField] private TextMeshProUGUI _challengeNameText;
    [SerializeField] private GameObject challengeDescriptionUI;
    [SerializeField] private TextMeshProUGUI _challengeDescriptionText;
    [SerializeField] private GameObject challengeTimeUI;
    [SerializeField] private TextMeshProUGUI _challengeTimeText;
    [SerializeField] private GameObject killedZombiesUI;
    [SerializeField] private TextMeshProUGUI _killedZombiesText;

    //EXPLOSIVE ENEMIES VARIABLES
    private bool _explosiveEnemiesSetup = false;
    

    //NO HIT VARIABLES
    private bool _noHitSetup = false;
    private bool _takedHit = false;


    //NO THROWABLE VARIABLES
    private bool _noThrowableSetup = false;



    //KILL IN AREA VARIABLES
    private bool _killInAreaSetup = false;
    [SerializeField] private GameObject areaPrefab;
    [SerializeField] private Transform[] areaSpawnPoints;
    private GameObject _instantiateArea;
    


    //KILL IN TIME VARIABLES
    private bool _killInTimeSetup = false;
    private int _zombiesToKill;
    private int _zombiesKilled = 0;


    //KILL WITH AIM VARIABLES
    private bool _killWithAimSetup = false;



    //KILL WITH MELEE VARIABLES
    private bool _killWithMeleeSetup = false;


    //DEFEND THE COFFEE MACHINE VARIABLES
    private bool destroyedCoffeeMachine = false;
    [SerializeField] private int _coffeeMachinesToDefend = 3;
    [SerializeField] private GameObject _coffeeMachine;
    private bool _defendTheCoffeeMachineSetup = false;
    private List<GameObject> _instantiatedCoffeeMachines;
    [SerializeField] private List<GameObject> _coffeMachineAreas;
    [SerializeField] private GameObject explosionCoffeeMachineFailPrefab;
    

    //SHARPSHOOTER VARIABLES
    private bool _sharpshooterSetup = false;
    private bool _missedShot = false;


    private void Start()
    {
        players = new List<GameObject>();
        challengeMachines = new List<ChallengeMachine>();
        _instantiatedCoffeeMachines = new List<GameObject>();
        _possibleChallengeMachineSpawnPoints = new List<Transform>();
        foreach (var challengeMachineSpawnPoint in challengeMachineSpawnPoints)
        {
            _possibleChallengeMachineSpawnPoints.Add(challengeMachineSpawnPoint);
        }
    }

    public bool StartChallenge(ScObChallengesSpecs challenge, ChallengeMachine challengeMachinee)
    {
        if (challengeInProgress || challengeCompleted || challengeFailed) return false;
        _startedChallengeMachine = challengeMachinee;
        if (players.Count == 0)
        {
            players = mainGameManager.getPlayers();
        }
        challengeTime = challenge.ChallengeTime;
        activeChallenge = challenge;
        challengeInProgress = true;
        currentChallengeText.SetActive(true);
        challengeName = activeChallenge.ChallengeName;
        challengeDescription = activeChallenge.ChallengeDescription;
        challengeReward = activeChallenge.ChallengeReward;
        challengeDifficulty = activeChallenge.ChallengeDifficulty;
        _zombiesToKill = activeChallenge.zombiesToKill;
        _missedShot = false;
        foreach (var challengeMachine in challengeMachines)
        {
            challengeMachine.setIsActivated(false);
        }
        
        return true;
    }

    public void StartExplosiveEnemiesChallenge()
    {
        if (!_explosiveEnemiesSetup)
        {
            //UI
            challengeNameUI.SetActive(true);
            _challengeNameText.text = challengeName;
            challengeDescriptionUI.SetActive(true);
            _challengeDescriptionText.text = challengeDescription;
            challengeTimeUI.SetActive(true);
            hordeManager.setSpecialEvent(true);
            hordeManager.setExplosiveZombieEvent(true);
            _explosiveEnemiesSetup = true;
        }
        currentChallengeTime += Time.deltaTime;
        _challengeTimeText.text = "Tempo restante: " + (challengeTime - currentChallengeTime).ToString("F0");        
        if(currentChallengeTime >= challengeTime)
        {
            
            hordeManager.setSpecialEvent(false);
            hordeManager.setExplosiveZombieEvent(false);
            _explosiveEnemiesSetup = false;
            SuccessChallenge();
        }
    }

    public void StartNoHitChallenge()
    {
        if (!_noHitSetup)
        {
            challengeNameUI.SetActive(true);
            _challengeNameText.text = challengeName;
            challengeDescriptionUI.SetActive(true);
            _challengeDescriptionText.text = challengeDescription;
            challengeTimeUI.SetActive(true);
            foreach (var player in players)
            {
                player.GetComponent<PlayerStats>().setIsNoHitChallenge(true);
            }

            _noHitSetup = true;
        }

        if (_takedHit)
        {
            FailChallenge();
        }
        else
        {
            currentChallengeTime += Time.deltaTime;
            _challengeTimeText.text = "Tempo restante: " + (challengeTime - currentChallengeTime).ToString("F0");
            if (currentChallengeTime >= challengeTime)
            {
                foreach (var player in players)
                {
                    player.GetComponent<PlayerStats>().setIsNoHitChallenge(false);

                }

                _noHitSetup = false;
                SuccessChallenge();
            }
        }
    }

    public void StartNoThrowableChallenge()
    {
        if(!_noThrowableSetup)
        { 
            challengeNameUI.SetActive(true);
            _challengeNameText.text = challengeName;
            challengeDescriptionUI.SetActive(true);
            _challengeDescriptionText.text = challengeDescription;
            challengeTimeUI.SetActive(true);
            foreach (var player in players)
            {
                player.GetComponent<ThrowablePlayerStats>().setIsInThrowableChallenge(true);
            }
            _noThrowableSetup = true;
        }

        if (currentChallengeTime <= challengeTime)
        {
            currentChallengeTime += Time.deltaTime;
            _challengeTimeText.text = "Tempo restante: " + (challengeTime - currentChallengeTime).ToString("F0");
        }
        else
        {
            foreach (var player in players)
            {
                player.GetComponent<ThrowablePlayerStats>().setIsInThrowableChallenge(false);
            }
            SuccessChallenge();
            _noThrowableSetup = false;
        }
    }
    

    public void StartKillInArea()
    {
        if( !_killInAreaSetup)
        {
            challengeNameUI.SetActive(true);
            _challengeNameText.text = challengeName;
            challengeDescriptionUI.SetActive(true);
            _challengeDescriptionText.text = challengeDescription;
            challengeTimeUI.SetActive(true);
            killedZombiesUI.SetActive(true);
            _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled +"/"+_zombiesToKill;

            int randomAreaIndex = Random.Range(0, areaSpawnPoints.Length);
            _instantiateArea = Instantiate(areaPrefab, areaSpawnPoints[randomAreaIndex].position, areaSpawnPoints[randomAreaIndex].rotation);
           
                foreach (var Player in players)
                {
                    Player.GetComponent<WeaponSystem>().setisKillInTimeChallengeActive(true);
                }
                _killInAreaSetup = true;
        }

        if (currentTimeToStartChallenge <= timeToStartChallenge)
        {
            currentTimeToStartChallenge += Time.deltaTime;
            _challengeTimeText.text = "Desafio começará em: " + (timeToStartChallenge - currentTimeToStartChallenge).ToString("F0");
        }

        if(currentTimeToStartChallenge >= timeToStartChallenge)
        {
            _challengeTimeText.text = "Tempo restante: " + (challengeTime - currentChallengeTime).ToString("F0");
            currentChallengeTime += Time.deltaTime;
            if((currentChallengeTime >= challengeTime) && _zombiesKilled < _zombiesToKill)
            {
                foreach (var Player in players)
                {
                    Player.GetComponent<WeaponSystem>().setisKillInTimeChallengeActive(false);
                }
                FailChallenge();
            }
            else if(_zombiesKilled >= _zombiesToKill)
            {
                foreach (var Player in players)
                {
                    Player.GetComponent<WeaponSystem>().setisKillInTimeChallengeActive(false);
                }
                SuccessChallenge();
            }
            Destroy(_instantiateArea);
            foreach (var Player in players)
            {
                Player.GetComponent<WeaponSystem>().SetIsInArea(false);
            }
            
        }
    }
    
    public void StartDefendTheCoffeeMachineChallenge()
    {
        if(!_defendTheCoffeeMachineSetup)
        { 
            challengeNameUI.SetActive(true);
            _challengeNameText.text = challengeName;
            challengeDescriptionUI.SetActive(true);
            _challengeDescriptionText.text = challengeDescription;
            challengeTimeUI.SetActive(true);

            int randomSpawnPoint = UnityEngine.Random.Range(0, _coffeMachineAreas.Count);
            //vai criar um array com todos os game objects filhos do _coffeMachineAreas[randomSpawnPoint]
            Transform[] coffeeMachineSpawnPoints = _coffeMachineAreas[randomSpawnPoint].GetComponentsInChildren<Transform>();
            
            for(int i= 0; i < _coffeeMachinesToDefend; i++)
            {
                _coffeeMachine = Instantiate(_coffeeMachine, coffeeMachineSpawnPoints[i].position, coffeeMachineSpawnPoints[i].transform.rotation);
                _instantiatedCoffeeMachines.Add(_coffeeMachine);
                hordeManager.setCoffeeMachineEvent(true);
            }
            StartCoroutine(delayToStartChallenge());
            _defendTheCoffeeMachineSetup = true;
        }
        else{
            if (currentTimeToStartChallenge <= timeToStartChallenge)
            {
                currentTimeToStartChallenge += Time.deltaTime;
                _challengeTimeText.text = "Desafio começará em + " + (timeToStartChallenge - currentTimeToStartChallenge).ToString("F0");
            }
            else{
                if (destroyedCoffeeMachine)
                {
                    hordeManager.setCoffeeMachineEvent(false);
                    mainGameManager.cancelCoffeeMachineEvent();
                    foreach (var coffeeMachine in _instantiatedCoffeeMachines)
                    {
                        GameObject explosion = Instantiate(explosionCoffeeMachineFailPrefab, coffeeMachine.transform.position, Quaternion.identity);
                        Destroy(coffeeMachine);
                    }
                    destroyedCoffeeMachine = false;
                    FailChallenge();
                }
                if(currentChallengeTime >= challengeTime)
                {
                    hordeManager.setCoffeeMachineEvent(false);
                    mainGameManager.cancelCoffeeMachineEvent();
                    _defendTheCoffeeMachineSetup = false;
                    SuccessChallenge();
                }
                else
                {
                    currentChallengeTime += Time.deltaTime;
                    _challengeTimeText.text = "Tempo restante: " + (challengeTime - currentChallengeTime).ToString("F0");
                } 
            }
        }
    }


    public void StartKillInTime()
    {
        if (!_killInTimeSetup)
        {
            challengeNameUI.SetActive(true);
            _challengeNameText.text = challengeName;
            challengeDescriptionUI.SetActive(true);
            _challengeDescriptionText.text = challengeDescription;
            challengeTimeUI.SetActive(true);
            killedZombiesUI.SetActive(true);
            _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled +"/"+_zombiesToKill;

            foreach (var Player in players)
            {
                Player.GetComponent<WeaponSystem>().setisKillInTimeChallengeActive(true);
            }
            _killInTimeSetup = true;
        }
        currentChallengeTime += Time.deltaTime;
        _challengeTimeText.text = "Tempo restante: " + (challengeTime - currentChallengeTime).ToString("F0");
        if((currentChallengeTime >= challengeTime) && _zombiesKilled < _zombiesToKill)
        {
            foreach (var Player in players)
            {
                Player.GetComponent<WeaponSystem>().setisKillInTimeChallengeActive(false);
            }
            FailChallenge();
        }
        else if(_zombiesKilled >= _zombiesToKill)
        {
            foreach (var Player in players)
            {
                Player.GetComponent<WeaponSystem>().setisKillInTimeChallengeActive(false);
            }
            SuccessChallenge();
        }
    }

    public void StartKillWithAim()
    {
        if (!_killWithAimSetup)
        {
            challengeNameUI.SetActive(true);
            _challengeNameText.text = challengeName;
            challengeDescriptionUI.SetActive(true);
            _challengeDescriptionText.text = challengeDescription;
            challengeTimeUI.SetActive(true);
            killedZombiesUI.SetActive(true);
            _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled +"/"+_zombiesToKill;

            foreach (var Player in players)
            {
                Player.GetComponent<WeaponSystem>().setisKillWithAimChallengeActive(true);
            }
            _killWithAimSetup = true;
        }
        currentChallengeTime += Time.deltaTime;
        _challengeTimeText.text = "Tempo restante: " + (challengeTime - currentChallengeTime).ToString("F0");
        if((currentChallengeTime >= challengeTime) && _zombiesKilled < _zombiesToKill)
        {
            foreach (var Player in players)
            {
                Player.GetComponent<WeaponSystem>().setisKillWithAimChallengeActive(false);
            }
            FailChallenge();
        }
        else if(_zombiesKilled >= _zombiesToKill)
        {
            foreach (var Player in players)
            {
                Player.GetComponent<WeaponSystem>().setisKillWithAimChallengeActive(false);
            }
            SuccessChallenge();
        }
    }
    

    public void StartKillWithMelee()
    {

        if (!_killWithMeleeSetup)
        {
            challengeNameUI.SetActive(true);
            _challengeNameText.text = challengeName;
            challengeDescriptionUI.SetActive(true);
            _challengeDescriptionText.text = challengeDescription;
            challengeTimeUI.SetActive(true);
            killedZombiesUI.SetActive(true);
            _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled +"/"+_zombiesToKill;
            foreach (var player in players)
            {
                player.GetComponent<WeaponSystem>().setisMeleeChallengeActive(true);
            }
            _killWithMeleeSetup = true;

        }else{
            _challengeTimeText.text = "Tempo restante: " + (challengeTime - currentChallengeTime).ToString("F0");
            currentChallengeTime += Time.deltaTime;
            if((currentChallengeTime >= challengeTime) && _zombiesKilled < _zombiesToKill)
            {
                FailChallenge();
            }
            else if(_zombiesKilled >= _zombiesToKill)
            {
                SuccessChallenge();
            } 
        }
    }


    public void StartSharpshooterChallenge()
    {
        if (!_sharpshooterSetup)
        {
            challengeNameUI.SetActive(true);
            _challengeNameText.text = challengeName;
            challengeDescriptionUI.SetActive(true);
            _challengeDescriptionText.text = challengeDescription;
            challengeTimeUI.SetActive(true);
            killedZombiesUI.SetActive(true);
            _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled +"/"+_zombiesToKill;
            _sharpshooterSetup = true;
        }else{
            
            _challengeTimeText.text = "Tempo restante: " + (challengeTime - currentChallengeTime).ToString("F0");
            currentChallengeTime += Time.deltaTime;
            
            if (((currentChallengeTime >= challengeTime) && _zombiesKilled < _zombiesToKill) || _missedShot)
            {
                FailChallenge();
                _missedShot = false;
            }
            else if(_zombiesKilled >= _zombiesToKill)
            {
                SuccessChallenge();
            } 
        }
    }
    
    

    public void SuccessChallenge()
    {
        challengeNameUI.SetActive(false);
        challengeDescriptionUI.SetActive(false);
        challengeTimeUI.SetActive(false);
        killedZombiesUI.SetActive(false);
        challengeInProgress = false;
        challengeCompleted = true;
        challengeFailed = false;
        currentChallengeText.SetActive(false);
        List<GameObject> players = mainGameManager.getAlivePlayers();
        foreach (GameObject player in players)
        {
            player.GetComponent<PlayerPoints>().addChallengePoints(activeChallenge.ChallengeReward);
        }
        StartCoroutine(CooldownBetweenChallenges());

    }


    public void FailChallenge()
    {
        challengeNameUI.SetActive(false);
        challengeDescriptionUI.SetActive(false);
        challengeTimeUI.SetActive(false);
        killedZombiesUI.SetActive(false);
        challengeInProgress = false;
        challengeCompleted = false;
        challengeFailed = true;
        currentChallengeText.SetActive(false);
        if (activeChallenge.havePenalty)
        {
            //penalidade do desafio
        }

        StartCoroutine(CooldownBetweenChallenges());
    }
    
    
    
    private IEnumerator CooldownBetweenChallenges()
    {
        yield return new WaitForSeconds(cooldownBetweenChallenges);
        resetChallengesInChallengeMachines();
        challengeInProgress = false;
        challengeCompleted = false;
        challengeFailed = false;
        _zombiesKilled = 0;
        currentChallengeTime = 0;
        _missedShot = false;
        if(_respawnChallengeMachineWhenCompleted)
            respawnChallengeMachines();
    }

    // Update is called once per frame
    void Update()
    {
        if (challengeInProgress)
        {
            switch (activeChallenge.ChallengeType)
            {
                case ScObChallengesSpecs.Type.ExplosiveEnemies:
                    StartExplosiveEnemiesChallenge();
                    break;
                case ScObChallengesSpecs.Type.NoHit:
                    StartNoHitChallenge();
                    break;
                case ScObChallengesSpecs.Type.NoThrowable:
                    StartNoThrowableChallenge();
                    break;
                case ScObChallengesSpecs.Type.KillInArea:
                    StartKillInArea();
                    break;
                case ScObChallengesSpecs.Type.KillInTime:
                    StartKillInTime();
                    break;
                case ScObChallengesSpecs.Type.KillWithAim:
                    StartKillWithAim();
                    break;
                case ScObChallengesSpecs.Type.KillWithMelee:
                    StartKillWithMelee();
                    break;
                case ScObChallengesSpecs.Type.DefendTheCoffeeMachine:
                    StartDefendTheCoffeeMachineChallenge();
                    break;
                case ScObChallengesSpecs.Type.Sharpshooter:
                    StartSharpshooterChallenge();
                    break;
            }
        }
    }
    
    public IEnumerator delayToStartChallenge()
    {
        yield return new WaitForSeconds(timeToStartChallenge);
        if (activeChallenge.ChallengeType == ScObChallengesSpecs.Type.DefendTheCoffeeMachine)
        {
            hordeManager.setCoffeeMachineEvent(true);
            foreach (var coffeeMachine in _instantiatedCoffeeMachines)
            {
                ChallengeCoffeeMachine challengeCoffeeMachine = coffeeMachine.GetComponent<ChallengeCoffeeMachine>();
                challengeCoffeeMachine.setChallengeManager(this);
                challengeCoffeeMachine.startChallenge();
                
            }
        }    
    }
    
    public void destroyCoffeeMachine()
    {
        destroyedCoffeeMachine = true;
    }
    public void setTakedHit(bool takedHit)
    {
        _takedHit = takedHit;
    }

    public void addZombieKilled()
    {
        if (challengeInProgress)
        {
            _zombiesKilled++;
            if (activeChallenge.ChallengeType == ScObChallengesSpecs.Type.Sharpshooter || activeChallenge.ChallengeType == ScObChallengesSpecs.Type.KillInTime || activeChallenge.ChallengeType == ScObChallengesSpecs.Type.KillInArea || 
                activeChallenge.ChallengeType == ScObChallengesSpecs.Type.KillWithAim || activeChallenge.ChallengeType == ScObChallengesSpecs.Type.KillWithMelee){
                Debug.Log("Matou um zumbi no desafio de matar zumbis");
                _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled + "/" + _zombiesToKill;
            }
        }
    }

    public void missedShot()
    {
        if(_sharpshooterSetup)
            _missedShot = true;
    }
    
    private void addChallengeMachine(ChallengeMachine challengeMachine)
    {
        challengeMachines.Add(challengeMachine);
    }
    
    public void addChalengeMachinesPrefab(GameObject challengeMachinePrefab)
    {
        challengeMachinesPrefabs.Add(challengeMachinePrefab);
    }
    
    public void removeChallengeMachine(ChallengeMachine challengeMachine)
    {
        challengeMachines.Remove(challengeMachine);
    }

    private void resetChallengesInChallengeMachines()
    {
        foreach (var challengeMachine in challengeMachines)
        {
            challengeMachine.InitializeChallengeMachine();
        }
    }

    public void spawnChallengeMachines()
    {
        for(int i=0; i < challengeMachinesToSpawn; i++)
        {
            int randomSpawnIndex = Random.Range(0, _possibleChallengeMachineSpawnPoints.Count);
            int randomMachineIndex = Random.Range(0, challengeMachinesPrefabs.Count);
            GameObject newChallengeMachine = Instantiate(challengeMachinesPrefabs[randomMachineIndex], _possibleChallengeMachineSpawnPoints[randomSpawnIndex].position, _possibleChallengeMachineSpawnPoints[randomSpawnIndex].rotation);
            _possibleChallengeMachineSpawnPoints.RemoveAt(randomSpawnIndex);
            challengeMachines.Add(newChallengeMachine.GetComponent<ChallengeMachine>());
        }
    }

    public void respawnChallengeMachines()
    {
        if (!challengeInProgress)
        {
            foreach (var machine in challengeMachines)
            {
                Destroy(machine);
            }
            challengeMachines.Clear();
            _possibleChallengeMachineSpawnPoints.Clear();
            foreach (var challengeMachineSpawnPoint in challengeMachineSpawnPoints)
            {
                _possibleChallengeMachineSpawnPoints.Add(challengeMachineSpawnPoint);
            }
            spawnChallengeMachines();
        }
        else
        {
            _respawnChallengeMachineWhenCompleted = true;
        }
    }
    
    public int getPlayerCount()
    {
        return mainGameManager.getPlayers().Count;
    }
}
