using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeManager : MonoBehaviour
{

    public ScObChallengesSpecs[] challengeList;
    private ScObChallengesSpecs activeChallenge; // O desafio ativo no momento.
    private bool challengeInProgress = false; // Flag para indicar se um desafio está em andamento.
    private bool challengeCompleted = false; // Flag para indicar se um desafio foi completado.
    private bool challengeFailed = false; // Flag para indicar se um desafio foi falhado.

    //ChallengeMachine
    [SerializeField] private ChallengeMachine challengeMachine;


    //GENERAL VARIABLES
    private float challengeTime;
    private GameObject[] players;
    private String challengeName;
    private String challengeDescription;
    private int challengeReward;
    private int challengeDifficulty;
    private float currentChallengeTime = 0;
    [SerializeField] private float cooldownBetweenChallenges = 10f;
    
    
    //Other components
    [SerializeField] private MainGameManager mainGameManager;
    [SerializeField] private HordeManager hordeManager;



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
    private float timeToStartArea = 5;
    private float currentTimeToStartArea = 0;


    //KILL IN TIME VARIABLES
    private int _zombiesToKill;
    private int _zombiesKilled = 0;


    //KILL WITH AIM VARIABLES
    private bool _killWithAimSetup = false;



    //KILL WITH MELEE VARIABLES
    private bool _killWithMeleeSetup = false;


    //DEFEND THE COFFEE MACHINE VARIABLES
    private bool _defendTheCoffeeMachineSetup = false;


    //SHARPSHOOTER VARIABLES
    private bool _sharpshooterSetup = false;
    private bool _missedShot = false;



    
    
    
    public void StartChallenge(ScObChallengesSpecs challenge)
    {
        if (challengeInProgress || challengeCompleted || challengeFailed) return;
        challengeTime = challenge.ChallengeTime;
        activeChallenge = challenge;
        challengeInProgress = true;
        challengeName = activeChallenge.ChallengeName;
        challengeDescription = activeChallenge.ChallengeDescription;
        challengeReward = activeChallenge.ChallengeReward;
        challengeDifficulty = activeChallenge.ChallengeDifficulty;
        _zombiesToKill = activeChallenge.zombiesToKill;
    }

    public void StartExplosiveEnemiesChallenge()
    {
        if (!_explosiveEnemiesSetup)
        {
            hordeManager.setSpecialEvent(true);
            hordeManager.setExplosiveZombieEvent(true);
            _explosiveEnemiesSetup = true;
        }
        currentChallengeTime += Time.deltaTime;
        
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
        //setup do desafio
        _noThrowableSetup = true;
    }
    //loop do desafio
    //if (playerHit
    }
    

    public void StartKillInArea()
    {
        if( !_killInAreaSetup)
        {
            _instantiateArea = Instantiate(areaPrefab, areaSpawnPoints[UnityEngine.Random.Range(0, areaSpawnPoints.Length)].position, Quaternion.identity);
            _killInAreaSetup = true;
        }
        if(currentTimeToStartArea <= timeToStartArea)
            currentTimeToStartArea += Time.deltaTime;
        if(currentTimeToStartArea >= timeToStartArea)
        {
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

    public void StartKillInTime()
    {
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

    public void StartKillWithAim()
    { 
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
    

    public void StartKillWithMelee()
    {
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

    public void StartDefendTheCoffeeMachineChallenge()
    {
        if(!_defendTheCoffeeMachineSetup)
        {
            //setup do desafio
            _defendTheCoffeeMachineSetup = true;
        }
        //loop do desafio
        //if (playerHit
    
    }

    public void StartSharpshooterChallenge()
    {
        currentChallengeTime += Time.deltaTime;
        if (((currentChallengeTime >= challengeTime) && _zombiesKilled < _zombiesToKill) || _missedShot)
        {
            FailChallenge();
        }
        else if(_zombiesKilled >= _zombiesToKill)
        {
            SuccessChallenge();
        }
        
    }
    
    

    public void SuccessChallenge()
    {
        challengeInProgress = false;
        challengeCompleted = true;
        challengeFailed = false;
        List<GameObject> players = mainGameManager.getAlivePlayers();
        foreach (GameObject player in players)
        {
            player.GetComponent<PlayerPoints>().addChallengePoints(activeChallenge.ChallengeReward);
        }
        StartCoroutine(CooldownBetweenChallenges());

    }


    public void FailChallenge()
    {
        challengeInProgress = false;
        challengeCompleted = false;
        challengeFailed = true;
        
        if (activeChallenge.havePenalty)
        {
            //penalidade do desafio
        }

        StartCoroutine(CooldownBetweenChallenges());
    }
    
    
    
    private IEnumerator CooldownBetweenChallenges()
    {
        yield return new WaitForSeconds(cooldownBetweenChallenges);
        challengeInProgress = false;
        challengeCompleted = false;
        challengeFailed = false;
        _zombiesKilled = 0;
        currentChallengeTime = 0;
        _missedShot = false;
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
    
    
    public void setTakedHit(bool takedHit)
    {
        _takedHit = takedHit;
    }

    public void addZombieKilled()
    {
        _zombiesKilled++;
    }

    public void missedShot()
    {
        _missedShot = true;
    }
}
