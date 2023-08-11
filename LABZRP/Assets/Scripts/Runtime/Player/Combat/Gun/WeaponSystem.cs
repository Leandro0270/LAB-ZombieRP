using System.Collections;
using Photon.Pun;
using Runtime.Challenges;
using Runtime.Player.Combat.PlayerStatus;
using Runtime.Player.Combat.Throwables;
using Runtime.Player.Points;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class WeaponSystem : MonoBehaviourPunCallbacks, IPunObservable
{
        
        
        private ScObGunSpecs _specsArma;
        private PlayerStats _playerStats;
        private PlayerPoints _playerPoints;
        private ChallengeManager _challengeManager;
        [SerializeField] private ThrowablePlayerStats _throwablePlayerStats;

        //Status das armas
        private float _danoMelee;
        private float _dano;
        private float _tempoEntreBalas, _tempoEntreMelee, _tempoRecarga, _dispersao, _distancia, _velocidadeBala, _reducaoDispersaoMirando, _slowWhileAimingPercent, _ForcaKnockback, _criticalChanceIncrementalPerBullet, _criticalDamagePercentage, _criticalBaseChancePercentage, _currentCriticalChance, _tempoEntreDisparos, _currentPauseBetweenShots;
        private int _maxBalas, _totalBalas, _tamanhoPente, _balasPorDisparo;
        private int _hitableEnemies;
        private bool _segurarGatilho, _haveKnockback;
        private int _balasRestantes, _disparosAEfetuar;
        private bool _isShotgun, _isSniper, _haveCriticalChance;
        
        //ações
        private bool _atirando, _prontoParaAtirar, _recarregando, _attMelee, _meleePronto, _incapactitado, _mirando, _hitted, _hittedWithMelee,_hittedWithAim, _isOnShotPause;
        private int NormalZombiesKilled = 0;
        private int SpecialZombiesKilled = 0;
        private int KilledWithAim = 0;
        private int KilledWithMelee = 0;
        private int missedShots = 0;


        //Referencia
        public GameObject miraLaser;
        public Transform canoDaArma;
        public BulletScript MeleeHitBox;
        public BulletScript _bala;
        public GameObject armaSpawn;
        [SerializeField] public PhotonView _photonView;

        //Grafico
        private PlayerAnimationManager _playerAnimationManager;
        private GameObject armaStart;

        //UI
        private BULLETS_UI _bulletsUI;
        public Slider reloadSlider;
        private Slider _reloadSliderInstance;
        //Challenges specs
        private bool _isChallengeActive;
        private bool _isSharpshooterChallengeActive;
        private bool _isMeleeChallengeActive;
        private bool _isKillInTimeChallengeActive;
        private bool _isKillInAreaChallengeActive;
        private bool _isKillWithAimChallengeActive;
        private bool _isInArea;
        private bool _missedShot;
        //Other settings
        private bool _isOnline = false;
        
//======================================================================================================
//Unity base functions
        private void Start()
        {
                _playerStats = GetComponent<PlayerStats>();
                _playerAnimationManager = GetComponentInChildren<PlayerAnimationManager>();
                _danoMelee = _playerStats.GetMeleeDamage();
                _playerPoints = GetComponent<PlayerPoints>();
                _tempoEntreMelee = _playerStats.GetTimeBetweenMelee();
                _challengeManager = _playerStats.GetChallengeManager();
                _isOnShotPause =true;
                ApplyGunSpecs();

        }

        private void Update()
        {
                if (!_playerStats)
                {
                        _playerStats = GetComponent<PlayerStats>();
                }
                if(!_playerAnimationManager)
                {
                        _playerAnimationManager = GetComponentInChildren<PlayerAnimationManager>();
                }
                if (!_playerPoints)
                {
                        _playerPoints = GetComponent<PlayerPoints>();
                }
                if (!_challengeManager)
                {
                        _challengeManager = _playerStats.GetChallengeManager();
                }

                if (_isOnShotPause && !_segurarGatilho && _balasPorDisparo > 1)
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
                if (_meleePronto && _prontoParaAtirar && _atirando && !_recarregando && _balasRestantes > 0 & !_incapactitado)
                {
                        
                        if (_balasRestantes >= _balasPorDisparo)
                        {
                                _disparosAEfetuar = _balasPorDisparo;
                                Atirar();
                        }
                        else
                        {
                                _disparosAEfetuar = _balasRestantes;
                                Atirar();
                        }

                }
                
                if(_recarregando)
                {
                        _reloadSliderInstance.value += Time.deltaTime;
                        _reloadSliderInstance.transform.position = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 6, 0));
                }

                if (_isOnline && !photonView.IsMine)
                {
                        if (!_meleePronto)
                        {
                                _playerAnimationManager.setAttack();
                        }
                        if (_mirando)
                        {
                                miraLaser.SetActive(true);
                        }
                        else
                        {
                                miraLaser.SetActive(false);
                        }
                        
                }

        }

