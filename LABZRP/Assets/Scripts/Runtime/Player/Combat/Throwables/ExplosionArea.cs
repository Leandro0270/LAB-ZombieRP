using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ExplosionArea : MonoBehaviourPunCallbacks
{
    [SerializeField] private ScObThrowableSpecs _throwableSpecs;
    [SerializeField] private bool isOnline = false;
    [SerializeField] private PhotonView photonView;
    [SerializeField] private bool isManualSetup = false;
    private ScObThrowableSpecs.Type _throwableType;
    
    //specs
    private bool _affectEnemies;
    private bool _affectAllies;
    private bool _isDamage;
    private bool _isHeal;
    private bool _isSlowing;
    private bool _isStunning;
    private bool _isPoisoning;
    private bool _isFreezing;
    private bool _isBurning;
    private float _damage;
    private float _health;
    private float _slowDown;
    private float _slowDownDuration;
    private float _stunDuration;
    private float _burnDuration;
    private float _effectDuration;
    private float _poisonDuration;
    private float _freezeDuration;
    
    
    //3D
    private GameObject _visualEffect;
    //intern variables
    private float _currentTime;
    private float effectTickTimer = 0;
    private bool _setupComplete = false;


    private void Start()
    {
        if(isManualSetup)
            Setup(_throwableSpecs);
    }

    private void Update()
    {
        if (_setupComplete)
        {
            effectTickTimer += Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(isOnline && !photonView.IsMine)
            return;
        if (_setupComplete)
        {
            if (_affectAllies)
            {
                PlayerStats playerStats = other.gameObject.GetComponent<PlayerStats>();
                if (playerStats)
                {

                    if (_isDamage)
                    {
                        if(isOnline)
                            playerStats.takeOnlineDamage(_damage*0.5f, false);
                        else
                            playerStats.takeDamage(_damage * 0.5f, false);
                    }

                    if (_isHeal)
                    {
                        playerStats.ReceiveHeal(_health);
                    }

                    if (_isSlowing)
                    {
                        playerStats.ReceiveTemporarySlow(_slowDownDuration, _slowDown);

                    }
                    
                    if (_isStunning)
                    {
                        playerStats.StunPlayer(_stunDuration);
                    }

                    if (_isBurning)
                    {
                        playerStats.BurnPlayer(_burnDuration);
                    }

                }
            }

            if (_affectEnemies)
            {
                EnemyStatus enemyStatus = other.gameObject.GetComponent<EnemyStatus>();
                if(enemyStatus)
                {
                    if (_isDamage)
                    {
                        enemyStatus.takeDamage(_damage,null, false, false, false);
                    }
                    

                    if (_isSlowing)
                    {

                        enemyStatus.ReceiveTemporarySlow(_slowDownDuration, _slowDown);

                    }

                    if (_isStunning)
                    {
                        enemyStatus.StunEnemy(_stunDuration);
                    }

                    if (_isBurning)
                    {
                        enemyStatus.burnEnemy(_burnDuration);
                    }
                }
            }
        }
    }

    private void OnCollisionStay(Collision collisionInfo)
    {
        if(isOnline && !photonView.IsMine)
            return;
        if (_affectAllies)
        {
            PlayerStats playerStats = collisionInfo.gameObject.GetComponent<PlayerStats>();
            if(playerStats){
                if (_isHeal)
                {
                    if(effectTickTimer >= 1)
                    {
                        if (playerStats)
                        {
                            playerStats.ReceiveHeal(_health);
                        }
                        effectTickTimer = 0;
                    }
                }

                if (_isBurning)
                {
                    playerStats.BurnPlayer(_burnDuration);
                }
            }
        }

        if (_affectEnemies)
        {
            EnemyStatus enemyStatus = collisionInfo.gameObject.GetComponent<EnemyStatus>();
            if (enemyStatus)
            {
                if(_isBurning)
                {
                    enemyStatus.burnEnemy(_burnDuration);
                }
            }
        }
    }


    public void setDamage(float damage)
    {
        _damage = damage;
    }
    // Start is called before the first frame update
    public void Setup(ScObThrowableSpecs throwableSpecs)
    {
        _throwableSpecs = throwableSpecs;
        _isDamage = _throwableSpecs.isDamage;
        _damage = _throwableSpecs.damage;
        _isHeal = _throwableSpecs.isHeal;
        _health = _throwableSpecs.health;
        _isSlowing = _throwableSpecs.isSlowDown;
        _slowDown = _throwableSpecs.slowDown;
        _slowDownDuration = _throwableSpecs.slowDownDuration;
        _isStunning = _throwableSpecs.isStun;
        _stunDuration = _throwableSpecs.stunDuration;
        _isBurning = _throwableSpecs.isBurn;
        _burnDuration = _throwableSpecs.burnDuration;
        _effectDuration = _throwableSpecs.effectDuration;
        _visualEffect = Instantiate(_throwableSpecs.visualEffect, transform.position, transform.rotation);
        _affectAllies = _throwableSpecs.affectAllies;
        _affectEnemies = _throwableSpecs.affectEnemies;
        StartCoroutine(waiter());
        _setupComplete = true;
    }
    
    IEnumerator waiter()
    {
        yield return new WaitForSeconds(_effectDuration);
        Destroy(_visualEffect, 4f);
        Destroy(gameObject);
    }
    
}

