using System;
using System.Collections;
using Photon.Pun;
using Runtime.Câmera.MainCamera;
using Runtime.Challenges;
using Runtime.Enemy.ZombieCombat.EnemyStatus;
using Runtime.Player.Combat.Throwables;
using Runtime.Player.Movement;
using Runtime.Player.Points;
using Runtime.Player.ScriptObjects.Combat;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Runtime.Player.Combat.PlayerStatus
{
    public class PlayerStats : MonoBehaviourPun, IPunObservable
    {
    
        //VISUAL
        public float delayBlood = 1f;
        private float _delayBloodTimer;
        [SerializeField] private GameObject[] lessBloodPrefabs;
        [SerializeField] private GameObject[] moreBloodPrefabs;
        [SerializeField] private PlayerAnimationManager playerAnimationManager;
        [SerializeField] private GameObject fireEffect;
        [SerializeField] private float bloodDispersion = 4;
    
        //Player Specs
   
        private ScObPlayerStats _playerStatus;
        private string _name;
        private bool _isBurning;
        private bool _isStunned;
        private bool _isDown;
        private bool _isDead;
        public float totalLife;
        public float life;
        private float _downLife = 100f;
        private float _speed;
        private float _revivalSpeed;
        private float _timeBetweenMelee;
        private float _meleeDamage;
        private bool _interacting;
        private bool _stopDeathLife;
        private bool _setupColorComplete;
        private bool _isIncapacitated;
        private bool _isSpeedSlowed;
        private int _maxThrowables;
        private bool _isWalkingForward;
        private bool _isWalkingBackward;
        private bool _isWalkingLeft;
        private bool _isWalkingRight;
        private bool _isIdle;
        private bool _burnTickDamage = true;
        private float _burnTickTime;
        private float _timeBurning;
        private EnemyStatus _enemyIncapacitator;
        //======================================================================================================
        //UI
        private HealthBar_UI _healthBarUi;
        [SerializeField] private GameObject playerUI;
        private Color _characterColor;
        [SerializeField] private GameObject playerHead;
        //======================================================================================================
        //Script components
        [SerializeField] private CharacterController characterController;
        [SerializeField] private ReviveScript reviveScript;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerRotation playerRotation;
        [SerializeField] private WeaponSystem weaponSystem;
        [SerializeField] private PlayerPoints playerPoints;
        [SerializeField] private ThrowablePlayerStats throwablePlayerStats;
        [SerializeField] private DecalProjector playerIndicator;
        [SerializeField] private BoxCollider boxCollider;
        private MainGameManager _mainGameManager;
        private CameraMovement _camera;
        private VendingMachineHorderGenerator _vendingMachineHordeGenerator;

        //======================================================================================================
        //ChallengeManager Variables
        private bool _challengeInProgress;
        private ChallengeManager _challengeManager;
        //NoHitChallenge=========================================
        
        //==================================================================================
        private bool _isOnline;





//======================================================================================================
//Unity base functions

        private void Start()
        {
            _InitializePlayerSpecs();
            _mainGameManager =GameObject.Find("GameManager").GetComponent<MainGameManager>();

        }
    

        private void Update()
        {

            if(!_setupColorComplete)
            {
                if(_healthBarUi){
                    if (_healthBarUi.getColor() != _characterColor)
                    {
                    
                        _healthBarUi.setColor(_characterColor);
                        _setupColorComplete = true;
                    }
                }
            }
            if (_isDown && !_isDead && !_stopDeathLife)
            {
                _healthBarUi.setColor(Color.gray);
                _healthBarUi.SetHealth((int)_downLife);
                _downLife -= Time.deltaTime;
            
            }
            if (_downLife <= 0)
            {
                PlayerDeath();
            }
        
            if(_isDown)
                if(_healthBarUi.getColor() != _characterColor)
                    _healthBarUi.setColor(Color.gray);
            if(_delayBloodTimer > 0)
                _delayBloodTimer -= Time.deltaTime;
            if (_isIncapacitated)
            {
                if (!_isDown)
                {
                    if (_enemyIncapacitator == null || _enemyIncapacitator.IsDeadEnemy())
                    {
                        CapacitatePlayer();
                    }
                } 
            }
            if (_isBurning)
            {
                fireEffect.SetActive(true);
                if (_burnTickDamage)
                {
                    TakeDamage(_playerStatus.burnDamagePerSecond, false);
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
                    fireEffect.SetActive(false);
                    _isBurning = false;
                }
            }

            if (_isOnline && !photonView.IsMine)
            {
                playerAnimationManager.setIsWalkingForward(_isWalkingForward);
                playerAnimationManager.setIsWalkingBackward(_isWalkingBackward);
                playerAnimationManager.setIsWalkingLeft(_isWalkingLeft);
                playerAnimationManager.setIsWalkingRight(_isWalkingRight);
                playerAnimationManager.setIsIdle(_isIdle);
            }
        }

//======================================================================================================
//Main functions


        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(life);
                stream.SendNext(_isDead);
                stream.SendNext(_isDown);
                stream.SendNext(_isIncapacitated);
                stream.SendNext(_downLife);
                stream.SendNext(_interacting);
                stream.SendNext(_isWalkingForward);
                stream.SendNext(_isWalkingBackward);
                stream.SendNext(_isWalkingLeft);
                stream.SendNext(_isWalkingRight);
                stream.SendNext(_isSpeedSlowed);
                stream.SendNext(_isStunned);
                stream.SendNext(_isBurning);

            }
            else
            {
                life = (float)stream.ReceiveNext();
                _isDead = (bool)stream.ReceiveNext();
                _isDown = (bool)stream.ReceiveNext();
                _isIncapacitated = (bool)stream.ReceiveNext();
                _downLife = (float)stream.ReceiveNext();
                _interacting = (bool)stream.ReceiveNext();
                _isWalkingForward = (bool)stream.ReceiveNext();
                _isWalkingBackward = (bool)stream.ReceiveNext();
                _isWalkingLeft = (bool)stream.ReceiveNext();
                _isWalkingRight = (bool)stream.ReceiveNext();
                _isSpeedSlowed = (bool)stream.ReceiveNext();
                _isStunned = (bool)stream.ReceiveNext();
                _isBurning = (bool)stream.ReceiveNext();

            }
        }

        public void TakeOnlineDamage(float damage, bool isCritical)
        {
            photonView.RPC("TakeDamage", RpcTarget.All, damage, isCritical);
        }

        [PunRPC]
        public void InstantiateBlood(Vector3 spawnPosition, bool isDown, bool isCritical)
        {
            if (!isDown)
            {
                if (_delayBloodTimer <= 0)
                {
                    _delayBloodTimer = delayBlood;
                    int randomLessBloodIndex = Random.Range(0, lessBloodPrefabs.Length);
                    GameObject blood1 = Instantiate(lessBloodPrefabs[randomLessBloodIndex], spawnPosition,
                        lessBloodPrefabs[randomLessBloodIndex].transform.rotation);
                    Destroy(blood1, 15f);
                }

            }
            else
            {
                int randomMoreBloodIndex = Random.Range(0, moreBloodPrefabs.Length);
                Vector3 position2 = transform.position;
                GameObject blood2 = Instantiate(moreBloodPrefabs[randomMoreBloodIndex],
                    new Vector3(position2.x, position2.y - 2f, position2.z),
                    moreBloodPrefabs[randomMoreBloodIndex].transform.rotation);
                Destroy(blood2, 15f);
                weaponSystem.SetIsIncapacitated(true);
                characterController.enabled = false;
                boxCollider.enabled = true;
                _camera.removePlayer(gameObject);
                playerMovement.SetCanMove(false);
                playerRotation.SetCanRotate(false);
                playerAnimationManager.setDowning();
                playerAnimationManager.setDown(true);
                weaponSystem.SetGunVisable(false);
                _healthBarUi.setColor(Color.gray);
                reviveScript.addDownCount();
            }



        }
    
        [PunRPC]
        public void TakeDamage(float damage, bool isCritical)
        {
            if(photonView.IsMine){
            
                if (!_isDown && !_isDead)
                {
                    if (_challengeInProgress)
                    {
                        _challengeManager.setTakedHit(true);
                    }
                    life -= damage;
              
                    if (_isOnline)
                    {                      
                        photonView.RPC("UpdateHealthBar", RpcTarget.All, life);
                        if (!_isBurning)
                        {
                            float y = Random.Range(-bloodDispersion, bloodDispersion);
                            float x = Random.Range(-bloodDispersion, bloodDispersion);
                            var position = transform.position;
                            Vector3 spawnPosition = new Vector3(position.x + y, position.y - 2f,
                                position.z + x);
                            photonView.RPC("InstantiateBlood", RpcTarget.All, spawnPosition, false, isCritical);
                        }
                    }
                    else
                    {
                        _healthBarUi.SetHealth((int)life);
                        if (!_isBurning)
                        {
                            float y = Random.Range(-bloodDispersion, bloodDispersion);
                            float x = Random.Range(-bloodDispersion, bloodDispersion);
                            Vector3 spawnPosition = new Vector3(transform.position.x + y, transform.position.y - 2f,
                                transform.position.z + x);
                            if (_delayBloodTimer <= 0)
                            {
                                int randomLessBloodIndex = Random.Range(0, lessBloodPrefabs.Length);
                                _delayBloodTimer = delayBlood;
                                GameObject blood1 = Instantiate(lessBloodPrefabs[randomLessBloodIndex], spawnPosition, lessBloodPrefabs[randomLessBloodIndex].transform.rotation);
                                Destroy(blood1, 15f);
                            }
                        }
                    }
                

                    if (life < 1)
                    {
                        _isDown = true;
                        if (_isOnline)
                        {
                            Vector3 spawnPoint = new Vector3(transform.position.x, transform.position.y - 2f, transform.position.z);
                            photonView.RPC("InstantiateBlood", RpcTarget.All, spawnPoint, true, isCritical);
                        }
                        else
                        {
                            int randomMoreBloodIndex = Random.Range(0, moreBloodPrefabs.Length);
                        
                            GameObject _blood2 = Instantiate(moreBloodPrefabs[randomMoreBloodIndex],
                                new Vector3(transform.position.x, transform.position.y - 2f, transform.position.z),
                                moreBloodPrefabs[randomMoreBloodIndex].transform.rotation);
                            Destroy(_blood2, 15f);
                            weaponSystem.SetIsIncapacitated(true);
                            characterController.enabled = false;
                            boxCollider.enabled = true;
                            _camera.removePlayer(gameObject);
                            playerMovement.SetCanMove(false);
                            playerRotation.SetCanRotate(false);
                            playerAnimationManager.setDowning();
                            playerAnimationManager.setDown(true);
                            weaponSystem.SetGunVisable(false);
                            _healthBarUi.setColor(Color.gray);
                            reviveScript.addDownCount();
                        }
                        _mainGameManager.removeDownedPlayer(this.gameObject);
                    }
                }
            }
        }
    
    

    
        public void Revived()
        {
            if (_isDown && !_isDead)
            {
                _mainGameManager.addDownedPlayer(gameObject);
                _isIncapacitated = false;
                weaponSystem.SetIsIncapacitated(false);
                characterController.enabled = true;
                boxCollider.enabled = false;
                playerRotation.SetCanRotate(true);
                playerMovement.SetCanMove(true);
                _isDown = false;
                playerAnimationManager.setDown(false);
                life = totalLife * 0.3f;
                _camera.addPlayer(gameObject);
                weaponSystem.SetGunVisable(true);
                _healthBarUi.setColor(_characterColor);
                if(_isOnline)
                    photonView.RPC("UpdateHealthBar", RpcTarget.All, life);
                else
                    _healthBarUi.SetHealth((int)life);
            }
        
        }
    

        private void PlayerDeath()
        {
        
            _vendingMachineHordeGenerator.removePlayer(gameObject);
            _isDead = true;
        
        }
    
        public void ReceiveHeal(float heal)
        {
        
            if (_isOnline)
            {
                if (photonView.IsMine)
                {
                    if (!_isDown && !_isDead)
                        life += heal;
                    if (life > totalLife)
                        life = totalLife;
                }
                photonView.RPC("UpdateHealthBar", RpcTarget.All, life);
            }
            else
            {
                _healthBarUi.SetHealth((int)life);
                if (!_isDown && !_isDead)
                    life += heal;
                if (life > totalLife)
                    life = totalLife;
            }

        }
        [PunRPC]
        public void UpdateHealthBar(float lifeOnline)
        {
            _healthBarUi.SetHealth((int)lifeOnline);
        }
    
        [PunRPC]
        public void UpdateName(string playerName)
        {
            _name = playerName;
        }
        private void _InitializePlayerSpecs()
        {
            _maxThrowables = _playerStatus.maxThrowableCapacity;
            _characterColor = _playerStatus.MainColor;
            if (_isOnline)
            {
                photonView.RPC("UpdateName", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
            }
            else
            {
                _name = _playerStatus._nickName;
            }
            _speed = _playerStatus.speed;
            totalLife = _playerStatus.health;
            life = totalLife;
            _revivalSpeed = _playerStatus.revivalSpeed;
            _timeBetweenMelee = _playerStatus.timeBeteweenMelee;
            _meleeDamage = _playerStatus.meleeDamage;
            var findHordeManager = GameObject.FindGameObjectWithTag("HorderManager");
            _vendingMachineHordeGenerator = findHordeManager.GetComponent<VendingMachineHorderGenerator>();
            _challengeManager = findHordeManager.GetComponent<ChallengeManager>();
            _vendingMachineHordeGenerator.addPlayer(gameObject);
            _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMovement>();
            _camera.addPlayer(gameObject);
            var findCanvaHud = GameObject.FindGameObjectWithTag("PlayersUiSpawn");
            if (findCanvaHud == null)
                Debug.LogError("Não foi encontrado o Canvas HUD, posicione ele na cena");
            PlayerUiHandler playerUiConfig;
            if (_isOnline)
            {
                if (photonView.IsMine)
                {
                    playerUiConfig = PhotonNetwork
                        .Instantiate("PlayerUI", findCanvaHud.transform.position, Quaternion.identity)
                        .GetComponent<PlayerUiHandler>();
                    playerUiConfig.setOnlinePlayer(this.gameObject);
                    var photonID = playerUiConfig.GetComponent<PhotonView>().ViewID;
                    // Invoke the RPC to set the parent on all clients
                    photonView.RPC("SetParent", RpcTarget.All, photonID);
                }

            }
            else
            {
                playerUiConfig =
                    Instantiate(playerUI, findCanvaHud.transform.position,Quaternion.identity).GetComponent<PlayerUiHandler>();
                playerUiConfig.transform.parent = findCanvaHud.transform;
                playerUiConfig.setPlayer(this.gameObject);
            }
            playerIndicator.material = _playerStatus.PlayerIndicator;
            throwablePlayerStats.setMaxCapacity(_maxThrowables);
        }

        [PunRPC]
        void SetParent(int photonID)
        {
            GameObject child = PhotonView.Find(photonID).gameObject;
            GameObject parent = PhotonView.Find(888).gameObject;
    
            if (parent != null && child != null)
            {
                child.transform.parent = parent.transform;
            }
        }
    
        public void BurnPlayer(float time)
        {
            if (_isOnline && photonView.IsMine)
            {
                _isBurning = true;
                _timeBurning = time;
            }
        }

        public void incapacitateOnline(int enemyPhotonID)
        {
            if (photonView.IsMine)
            {
                var enemyIncapacitator = PhotonView.Find(enemyPhotonID).gameObject.GetComponent<EnemyStatus>();
                IncapacitatePlayer(enemyIncapacitator);
            }
        }

        public void OnlineCharacterController(bool isEnabled)
        {
            photonView.RPC("OnlineCharacterControllerRPC", RpcTarget.All, isEnabled);
        }

        [PunRPC]
        public void OnlineCharacterControllerRPC(bool isEnabled)
        {
            if (photonView.IsMine)
            {
                characterController.enabled = isEnabled;
            }
        }

        public void ChangeTransformParent(int photonIDParent, bool resetVector3, bool changeToNull)
    
        {
            if (PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("ChangeTransformParentRPC", RpcTarget.All, photonIDParent, resetVector3, changeToNull);
            }
        }
    
        [PunRPC]
        public void ChangeTransformParentRPC(int photonIDParent, bool resetVector3, bool changeToNull)
        {
            if (photonView.IsMine)
            {
                if (changeToNull)
                {
                    transform.SetParent(null);
                    Vector3 position = transform.position;
                    position = new Vector3(position.x, 59, position.z);
                    transform.position = position;
                }
                else{
                    GameObject parent = PhotonView.Find(photonIDParent).gameObject;
                    gameObject.transform.SetParent(parent.transform);

                    if (resetVector3)
                        transform.localPosition = Vector3.zero;
                }
            }
        }
        public void setIsOnline(bool isOnline)
        {
            _isOnline = isOnline;
        }
        public void SetMovementAnimationStats(PlayerMovement.PlayerDirection direction)
        {
            float slowPercentage = 0;
            bool isSpeedSlowedByRotation = false;
            switch (direction)
            {
                case PlayerMovement.PlayerDirection.Forward:
                    _isWalkingForward = true;
                    _isWalkingBackward = false;
                    _isWalkingLeft = false;
                    _isWalkingRight = false;
                    _isIdle = false;
                    break;
            
                case PlayerMovement.PlayerDirection.Back:
                    isSpeedSlowedByRotation = true;
                    slowPercentage = 0.8f;
                    _isWalkingForward = false;
                    _isWalkingBackward = true;
                    _isWalkingLeft = false;
                    _isWalkingRight = false;
                    _isIdle = false;
                    break;
            
                case PlayerMovement.PlayerDirection.Left:
                    isSpeedSlowedByRotation = true;
                    slowPercentage = 0.9f;
                    _isWalkingForward = false;
                    _isWalkingBackward = false;
                    _isWalkingLeft = true;
                    _isWalkingRight = false;
                    _isIdle = false;
                    break;
            
                case PlayerMovement.PlayerDirection.Right:
                    isSpeedSlowedByRotation = true;
                    slowPercentage = 0.9f;
                    _isWalkingForward = false;
                    _isWalkingBackward = false;
                    _isWalkingLeft = false;
                    _isWalkingRight = true;
                    _isIdle = false;
                    break;
            
                default:
                    _isWalkingForward = false;
                    _isWalkingBackward = false;
                    _isWalkingLeft = false;
                    _isWalkingRight = false;
                    _isIdle = true;
                    break;
            }
        
            playerMovement.SetRotationSlowPercentage(slowPercentage, isSpeedSlowedByRotation);
            playerAnimationManager.setIsWalkingForward(_isWalkingForward);
            playerAnimationManager.setIsWalkingBackward(_isWalkingBackward);
            playerAnimationManager.setIsWalkingLeft(_isWalkingLeft);
            playerAnimationManager.setIsWalkingRight(_isWalkingRight);
            playerAnimationManager.setIsIdle(_isIdle);

        }
    
        public bool AddItemThrowable(ScObThrowableSpecs throwable)
        {
            return throwablePlayerStats.addThrowable(throwable);
        }
        public void SetHealthBarUi(HealthBar_UI healthBarUi)
        {
            _healthBarUi = healthBarUi;
            healthBarUi.setColor(_characterColor);
            healthBarUi.setMaxHealth((int)totalLife);
        }
        public bool GetIsDown()
        {
            return _isDown;
        }

        public bool GetIsDead()
        {
            return _isDead;
        }
    
        public float GetLife()
        {
            return life;
        }
    
        public float GetTotalLife()
        {
            return totalLife;
        }
    
        public void ReceiveTemporarySlow(float time, float speed)
        {
            if (photonView.IsMine || !_isOnline)
            {
                if (!_isSpeedSlowed)
                {
                    _isSpeedSlowed = true;
                    playerMovement.SetEffectSpeedSlowPercentage(speed,true);
                    StartCoroutine(ResetTemporarySpeed(time ));
                }
            }
        }
        private IEnumerator ResetTemporarySpeed(float time)
        {
            yield return new WaitForSeconds(time);
            _isSpeedSlowed = false;
            playerMovement.SetEffectSpeedSlowPercentage(0, false);

        }

        public void AimSlow(float newSlow, bool isAiming)
        {
            playerMovement.SetAiming(newSlow, isAiming);

        }

        public void InitializePlayerMovementSpeed()
        {
            playerMovement.SetSpeed(_speed);
        }

        public float GetMeleeDamage()
        {
            return _meleeDamage;
        }
    
        public float GetTimeBetweenMelee()
        {
            return _timeBetweenMelee;
        }
    
        public float GetRevivalSpeed()
        {
            return _revivalSpeed;
        }
    
        public void SetInteracting(bool value)
        {
            _interacting = value;
        }
    
        public bool GetInteracting()
        {
            return _interacting;
        }
    
        public void StopDeathCounting(bool value)
        {
            _stopDeathLife = value;
        }
    
        public void SetPlayerStats(ScObPlayerStats stats)
        {
            _playerStatus = stats;
        }
        public float GetSpeed()
        {
            return _speed;
        }
    
        public PlayerPoints GetPlayerPoints()
        {
            return playerPoints;
        }
    
    
        public bool GetIsIncapacitated()
        {
            return _isIncapacitated;
        }

        public void IncapacitatePlayer(EnemyStatus enemy)
        { 
            _enemyIncapacitator = enemy;
            _isIncapacitated = true;
            weaponSystem.SetIsIncapacitated(true);
            characterController.enabled = false;
            playerMovement.SetCanMove(false);
            playerRotation.SetCanRotate(false);
            weaponSystem.SetGunVisable(false);
        }

        [PunRPC]
        public void CapacitateOnlinePlayer()
        {
            if (photonView.IsMine)
            {
                CapacitatePlayer();
            }
        }
        public void CapacitatePlayer()
        {
        
            _isIncapacitated = false;
            weaponSystem.SetIsIncapacitated(false);
            characterController.enabled = true;
            playerMovement.SetCanMove(true);
            playerRotation.SetCanRotate(true);
            weaponSystem.SetGunVisable(true);
        }
    
    
        public void StunPlayer(float time)
        {
            if (!_isOnline || photonView.IsMine)
            {
                _isStunned = true;
                playerMovement.SetCanMove(false);
                playerRotation.SetCanRotate(false);
                weaponSystem.SetIsIncapacitated(true);
                weaponSystem.SetGunVisable(false);
                StartCoroutine(StunPlayerCounting(time));
            }
        }
    
    
        private IEnumerator StunPlayerCounting(float time)
        {
            yield return new WaitForSeconds(time);
            _isStunned = false;
            playerMovement.SetCanMove(true);
            playerRotation.SetCanRotate(true);
            weaponSystem.SetIsIncapacitated(true);
            weaponSystem.SetGunVisable(true);
        }


        public WeaponSystem GetWeaponSystem()
        {
            return weaponSystem;
        }
    
        public ChallengeManager GetChallengeManager()
        {
            return _challengeManager;
        }

        public string GetPlayerName()
        {
            return _name;
        }
        
        public void SetIsNoHitChallenge(bool value)
        {
            _challengeInProgress = value;
        }
    
    }
}
