using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class PlayerStats : MonoBehaviourPun, IPunObservable
{
    
    //VISUAL
    public float delayBlood = 1f;
    private float _delayBloodTimer; 
    public GameObject blood1;
    public GameObject bloodSplash;
    public GameObject blood2;
    private PlayerAnimationManager _playerAnimationManager;
    public GameObject fireEffect;
    public float dispersaoSangue = 4;
    
    //Player Specs
   
    private ScObPlayerStats _playerStatus;
    private String _name;
    private bool _isAiming;
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
    private bool _stopDeathLife = false;
    private bool _SetupColorComplete = false;
    private bool _isIncapatitated = false;
    private bool _isSpeedSlowed = false;
    private int _maxThrowableItens;
    private int _maxAuxiliaryItens;
    private int _maxGunItens;
    private bool _isWalkingForward = false;
    private bool _isWalkingBackward = false;
    private bool _isWalkingLeft = false;
    private bool _isWalkingRight = false;
    private bool _isIdle = false;
    private bool _burnTickDamage = true;
    private float _burnTickTime = 0;
    private float _timeBurning = 0;
    private GameObject EnemyInCapacitator;
    
    //======================================================================================================
    //UI
    private HealthBar_UI _healthBarUi;
    public GameObject PlayerUI;
    private Color _characterColor;
    [SerializeField] private GameObject _playerHead;
    //======================================================================================================
    //Script components
    private MainGameManager _mainGameManager;
    public DecalProjector _playerIndicator;
    private CameraMovement _camera;
    private PlayerMovement _playerMovement;
    private PlayerRotation _playerRotation;
    private WeaponSystem _weaponSystem;
    private VendingMachineHorderGenerator _vendingMachineHorderGenerator;
    private PlayerPoints _playerPoints;
    private ThrowablePlayerStats _throwablePlayerStats;
    //======================================================================================================
    //ChallengeManager Variables
    private bool _challengeInProgress;
    private ChallengeManager _challengeManager;
    //NoHitChallenge===============================================
    private bool _noHitChallenge;
    //======================================================================================================
    //Other components
    private CharacterController _characterController;
    private bool _isOnline = false;





//======================================================================================================
//Unity base functions

    private void Start()
    {
        _InitializePlayerSpecs();
        _mainGameManager =GameObject.Find("GameManager").GetComponent<MainGameManager>();

    }
    

        private void Update()
    {

        if(!_SetupColorComplete)
        {
            if(_healthBarUi){
                if (_healthBarUi.getColor() != _characterColor)
                {
                    
                    _healthBarUi.setColor(_characterColor);
                    _SetupColorComplete = true;
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
        if (_isIncapatitated)
        {
            if (!_isDown)
            {
                if (EnemyInCapacitator != null || EnemyInCapacitator.GetComponent<EnemyStatus>().isDeadEnemy())
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
                takeDamage(_playerStatus.burnDamagePerSecond);
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
            _playerAnimationManager.setIsWalkingForward(_isWalkingForward);
            _playerAnimationManager.setIsWalkingBackward(_isWalkingBackward);
            _playerAnimationManager.setIsWalkingLeft(_isWalkingLeft);
            _playerAnimationManager.setIsWalkingRight(_isWalkingRight);
            _playerAnimationManager.setIsIdle(_isIdle);
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
            stream.SendNext(_isIncapatitated);
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
            _isIncapatitated = (bool)stream.ReceiveNext();
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

    public void takeOnlineDamage(float damage)
    {
        photonView.RPC("takeDamage", RpcTarget.All, damage);
    }

    [PunRPC]
    public void instantiateBlood(Vector3 spawnPosition, bool isDown)
    {
        if (!isDown)
        {
            GameObject _bloodSplash = Instantiate(bloodSplash, transform.position, transform.rotation);
            Destroy(_bloodSplash, 2f);
            GameObject _blood1 = Instantiate(blood1, spawnPosition, blood1.transform.rotation);
            Destroy(_blood1, 15f);
            
        }
        else
        {
            GameObject _blood2 = Instantiate(blood2,
                new Vector3(transform.position.x, transform.position.y - 2f, transform.position.z),
                blood2.transform.rotation);
            Destroy(_blood2, 15f);
            _weaponSystem.SetIsIncapacitated(true);
            _characterController.enabled = false;
            GetComponent<BoxCollider>().enabled = true;
            _camera.removePlayer(gameObject);
            _playerMovement.setCanMove(false);
            _playerRotation.setCanRotate(false);
            _playerAnimationManager.setDowning();
            _playerAnimationManager.setDown(true);
            _weaponSystem.SetGunVisable(false);
            _healthBarUi.setColor(Color.gray);
            GetComponent<ReviveScript>().addDownCount();
        }



    }
    
    [PunRPC]
    public void takeDamage(float damage)
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
                        photonView.RPC("updateHealthBar", RpcTarget.All, life);
                        if (!_isBurning)
                        {
                            float y = Random.Range(-dispersaoSangue, dispersaoSangue);
                            float x = Random.Range(-dispersaoSangue, dispersaoSangue);
                            Vector3 spawnPosition = new Vector3(transform.position.x + y, transform.position.y - 2f,
                                transform.position.z + x);
                            if (_delayBloodTimer <= 0)
                            {
                                _delayBloodTimer = delayBlood;
                                photonView.RPC("instantiateBlood", RpcTarget.All, spawnPosition, false);
                            }
                        }
                    }
                    else
                    {
                        _healthBarUi.SetHealth((int)life);
                        if (!_isBurning)
                        {
                            float y = Random.Range(-dispersaoSangue, dispersaoSangue);
                            float x = Random.Range(-dispersaoSangue, dispersaoSangue);
                            Vector3 spawnPosition = new Vector3(transform.position.x + y, transform.position.y - 2f,
                                transform.position.z + x);
                            if (_delayBloodTimer <= 0)
                            {
                                _delayBloodTimer = delayBlood;
                                GameObject _blood1 = Instantiate(blood1, spawnPosition, blood1.transform.rotation);
                                Destroy(_blood1, 15f);
                                GameObject _bloodSplash =
                                    Instantiate(bloodSplash, transform.position, transform.rotation);
                                Destroy(_bloodSplash, 2f);
                            }
                        }
                    }
                

                if (life < 1)
                {
                    _isDown = true;
                    if (_isOnline)
                    {
                        Vector3 spawnpoint = new Vector3(transform.position.x, transform.position.y - 2f, transform.position.z);
                        photonView.RPC("instantiateBlood", RpcTarget.All, spawnpoint, true);
                    }
                    else
                    {
                        GameObject _blood2 = Instantiate(blood2,
                            new Vector3(transform.position.x, transform.position.y - 2f, transform.position.z),
                            blood2.transform.rotation);
                        Destroy(_blood2, 15f);
                        _weaponSystem.SetIsIncapacitated(true);
                        _characterController.enabled = false;
                        GetComponent<BoxCollider>().enabled = true;
                        _camera.removePlayer(gameObject);
                        _playerMovement.setCanMove(false);
                        _playerRotation.setCanRotate(false);
                        _playerAnimationManager.setDowning();
                        _playerAnimationManager.setDown(true);
                        _weaponSystem.SetGunVisable(false);
                        _healthBarUi.setColor(Color.gray);
                        GetComponent<ReviveScript>().addDownCount();
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
            _isIncapatitated = false;
            _weaponSystem.SetIsIncapacitated(false);
            _characterController.enabled = true;
            GetComponent<BoxCollider>().enabled = false;
            _playerRotation.setCanRotate(true);
            _playerMovement.setCanMove(true);
            _isDown = false;
            _playerAnimationManager.setDown(false);
            life = totalLife * 0.3f;
            _camera.addPlayer(gameObject);
            _weaponSystem.SetGunVisable(true);
            _healthBarUi.setColor(_characterColor);
            if(_isOnline)
                photonView.RPC("updateHealthBar", RpcTarget.All, life);
            else
                _healthBarUi.SetHealth((int)life);
        }
        
    }
    

    public void PlayerDeath()
    {
        
        _vendingMachineHorderGenerator.removePlayer(gameObject);
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
            photonView.RPC("updateHealthBar", RpcTarget.All, life);
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
    public void updateHealthBar(float lifeOnline)
    {
        _healthBarUi.SetHealth((int)lifeOnline);
    }
    
    [PunRPC]
    public void updateName(string name)
    {
        _name = name;
    }
    private void _InitializePlayerSpecs()
    {
        _weaponSystem = GetComponent<WeaponSystem>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerRotation = GetComponent<PlayerRotation>();
        _playerPoints = GetComponent<PlayerPoints>();
        _characterController = GetComponent<CharacterController>();
        _throwablePlayerStats = GetComponent<ThrowablePlayerStats>();
        _maxThrowableItens = _playerStatus.maxThrowableCapacity;
        _maxGunItens = _playerStatus.maxGunCapacity;
        _maxAuxiliaryItens = _playerStatus.maxAuxiliaryCapacity;
        _characterColor = _playerStatus.MainColor;
        if (_isOnline)
        {
            photonView.RPC("updateName", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
        }
        else
        {
            _name = _playerStatus.name;
        }
        _speed = _playerStatus.speed;
        totalLife = _playerStatus.health;
        life = totalLife;
        _revivalSpeed = _playerStatus.revivalSpeed;
        _timeBetweenMelee = _playerStatus.timeBeteweenMelee;
        _meleeDamage = _playerStatus.meleeDamage;
        _vendingMachineHorderGenerator = GameObject.FindGameObjectWithTag("HorderManager").GetComponent<VendingMachineHorderGenerator>();
        _challengeManager = GameObject.FindGameObjectWithTag("HorderManager").GetComponent<ChallengeManager>();
        _vendingMachineHorderGenerator.addPlayer(gameObject);
        _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMovement>();
        _camera.addPlayer(gameObject);
        _playerAnimationManager = GetComponentInChildren<PlayerAnimationManager>();
        GameObject findCanvaHud = GameObject.FindGameObjectWithTag("PlayersUiSpawn");
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
                int photonID = playerUiConfig.GetComponent<PhotonView>().ViewID;
                // Invoke the RPC to set the parent on all clients
                photonView.RPC("SetParent", RpcTarget.All, photonID);
            }

        }
        else
        {
            playerUiConfig =
                Instantiate(PlayerUI, findCanvaHud.transform).GetComponent<PlayerUiHandler>();
            playerUiConfig.transform.parent = findCanvaHud.transform;
            playerUiConfig.setPlayer(this.gameObject);
        }


        _playerIndicator.material = _playerStatus.PlayerIndicator;
        _throwablePlayerStats.setMaxCapacity(_maxThrowableItens);
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
        GameObject enemy = PhotonView.Find(enemyPhotonID).gameObject;
        if (photonView.IsMine)
        {
            IncapacitatePlayer(enemy);
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
            GetComponent<CharacterController>().enabled = isEnabled;
        }
    }

    public void changeTransformParent(int PhotonIDParent, bool resetVector3, bool changeToNull)
    
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("changeTransformParentRPC", RpcTarget.All, PhotonIDParent, resetVector3, changeToNull);
        }
    }
    
    [PunRPC]
    public void changeTransformParentRPC(int PhotonIDParent, bool resetVector3, bool changeToNull)
    {
        if (photonView.IsMine)
        {
            if (changeToNull)
            {
                transform.SetParent(null);
                transform.position = new Vector3(transform.position.x, 59,
                    transform.position.z);
            }
            else{
                GameObject parent = PhotonView.Find(PhotonIDParent).gameObject;
                gameObject.transform.SetParent(parent.transform);

                if (resetVector3)
                    transform.localPosition = Vector3.zero;
            }
        }
    }
    
    //================================================================================================
    //Getters and Setters
    public void setIsOnline(bool isOnline)
    {
        _isOnline = isOnline;
    }
    public void setMovementAnimationStats(PlayerMovement.PlayerDirection _direction)
    {
        if (_direction == PlayerMovement.PlayerDirection.FORWARD)
        {
            _playerMovement.setSpeed(_speed);
            _isWalkingForward = true;
            _isWalkingBackward = false;
            _isWalkingLeft = false;
            _isWalkingRight = false;
            _isIdle = false;
        }
        else if( _direction == PlayerMovement.PlayerDirection.BACK)
        {
            _playerMovement.setSpeed(_speed*0.8f);
            _isWalkingForward = false;
            _isWalkingBackward = true;
            _isWalkingLeft = false;
            _isWalkingRight = false;
            _isIdle = false;
        }
        else if (_direction == PlayerMovement.PlayerDirection.LEFT)
        {
            _playerMovement.setSpeed(_speed*0.9f);
            _isWalkingForward = false;
            _isWalkingBackward = false;
            _isWalkingLeft = true;
            _isWalkingRight = false;
            _isIdle = false;
        }
        else if (_direction == PlayerMovement.PlayerDirection.RIGHT)
        {
            _playerMovement.setSpeed(_speed*0.9f);
            _isWalkingForward = false;
            _isWalkingBackward = false;
            _isWalkingLeft = false;
            _isWalkingRight = true;
            _isIdle = false;
        }
        else
        {
            _playerMovement.setSpeed(_speed);
            _isWalkingForward = false;
            _isWalkingBackward = false;
            _isWalkingLeft = false;
            _isWalkingRight = false;
            _isIdle = true;
        }
        
        _playerAnimationManager.setIsWalkingForward(_isWalkingForward);
        _playerAnimationManager.setIsWalkingBackward(_isWalkingBackward);
        _playerAnimationManager.setIsWalkingLeft(_isWalkingLeft);
        _playerAnimationManager.setIsWalkingRight(_isWalkingRight);
        _playerAnimationManager.setIsIdle(_isIdle);

    }
    
    public ThrowablePlayerStats getThrowablePlayerStats()
    {
        return _throwablePlayerStats;
    }
    
    public bool addItemThrowable(ScObThrowableSpecs throwable)
    {
        return _throwablePlayerStats.addThrowable(throwable);
    }
    public void sethealthBarUi(HealthBar_UI healthBarUi)
    {
        _healthBarUi = healthBarUi;
        healthBarUi.setColor(_characterColor);
        healthBarUi.setMaxHealth((int)totalLife);
    }
    public bool verifyDown()
    {
        return _isDown;
    }

    public bool verifyDeath()
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
                float updatedSpeed = _speed - speed;
                float baseSpeed = _speed;
                _speed = updatedSpeed;
                _playerMovement.setSpeed(updatedSpeed);
                StartCoroutine(resetTemporarySpeed(time, baseSpeed));
            }
        }
    }
    private IEnumerator resetTemporarySpeed(float time, float baseSpeed)
    {
        yield return new WaitForSeconds(time);
        _speed = baseSpeed;
        _isSpeedSlowed = false;
        _playerMovement.setSpeed(baseSpeed);
    }

    public void aimSlow(float newSlow, bool auxBool)
    {
        if (auxBool){
            _isAiming = auxBool;
            _playerMovement.setSpeed(newSlow);
        }
        else{
            _isAiming = auxBool;
            _playerMovement.setSpeed(_speed);
        }

}

    public void updateSpeedMovement()
    {
        _playerMovement.setSpeed(_speed);
    }

    public float getMeleeDamage()
    {
        return _meleeDamage;
    }
    
    public float getTimeBetweenMelee()
    {
        return _timeBetweenMelee;
    }
    
    public float getRevivalSpeed()
    {
        return _revivalSpeed;
    }
    
    public void setInteracting(bool value)
    {
        _interacting = value;
    }
    
    public bool getInteracting()
    {
        return _interacting;
    }
    
    public void stopDeathCounting(bool value)
    {
        _stopDeathLife = value;
    }
    
    public void setPlayerStats(ScObPlayerStats stats)
    {
        _playerStatus = stats;
    }
    public float getSpeed()
    {
        return _speed;
    }
    
    public PlayerPoints getPlayerPoints()
    {
        return _playerPoints;
    }
    
    
    public bool getIsIncapacitated()
    {
        return _isIncapatitated;
    }
    public PlayerMovement getPlayerMovement()
    {
        return _playerMovement;
    }

    

    
    public void IncapacitatePlayer(GameObject enemy)
    { 
        EnemyInCapacitator = enemy;
        _isIncapatitated = true;
        _weaponSystem.SetIsIncapacitated(true);
        _characterController.enabled = false;
        _playerMovement.setCanMove(false);
        _playerRotation.setCanRotate(false);
        _weaponSystem.SetGunVisable(false);
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
        
        _isIncapatitated = false;
        _weaponSystem.SetIsIncapacitated(false);
        _characterController.enabled = true;
        _playerMovement.setCanMove(true);
        _playerRotation.setCanRotate(true);
        _weaponSystem.SetGunVisable(true);
    }
    
    
    public void StunPlayer(float time)
    {
        if (!_isOnline || photonView.IsMine)
        {
            _isStunned = true;
            _playerMovement.setCanMove(false);
            _playerRotation.setCanRotate(false);
            _weaponSystem.SetIsIncapacitated(true);
            _weaponSystem.SetGunVisable(false);
            StartCoroutine(StunPlayerCounting(time));
        }
    }
    
    
    private IEnumerator StunPlayerCounting(float time)
    {
        yield return new WaitForSeconds(time);
        _isStunned = false;
        _playerMovement.setCanMove(true);
        _playerRotation.setCanRotate(true);
        _weaponSystem.SetIsIncapacitated(true);
        _weaponSystem.SetGunVisable(true);
    }
    public void setIsNoHitChallenge(bool value)
    {
        _noHitChallenge = value;
    }

    public WeaponSystem getWeaponSystem()
    {
        return _weaponSystem;
    }
    
    public ChallengeManager getChallengeManager()
    {
        return _challengeManager;
    }
    
    public void setMainGameManager(MainGameManager gameManager)
    {
        _mainGameManager = gameManager;
    }

    public GameObject getPlayerHead()
    {
        return _playerHead;
    }
    
    public String getPlayerName()
    {
        return _name;
    }
    
}
