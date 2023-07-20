using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class EnemyStatus : MonoBehaviourPunCallbacks, IPunObservable
{
    public ScObEnemyStats _status;
    private HordeManager hordeManager;
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
    
    [SerializeField] private bool isOnline = false;
    [SerializeField] PhotonView photonView;
    
    //Special events
    //Explosive enemies===============================================
    [SerializeField] private GameObject[] explosivesPrefabs;
    [SerializeField] private GameObject explosiveDeathEffect;
    [SerializeField] private string[] randomExplosiveName;
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
        if(isOnline && !PhotonNetwork.IsMasterClient)
            return;
        
        if (isBurning)
        {
            FireDamage.SetActive(true);
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
                if(isOnline)
                    photonView.RPC("disableFireDamageRPC", RpcTarget.All);
            }
        }
    }

    
    [PunRPC]
    public void MasterClientHandleDamageRPC(float damage)
    {
        _life -= damage;
        if (_specialZombiesAttacks)
        {
            _specialZombiesAttacks.setLife(_life);
        }

        if (_life <= 0)
        {
            killEnemy();
        }
    }
    public void takeDamage(float damage)
    {
        if (!isBurning)
        {
            GameObject NewBloodParticle = Instantiate(blood1,
                new Vector3(transform.position.x, 57, transform.position.z),
                blood1.transform.rotation);
            Destroy(NewBloodParticle, 8f);
        }

        if(isOnline && !PhotonNetwork.IsMasterClient)
            photonView.RPC("MasterClientHandleDamageRPC", RpcTarget.MasterClient, damage);
        else
        {
            _life -= damage;
            if (_specialZombiesAttacks)
            {
                _specialZombiesAttacks.setLife(_life);
            }
        }

        if (_life <= 0)
        {
            if(PhotonNetwork.IsMasterClient || !isOnline)
                killEnemy();
        }
    }

    [PunRPC]
    public void instantiateExplosiveEffect()
    {
        GameObject effectInstantiate = Instantiate(explosiveDeathEffect,transform.position, Quaternion.identity);
        Destroy(effectInstantiate, 2f);
    }
    public void killEnemy()
    {
        if (!isOnline || PhotonNetwork.IsMasterClient)
        {
            if (isOnExplosiveEvents)
            {
                if (isOnline)
                    photonView.RPC("disableCapsuleColliderRPC", RpcTarget.All);
                else
                    GetComponent<CapsuleCollider>().enabled = false;
                GetComponent<BoxCollider>().enabled = true;
                isDead = true;
                GetComponent<EnemyFollow>().setIsAlive(false);
                if (PhotonNetwork.IsMasterClient || !isOnline)
                {
                    hordeManager.decrementZombiesAlive(gameObject);
                }

                if (!isOnline || PhotonNetwork.IsMasterClient)
                {
                    int randomExplosiveIndex = UnityEngine.Random.Range(0, explosivesPrefabs.Length);
                    GameObject randomExplosive =
                        explosivesPrefabs[UnityEngine.Random.Range(0, explosivesPrefabs.Length)];
                    var position = transform.position;
                    if (isOnline)
                    {
                        PhotonNetwork.Instantiate(randomExplosiveName[randomExplosiveIndex], position,
                            Quaternion.identity);
                        photonView.RPC("instantiateExplosiveEffect", RpcTarget.All);
                    }
                    else
                    {
                        Instantiate(randomExplosive, position, Quaternion.identity);
                        GameObject effectInstantiate = Instantiate(explosiveDeathEffect, position, Quaternion.identity);
                        Destroy(effectInstantiate, 2f);
                    }
                }

                if (isOnline)
                    PhotonNetwork.Destroy(gameObject);
                else
                    Destroy(gameObject);
            }
            else
            {

                if (isOnline)
                    photonView.RPC("disableCapsuleColliderRPC", RpcTarget.All);
                else
                    GetComponent<CapsuleCollider>().enabled = false;
                GetComponent<BoxCollider>().enabled = true;
                isDead = true;
                if (!isBurning)
                {
                    GameObject NewBloodParticle = Instantiate(blood2,
                        new Vector3(transform.position.x, 57, transform.position.z),
                        blood2.transform.rotation);
                    Destroy(NewBloodParticle, 8f);
                }

                _animator.setTarget(false);
                if (isOnline)
                {
                    photonView.RPC("AnimationHandlerRPC", RpcTarget.All, "triggerDown");
                }
                else
                    _animator.triggerDown();

                GetComponent<EnemyFollow>().setIsAlive(false);
                if (PhotonNetwork.IsMasterClient || !isOnline)
                {
                    hordeManager.decrementZombiesAlive(gameObject);
                    StartCoroutine(waiterToDestroy());
                }
            }
        }
    }
    
    [PunRPC]
    public void disableCapsuleColliderRPC()
    {
        GetComponent<CapsuleCollider>().enabled = false;
    }
    
    [PunRPC]
    public void AnimationHandlerRPC(string trigger)
    {
        if(trigger == "triggerDown"){
            _animator.triggerDown();
        }
    }
    public void setNewDestination(Vector3 destination)
    {
        _enemyFollow.setFollowPlayers(false);
        _enemyFollow.setNewDestination(destination);
    }
    
    [PunRPC]
    public void StunEnemyRPC(float time, float speed)
    {
        StunEnemy(time);
    }
    public void StunEnemy(float time)
    {
        if (isOnline && !PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("StunEnemyRPC", RpcTarget.MasterClient, time);
        }
        else
        {
            _enemyFollow.setIsStoped(true);
            _enemyFollow.setFollowPlayers(false);
            _enemyFollow.setCanWalk(false);
            StartCoroutine(resetStun(time));
            
        }
    }

    [PunRPC]
    public void burnEnemyRPC(float time)
    {
        FireDamage.SetActive(true);
        if (PhotonNetwork.IsMasterClient)
        {
            burnEnemy(time);

        }
    }
    
    [PunRPC]
    public void disableFireDamageRPC()
    {
        FireDamage.SetActive(false);
    }
    public void burnEnemy(float time)
    {
        if (isOnline && !PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("burnEnemyRPC", RpcTarget.All, time);
        }
        else
        {
            isBurning = true;
            timeBurning = time;
        }
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
        if(isOnline && PhotonNetwork.IsMasterClient)
            PhotonNetwork.Destroy(gameObject);
        else if(!isOnline)
            Destroy(gameObject);
    }

    [PunRPC]
    public void ReceiveTemporarySlowRPC(float time, float speed)
    {
        ReceiveTemporarySlow(time, speed);
    }
    public void ReceiveTemporarySlow(float time, float speed)
    {
        if (isOnline && !PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ReceiveTemporarySlowRPC", RpcTarget.MasterClient, time, speed);
        }
        else
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isDead);
            stream.SendNext(_life);
            stream.SendNext(_speed);
            stream.SendNext(_damage);
  
            
        }
        else
        {
            isDead = (bool)stream.ReceiveNext();
            _life = (float)stream.ReceiveNext();
            _speed = (float)stream.ReceiveNext();
            _damage = (float)stream.ReceiveNext();
     
        }
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
    
    public void setHordeManager(HordeManager hordeManager)
    {
        this.hordeManager = hordeManager;
    }
    
    public void setExplosiveZombieEvent(bool isOnExplosiveEvents)
    {
        this.isOnExplosiveEvents = isOnExplosiveEvents;
    }
    
    public void gameIsOver()
    {
        _enemyFollow.setFollowPlayers(false);
        _enemyFollow.setCanWalk(false);
        _enemyFollow.setIsStoped(true);
        
    }
    
}
