using System.Collections;
using Photon.Pun;
using Runtime.Challenges;
using Runtime.Player.Combat.Melee;
using Runtime.Player.Combat.PlayerStatus;
using Runtime.Player.Combat.Throwables;
using Runtime.Player.Points;
using Runtime.Player.ScriptObjects.Combat;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;


namespace Runtime.Player.Combat.Gun
{
        public class WeaponSystem : MonoBehaviourPunCallbacks, IPunObservable
        {
            //Player ===============================================================================================
            [Header("===============PLAYER CONFIGURATION===============")]
            [SerializeField] private bool isOnline;
            //Current Status-------------------------------------------------------------------------------
            
                        private bool 
                            _incapacitated,
                            _isDead,
                            _isDown;
                //ChallengeSpecs---------------------------------------------------------------------------------
                
                [Space (20)]
                [Header("CHALLENGE STATUS")]
                [Space (10)]
                
                private bool 
                        _isChallengeActive,
                         _isSharpshooterChallengeActive,
                         _isMeleeChallengeActive,
                         _isKillInTimeChallengeActive,
                         _isKillInAreaChallengeActive,
                         _isKillWithAimChallengeActive,
                         _isInArea,
                         _missedShot;
                //Current match stats----------------------------------------------------------------------------
                
                [Space (20)]
                [Header("CURRENT MATCH STATS")]
                [Space (10)]
                
                private int _normalZombiesKilled;
                private int _specialZombiesKilled;
                private int _killedWithAim;
                private int _killedWithMelee;
                private int _missedShots;
                
                //Required scripts------------------------------------------------------------------------------
                
                [Space (20)]
                [Header("REQUIRED SCRIPTS")]
                [Space (10)]
               
                
                [SerializeField] private ThrowablePlayerStats throwablePlayerStats;
                [SerializeField] private PlayerStats playerStats;
                [SerializeField] private PlayerPoints playerPoints;
                [SerializeField] private PlayerAnimationManager playerAnimationManager;
                private ChallengeManager _challengeManager;

                
                //Prefabs---------------------------------------------------------------------------------------

                [Space(20)] [Header("PREFABS")] [Space(10)] [SerializeField]
                
                private string bulletPoolName;
                [SerializeField] private BulletScript bulletPrefab;
                [SerializeField] private Slider reloadSlider;
                
                //SpawnPoints-----------------------------------------------------------------------------------
                
                [Space (20)]
                [Header("SPAWN POINTS")]
                [Space (10)]
                
                [SerializeField] private Transform bulletSpawnPoint;
                [SerializeField] private GameObject gunModelSpawnPoint;
                
                //Initialized Objects---------------------------------------------------------------------------
                [Space (20)]
                [Header("INITIALIZED OBJECTS")]
                [Space (10)]
                
                private GameObject _gunModelStart;
                private BULLETS_UI _bulletsUI;
                private Slider _reloadSliderInstance;
                
            //Combat Specs =========================================================================================
                //Gun Specs ----------------------------------------------------------------------------------------
                [Space (40)]
                [Header("===============GUN CONFIGURATION===============")]
                [Space (10)]
                [SerializeField] private GameObject laserSightObject;
                private ScObGunSpecs _gunSpecs;
                private float 
                        _bulletDamage,
                        _timeBetweenBullets,
                        _reloadTime,
                        _gunSpread,
                        _gunSpreadWhileAiming,
                        _range,
                        _bulletSpeed,
                        _percentageSlowWhileAiming,
                        _knockBackForce,
                        _criticalChanceIncrementalPerBullet,
                        _criticalDamagePercentage,
                        _criticalBaseChancePercentage,
                        _timeBetweenShots;
                private int
                        _maxBulletStorage,
                        _magazineSize,
                        _bulletsPerShots,
                        _hittableEnemiesPerShot;
                private bool
                        _isFullAutoGun,
                        _haveKnockBackForce,
                        _haveCriticalChance,
                        _isShotgun,
                        _isSniper;
                
