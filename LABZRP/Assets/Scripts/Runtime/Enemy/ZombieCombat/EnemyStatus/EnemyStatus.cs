using System.Collections;
using Photon.Pun;
using Runtime.Enemy.Animation;
using Runtime.Enemy.HorderMode;
using Runtime.Enemy.ScriptObjects.EnemyBase;
using Runtime.Enemy.ZombieCombat.ZombieBehaviour;
using Runtime.Player.Combat.Gun;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Enemy.ZombieCombat.EnemyStatus
{
    public class EnemyStatus : MonoBehaviourPunCallbacks, IPunObservable
    {
        [SerializeField] private CapsuleCollider capsuleCollider;
        [SerializeField] private BoxCollider boxCollider;
        [SerializeField] private ScObEnemyStats status;
        private HordeManager _hordeManager;
        [SerializeField] private GameObject fireDamage;
        [SerializeField] private GameObject[] lessBloodPrefabs;
        [SerializeField] private GameObject[] moreBloodPrefabs;
        private bool _isDead;
        private float _totalLife;
        private float _life;
        private float _speed;
        private float _damage;
        private bool _isSpecial;
        [SerializeField] private ZombieAnimationController animator;
        [SerializeField] private EnemyNavMeshFollow enemyNavMeshFollow;
        private SpecialZombiesAttacks.SpecialZombiesAttacks _specialZombiesAttacks;
        private int _points;
        private bool _burnTickDamage = true;
        private float _burnTickTime;
        private bool _isBurning;
        private float _timeBurning;
        private bool _isSpeedSlowed;
        [SerializeField] private bool isOnline;
    
        //Special events
        //Explosive enemies===============================================
        [SerializeField] private GameObject[] explosivesPrefabs;
        [SerializeField] private GameObject explosiveDeathEffect;
        [SerializeField] private string[] randomExplosiveName;
        private bool _isOnExplosiveEvents;


        void Awake()
        {
            _totalLife = status.health;
            _life = _totalLife;
            _speed = status.speed;
            _damage = status.damage;
            _isSpecial = status.isSpecial;
        }

        private void Start()
        {
            enemyNavMeshFollow.getEnemy().speed = _speed;
            if(_isSpecial)
                _points = _specialZombiesAttacks.GetPoints();
            else
                _points = 10;

        }
    
        private void Update()
        {
            if(isOnline && !PhotonNetwork.IsMasterClient)
                return;
        
            if (_isBurning)
            {
                fireDamage.SetActive(true);
                if (_burnTickDamage && !_isDead)
                {
                    TakeDamage(status.burnDamagePerSecond,null,false,false,false);
                    _burnTickTime = 0;
                    _burnTickDamage = false;
                }
                else
                {
                    _burnTickTime += Time.deltaTime;
                    if(_burnTickTime >= 1)
                        _burnTickDamage = true;
                }
            
                _timeBurning -= Time.deltaTime;
                if (_timeBurning <= 0)
                {
                    _isBurning = false;
                    fireDamage.SetActive(false);
                    if(isOnline)
                        photonView.RPC("DisableFireDamageRPC", RpcTarget.All);
                }
            }
        }

    
        [PunRPC]
        public void MasterClientHandleDamageRPC(float damage, int photonId, bool isAiming, bool isMelee, bool isCritical)
        {
            WeaponSystem playerShooter = PhotonView.Find(photonId).GetComponent<WeaponSystem>();
            TakeDamage(damage,playerShooter, isAiming, isMelee, isCritical);
        }

        private void RewardPlayerShooter(WeaponSystem playerShooter, bool isAiming, bool isMelee)
        {

            if (isAiming)
            {
                playerShooter.addKilledZombieWithAim();

            }

            else if (isMelee)
            {
                playerShooter.addKilledZombieWithMelee();

            }

            if (GetIsSpecial())
            {
                int points = GetPoints();
                playerShooter.addKilledSpecialZombie(points);
            }
            else
            {
                playerShooter.addKilledNormalZombie();

            }
        }
    
        [PunRPC]
        public void RewardPlayerShooterRPC(int photonId, bool isAiming, bool isMelee)
        {
            WeaponSystem playerShooter = PhotonView.Find(photonId).GetComponent<WeaponSystem>();
            if (playerShooter.photonView.IsMine)
            {
                if (isAiming)
                {
                    playerShooter.addKilledZombieWithAim();

                }

                else if (isMelee)
                {
                    playerShooter.addKilledZombieWithMelee();

                }

                if (GetIsSpecial())
                {
                    int points = GetPoints();
                    playerShooter.addKilledSpecialZombie(points);
                }
                else
                {
                    playerShooter.addKilledNormalZombie();

                }
            }
        }

        public void TakeDamage(float damage, WeaponSystem playerShooter, bool isAiming, bool isMelee, bool isCritical)
        {
        
            if (!_isBurning)
            {
                int randomLessBlood = Random.Range(0, lessBloodPrefabs.Length);
                Vector3 transformPosition = transform.position;
                GameObject newBloodParticle = Instantiate(lessBloodPrefabs[randomLessBlood],
                new Vector3(transformPosition.x, 57, transformPosition.z),
                    lessBloodPrefabs[randomLessBlood].transform.rotation);
                Destroy(newBloodParticle, 8f);
            }

            if (isOnline && !PhotonNetwork.IsMasterClient)
            {
                var photonId = playerShooter.photonView.ViewID;
                photonView.RPC("MasterClientHandleDamageRPC", RpcTarget.MasterClient, damage, photonId, isAiming, isMelee, isCritical);
            }
            else
            {
                _life -= damage;
                if (_specialZombiesAttacks)
                {
                    _specialZombiesAttacks.SetLife(_life);
                }
            }
        
            if (_life <= 0)
            {
                if (playerShooter != null)
                {
                
                    KillEnemy();
                    if (isOnline)
                    {
                        var photonId = playerShooter.photonView.ViewID;
                        photonView.RPC("RewardPlayerShooterRPC", RpcTarget.All, photonId, isAiming,
                            isMelee);
                    }
                    else
                        RewardPlayerShooter(playerShooter, isAiming, isMelee);
                    
                }

            }
        
        }

        [PunRPC]
        public void InstantiateExplosiveEffect()
        {
            GameObject effectInstantiate = Instantiate(explosiveDeathEffect,transform.position, Quaternion.identity);
            Destroy(effectInstantiate, 2f);
        }
        public void KillEnemy()
        {
            if (!isOnline || PhotonNetwork.IsMasterClient)
            {
                if (!_isDead)
                {
                    if (isOnline)
                        photonView.RPC("DisableCapsuleColliderRPC", RpcTarget.All);
                    else
                        capsuleCollider.enabled = false;
                    boxCollider.enabled = true;
                    _isDead = true;
                    enemyNavMeshFollow.setIsAlive(false);
                    if (PhotonNetwork.IsMasterClient || !isOnline)
                    {
                        _hordeManager.DecrementZombiesAlive(gameObject);
                    }

                    if (_isOnExplosiveEvents && !_isSpecial)
                    {
                        if (!isOnline || PhotonNetwork.IsMasterClient)
                        {
                            int randomExplosiveIndex = 0;
                            if (isOnline)
                                randomExplosiveIndex = Random.Range(0, randomExplosiveName.Length);
                            else
                                randomExplosiveIndex = Random.Range(0, explosivesPrefabs.Length);

                            GameObject randomExplosive =
                                explosivesPrefabs[Random.Range(0, explosivesPrefabs.Length)];
                            var position = transform.position;

                            if (isOnline)
                            {
                                PhotonNetwork.Instantiate(randomExplosiveName[randomExplosiveIndex], position,
                                    Quaternion.identity);
                                photonView.RPC("InstantiateExplosiveEffect", RpcTarget.All);
                            }
                            else
                            {
                                Instantiate(randomExplosive, position, Quaternion.identity);
                                GameObject effectInstantiate =
                                    Instantiate(explosiveDeathEffect, position, Quaternion.identity);
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
                        if (!_isBurning)
                        {
                            int randomMoreBlood = Random.Range(0, moreBloodPrefabs.Length);
                            GameObject newBloodParticle = Instantiate(moreBloodPrefabs[randomMoreBlood],
                                new Vector3(transform.position.x, 57, transform.position.z),
                                moreBloodPrefabs[randomMoreBlood].transform.rotation);
                            Destroy(newBloodParticle, 8f);
                        }

                        animator.setTarget(false);
                        if (isOnline)
                            photonView.RPC("AnimationHandlerRPC", RpcTarget.All, "triggerDown");
                        else
                            animator.triggerDown();

                        enemyNavMeshFollow.setIsAlive(false);
                        if (PhotonNetwork.IsMasterClient || !isOnline)
                        {
                            StartCoroutine(WaiterToDestroy());
                        }
                    }
                }
            }
        }
    
        [PunRPC]
        public void DisableCapsuleColliderRPC()
        {
            capsuleCollider.enabled = false;
            boxCollider.enabled = true;
            if (_isOnExplosiveEvents || _isBurning) return;
            int randomMoreBlood = Random.Range(0, moreBloodPrefabs.Length);
            GameObject newBloodParticle = Instantiate(moreBloodPrefabs[randomMoreBlood],
                    new Vector3(transform.position.x, 57, transform.position.z),
                    moreBloodPrefabs[randomMoreBlood].transform.rotation);
            Destroy(newBloodParticle, 8f);
            
        }
    
        [PunRPC]
        public void AnimationHandlerRPC(string trigger)
        {
            if(trigger == "triggerDown"){
                animator.triggerDown();
            }

            if (trigger == "setTarget")
            {
                animator.setTarget(false);
            }
        
        }
        public void SetNewDestination(Vector3 destination)
        {
            enemyNavMeshFollow.setFollowPlayers(false);
            enemyNavMeshFollow.setNewDestination(destination);
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
                enemyNavMeshFollow.setIsStoped(true);
                enemyNavMeshFollow.setFollowPlayers(false);
                enemyNavMeshFollow.setCanWalk(false);
                StartCoroutine(ResetStun(time));
            
            }
        }

        [PunRPC]
        public void BurnEnemyRPC(float time)
        {
            fireDamage.SetActive(true);
            if (PhotonNetwork.IsMasterClient)
            {
                BurnEnemy(time);

            }
        }
    
        [PunRPC]
        public void DisableFireDamageRPC()
        {
            fireDamage.SetActive(false);
        }
        public void BurnEnemy(float time)
        {
            if (isOnline && !PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("BurnEnemyRPC", RpcTarget.All, time);
            }
            else
            {
                _isBurning = true;
                _timeBurning = time;
            }
        }

        private IEnumerator ResetStun(float time)
        {
            yield return new WaitForSeconds(time);
            enemyNavMeshFollow.setCanWalk(true);
            enemyNavMeshFollow.setIsStoped(false);
            enemyNavMeshFollow.setFollowPlayers(true);
        }
        IEnumerator WaiterToDestroy()
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
                    enemyNavMeshFollow.setSpeed(updatedSpeed);
                    StartCoroutine(ResetTemporarySpeed(time, baseSpeed));
                }
            }
        }

        private bool GetIsSpecial()
        {
            return _isSpecial;
        }

        public float getBaseLife()
        {
            return status.health;
        }

        public bool IsDeadEnemy()
        {
            return _isDead;
        }

        public float GetDamage()
        {
            return _damage;
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_isDead);
                stream.SendNext(_life);
                stream.SendNext(_speed);
                stream.SendNext(_damage);
  
            
            }
            else
            {
                _isDead = (bool)stream.ReceiveNext();
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
            StartCoroutine(ResetTemporaryDamage(time, baseDamage));
        }

        private IEnumerator ResetTemporaryDamage(float time, float baseDamage)
        {
            yield return new WaitForSeconds(time);
            _damage = baseDamage;
        }

        public void ReceiveTemporarySpeed(float time, float speed)
        {
            float updatedSpeed = _speed + speed;
            float baseSpeed = _speed;
            enemyNavMeshFollow.setSpeed(updatedSpeed);
            StartCoroutine(ResetTemporarySpeed(time, baseSpeed));
        }
        private IEnumerator ResetTemporarySpeed(float time, float baseSpeed)
        {
            yield return new WaitForSeconds(time);
            _speed = baseSpeed;
            enemyNavMeshFollow.setSpeed(baseSpeed);
        }
    
        public void PermanentDamage(float damage)
        {
            _damage += damage;
        }
    
        public void PermanentSpeed(float speed)
        {
            _speed += speed;
            enemyNavMeshFollow.getEnemy().speed = _speed;
        }
    
        public void ReceiveLife(float life)
        {
            _life += life;
            if (_life > _totalLife)
            {
                _life = _totalLife;
            }
        }
    
        public void SetTotalLife(float life)
        {
            _totalLife = life;
        }
        public void SetCurrentLife(float life)
        {
            _life = life;
        }
    
        public SpecialZombiesAttacks.SpecialZombiesAttacks GetSpecialZombieAttack()
        {
            return _specialZombiesAttacks;
        }

        private int GetPoints()
        {
            return _points;
        }
    
        public void SetHordeManager(HordeManager hordeManager)
        {
            _hordeManager = hordeManager;
        }
    
        public void SetExplosiveZombieEvent(bool isOnExplosiveEvents)
        {
            _isOnExplosiveEvents = isOnExplosiveEvents;
        }
    
        public void GameIsOver()
        {
            enemyNavMeshFollow.setFollowPlayers(false);
            enemyNavMeshFollow.setCanWalk(false);
            enemyNavMeshFollow.setIsStoped(true);
        
        }
    
    }
}