//======================================================================================================
//Main Functions


        private void melee()
        {
                if (_danoMelee == 0)
                {
                        _danoMelee = _playerStats.GetMeleeDamage();
                } 
                _meleePronto = false;
                BulletScript hitboxMelee;
                if (_isOnline)
                {
                        GameObject meleeHitBox = PhotonNetwork.Instantiate("meleeHitBox", canoDaArma.position, canoDaArma.rotation);
                        hitboxMelee = meleeHitBox.GetComponent<BulletScript>();
                        hitboxMelee.SetDamage(_danoMelee);
                        hitboxMelee.setMelee(true);
                        hitboxMelee.setShooter(this);

                }
                else
                {
                        hitboxMelee = Instantiate(MeleeHitBox, canoDaArma.position, canoDaArma.rotation);
                        hitboxMelee.SetDamage(_danoMelee);
                        hitboxMelee.setMelee(true);
                        hitboxMelee.setShooter(this);
                }
                
                _playerAnimationManager.setAttack();
                Invoke("ResetarMelee", _tempoEntreMelee);

        }
        
        private void Atirar()
        {
                if (!_segurarGatilho && _balasPorDisparo > 1)
                {
                        _isOnShotPause = true;
                        _currentPauseBetweenShots = _tempoEntreDisparos;
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

                _prontoParaAtirar = false;
                if (!_isShotgun)
                {
                        //calcular direção dos tiros com a dispersão de bala
                        float y = Random.Range(-_dispersao, _dispersao);
                        Vector3 auxVector = canoDaArma.rotation.eulerAngles;
                        Quaternion dispersaoCalculada = Quaternion.Euler(auxVector.x, auxVector.y + y, auxVector.z);

                        //Spawn da bala
                        if (_isOnline)
                        {
                                GameObject bullet = PhotonNetwork.Instantiate("bullet", canoDaArma.position, dispersaoCalculada);
                                BulletScript bala = bullet.GetComponent<BulletScript>();
                                bala.setDistancia(_distancia);
                                bala.setVelocidadeBalas(_velocidadeBala);
                                bala.SetDamage(_dano);
                                bala.setIsAiming(_mirando);
                                bala.setShooter(this);
                                bala.setHitableEnemies(_hitableEnemies);
                                bala.setIsCritical(isCritical, _criticalDamagePercentage);
                        }
                        else
                        {


                                BulletScript bala = Instantiate(_bala, canoDaArma.transform.position,
                                        dispersaoCalculada);
                                bala.SetDamage(_dano);
                                bala.setShooter(this);
                                bala.setIsAiming(_mirando);
                                bala.setDistancia(_distancia);
                                bala.setIsKnockback(_haveKnockback);
                                bala.setKnockbackForce(_ForcaKnockback);
                                bala.setVelocidadeBalas(_velocidadeBala);
                                bala.setHitableEnemies(_hitableEnemies);
                                bala.setIsCritical(isCritical, _criticalDamagePercentage);
                        }
                        _balasRestantes--;
                        _disparosAEfetuar--;
                        _bulletsUI.setBalasPente(_balasRestantes);

                        if (_segurarGatilho)
                        {
                                Invoke("ResetarTiro", _tempoEntreBalas);
                        }
                        else
                        {
                                Invoke("ResetarTiro", _tempoEntreDisparos);
                        }

                        if (_disparosAEfetuar > 0 && _balasRestantes > 0)
                        {
                                Invoke("Atirar", _tempoEntreBalas);
                        }
                }
                else
                {
                        for (int i = 0; i < _balasPorDisparo; i++)
                        {
                                //calcular direção dos tiros com a dispersão de bala
                                float y = Random.Range(-_dispersao, _dispersao);
                                Vector3 auxVector = canoDaArma.rotation.eulerAngles;
                                Quaternion dispersaoCalculada = Quaternion.Euler(auxVector.x, auxVector.y + y, auxVector.z);
                                if (_isOnline)
                                {
                                        GameObject bullet = PhotonNetwork.Instantiate("bullet", canoDaArma.position, dispersaoCalculada);
                                        BulletScript bala = bullet.GetComponent<BulletScript>();
                                        bala.setDistancia(_distancia);
                                        bala.setVelocidadeBalas(_velocidadeBala);
                                        bala.setIsAiming(_mirando);
                                        bala.SetDamage(_dano);
                                        bala.setShooter(this);
                                        bala.setHitableEnemies(_hitableEnemies);
                                        bala.setIsCritical(isCritical, _criticalDamagePercentage);
                                }
                                else
                                {
                                        BulletScript bala = Instantiate(_bala, canoDaArma.transform.position, dispersaoCalculada);
                                        bala.setDistancia(_distancia);
                                        bala.setVelocidadeBalas(_velocidadeBala);
                                        bala.setIsAiming(_mirando);
                                        bala.SetDamage(_dano);
                                        bala.setShooter(this);
                                        bala.setHitableEnemies(_hitableEnemies);
                                        bala.setIsCritical(isCritical, _criticalDamagePercentage);
                                }
                        }
                        _balasRestantes -= _balasPorDisparo;
                        _bulletsUI.setBalasPente(_balasRestantes);
                        Invoke("ResetarTiro", _tempoEntreBalas);
                        
                        
                }
        }

        private void Recarregar()
        {
                if (_totalBalas > 0 && _balasRestantes < _tamanhoPente && !_recarregando)
                {
                        // if (_isOnline)
                        // {
                        //         GameObject slide = PhotonNetwork.Instantiate("reloadSlider", transform.position, Quaternion.identity);
                        //         _reloadSliderInstance = slide.GetComponent<Slider>();
                        //         int reloadSliderPhotonId = slide.GetComponent<PhotonView>().ViewID;
                        //         photonView.RPC("reloadSliderOnline", RpcTarget.All, reloadSliderPhotonId, _tempoRecarga);
                        //
                        // }
                        // else
                        // {
                                _reloadSliderInstance =
                                        Instantiate(reloadSlider, transform.position, Quaternion.identity);
                                _reloadSliderInstance.transform.SetParent(GameObject.Find("Canva").transform);
                                _reloadSliderInstance.maxValue = _tempoRecarga;
                                _prontoParaAtirar = false;
                                _recarregando = true;
                                Invoke("ReloadTerminado", _tempoRecarga);
                        //}
                }
        }
        
        public void ChangeGun(ScObGunSpecs NewGun)
        {
                _specsArma = NewGun;
                Destroy(armaStart);
                ApplyGunSpecs();
        }



//======================================================================================================
//Aux Functions

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
                if (stream.IsWriting)
                {
                        stream.SendNext(_balasRestantes);
                        stream.SendNext(_totalBalas);
                        stream.SendNext(_mirando);
                        stream.SendNext(NormalZombiesKilled);
                        stream.SendNext(SpecialZombiesKilled);

                }
                else
                {
                       _balasRestantes = (int)stream.ReceiveNext();
                        _totalBalas = (int)stream.ReceiveNext();
                        _mirando = (bool)stream.ReceiveNext();
                        NormalZombiesKilled = (int)stream.ReceiveNext();
                        SpecialZombiesKilled = (int)stream.ReceiveNext();
                        
                }
        }
        [PunRPC]
        public void reloadSliderOnline(int sliderPhotonId, float reloadTime)
        {
                GameObject reloadSlider = PhotonView.Find(sliderPhotonId).gameObject;
                reloadSlider.transform.SetParent(GameObject.Find("Canva").transform);
                _reloadSliderInstance = reloadSlider.GetComponent<Slider>();
                _reloadSliderInstance.maxValue = reloadTime;
                _prontoParaAtirar = false;
                _recarregando = true;
                Invoke("ReloadTerminado", reloadTime);
        
        }

        
        
        private void ReloadTerminado()
        {
                _throwablePlayerStats.setCanceledThrow(false);

                if (_totalBalas >= _tamanhoPente)
                {
                        _totalBalas -= _tamanhoPente - _balasRestantes;
                        _balasRestantes = _tamanhoPente;
                }
                else if (_totalBalas < _tamanhoPente)
                {
                        _balasRestantes = _totalBalas;
                        _totalBalas = 0;
                }

                _recarregando = false;
                _bulletsUI.setBalasPente(_balasRestantes);
                _bulletsUI.setBalasTotal(_totalBalas);
                Destroy(_reloadSliderInstance.gameObject);
                ResetarTiro();
        }


        private void ResetarTiro()
        {
                _prontoParaAtirar = true;
        }

        private void ResetarMelee()
        {
                _throwablePlayerStats.setCanceledThrow(false);
                _meleePronto = true;
        }

        private void ApplyGunSpecs()
        {
                _isShotgun = _specsArma.isShotgun;
                if (_isShotgun)
                {
                        _tamanhoPente = _specsArma.tamanhoPente;
                        _tamanhoPente *= _specsArma.balasPorDisparo;
                        _balasRestantes = _tamanhoPente;
                        _maxBalas = _specsArma.totalBalas;
                        _maxBalas *= _specsArma.balasPorDisparo;
                        _totalBalas = _maxBalas;
                }
                else
                {
                        _tamanhoPente = _specsArma.tamanhoPente;
                        _balasRestantes = _tamanhoPente;
                        _maxBalas = _specsArma.totalBalas;
                        _totalBalas = _maxBalas;
                }
                _isSniper = _specsArma.isSniper;
                _distancia = _specsArma.range;
                _velocidadeBala = _specsArma.speedBullet;
                _hitableEnemies = _specsArma.hitableEnemies;
                _slowWhileAimingPercent = (_specsArma.slowWhileAimingPercent/100);
                _haveKnockback = _specsArma.haveKnockback;
                _ForcaKnockback = _specsArma.knockbackForce;
                _dano = _specsArma.dano;
                _dispersao = _specsArma.dispersao;
                _tempoEntreDisparos = _specsArma.tempoEntreDisparos;
                _balasPorDisparo = _specsArma.balasPorDisparo;
                _tempoRecarga = _specsArma.tempoRecarga;
                _segurarGatilho = _specsArma.segurarGatilho;
                _tempoEntreBalas = (60/_specsArma.BalasPorMinuto);
                _reducaoDispersaoMirando = _specsArma.reducaoDispersaoMirando;
                _haveCriticalChance = _specsArma.haveCriticalChance;
                _criticalDamagePercentage = _specsArma.criticalDamagePercentage;
                _criticalChanceIncrementalPerBullet = _specsArma.criticalChanceIncrementalPerBullet;
                _criticalBaseChancePercentage = _specsArma.criticalBaseChancePercentage;
                StartCoroutine(waitToEnableGun(2));
                _meleePronto = true;
                armaStart = Instantiate(_specsArma.modelo3d, armaSpawn.transform.position,
                       armaSpawn.transform.rotation);
                armaStart.transform.parent = armaSpawn.transform;
                _bulletsUI.setIsShotgun(_isShotgun, _balasPorDisparo);
                _bulletsUI.setBalasPente(_balasRestantes);
                _bulletsUI.setBalasTotal(_totalBalas);
        }
        
        public IEnumerator waitToEnableGun(float atraso)
        {
                yield return new WaitForSeconds(atraso);
                _currentPauseBetweenShots = 0;
                _prontoParaAtirar = true;
        }
        