                //In-game gun status-----------------------------------------------------------------------------------
                [Space (20)]
                [Header("GUN STATUS")]
                [Space (10)]
                
                private float 
                        _currentCriticalChance,
                        _currentPauseBetweenShots;
                private int 
                        _currentBulletStorage,
                        _currentMagazineBullets,
                        _remainingShots;
                private bool
                        _shooting,
                        _readyForShooting,
                        _reloading,
                        _aiming,
                        _isOnShotPause;
            
                
                //Melee Specs--------------------------------------------------------------------------------------------
                [Space (30)]
                [Header("===============MELEE CONFIGURATION===============")]
                [Space (10)]
                
                [SerializeField] private MeleeSystem meleeSystem;
                
                private ScObMeleeSpecs _meleeSpecs;
                private float 
                    _meleeDamage,
                    _meleeRange,
                    _meleeKnockBackForce,
                    _meleeHorizontalArea,
                    _meleeVerticalArea,
                    _meleeDelayBetweenAttacks,
                    _meleeAttackDuration,
                    _meleeCriticalDamagePercentage,
                    _meleeCriticalChance;
                private bool 
                    _meleeHaveCriticalChance,
                    _meleeHaveKnockBack;
                private int 
                    _meleeHittableEnemies;
                
                //In-game melee status-----------------------------------------------------------------------------------
                [Space (20)]
                [Header("MELEE STATUS")]
                [Space (10)]
                
                private bool _meleeReady;
              
            
                
                
        
//======================================================================================================
//Unity base functions
                private void Start()
                {
                        if(playerStats == null)
                                playerStats = GetComponent<PlayerStats>();
                        if(playerAnimationManager == null)
                                playerAnimationManager = GetComponentInChildren<PlayerAnimationManager>();
                        if(playerPoints == null)
                            playerPoints = GetComponent<PlayerPoints>();
                        
                        
                        _challengeManager = playerStats.GetChallengeManager();
                        _isOnShotPause =true;
                        
                }

                private void Update()
                {
                        if (!_challengeManager)
                        {
                                _challengeManager = playerStats.GetChallengeManager();
                        }

                        if (_isOnShotPause && !_isFullAutoGun && _bulletsPerShots > 1)
                        {
                                if(_currentPauseBetweenShots > 0)
                                {
                                        _currentPauseBetweenShots -= Time.deltaTime;
                                }
                                else
                                {
                                        _isOnShotPause = false;
                                }
                        }
                        if (_meleeReady && _readyForShooting && _shooting && !_reloading && _currentMagazineBullets > 0 & !_incapacitated)
                        {
                        
                                if (_currentMagazineBullets >= _bulletsPerShots)
                                {
                                        _remainingShots = _bulletsPerShots;
                                        Atirar();
                                }
                                else
                                {
                                        _remainingShots = _currentMagazineBullets;
                                        Atirar();
                                }

                        }
                        //Instancia o slider de reload no player
                        if(_reloading)
                        {
                                _reloadSliderInstance.value += Time.deltaTime;
                                _reloadSliderInstance.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 6, 0));
                        }
                        
                        if (isOnline && !photonView.IsMine)
                        {
                                if (!_meleeReady)
                                    playerAnimationManager.setAttack();
                            
                                laserSightObject.SetActive(_aiming);
                        }

                }

