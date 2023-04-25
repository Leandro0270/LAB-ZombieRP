using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    public ScObEnemyStats _status;
    private GameObject hordeManager;
    public GameObject FireDamage;
    public GameObject blood1;
    public GameObject blood2;
    private bool isDead = false;
    private float totalLife;
    private float _life;
    private float _speed;
    private float _damage;
    private bool isSpecial = false;
    [SerializeField] private ZombieAnimationController _animator;
    private EnemyFollow _enemyFollow;
    private SpecialZombiesAttacks _specialZombiesAttacks;
    private int points;
    
    private bool burnTickDamage = true;
    private float burnTickTime = 0;
    private bool isBurning = false;
    private float timeBurning = 0;
    private bool _isSpeedSlowed = false;
    
    
    
    //Special events
    //Explosive enemies===============================================
    [SerializeField] private GameObject[] explosivesPrefabs;
    [SerializeField] private GameObject explosiveDeathEffect;
    private bool isOnExplosiveEvents = false;


    // Start is called before the first frame update
    void Awake()
    {
        _enemyFollow = GetComponent<EnemyFollow>();
        _specialZombiesAttacks = GetComponent<SpecialZombiesAttacks>();
        totalLife = _status.health;
        _life = totalLife;
        _speed = _status.speed;
        _damage = _status.damage;
        isSpecial = _status.isSpecial;
    }

    // Update is called once per frame

    private void Start()
    {
        _enemyFollow.getEnemy().speed = _speed;
        if(isSpecial)
            points = _specialZombiesAttacks.getPoints();
        else
            points = 10;

    }


    private void Update()
    {
        if (isBurning)
        {
            if (burnTickDamage && !isDead)
            {
                takeDamage(_status.burnDamagePerSecond);
                burnTickTime = 0;
                burnTickDamage = false;
            }
            else
            {
                burnTickTime += Time.deltaTime;
                if(burnTickTime >= 1)
                    burnTickDamage = true;
            }
            
            timeBurning -= Time.deltaTime;
            if (timeBurning <= 0)
            {
                isBurning = false;
                FireDamage.SetActive(false);
            }
        }
    }

    public bool takeDamage(float damage)
    {
        _life -= damage;
        if (_specialZombiesAttacks)
        {
            _specialZombiesAttacks.setLife(_life);
        }

        //instancia o objeto blood1 na posição do inimigo
        GameObject NewBloodParticle = Instantiate(blood1, new Vector3(transform.position.x, 57, transform.position.z),
            blood1.transform.rotation);
        //destroi o objeto blood1 depois de 4 segundos
        Destroy(NewBloodParticle, 8f);

        if (_life <= 0)
        {
            killEnemy();
            return true;
        }
        else
        {
            return false;
        }
}

    public void killEnemy()
    {
        if (isOnExplosiveEvents)
        {
            Destroy(GetComponent<CapsuleCollider>());
            GetComponent<BoxCollider>().enabled = true;
            isDead = true;
            GetComponent<EnemyFollow>().setIsAlive(false);
            hordeManager.GetComponent<HordeManager>().decrementZombiesAlive(gameObject);
            GameObject randomExplosive = explosivesPrefabs[UnityEngine.Random.Range(0, explosivesPrefabs.Length)];
            var position = transform.position;
            Instantiate(randomExplosive, position, Quaternion.identity);
            GameObject effectInstantiate = Instantiate(explosiveDeathEffect, position, Quaternion.identity);
            Destroy(effectInstantiate, 2f);
            Destroy(gameObject);
        }
        else
        {

            Destroy(GetComponent<CapsuleCollider>());
            GetComponent<BoxCollider>().enabled = true;
            isDead = true;
            GameObject NewBloodParticle = Instantiate(blood2,
                new Vector3(transform.position.x, 57, transform.position.z),
                blood2.transform.rotation);
            Destroy(NewBloodParticle, 8f);
            _animator.setTarget(false);
            _animator.triggerDown();
            GetComponent<EnemyFollow>().setIsAlive(false);
            hordeManager.GetComponent<HordeManager>().decrementZombiesAlive(gameObject);
            StartCoroutine(waiterToDestroy());
        }

    }
    
    
    public void setNewDestination(Vector3 destination)
    {
        _enemyFollow.setFollowPlayers(false);
        _enemyFollow.setNewDestination(destination);
    }
    
    public void StunEnemy(float time)
    {
        _enemyFollow.setIsStoped(true);
        _enemyFollow.setFollowPlayers(false);
        _enemyFollow.setCanWalk(false);
        StartCoroutine(resetStun(time));
    }
    
    public void burnEnemy(float time)
    {
        isBurning = true;
        timeBurning = time;
        FireDamage.SetActive(true);

    }

    private IEnumerator resetStun(float time)
    {
        yield return new WaitForSeconds(time);
        _enemyFollow.setCanWalk(true);
        _enemyFollow.setIsStoped(false);
        _enemyFollow.setFollowPlayers(true);
    }
    IEnumerator waiterToDestroy()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }

    public void ReceiveTemporarySlow(float time, float speed)
    {
        if (!_isSpeedSlowed)
        {
            _isSpeedSlowed = true;
            float updatedSpeed = _speed - speed;
            float baseSpeed = _speed;
            _speed = updatedSpeed;
            _enemyFollow.setSpeed(updatedSpeed);
            StartCoroutine(resetTemporarySpeed(time, baseSpeed));
        }
    }
    
    

    public bool getIsSpecial()
    {
        return isSpecial;
    }

    public float get_life()
    {
        return _status.health;
    }

    public bool isDeadEnemy()
    {
        return isDead;
    }

    public float getDamage()
    {
        return _damage;
    }
    
    public EnemyFollow getEnemyFollow()
    {
        return _enemyFollow;
    }


    public void ReceiveTemporaryDamage(float time, float damage)
    {

        float updatedDamage = _damage + damage;
        float baseDamage = _damage;
        _damage = updatedDamage;
        StartCoroutine(resetTemporaryDamage(time, baseDamage));
    }

    private IEnumerator resetTemporaryDamage(float time, float baseDamage)
    {
        yield return new WaitForSeconds(time);
        _damage = baseDamage;
    }

    public void ReceiveTemporarySpeed(float time, float speed)
    {
        float updatedSpeed = _speed + speed;
        float baseSpeed = _speed;
        _enemyFollow.setSpeed(updatedSpeed);
        StartCoroutine(resetTemporarySpeed(time, baseSpeed));
    }
    private IEnumerator resetTemporarySpeed(float time, float baseSpeed)
    {
          yield return new WaitForSeconds(time);
            _speed = baseSpeed;
            _enemyFollow.setSpeed(baseSpeed);
    }
    
    public void PermanentDamage(float damage)
    {
        _damage += damage;
    }
    
    public void PermanentSpeed(float speed)
    {
        _speed += speed;
        _enemyFollow.getEnemy().speed = _speed;
    }
    
    public void receiveLife(float life)
    {
        _life += life;
        if (_life > totalLife)
        {
            _life = totalLife;
        }
    }
    
    public void setTotalLife(float life)
    {
        totalLife = life;
    }
    public void setCurrentLife(float life)
    {
        _life = life;
    }
    
    public SpecialZombiesAttacks getSpecialZombieAttack()
    {
        return _specialZombiesAttacks;
    }
    public int getPoints()
    {
        return points;
    }
    
    public void setHordeManager(GameObject hordeManager)
    {
        this.hordeManager = hordeManager;
    }
    
    public void setExplosiveZombieEvent(bool isOnExplosiveEvents)
    {
        this.isOnExplosiveEvents = isOnExplosiveEvents;
    }
    
}
