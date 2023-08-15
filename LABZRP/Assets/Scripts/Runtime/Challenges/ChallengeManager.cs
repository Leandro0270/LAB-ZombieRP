using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Runtime.Enemy.HorderMode;
using Runtime.Enemy.ScriptObjects.HordeMode.Challenges;
using Runtime.Player.Combat.Gun;
using Runtime.Player.Combat.PlayerStatus;
using Runtime.Player.Combat.Throwables;
using Runtime.Player.Points;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Challenges
{
    public class ChallengeManager : MonoBehaviourPunCallbacks, IPunObservable
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
        [SerializeField] private List<String> challengeMachinesPrefabsStringNames;
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
        [SerializeField] private string ChallengeAreaName = "ChallengeArea";
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
        [SerializeField] private string coffeeMachineName = "CoffeeMachine";
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
        
        public void setIsOnline(bool isOnline)
        {
            _isOnline = isOnline;
        }

        [PunRPC]
        public void updateChallengeTextRPC(String challengeName, String challengeDescription)
        {
            challengeNameUI.SetActive(true);
            _challengeNameText.text = challengeName;
            challengeDescriptionUI.SetActive(true);
            _challengeDescriptionText.text = challengeDescription;
            challengeTimeUI.SetActive(true);
        }
    
        [PunRPC]
        public void updateChallengeTime(float _challengeTime, float _currentChallengeTime)
        {
            _challengeTimeText.text = "Tempo restante: " + (_challengeTime - _currentChallengeTime).ToString("F0");
        }


        [PunRPC]
        public void StartChallengeRPC(int CMPhotonViewId)
        {
            ChallengeMachine challengeMachine = PhotonView.Find(CMPhotonViewId).GetComponent<ChallengeMachine>();
            ScObChallengesSpecs challenge = challengeMachine.getCurrentChallenge();
            StartChallenge(challenge, challengeMachine);
        }
    
        public bool StartChallenge(ScObChallengesSpecs challenge, ChallengeMachine challengeMachinee)
        {
            if (_isOnline && PhotonNetwork.IsMasterClient)
            {
                int challengeMachinePhotonViewId = challengeMachinee.photonView.ViewID;
                photonView.RPC("StartChallengeRPC", RpcTarget.Others, challengeMachinePhotonViewId);
            
            }
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
                if (_isOnline)
                {
                    photonView.RPC("updateChallengeTextRPC", RpcTarget.All, challengeName, challengeDescription);
                }
                else
                {
                    challengeNameUI.SetActive(true);
                    _challengeNameText.text = challengeName;
                    challengeDescriptionUI.SetActive(true);
                    _challengeDescriptionText.text = challengeDescription;
                    challengeTimeUI.SetActive(true);
                }
                hordeManager.setSpecialEvent(true);
                hordeManager.setExplosiveZombieEvent(true);
                _explosiveEnemiesSetup = true;
            }
            currentChallengeTime += Time.deltaTime;
            if (_isOnline)
            {
                photonView.RPC("updateChallengeTime", RpcTarget.All, challengeTime, currentChallengeTime);
            }
            else
            {
                updateChallengeTime(challengeTime, currentChallengeTime);        
            }

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
                //UI
                if (_isOnline)
                {
                    photonView.RPC("updateChallengeTextRPC", RpcTarget.All, challengeName, challengeDescription);
                    photonView.RPC("startChallengesRPC", RpcTarget.All, "noHit", true, false);
                }
                else
                {
                    challengeNameUI.SetActive(true);
                    _challengeNameText.text = challengeName;
                    challengeDescriptionUI.SetActive(true);
                    _challengeDescriptionText.text = challengeDescription;
                    challengeTimeUI.SetActive(true);
                    foreach (var player in players)
                    {
                        player.GetComponent<PlayerStats>().SetIsNoHitChallenge(true);
                    }
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
                if (_isOnline)
                {
                    photonView.RPC("updateChallengeTime", RpcTarget.All, challengeTime, currentChallengeTime);
                }
                else
                {
                    updateChallengeTime(challengeTime, currentChallengeTime);
                }
                
                if (currentChallengeTime >= challengeTime)
                {
                    if (!_isOnline)
                    {

                        foreach (var player in players)
                        {
                            player.GetComponent<PlayerStats>().SetIsNoHitChallenge(false);

                        }
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
            
                //UI
                if (_isOnline)
                {
                    photonView.RPC("updateChallengeTextRPC", RpcTarget.All, challengeName, challengeDescription);
                    photonView.RPC("startChallengesRPC", RpcTarget.All, "noThrowable", true,false);

                }
                else
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
                }
                _noThrowableSetup = true;
            }

            if (currentChallengeTime <= challengeTime)
            {
                currentChallengeTime += Time.deltaTime;
                if (_isOnline)
                {
                    photonView.RPC("updateChallengeTime", RpcTarget.All, challengeTime, currentChallengeTime);
                }
                else
                {
                    updateChallengeTime(challengeTime, currentChallengeTime);
                }
            }
            else
            {
                if (!_isOnline)
                {
                    foreach (var player in players)
                    {
                        player.GetComponent<ThrowablePlayerStats>().setIsInThrowableChallenge(false);
                    }
                }
                SuccessChallenge();
                _noThrowableSetup = false;
            }
        }
    
   
    
        public void StartKillInArea()
        {
            if( !_killInAreaSetup)
            {
                int randomAreaIndex = Random.Range(0, areaSpawnPoints.Length);

                if (_isOnline)
                {
                    photonView.RPC("updateChallengeTextRPC", RpcTarget.All, challengeName, challengeDescription);
                    photonView.RPC("startChallengesRPC", RpcTarget.All, "killInArea", true, false);
                    PhotonNetwork.Instantiate(ChallengeAreaName, areaSpawnPoints[randomAreaIndex].position, areaSpawnPoints[randomAreaIndex].rotation);
                    _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled + "/" + _zombiesToKill;

                }
                else
                {
                    foreach (var Player in players)
                    {
                        Player.GetComponent<WeaponSystem>().setisKillInTimeChallengeActive(true);
                    }
                    challengeNameUI.SetActive(true);
                    _challengeNameText.text = challengeName;
                    challengeDescriptionUI.SetActive(true);
                    _challengeDescriptionText.text = challengeDescription;
                    challengeTimeUI.SetActive(true);
                    killedZombiesUI.SetActive(true);
                    _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled + "/" + _zombiesToKill;
                    _instantiateArea = Instantiate(areaPrefab, areaSpawnPoints[randomAreaIndex].position,
                        areaSpawnPoints[randomAreaIndex].rotation);
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
                    if (!_isOnline)
                    {
                        foreach (var Player in players)
                        {
                            Player.GetComponent<WeaponSystem>().setisKillInTimeChallengeActive(false);
                        }
                    }
                    FailChallenge();
                }
            
            
                else if(_zombiesKilled >= _zombiesToKill)
                {
                    if (!_isOnline)
                    {
                        foreach (var Player in players)
                        {
                            Player.GetComponent<WeaponSystem>().setisKillInTimeChallengeActive(false);
                        }
                    }
                    SuccessChallenge();
                }

            
            
                if (_isOnline)
                {
                    PhotonNetwork.Destroy(_instantiateArea);

                }
                else
                {
                    Destroy(_instantiateArea);
                    foreach (var Player in players)
                    {
                        Player.GetComponent<WeaponSystem>().SetIsInArea(false);
                    }
                }

            }
        }
    
  
        public void StartDefendTheCoffeeMachineChallenge()
        {
            if(!_defendTheCoffeeMachineSetup)
            {
                if (_isOnline)
                {
                    photonView.RPC("updateChallengeTextRPC", RpcTarget.All, challengeName, challengeDescription);
                }
                else
                {
                    challengeNameUI.SetActive(true);
                    _challengeNameText.text = challengeName;
                    challengeDescriptionUI.SetActive(true);
                    _challengeDescriptionText.text = challengeDescription;
                    challengeTimeUI.SetActive(true);
                }
            

                int randomSpawnPoint = UnityEngine.Random.Range(0, _coffeMachineAreas.Count);
                //vai criar um array com todos os game objects filhos do _coffeMachineAreas[randomSpawnPoint]
                Transform[] coffeeMachineSpawnPoints = _coffeMachineAreas[randomSpawnPoint].GetComponentsInChildren<Transform>();

                if (_isOnline)
                {
                    for (int i = 0; i < _coffeeMachinesToDefend; i++)
                    {
                        _coffeeMachine = PhotonNetwork.Instantiate(coffeeMachineName, coffeeMachineSpawnPoints[0].position,
                            coffeeMachineSpawnPoints[0].transform.rotation);
                        _instantiatedCoffeeMachines.Add(_coffeeMachine);
                        hordeManager.setCoffeeMachineEvent(true);
                    }
                    photonView.RPC("startChallengesRPC", RpcTarget.All, "defendTheCoffeeMachine", true, false);

                }
                else
                {
                    for (int i = 0; i < _coffeeMachinesToDefend; i++)
                    {
                        _coffeeMachine = Instantiate(_coffeeMachine, coffeeMachineSpawnPoints[i].position,
                            coffeeMachineSpawnPoints[i].transform.rotation);
                        _instantiatedCoffeeMachines.Add(_coffeeMachine);
                        hordeManager.setCoffeeMachineEvent(true);
                    }
                }

                StartCoroutine(delayToStartChallenge());
                _defendTheCoffeeMachineSetup = true;
            }
            else{
                if (currentTimeToStartChallenge < timeToStartChallenge)
                {
                    currentTimeToStartChallenge += Time.deltaTime;
                    _challengeTimeText.text = "Desafio começará em + " + (timeToStartChallenge - currentTimeToStartChallenge).ToString("F0");
                }
                else{
                    if (destroyedCoffeeMachine)
                    {
                        hordeManager.setCoffeeMachineEvent(false);
                        mainGameManager.cancelCoffeeMachineEvent();
                        if (_isOnline)
                        {
                            foreach (var coffeeMachine in _instantiatedCoffeeMachines)
                            {
                                PhotonNetwork.Instantiate("explosionCoffeeMachineFailPrefab", coffeeMachine.transform.position, Quaternion.identity);
                                PhotonNetwork.Destroy(coffeeMachine);
                            }
                        }
                        else
                        {
                            foreach (var coffeeMachine in _instantiatedCoffeeMachines)
                            {
                                Instantiate(explosionCoffeeMachineFailPrefab, coffeeMachine.transform.position,
                                    Quaternion.identity);
                                Destroy(coffeeMachine);
                            }
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
                if (_isOnline)
                {
                    photonView.RPC("updateChallengeTextRPC", RpcTarget.All, challengeName, challengeDescription);
                    photonView.RPC("startChallengesRPC", RpcTarget.All, "killInTime", true, false);
                    _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled + "/" + _zombiesToKill;

                }
                else
                {
                    challengeNameUI.SetActive(true);
                    _challengeNameText.text = challengeName;
                    challengeDescriptionUI.SetActive(true);
                    _challengeDescriptionText.text = challengeDescription;
                    challengeTimeUI.SetActive(true);
                    killedZombiesUI.SetActive(true);
                    _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled + "/" + _zombiesToKill;

                    foreach (var Player in players)
                    {
                        Player.GetComponent<WeaponSystem>().setisKillInTimeChallengeActive(true);
                    }

                }
                _killInTimeSetup = true;

            }
            currentChallengeTime += Time.deltaTime;
            _challengeTimeText.text = "Tempo restante: " + (challengeTime - currentChallengeTime).ToString("F0");
            if((currentChallengeTime >= challengeTime) && _zombiesKilled < _zombiesToKill)
            {
                if (!_isOnline)
                {
                    foreach (var Player in players)
                    {
                        Player.GetComponent<WeaponSystem>().setisKillInTimeChallengeActive(false);
                    }
                }

                FailChallenge();
            }
            else if (_zombiesKilled >= _zombiesToKill)
            {
                if (!_isOnline)
                {
                    foreach (var Player in players)
                    {
                        Player.GetComponent<WeaponSystem>().setisKillInTimeChallengeActive(false);
                    }

                }
                SuccessChallenge();

            }
        }

    
    
        public void StartKillWithAim()
        {
            if (!_killWithAimSetup)
            {
                if (_isOnline)
                {
                    photonView.RPC("updateChallengeTextRPC", RpcTarget.All, challengeName, challengeDescription);
                    photonView.RPC("startChallengesRPC", RpcTarget.All, "killWithAim", true, false);
                    _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled + "/" + _zombiesToKill;
                }
                else
                {
                    challengeNameUI.SetActive(true);
                    _challengeNameText.text = challengeName;
                    challengeDescriptionUI.SetActive(true);
                    _challengeDescriptionText.text = challengeDescription;
                    challengeTimeUI.SetActive(true);
                    killedZombiesUI.SetActive(true);
                    _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled + "/" + _zombiesToKill;

                    foreach (var Player in players)
                    {
                        Player.GetComponent<WeaponSystem>().setisKillWithAimChallengeActive(true);
                    }

                }
                _killWithAimSetup = true;

            }

            currentChallengeTime += Time.deltaTime;
            _challengeTimeText.text = "Tempo restante: " + (challengeTime - currentChallengeTime).ToString("F0");
            if((currentChallengeTime >= challengeTime) && _zombiesKilled < _zombiesToKill)
            {
                if (!_isOnline)
                {
                    foreach (var Player in players)
                    {
                        Player.GetComponent<WeaponSystem>().setisKillWithAimChallengeActive(false);
                    }
                }

                FailChallenge();
            }
            else if(_zombiesKilled >= _zombiesToKill)
            {
                if (!_isOnline){
                    foreach (var Player in players)
                    {
                        Player.GetComponent<WeaponSystem>().setisKillWithAimChallengeActive(false);
                    }
                }
                SuccessChallenge();
            }
        }
    
 
        public void StartKillWithMelee()
        {

            if (!_killWithMeleeSetup)
            {
                if (_isOnline)
                {
                    photonView.RPC("updateChallengeTextRPC", RpcTarget.All, challengeName, challengeDescription);
                    photonView.RPC("startChallengesRPC", RpcTarget.All, "killWithMelee", true, false);
                }
                else
                {
                    challengeNameUI.SetActive(true);
                    _challengeNameText.text = challengeName;
                    challengeDescriptionUI.SetActive(true);
                    _challengeDescriptionText.text = challengeDescription;
                    challengeTimeUI.SetActive(true);
                    killedZombiesUI.SetActive(true);
                    _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled + "/" + _zombiesToKill;
                    foreach (var player in players)
                    {
                        player.GetComponent<WeaponSystem>().setisMeleeChallengeActive(true);
                    }
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

        [PunRPC]
        public void startChallengesRPC(string challengeType, bool isActive, bool aux)
        {
            switch (challengeType)
            {
                case "noHit":
                    foreach (var player in players)
                    {
                        player.GetComponent<PlayerStats>().SetIsNoHitChallenge(isActive);
                    }
                    break;
                case "noThrowable":
                    foreach (var player in players)
                    {
                        player.GetComponent<ThrowablePlayerStats>().setIsInThrowableChallenge(isActive);
                    }
                    break;
                case "killInArea":
                    killedZombiesUI.SetActive(true);
                    foreach (var Player in players)
                    {
                        Player.GetComponent<WeaponSystem>().setisKillInTimeChallengeActive(isActive);
                    }

                    if (aux)
                    {
                        foreach (var Player in players)
                        {
                            Player.GetComponent<WeaponSystem>().SetIsInArea(aux);
                        }
                    }
                    break;
                case "killInTime":
                    killedZombiesUI.SetActive(true);
                    foreach (var Player in players)
                    {
                        Player.GetComponent<WeaponSystem>().setisKillInTimeChallengeActive(isActive);
                    }
                    break;
                case "killWithAim":
                    killedZombiesUI.SetActive(isActive);
                    foreach (var Player in players)
                    {
                        Player.GetComponent<WeaponSystem>().setisKillWithAimChallengeActive(isActive);
                    }
                    break;
            
                case "killWithMelee":
                    killedZombiesUI.SetActive(isActive);
                    foreach (var Player in players)
                    {
                        Player.GetComponent<WeaponSystem>().setisMeleeChallengeActive(isActive);
                    }
                    break;
                case "sharpshooter":
                    foreach (var Player in players)
                    {
                        Player.GetComponent<WeaponSystem>().setIsSharpshooterChallengeActive(isActive);
                    }
                    break;
                case "defendTheCoffeeMachine":
                    hordeManager.setCoffeeMachineEvent(isActive);
                    break;
                case "cancelChallenge":
                    foreach (var Player in players)
                    {
                        Player.GetComponent<WeaponSystem>().setIsSharpshooterChallengeActive(false);
                        Player.GetComponent<WeaponSystem>().setisMeleeChallengeActive(false);
                        Player.GetComponent<WeaponSystem>().setisKillWithAimChallengeActive(false);
                        Player.GetComponent<WeaponSystem>().setisKillInTimeChallengeActive(false);
                        Player.GetComponent<WeaponSystem>().setisKillInAreaChallengeActive(false);
                        Player.GetComponent<WeaponSystem>().SetIsInArea(false);
                        hordeManager.setCoffeeMachineEvent(false);
                        Player.GetComponent<ThrowablePlayerStats>().setIsInThrowableChallenge(false);
                        Player.GetComponent<PlayerStats>().SetIsNoHitChallenge(false);
                    }
                    break;
            }
        }


        public void StartSharpshooterChallenge()
        {
            if (!_sharpshooterSetup)
            {
                if (_isOnline)
                {
                    photonView.RPC("updateChallengeTextRPC", RpcTarget.All, challengeName, challengeDescription);
                    photonView.RPC("startChallengesRPC", RpcTarget.All, "sharpshooter", true, false);
                    _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled +"/"+_zombiesToKill;

                }
                else
                {
                    challengeNameUI.SetActive(true);
                    _challengeNameText.text = challengeName;
                    challengeDescriptionUI.SetActive(true);
                    _challengeDescriptionText.text = challengeDescription;
                    challengeTimeUI.SetActive(true);
                    killedZombiesUI.SetActive(true);
                    _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled + "/" + _zombiesToKill;
                    foreach (var player in players)
                    {
                        player.GetComponent<WeaponSystem>().setIsSharpshooterChallengeActive(true);
                    }
                    _sharpshooterSetup = true;
                }
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

        [PunRPC]
        public void finishChallenge(bool completed)
        {
            if (completed)
            {
                SuccessChallenge();
            }
            else
            {
                FailChallenge();
            }
        }

        public void SuccessChallenge()
        {
            challengeInProgress = false;
            if (_isOnline && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("startChallengesRPC", RpcTarget.All, "cancelChallenge", false, false);
                photonView.RPC("finishChallenge", RpcTarget.Others, true);
            }
            else
            {
                foreach (var Player in players)
                {
                    WeaponSystem weaponSystem = Player.GetComponent<WeaponSystem>();
                    weaponSystem.setIsSharpshooterChallengeActive(false);
                    weaponSystem.setisMeleeChallengeActive(false);
                    weaponSystem.setisKillWithAimChallengeActive(false);
                    weaponSystem.setisKillInTimeChallengeActive(false);
                    weaponSystem.setisKillInAreaChallengeActive(false);
                    weaponSystem.SetIsInArea(false);
                    Player.GetComponent<ThrowablePlayerStats>().setIsInThrowableChallenge(false);
                    Player.GetComponent<PlayerStats>().SetIsNoHitChallenge(false);
                }
            }
            challengeNameUI.SetActive(false);
            challengeDescriptionUI.SetActive(false);
            challengeTimeUI.SetActive(false);
            killedZombiesUI.SetActive(false);
            challengeCompleted = true;
            challengeFailed = false;
            challengeInProgress = false;
            currentChallengeText.SetActive(false);
            if (PhotonNetwork.IsMasterClient || !_isOnline)
            {
                List<GameObject> players = mainGameManager.getAlivePlayers();
                foreach (GameObject player in players)
                {
                    player.GetComponent<PlayerPoints>().addChallengePoints(activeChallenge.ChallengeReward);
                }

                StartCoroutine(CooldownBetweenChallenges());
            }

        }


        public void FailChallenge()
        {
            challengeInProgress = false;
            if (_isOnline && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("startChallengesRPC", RpcTarget.All, "cancelChallenge", false, false);
                photonView.RPC("finishChallenge", RpcTarget.Others, true);
            }
            else
            {
                foreach (var Player in players)
                {
                    WeaponSystem weaponSystem = Player.GetComponent<WeaponSystem>();
                    weaponSystem.setIsSharpshooterChallengeActive(false);
                    weaponSystem.setisMeleeChallengeActive(false);
                    weaponSystem.setisKillWithAimChallengeActive(false);
                    weaponSystem.setisKillInTimeChallengeActive(false);
                    weaponSystem.setisKillInAreaChallengeActive(false);
                    weaponSystem.SetIsInArea(false);
                    Player.GetComponent<ThrowablePlayerStats>().setIsInThrowableChallenge(false);
                    Player.GetComponent<PlayerStats>().SetIsNoHitChallenge(false);
                }
            }
            challengeNameUI.SetActive(false);
            challengeDescriptionUI.SetActive(false);
            challengeTimeUI.SetActive(false);
            killedZombiesUI.SetActive(false);
            challengeInProgress = false;
            challengeCompleted = false;
            challengeFailed = true;
            currentChallengeText.SetActive(false);
            if(PhotonNetwork.IsMasterClient)
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
            if(!_isOnline || PhotonNetwork.IsMasterClient){
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
            if (_isOnline)
                photonView.RPC("setTakedHitRPC", RpcTarget.MasterClient, takedHit);
            else
                _takedHit = takedHit;
        }
        [PunRPC]
        public void setTakedHitRPC(bool takedHit)
        {
            _takedHit = takedHit;
        }

        public void addZombieKilled()
        {
            if (_isOnline)
                photonView.RPC("addZombieKilledRPC", RpcTarget.MasterClient);
            else
            {
                if (challengeInProgress)
                {
                    _zombiesKilled++;
                    if (activeChallenge.ChallengeType == ScObChallengesSpecs.Type.Sharpshooter ||
                        activeChallenge.ChallengeType == ScObChallengesSpecs.Type.KillInTime ||
                        activeChallenge.ChallengeType == ScObChallengesSpecs.Type.KillInArea ||
                        activeChallenge.ChallengeType == ScObChallengesSpecs.Type.KillWithAim ||
                        activeChallenge.ChallengeType == ScObChallengesSpecs.Type.KillWithMelee)
                    {
                        _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled + "/" + _zombiesToKill;
                    }
                }
            }
        }


        [PunRPC]
        public void addZombieKilledRPC()
        {
            if (challengeInProgress)
            {
                _zombiesKilled++;
                if (activeChallenge.ChallengeType == ScObChallengesSpecs.Type.Sharpshooter ||
                    activeChallenge.ChallengeType == ScObChallengesSpecs.Type.KillInTime ||
                    activeChallenge.ChallengeType == ScObChallengesSpecs.Type.KillInArea ||
                    activeChallenge.ChallengeType == ScObChallengesSpecs.Type.KillWithAim ||
                    activeChallenge.ChallengeType == ScObChallengesSpecs.Type.KillWithMelee)
                {
                    _killedZombiesText.text = "Zumbis Mortos: " + _zombiesKilled + "/" + _zombiesToKill;
                }
            }
        }
    
        [PunRPC]
        public void missedShotRPC()
        {
            if(_sharpshooterSetup)
                _missedShot = true;
        }

        public void missedShot()
        {
            if (_isOnline)
                photonView.RPC("missedShotRPC", RpcTarget.MasterClient);
            else
            {
                if (_sharpshooterSetup)
                    _missedShot = true;
            }
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

            if (_isOnline)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    for(int i=0; i < challengeMachinesToSpawn; i++)
                    {
                        int randomSpawnIndex = Random.Range(0, _possibleChallengeMachineSpawnPoints.Count);
                        int randomMachineIndex = Random.Range(0, challengeMachinesPrefabs.Count);
                        GameObject newChallengeMachine = PhotonNetwork.Instantiate(challengeMachinesPrefabsStringNames[randomMachineIndex], _possibleChallengeMachineSpawnPoints[randomSpawnIndex].position, _possibleChallengeMachineSpawnPoints[randomSpawnIndex].rotation);
                        _possibleChallengeMachineSpawnPoints.RemoveAt(randomSpawnIndex);
                        challengeMachines.Add(newChallengeMachine.GetComponent<ChallengeMachine>());
                    }
                }
            }
            else
            {

                for (int i = 0; i < challengeMachinesToSpawn; i++)
                {
                    int randomSpawnIndex = Random.Range(0, _possibleChallengeMachineSpawnPoints.Count);
                    int randomMachineIndex = Random.Range(0, challengeMachinesPrefabs.Count);
                    GameObject newChallengeMachine = Instantiate(challengeMachinesPrefabs[randomMachineIndex],
                        _possibleChallengeMachineSpawnPoints[randomSpawnIndex].position,
                        _possibleChallengeMachineSpawnPoints[randomSpawnIndex].rotation);
                    _possibleChallengeMachineSpawnPoints.RemoveAt(randomSpawnIndex);
                    challengeMachines.Add(newChallengeMachine.GetComponent<ChallengeMachine>());
                }
            }
        }

        public void respawnChallengeMachines()
        {
            if (!challengeInProgress)
            {
                if (_isOnline)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        foreach (var machine in challengeMachines)
                        {
                            PhotonNetwork.Destroy(machine.gameObject);
                        }
                    }
                }
                else
                {
                    foreach (var machine in challengeMachines)
                    {
                        Destroy(machine);
                    }
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
    
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_challengeTimeText.text);
                stream.SendNext(_killedZombiesText.text);
            
  
            
            }
            else
            {
                _challengeTimeText.text = (string)stream.ReceiveNext();
                _killedZombiesText.text = (string)stream.ReceiveNext();

            }
        }
    }
}