//======================================================================================================
//Main Functions


                private void Melee()
                {
                        _meleeReady = false;
                        meleeSystem.Attack();
                        playerAnimationManager.setAttack();
                        Invoke("ResetarMelee", _meleeDelayBetweenAttacks);

                }
        
                private void Atirar()
                {
                        if (!_isFullAutoGun && _bulletsPerShots > 1)
                        {
                                _isOnShotPause = true;
                                _currentPauseBetweenShots = _timeBetweenShots;
                        }
                        bool isCritical = false;
                        if (_haveCriticalChance)
                        {
                                float random = Random.Range(0, 100);
                                if (random <= _currentCriticalChance)
                                {
                                        isCritical = true;
                                        _currentCriticalChance = _criticalBaseChancePercentage;
                                }
                                else
                                {
                                        _currentCriticalChance += _criticalChanceIncrementalPerBullet;
                                }
                        }

                        _readyForShooting = false;
                        if (!_isShotgun)
                        {
                                //calcular direção dos tiros com a dispersão de bala
                                float y = Random.Range(-_gunSpread, _gunSpread);
                                Vector3 auxVector = bulletSpawnPoint.rotation.eulerAngles;
                                Quaternion dispersaoCalculada = Quaternion.Euler(auxVector.x, auxVector.y + y, auxVector.z);

                                //Spawn da bala
                                if (isOnline)
                                {
                                        GameObject bullet = PhotonNetwork.Instantiate(bulletPoolName, bulletSpawnPoint.position, dispersaoCalculada);
                                        BulletScript bala = bullet.GetComponent<BulletScript>();
                                        bala.setDistancia(_range);
                                        bala.setVelocidadeBalas(_bulletSpeed);
                                        bala.SetDamage(_bulletDamage);
                                        bala.setIsAiming(_aiming);
                                        bala.setShooter(this);
                                        bala.setHitableEnemies(_hittableEnemiesPerShot);
                                        bala.setIsCritical(isCritical, _criticalDamagePercentage);
                                }
                                else
                                {


                                        BulletScript bala = Instantiate(bulletPrefab, bulletSpawnPoint.transform.position,
                                                dispersaoCalculada);
                                        bala.SetDamage(_bulletDamage);
                                        bala.setShooter(this);
                                        bala.setIsAiming(_aiming);
                                        bala.setDistancia(_range);
                                        bala.setIsKnockback(_haveKnockBackForce);
                                        bala.setKnockbackForce(_knockBackForce);
                                        bala.setVelocidadeBalas(_bulletSpeed);
                                        bala.setHitableEnemies(_hittableEnemiesPerShot);
                                        bala.setIsCritical(isCritical, _criticalDamagePercentage);
                                }
                                _currentMagazineBullets--;
                                _remainingShots--;
                                _bulletsUI.setBalasPente(_currentMagazineBullets);

                                if (_isFullAutoGun)
                                {
                                        Invoke("ResetarTiro", _timeBetweenBullets);
                                }
                                else
                                {
                                        Invoke("ResetarTiro", _timeBetweenShots);
                                }

                                if (_remainingShots > 0 && _currentMagazineBullets > 0)
                                {
                                        Invoke("Atirar", _timeBetweenBullets);
                                }
                        }
                        else
                        {
                                for (int i = 0; i < _bulletsPerShots; i++)
                                {
                                        //calcular direção dos tiros com a dispersão de bala
                                        float y = Random.Range(-_gunSpread, _gunSpread);
                                        Vector3 auxVector = bulletSpawnPoint.rotation.eulerAngles;
                                        Quaternion dispersaoCalculada = Quaternion.Euler(auxVector.x, auxVector.y + y, auxVector.z);
                                        if (isOnline)
                                        {
                                                GameObject bullet = PhotonNetwork.Instantiate("bullet", bulletSpawnPoint.position, dispersaoCalculada);
                                                BulletScript bala = bullet.GetComponent<BulletScript>();
                                                bala.setDistancia(_range);
                                                bala.setVelocidadeBalas(_bulletSpeed);
                                                bala.setIsAiming(_aiming);
                                                bala.SetDamage(_bulletDamage);
                                                bala.setShooter(this);
                                                bala.setHitableEnemies(_hittableEnemiesPerShot);
                                                bala.setIsCritical(isCritical, _criticalDamagePercentage);
                                        }
                                        else
                                        {
                                                BulletScript bala = Instantiate(bulletPrefab, bulletSpawnPoint.transform.position, dispersaoCalculada);
                                                bala.setDistancia(_range);
                                                bala.setVelocidadeBalas(_bulletSpeed);
                                                bala.setIsAiming(_aiming);
                                                bala.SetDamage(_bulletDamage);
                                                bala.setShooter(this);
                                                bala.setHitableEnemies(_hittableEnemiesPerShot);
                                                bala.setIsCritical(isCritical, _criticalDamagePercentage);
                                        }
                                }
                                _currentMagazineBullets -= _bulletsPerShots;
                                _bulletsUI.setBalasPente(_currentMagazineBullets);
                                Invoke("ResetarTiro", _timeBetweenBullets);
                        
                        
                        }
                }

                private void Recarregar()
                {
                        if (_currentBulletStorage > 0 && _currentMagazineBullets < _magazineSize && !_reloading)
                        {
                                _reloadSliderInstance =
                                        Instantiate(reloadSlider, transform.position, Quaternion.identity);
                                _reloadSliderInstance.transform.SetParent(GameObject.Find("Canva").transform);
                                _reloadSliderInstance.maxValue = _reloadTime;
                                _readyForShooting = false;
                                _reloading = true;
                                Invoke("ReloadTerminado", _reloadTime);
                                //}
                        }
                }
        
                public void ChangeGun(ScObGunSpecs NewGun)
                {
                        _gunSpecs = NewGun;
                        Destroy(_gunModelStart);
                        ApplyGunSpecs();
                }
                
                public void ChangeMelee(ScObMeleeSpecs NewMelee)
                {
                        _meleeSpecs = NewMelee;
                }



