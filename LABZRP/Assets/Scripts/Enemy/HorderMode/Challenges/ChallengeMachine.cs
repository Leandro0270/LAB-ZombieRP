using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ChallengeMachine : MonoBehaviour
{

    [SerializeField] private Transform ModelSpawnPoint;
    private ChallengeManager _challengeManager;
    private bool _isActivated = false;
    private bool _isStarted = false;
    private bool _isCompleted = false;
    private ScObChallengesSpecs[] _challenges;
    private int _currentChallenge = 0;
    private int _currentWave = 0;
    private int _currentEnemy = 0;
    private int _currentEnemySpawned = 0;
    private GameObject _current3dModel;


    private void Start()
    {

        _challengeManager = GameObject.FindGameObjectWithTag("HorderManager").GetComponent<ChallengeManager>();
        _challengeManager.addChallengeMachine(this);
        InitializeChallengeMachine();
        
    }

    public void InitializeChallengeMachine()
    {
        _currentChallenge = Random.Range(0, _challenges.Length);
        _current3dModel = Instantiate(_challenges[_currentChallenge].Model3dChallengeMachine, ModelSpawnPoint.position, ModelSpawnPoint.rotation);
    }


    private void OnTriggerStay(Collider other)
    {
        PlayerStats playerStats = other.GetComponent<PlayerStats>();
        if(playerStats && _isActivated)
        {
            if (playerStats.getInteracting() && !playerStats.verifyDown())
            {
                if(_challengeManager.StartChallenge(_challenges[_currentChallenge], this))
                {
                    _isActivated = false;
                    _isStarted = true;
                    _challengeManager.addChallengeMachine(this);

                }
            }
        }
    }

    public void setIsActivated(bool value)
    {
        _isActivated = value;
    }
}
