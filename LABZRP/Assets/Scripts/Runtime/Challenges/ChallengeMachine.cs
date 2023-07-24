using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ChallengeMachine : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform ModelSpawnPoint;
    private ChallengeManager _challengeManager;
    private bool _isActivated = true;
    private bool _isStarted = false;
    private bool _isCompleted = false;
    [SerializeField] private ScObChallengesSpecs[] _challenges;
    [Range(1, 3)]
    [SerializeField] private int difficulty = 1;
    private int _currentChallenge = 0;
    private int _currentWave = 0;
    private int _currentEnemy = 0;
    private int _currentEnemySpawned = 0;
    private GameObject _current3dModel;
    [SerializeField] private TextMeshPro ScreenPoints;
    [SerializeField] private bool changeChallenges = false;
    [SerializeField] private bool isOnline = false;
    [SerializeField] private PhotonView photonView;


    private void Start()
    {
        _challengeManager = GameObject.FindGameObjectWithTag("HorderManager").GetComponent<ChallengeManager>();
        if(!isOnline || PhotonNetwork.IsMasterClient){
            InitializeChallengeMachine();
        }
        
    }

    private void Update()
    {
        if (changeChallenges)
        {
            DebugRandomizeChallenge();
        }
    }

    public void InitializeChallengeMachine()
    {
        _currentChallenge = Random.Range(0, _challenges.Length);
        if(isOnline){
            if(_current3dModel)
                photonView.RPC("Model3dChallengeMachine", RpcTarget.All, true, _currentChallenge);
            else
            {
                photonView.RPC("Model3dChallengeMachine", RpcTarget.All, false, _currentChallenge);
            }

        }
        else
        {
            if (_current3dModel)
                Model3dChallengeMachine(true, _currentChallenge);
            else
                Model3dChallengeMachine(false, _currentChallenge);
        }
    }

    [PunRPC]
    public void Model3dChallengeMachine(bool destroy, int _currentChallengeRPC)
    {
        if(destroy)
            Destroy(_current3dModel);
        else
        {
            _current3dModel = Instantiate(_challenges[_currentChallengeRPC].Model3dChallengeMachine,
                ModelSpawnPoint.position, ModelSpawnPoint.rotation);
            _current3dModel.transform.parent = transform;
            ScreenPoints.text = _challenges[_currentChallengeRPC].ChallengeReward.ToString();
        }
    }


    private void OnTriggerStay(Collider other)
    {
        if (!isOnline || PhotonNetwork.IsMasterClient)
        {
            PlayerStats playerStats = other.GetComponent<PlayerStats>();
            if (playerStats && _isActivated)
            {
                if (playerStats.getInteracting() && !playerStats.verifyDown())
                {
                    if (_challengeManager.StartChallenge(_challenges[_currentChallenge], this))
                    {
                        _isActivated = false;
                        _isStarted = true;
                        ScObChallengesSpecs challenge = _challenges[_currentChallenge];
                        challenge.zombiesToKill = (_challenges[_currentChallenge].zombiesToKill * difficulty) +
                                                  (_challengeManager.getPlayerCount() * 15);
                        challenge.ChallengeReward = (_challenges[_currentChallenge].ChallengeReward * difficulty);
                        _challengeManager.StartChallenge(_challenges[_currentChallenge], this);

                    }
                }
            }
        }
    }

    public void setIsActivated(bool value)
    {
        if (isOnline)
            photonView.RPC("setIsActivatedRPC", RpcTarget.All, value);
        else
            _isActivated = value;
    }
    
    [PunRPC]
    public void setIsActivatedRPC(bool value)
    {
        _isActivated = value;
    }

    private void DebugRandomizeChallenge()
    {
        changeChallenges = false;
        InitializeChallengeMachine();
    }
    
}