//======================================================================================================
//Aux Functions

                public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
                {
                        if (stream.IsWriting)
                        {
                                stream.SendNext(_currentMagazineBullets);
                                stream.SendNext(_currentBulletStorage);
                                stream.SendNext(_aiming);
                                stream.SendNext(_normalZombiesKilled);
                                stream.SendNext(_specialZombiesKilled);

                        }
                        else
                        {
                                _currentMagazineBullets = (int)stream.ReceiveNext();
                                _currentBulletStorage = (int)stream.ReceiveNext();
                                _aiming = (bool)stream.ReceiveNext();
                                _normalZombiesKilled = (int)stream.ReceiveNext();
                                _specialZombiesKilled = (int)stream.ReceiveNext();
                        
                        }
                }
                [PunRPC]
                public void reloadSliderOnline(int sliderPhotonId, float reloadTime)
                {
                        GameObject reloadSlider = PhotonView.Find(sliderPhotonId).gameObject;
                        reloadSlider.transform.SetParent(GameObject.Find("Canva").transform);
                        _reloadSliderInstance = reloadSlider.GetComponent<Slider>();
                        _reloadSliderInstance.maxValue = reloadTime;
                        _readyForShooting = false;
                        _reloading = true;
                        Invoke("ReloadTerminado", reloadTime);
                }

        
        
                private void ReloadTerminado()
                {
                        throwablePlayerStats.setCanceledThrow(false);

                        if (_currentBulletStorage >= _magazineSize)
                        {
                                _currentBulletStorage -= _magazineSize - _currentMagazineBullets;
                                _currentMagazineBullets = _magazineSize;
                        }
                        else if (_currentBulletStorage < _magazineSize)
                        {
                                _currentMagazineBullets = _currentBulletStorage;
                                _currentBulletStorage = 0;
                        }

                        _reloading = false;
                        _bulletsUI.setBalasPente(_currentMagazineBullets);
                        _bulletsUI.setBalasTotal(_currentBulletStorage);
                        Destroy(_reloadSliderInstance.gameObject);
                        ResetarTiro();
                }


                private void ResetarTiro()
                {
                        _readyForShooting = true;
                }

                private void ResetarMelee()
                {
                        throwablePlayerStats.setCanceledThrow(false);
                        _meleeReady = true;
                }

                private void ApplyGunSpecs()
                {
                        _isShotgun = _gunSpecs.isShotgun;
                        if (_isShotgun)
                        {
                                _magazineSize = _gunSpecs.tamanhoPente;
                                _magazineSize *= _gunSpecs.balasPorDisparo;
                                _currentMagazineBullets = _magazineSize;
                                _maxBulletStorage = _gunSpecs.totalBalas;
                                _maxBulletStorage *= _gunSpecs.balasPorDisparo;
                                _currentBulletStorage = _maxBulletStorage;
                        }
                        else
                        {
                                _magazineSize = _gunSpecs.tamanhoPente;
                                _currentMagazineBullets = _magazineSize;
                                _maxBulletStorage = _gunSpecs.totalBalas;
                                _currentBulletStorage = _maxBulletStorage;
                        }
                        _isSniper = _gunSpecs.isSniper;
                        _range = _gunSpecs.range;
                        _bulletSpeed = _gunSpecs.speedBullet;
                        _hittableEnemiesPerShot = _gunSpecs.hitableEnemies;
                        _percentageSlowWhileAiming = (_gunSpecs.slowWhileAimingPercent/100);
                        _haveKnockBackForce = _gunSpecs.haveKnockback;
                        _knockBackForce = _gunSpecs.knockbackForce;
                        _bulletDamage = _gunSpecs.dano;
                        _gunSpread = _gunSpecs.dispersao;
                        _timeBetweenShots = _gunSpecs.tempoEntreDisparos;
                        _bulletsPerShots = _gunSpecs.balasPorDisparo;
                        _reloadTime = _gunSpecs.tempoRecarga;
                        _isFullAutoGun = _gunSpecs.segurarGatilho;
                        _timeBetweenBullets = (60/_gunSpecs.BalasPorMinuto);
                        _gunSpreadWhileAiming = _gunSpecs.reducaoDispersaoMirando;
                        _haveCriticalChance = _gunSpecs.haveCriticalChance;
                        _criticalDamagePercentage = _gunSpecs.criticalDamagePercentage;
                        _criticalChanceIncrementalPerBullet = _gunSpecs.criticalChanceIncrementalPerBullet;
                        _criticalBaseChancePercentage = _gunSpecs.criticalBaseChancePercentage;
                        StartCoroutine(waitToEnableGun(2));
                        _meleeReady = true;
                        _gunModelStart = Instantiate(_gunSpecs.modelo3d, gunModelSpawnPoint.transform.position,
                                gunModelSpawnPoint.transform.rotation);
                        _gunModelStart.transform.parent = gunModelSpawnPoint.transform;
                        _bulletsUI.setIsShotgun(_isShotgun, _bulletsPerShots);
                        _bulletsUI.setBalasPente(_currentMagazineBullets);
                        _bulletsUI.setBalasTotal(_currentBulletStorage);
                }
        
                public IEnumerator waitToEnableGun(float atraso)
                {
                        yield return new WaitForSeconds(atraso);
                        _currentPauseBetweenShots = 0;
                        _readyForShooting = true;
                }
        


