using System;
using System.Collections;
using Photon.Pun;
using Runtime.Câmera.MainCamera;
using Runtime.Challenges;
using Runtime.Enemy.ZombieCombat.EnemyStatus;
using Runtime.Player.Animation;
using Runtime.Player.Combat.Gun;
using Runtime.Player.Combat.Throwables;
using Runtime.Player.Movement;
using Runtime.Player.PlayerCustomization;
using Runtime.Player.Points;
using Runtime.Player.ScriptObjects.Combat;
using UI;
using UI.Life;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Runtime.Player.Combat.PlayerStatus
{
    public class PlayerStats : MonoBehaviourPunCallbacks, IPunObservable
    {
    
        [Header("=======================REQUIRED SCRIPTS=======================")]
        [Space (20)]
        [SerializeField] private PlayerAnimationManager playerAnimationManager;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private ReviveScript reviveScript;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private PlayerRotation playerRotation;
        [SerializeField] private WeaponSystem weaponSystem;
        [SerializeField] private PlayerPoints playerPoints;
        [SerializeField] private ThrowablePlayerStats throwablePlayerStats;
        [SerializeField] private DecalProjector playerIndicator;
        [SerializeField] private BoxCollider boxCollider;
        [SerializeField] private CustomizePlayerInGame customizePlayerInGame;
        [Space (20)]
        [Header("=======================REQUIRED PREFABS=======================")]
        [Space (20)]
        //VISUAL
        [SerializeField] private GameObject[] lessBloodPrefabs;
        [SerializeField] private GameObject[] moreBloodPrefabs;
        [SerializeField] private GameObject playerUI;
        [SerializeField] private float bloodDispersion = 4;
        
        [Space (20)]
        [Header("=======================GAMEOBJECTS REFERENCES=======================")]
        [Space (20)]
        [SerializeField] private GameObject fireEffect;
        
        
        [Space (20)]
        [Header("=======================OTHER SETTINGS REFERENCES=======================")]
        [Space (20)]
        [SerializeField] private float delayBlood = 1f;
        [SerializeField] private bool isOnline;

   
        //Internal variables =================================================================
        
        //Required Scripts
        private MainGameManager _mainGameManager;
        private CmGameplay _camera;
        private VendingMachineHorderGenerator _vendingMachineHordeGenerator;
        
        
        //Player specs
        private ScObPlayerStats _playerStatus;
        private string _name;
        private bool 
        _isBurning,
        _isStunned,
        _isDown,
        _isDead,
        _interacting,
        _stopDeathLife,
        _setupColorComplete,
        _isIncapacitated,
        _isSpeedSlowed;

        private float _totalLife,
            _speed,
            _revivalSpeed;
        
        private int _maxThrowables;
        
        private Color _characterColor;
            
        //In game variables
        private float _currentLife,
            _downLife = 100f,
            _burnTickTime,_timeBurning;
        private EnemyStatus _enemyIncapacitator;
        private float _delayBloodTimer;
        
        //Player Look Direction
        private bool _isWalkingForward;
        private bool _isWalkingBackward;
        private bool _isWalkingLeft;
        private bool _isWalkingRight;
        private bool _isIdle;
            
            
        //instance variables
        private HealthBar_UI _healthBarUi;
        private PlayerHeadUiHandler _playerHeadUiHandler;
        
        //ChallengeManager Variables
        private bool _challengeInProgress;
        private ChallengeManager _challengeManager;





//======================================================================================================

        private void Start()
        {
            _mainGameManager = GameObject.Find("GameManager").GetComponent<MainGameManager>();
            _InitializePlayerSpecs();
        }

        private void ReduceDownedPlayerLife()
        {
            _downLife -= Time.deltaTime;
            _healthBarUi.SetHealth(_downLife);
            if(_downLife <= 0)
                PlayerDeath();
        }
        private void Update()
        {
            if(!_setupColorComplete)
            {
                if (_healthBarUi is null) return;
                
                    if (_healthBarUi.GetPlayerSetupColor() != _characterColor)
                    {
                        _healthBarUi.SetupPlayerColor(_characterColor);
                        _setupColorComplete = true;
                    }
                
            }
            
            
            if (_isDown && !_isDead && !_stopDeathLife)
            {
                ReduceDownedPlayerLife();
            }
        

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
                if (_burnTickTime >= 1)
                {
                    TakeDamage(_playerStatus.burnDamagePerSecond, false);
                    _burnTickTime = 0;
                }
                else
                    _burnTickTime += Time.deltaTime;
                
            
                _timeBurning -= Time.deltaTime;
                if (_timeBurning <= 0)
                {
                    fireEffect.SetActive(false);
                    _isBurning = false;
                }
            }

            if (isOnline && !photonView.IsMine)
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
                stream.SendNext(_currentLife);
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
                _currentLife = (float)stream.ReceiveNext();
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
            photonView.RPC("TakeDamage", RpcTarget.Others, damage, isCritical);
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
                _camera.RemoveTargetPlayer(gameObject.transform);
                playerMovement.SetCanMove(false);
                playerRotation.SetCanRotate(false);
                playerAnimationManager.setDowning();
                playerAnimationManager.setDown(true);
                weaponSystem.SetGunVisable(false);
                _healthBarUi.DownPlayer();
                reviveScript.addDownCount();
            }



        }
    
        [PunRPC]
        public void TakeDamage(float damage, bool isCritical)
        {
            if (!photonView.IsMine || _isDown || _isDead) return;
            
            
            if (_challengeInProgress)
                _challengeManager.setTakedHit(true);
            
            _currentLife -= damage;
            _playerHeadUiHandler.TakeDamage();
            if (isOnline)
            {                      
                photonView.RPC("UpdateHealthBar", RpcTarget.All, _currentLife);
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
                _healthBarUi.SetHealth(_currentLife);
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
                

            if (_currentLife < 1)
            {
                DownPlayer(isCritical);
            }
        }

        private void DownPlayer(bool isCritical)
        {
                _isDown = true;
                _healthBarUi.DownPlayer();
                _playerHeadUiHandler.DownPlayer(true);

                if (isOnline)
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
                    _camera.RemoveTargetPlayer(gameObject.transform);
                    playerMovement.SetCanMove(false);
                    playerRotation.SetCanRotate(false);
                    playerAnimationManager.setDowning();
                    playerAnimationManager.setDown(true);
                    weaponSystem.SetGunVisable(false);
                    _healthBarUi.DownPlayer();
                    reviveScript.addDownCount();
                }
                _mainGameManager.removeDownedPlayer(this.gameObject);
            
        }

    
        public void Revived()
        {
            if (!_isDown || _isDead) return;
            
            _playerHeadUiHandler.DownPlayer(false);
            _mainGameManager.addDownedPlayer(gameObject);
            _isIncapacitated = false;
            weaponSystem.SetIsIncapacitated(false);
            characterController.enabled = true;
            boxCollider.enabled = false;
            playerRotation.SetCanRotate(true);
            playerMovement.SetCanMove(true);
            _isDown = false;
            playerAnimationManager.setDown(false);
            _currentLife = _totalLife * 0.3f;
            _camera.AddTargetPlayer(gameObject.transform);
            weaponSystem.SetGunVisable(true);
            _healthBarUi.RevivePlayer();
            if(isOnline)
                photonView.RPC("UpdateHealthBar", RpcTarget.All, _currentLife);
            else
                _healthBarUi.SetHealth(_currentLife);

        }
    

        private void PlayerDeath()
        {
        
            _vendingMachineHordeGenerator.removePlayer(gameObject);
            _isDead = true;
        
        }
    
        public void ReceiveHeal(float heal)
        {
        
            if (isOnline)
            {
                if (photonView.IsMine)
                {
                    if (!_isDown && !_isDead)
                        _currentLife += heal;
                    if (_currentLife > _totalLife)
                        _currentLife = _totalLife;
                }
                photonView.RPC("UpdateHealthBar", RpcTarget.All, _currentLife);
            }
            else
            {
                _healthBarUi.SetHealth(_currentLife);
                if (!_isDown && !_isDead)
                    _currentLife += heal;
                if (_currentLife > _totalLife)
                    _currentLife = _totalLife;
            }

        }
        [PunRPC]
        public void UpdateHealthBar(float lifeOnline)
        {
            _healthBarUi.SetHealth(lifeOnline);
        }
    
        [PunRPC]
        public void UpdateName(string playerName)
        {
            _name = playerName;
        }
        private void _InitializePlayerSpecs()
        {
            _maxThrowables = _playerStatus.maxThrowableCapacity;
            _characterColor = _playerStatus.mainColor;
            if (isOnline)
            {
                photonView.RPC("UpdateName", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
            }
            else
            {
                _name = _playerStatus.nickName;
            }
            _speed = _playerStatus.speed;
            _totalLife = _playerStatus.health;
            _currentLife = _totalLife;
            _revivalSpeed = _playerStatus.revivalSpeed;
            var findHordeManager = GameObject.FindGameObjectWithTag("HorderManager");
            _vendingMachineHordeGenerator = findHordeManager.GetComponent<VendingMachineHorderGenerator>();
            _challengeManager = findHordeManager.GetComponent<ChallengeManager>();
            _vendingMachineHordeGenerator.addPlayer(gameObject);
            _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CmGameplay>();
            _camera.StartMatchAddPlayer(gameObject.transform);
            var findCanvaHud = GameObject.FindGameObjectWithTag("PlayersUiSpawn");
            if (findCanvaHud == null)
                Debug.LogError("Não foi encontrado o Canvas HUD, posicione ele na cena");
            PlayerUiHandler playerUiConfig;
            _playerHeadUiHandler = _mainGameManager.getPlayerHeadUiHandler();
            _playerHeadUiHandler.SetPlayerSkinSpecs(customizePlayerInGame.GetPlayerCustom());
            if (isOnline)
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
                var uiTransform = playerUiConfig.transform;
                uiTransform.parent = findCanvaHud.transform;
                uiTransform.localScale = Vector3.one;
                playerUiConfig.setPlayer(this.gameObject);
            }
            playerIndicator.material = _playerStatus.playerIndicator;
            throwablePlayerStats.setMaxCapacity(_maxThrowables);
        }

        [PunRPC]
        void SetParent(int photonID)
        {
            GameObject child = PhotonView.Find(photonID).gameObject;
            GameObject parent = PhotonView.Find(888).gameObject;

            if (parent == null || child == null) return;
            
            var uiTransform = child.transform;
            uiTransform.parent = parent.transform;
            uiTransform.localScale = Vector3.one;
        }
    
        public void BurnPlayer(float time)
        {
            if (isOnline && photonView.IsMine)
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
            this.isOnline = isOnline;
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
            healthBarUi.SetupPlayerColor(_characterColor);
            healthBarUi.SetMaxHealth(_totalLife);
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
            return _currentLife;
        }
    
        public float GetTotalLife()
        {
            return _totalLife;
        }
    
        public void ReceiveTemporarySlow(float time, float speed)
        {
            if (photonView.IsMine || !isOnline)
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
            if (!isOnline || photonView.IsMine)
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
    
        public PlayerHeadUiHandler GetPlayerHeadUiHandler()
        {
            return _playerHeadUiHandler;
        }
    }
}
