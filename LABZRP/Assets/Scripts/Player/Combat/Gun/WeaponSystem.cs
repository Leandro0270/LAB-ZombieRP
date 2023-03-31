using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class WeaponSystem : MonoBehaviour
{
        private ScObGunSpecs _specsArma;
        private PlayerStats _playerStats;
        private PlayerPoints _playerPoints;

        //Status das armas
        private float _danoMelee;
        private int _dano;
        private float _tempoEntreDisparos, _tempoEntreMelee, _tempoRecarga, _dispersao;
        private int _maxBalas, _totalBalas, _tamanhoPente, _balasPorDisparo;
        private bool _segurarGatilho;
        private int _balasRestantes, _disparosAEfetuar;
        private bool _isShotgun;

        //ações
        private bool _atirando, _prontoParaAtirar, _recarregando, _attMelee, _meleePronto;
        private int NormalZombiesKilled;
        private int SpecialZombiesKilled;


        //Referencia
        public Transform canoDaArma;
        public BulletScript MeleeHitBox;
        public BulletScript _bala;
        public GameObject armaSpawn;

        //Grafico
        private PlayerAnimationManager _playerAnimationManager;
        private GameObject armaStart;

        //UI
        private BULLETS_UI _bulletsUI;
        public Slider reloadSlider;
        private Slider _reloadSliderInstance;

        //Debug
        public ScObGunSpecs novaArma;
        public bool changeGUN = false;

//======================================================================================================
//Unity base functions
        private void Start()
        {
                _playerStats = GetComponent<PlayerStats>();
                _playerAnimationManager = GetComponentInChildren<PlayerAnimationManager>();
                _danoMelee = _playerStats.getMeleeDamage();
                _playerPoints = GetComponent<PlayerPoints>();
                _tempoEntreMelee = _playerStats.getTimeBetweenMelee();
                ApplyGunSpecs();

        }

        private void Update()
        {
                


                if (_meleePronto && _prontoParaAtirar && _atirando && !_recarregando && _balasRestantes > 0)
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

        }

//======================================================================================================
//Main Functions
        private void melee()
        {
                if (_danoMelee == 0)
                {
                        _danoMelee = _playerStats.getMeleeDamage();
                } 
                _meleePronto = false;
                BulletScript hitboxMelee = Instantiate(MeleeHitBox, canoDaArma.position, canoDaArma.rotation);
                hitboxMelee.SetDamage(_danoMelee);
                hitboxMelee.setMelee(true);
                _playerAnimationManager.setAttack();
                Invoke("ResetarMelee", _tempoEntreMelee);

        }

        private void Atirar()
        {
                _prontoParaAtirar = false;
                if (!_isShotgun)
                {
                        //calcular direção dos tiros com a dispersão de bala
                        float y = Random.Range(-_dispersao, _dispersao);
                        Vector3 auxVector = canoDaArma.rotation.eulerAngles;
                        Quaternion dispersaoCalculada = Quaternion.Euler(auxVector.x, auxVector.y + y, auxVector.z);

                        //Spawn da bala
                        BulletScript bala = Instantiate(_bala, canoDaArma.transform.position, dispersaoCalculada);
                        bala.SetDamage(_dano);
                        bala.setShooter(this);
                        _balasRestantes--;
                        _disparosAEfetuar--;
                        _bulletsUI.setBalasPente(_balasRestantes);
                        Invoke("ResetarTiro", _tempoEntreDisparos);
                        if (_disparosAEfetuar > 0 && _balasRestantes > 0)
                        {
                                Invoke("Atirar", _tempoEntreDisparos);
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
                                //Spawn da bala
                                BulletScript bala = Instantiate(_bala, canoDaArma.transform.position, dispersaoCalculada);
                                bala.SetDamage(_dano);
                                bala.setShooter(this);
                                
                        }
                        _balasRestantes -= _balasPorDisparo;
                        _bulletsUI.setBalasPente(_balasRestantes);
                        Invoke("ResetarTiro", _tempoEntreDisparos);
                }
        }

        private void Recarregar()
        {
                if (_totalBalas > 0 && _balasRestantes < _tamanhoPente && !_recarregando)
                {
                        _reloadSliderInstance = Instantiate(reloadSlider, transform.position, Quaternion.identity);
                        _reloadSliderInstance.transform.SetParent(GameObject.Find("Canvas").transform);
                        _reloadSliderInstance.maxValue = _tempoRecarga;
                        _prontoParaAtirar = false;
                        _recarregando = true;
                        Invoke("ReloadTerminado", _tempoRecarga);
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
        private void ReloadTerminado()
        {
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
                _meleePronto = true;
        }

        private void ApplyGunSpecs()
        {
                _isShotgun = _specsArma.isShotgun;
                if (_isShotgun)
                {
                        _tamanhoPente = _specsArma.tamanhoPente;
                        _tamanhoPente *= 6;
                        _balasRestantes = _tamanhoPente;
                        _maxBalas = _specsArma.totalBalas;
                        _maxBalas *= 6;
                        _totalBalas = _maxBalas;
                }
                else
                {
                        _tamanhoPente = _specsArma.tamanhoPente;
                        _balasRestantes = _tamanhoPente;
                        _maxBalas = _specsArma.totalBalas;
                        _totalBalas = _maxBalas;
                }

                _dano = _specsArma.dano;
                _dispersao = _specsArma.dispersao;
                _balasPorDisparo = _specsArma.balasPorDisparo;
                _tempoRecarga = _specsArma.tempoRecarga;
                _segurarGatilho = _specsArma.segurarGatilho;
                _tempoEntreDisparos = _specsArma.tempoEntreDisparos;
                _prontoParaAtirar = true;
                _meleePronto = true;
                armaStart = Instantiate(_specsArma.modelo3d, armaSpawn.transform.position,
                       armaSpawn.transform.rotation);
                armaStart.transform.parent = armaSpawn.transform;
        }
        


//================================================================================================
        //Input Actions
        public void AuxMelee()
        {
                if (!_atirando && _meleePronto && !_recarregando && _prontoParaAtirar)
                        melee();
        }
        public void AuxReload()
        {
                if (_balasRestantes < _tamanhoPente && !_recarregando && !_attMelee)
                        Recarregar();
        }
        
        public void AuxShootPress(InputAction.CallbackContext ctx)
        {
                switch (ctx.phase)
                {
                        case InputActionPhase.Performed:
                                if (_segurarGatilho)
                                        _atirando = true;
                                else
                                {
                                        _atirando = false;
                                }

                                break;
                        case InputActionPhase.Started:
                                _atirando = true;
                                break;
                        case InputActionPhase.Canceled:
                                _atirando = false;
                                break;
                }
        }
        
        //================================================================================================
        //Map interaction
        
        public void ReceiveAmmo(int ammo)
        {
                PlayerStats status = GetComponent<PlayerStats>();
                if (!status.verifyDown() && !status.verifyDeath())
                        _totalBalas += ammo;
                if (_totalBalas > _maxBalas)
                        _totalBalas = _maxBalas;
                _bulletsUI.setBalasPente(_balasRestantes);
                _bulletsUI.setBalasTotal(_totalBalas);
        }
        //================================================================================================
        //Getters and setters

        public void addKilledNormalZombie()
        {
                _playerPoints.addPoints();
                NormalZombiesKilled++;
        }

        public void addKilledSpecialZombie()
        {
                SpecialZombiesKilled++;
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

        public int GetBalasPente()
        {
                return _balasRestantes;
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
                _bulletsUI.initializeHud(_tamanhoPente, _totalBalas, _isShotgun);
        }
        //================================================================================================

}