//================================================================================================
                //Input Actions
                public void AuxMelee()
                {
                        if (!_shooting && _meleeReady && !_reloading && _readyForShooting)
                        {
                                Melee();
                                throwablePlayerStats.setCanceledThrow(true);

                        }
                }
                public void AuxReload()
                {
                        if (_currentMagazineBullets < _magazineSize && !_reloading && !_meleeReady)
                        {
                                Recarregar();
                                throwablePlayerStats.setCanceledThrow(true);
                        }
                }
        
                public void AuxShootPress(InputAction.CallbackContext ctx)
                {
                        switch (ctx.phase)
                        {
                                case InputActionPhase.Performed:
                                        if (_isFullAutoGun)
                                        {
                                                _shooting = true;
                                                throwablePlayerStats.setCanceledThrow(true);
                                        }
                                        else
                                        {
                                                throwablePlayerStats.setCanceledThrow(false);
                                                _shooting = false;
                                        }

                                        break;
                                case InputActionPhase.Started:
                                        _shooting = true;
                                        throwablePlayerStats.setCanceledThrow(true);
                                        break;
                                case InputActionPhase.Canceled:
                                        _shooting = false;
                                        throwablePlayerStats.setCanceledThrow(false);

                                        break;
                        }
                }

                public void AuxAimPress(InputAction.CallbackContext ctx)
                {
                        if (!playerStats.GetIsDown() && !playerStats.GetIsIncapacitated())
                        {
                                switch (ctx.phase)
                                {
                                        case InputActionPhase.Started:
                                        
                                                playerStats.AimSlow(_percentageSlowWhileAiming, true);
                                                _gunSpread *= (_gunSpreadWhileAiming / 100);
                                                laserSightObject.SetActive(true);
                                                _aiming = true;
                                                throwablePlayerStats.setCanceledThrow(true);
                                                break;
                                        case InputActionPhase.Canceled:
                                                playerStats.AimSlow(0, false);
                                                laserSightObject.SetActive(false);
                                                _gunSpread *= (100 / _gunSpreadWhileAiming);
                                                throwablePlayerStats.setCanceledThrow(false);
                                                _aiming = false;
                                                break;
                                }

                        }
                }


                //================================================================================================
                //Map interaction

                public void ReceiveAmmo(int ammo)
                {
                        if (!playerStats.GetIsDown() && !playerStats.GetIsDead())
                                _currentBulletStorage += ammo;
                        if (_currentBulletStorage > _maxBulletStorage)
                                _currentBulletStorage = _maxBulletStorage;
                        _bulletsUI.setBalasPente(_currentMagazineBullets);
                        _bulletsUI.setBalasTotal(_currentBulletStorage);
                }
                //================================================================================================
                //Getters and setters
        
                [PunRPC]
                public void addKilledNormalZombieOnline()
                {
                        playerPoints.addPointsNormalZombieKilled();
                        _normalZombiesKilled++;
                }

                public void cancelAim()
                {
                        playerStats.AimSlow(0, false);
                        laserSightObject.SetActive(false);
                        _gunSpread *= (100 / _gunSpreadWhileAiming);
                        throwablePlayerStats.setCanceledThrow(false);
                        _aiming = false;
                }
                public void setIsOnline(bool isOnline)
                {
                        this.isOnline = isOnline;
                }
                public void addKilledNormalZombie()
                {
                
                        playerPoints.addPointsNormalZombieKilled();
                        _normalZombiesKilled++;
                        if (_isKillInTimeChallengeActive) 
                                _challengeManager.addZombieKilled();
                        if(_isSharpshooterChallengeActive)
                                _challengeManager.addZombieKilled();

                }


                [PunRPC]
                public void addKilledSpecialZombieOnline(int points)
                {
                        playerPoints.addPointsSpecialZombiesKilled(points);
                        _specialZombiesKilled++;
                }
                public void addKilledSpecialZombie(int points)
                {
                        if (isOnline)
                        {
                                photonView.RPC("addKilledSpecialZombieOnline", RpcTarget.All, points);
                        }
                        else
                        {
                                playerPoints.addPointsSpecialZombiesKilled(points);
                                _specialZombiesKilled++;
                        }

                        if (!_isChallengeActive) return;
                
                        if (_isKillInAreaChallengeActive)
                        {
                                if (_isInArea)
                                        _challengeManager.addZombieKilled();
                        }

                        if (_isKillInTimeChallengeActive)
                        {
                                _challengeManager.addZombieKilled();
                        }
                        if(_isSharpshooterChallengeActive)
                                _challengeManager.addZombieKilled();
                }
                public int GetAtualAmmo()
                {
                        return _currentBulletStorage;
                }

                public int GetMaxBalas()
                {
                        return _maxBulletStorage;
                }

                public bool GetIsShotgun()
                {
                        return _isShotgun;
                }
        
                public void SetIsInArea(bool aux)
                {
                        _isInArea = aux;
                }

                public int GetBalasPente()
                {
                        return _currentMagazineBullets;
                }
        
        
                public void SetIsIncapacitated(bool aux)
                {
                        _incapacitated = aux;
                }
        
                public void SetGunVisable(bool isGunVisable)
                {
                        _gunModelStart.SetActive(isGunVisable);
                }
        
                public void SetProntoparaAtirar(bool aux)
                {
                        _readyForShooting = aux;
                }
        
                public void InitializeCombatScriptStatus(ScObGunSpecs gun, ScObMeleeSpecs melee)
                {
                        _meleeSpecs = melee;
                        _meleeDamage = melee.damage;
                        _meleeRange = melee.attackRange;
                        _meleeAttackDuration = melee.attackDuration;
                        _meleeHorizontalArea = melee.horizontalArea;
                        _meleeVerticalArea = melee.verticalArea;
                        _meleeHaveCriticalChance = melee.haveCriticalChance;
                        _meleeKnockBackForce = melee.knockBackForce;
                        _meleeDelayBetweenAttacks = melee.delayBetweenAttacks;
                        _meleeHaveKnockBack = melee.haveKnockBack;
                        _meleeHittableEnemies = melee.hittableEnemies;
                        _meleeCriticalDamagePercentage = melee.criticalDamagePercentage;
                        _meleeCriticalChance = melee.criticalChance;
                        meleeSystem.ApplyMeleeAttackStats(_meleeDamage, _meleeHaveCriticalChance, _meleeAttackDuration, _meleeHorizontalArea, _meleeVerticalArea, _meleeRange,_meleeCriticalDamagePercentage,_meleeCriticalChance, _meleeHittableEnemies, _meleeHaveKnockBack, _meleeKnockBackForce);
                        _gunSpecs = gun;
                        ApplyGunSpecs();
                }
        
                public void setBullets_UI(BULLETS_UI bullets)
                {
                        _bulletsUI = bullets;
                        _bulletsUI.initializeHud(_magazineSize, _currentBulletStorage, _isShotgun,_bulletsPerShots);
                }

                public bool GetIsSniper()
                {
                        return _isSniper;
                }



                public void addKilledZombieWithAim()
                {
                        _killedWithAim++;
                        if (_isChallengeActive)
                        {
                                if (_isKillWithAimChallengeActive)
                                { 
                                        _challengeManager.addZombieKilled();
                                }
                        }
                }
        
        
                public void addKilledZombieWithMelee()
                {
                        _killedWithMelee++;
                        if (_isChallengeActive)
                        {
                                if (_isMeleeChallengeActive)
                                {
                                        _challengeManager.addZombieKilled();
                                }
                        }
                }
        
                public void missedShot()
                {
                        _missedShots++;
                        _challengeManager.missedShot();
                }
        
                public void setisMeleeChallengeActive(bool aux)
                {
                        _isChallengeActive = aux;
                        _isMeleeChallengeActive = aux;
                }
                public void setisKillWithAimChallengeActive(bool aux)
                {
                        _isChallengeActive = aux;

                        _isKillWithAimChallengeActive = aux;
                }
        
                public void setisKillInAreaChallengeActive(bool aux)
                {
                        _isChallengeActive = aux;
                        _isKillInAreaChallengeActive = aux;
                }
        
                public void setisKillInTimeChallengeActive(bool aux)
                {
                        _isChallengeActive = aux;
                        _isKillInTimeChallengeActive = aux;
                }
        
                public void setIsSharpshooterChallengeActive(bool aux)
                {
                        _isChallengeActive = aux;
                        _isSharpshooterChallengeActive = aux;
                }
                public void set_challengeManager(ChallengeManager challengeManager)
                {
                        _challengeManager = challengeManager;
                }
        
                public int getTotalKilledZombies()
                {
                        return _normalZombiesKilled + _specialZombiesKilled;
                }
        

                //================================================================================================

        }
}