//================================================================================================
        //Input Actions
        public void AuxMelee()
        {
                if (!_atirando && _meleePronto && !_recarregando && _prontoParaAtirar)
                {
                        melee();
                        _throwablePlayerStats.setCanceledThrow(true);

                }
        }
        public void AuxReload()
        {
                if (_balasRestantes < _tamanhoPente && !_recarregando && !_attMelee)
                {
                        Recarregar();
                        _throwablePlayerStats.setCanceledThrow(true);
                }
        }
        
        public void AuxShootPress(InputAction.CallbackContext ctx)
        {
                switch (ctx.phase)
                {
                        case InputActionPhase.Performed:
                                if (_segurarGatilho)
                                {
                                        _atirando = true;
                                        _throwablePlayerStats.setCanceledThrow(true);
                                }
                                else
                                {
                                        _throwablePlayerStats.setCanceledThrow(false);
                                        _atirando = false;
                                }

                                break;
                        case InputActionPhase.Started:
                                _atirando = true;
                                _throwablePlayerStats.setCanceledThrow(true);
                                break;
                        case InputActionPhase.Canceled:
                                _atirando = false;
                                _throwablePlayerStats.setCanceledThrow(false);

                                break;
                }
        }

        public void AuxAimPress(InputAction.CallbackContext ctx)
        {
                if (!_playerStats.GetIsDown() && !_playerStats.GetIsIncapacitated())
                {
                        switch (ctx.phase)
                        {
                                case InputActionPhase.Started:
                                        
                                        _playerStats.AimSlow(_slowWhileAimingPercent, true);
                                        _dispersao *= (_reducaoDispersaoMirando / 100);
                                        miraLaser.SetActive(true);
                                        _mirando = true;
                                        _throwablePlayerStats.setCanceledThrow(true);
                                        break;
                                case InputActionPhase.Canceled:
                                        _playerStats.AimSlow(0, false);
                                        miraLaser.SetActive(false);
                                        _dispersao *= (100 / _reducaoDispersaoMirando);
                                        _throwablePlayerStats.setCanceledThrow(false);
                                        _mirando = false;
                                        break;
                        }

                }
        }


        //================================================================================================
        //Map interaction

        public void ReceiveAmmo(int ammo)
        {
                PlayerStats status = GetComponent<PlayerStats>();
                if (!status.GetIsDown() && !status.GetIsDead())
                        _totalBalas += ammo;
                if (_totalBalas > _maxBalas)
                        _totalBalas = _maxBalas;
                _bulletsUI.setBalasPente(_balasRestantes);
                _bulletsUI.setBalasTotal(_totalBalas);
                
        }
        //================================================================================================
        //Getters and setters
        
        [PunRPC]
        public void addKilledNormalZombieOnline()
        {
                _playerPoints.addPointsNormalZombieKilled();
                NormalZombiesKilled++;
        }

        public void cancelAim()
        {
                _playerStats.AimSlow(0, false);
                miraLaser.SetActive(false);
                _dispersao *= (100 / _reducaoDispersaoMirando);
                _throwablePlayerStats.setCanceledThrow(false);
                _mirando = false;
        }
        public void setIsOnline(bool isOnline)
        {
                _isOnline = isOnline;
        }
        public void addKilledNormalZombie()
        {
                
                _playerPoints.addPointsNormalZombieKilled();
                NormalZombiesKilled++;
                if (_isKillInTimeChallengeActive) 
                        _challengeManager.addZombieKilled();
                if(_isSharpshooterChallengeActive)
                        _challengeManager.addZombieKilled();

        }


        [PunRPC]
        public void addKilledSpecialZombieOnline(int points)
        {
                _playerPoints.addPointsSpecialZombiesKilled(points);
                SpecialZombiesKilled++;
        }
        public void addKilledSpecialZombie(int points)
        {
                if (_isOnline)
                {
                        photonView.RPC("addKilledSpecialZombieOnline", RpcTarget.All, points);
                }
                else
                {
                        _playerPoints.addPointsSpecialZombiesKilled(points);
                        SpecialZombiesKilled++;
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
                return _totalBalas;
        }

        public int GetMaxBalas()
        {
                return _maxBalas;
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
                return _balasRestantes;
        }
        
        
        public void SetIsIncapacitated(bool aux)
        {
                _incapactitado = aux;
        }
        
        public void SetGunVisable(bool isGunVisable)
        {
                armaStart.SetActive(isGunVisable);
        }
        
        public void SetProntoparaAtirar(bool aux)
        {
                _prontoParaAtirar = aux;
        }
        
        public void SetGunStatus(ScObGunSpecs gun)
        {
                _specsArma = gun;
        }
        
        public void setBullets_UI(BULLETS_UI bullets)
        {
                _bulletsUI = bullets;
                _bulletsUI.initializeHud(_tamanhoPente, _totalBalas, _isShotgun,_balasPorDisparo);
        }

        public bool GetIsSniper()
        {
                return _isSniper;
        }



        public void addKilledZombieWithAim()
        {
                KilledWithAim++;
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
                KilledWithMelee++;
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
                missedShots++;
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
                return NormalZombiesKilled + SpecialZombiesKilled;
        }
        

        //================================================================================================

}


