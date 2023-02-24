using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class WeaponSystem : MonoBehaviour
{
        private ScObGunSpecs _specsArma;

        //Status das armas
        public int danoMelee;
        private int _dano;
        private float _tempoEntreDisparos, _tempoEntreMelee, _tempoRecarga, _dispersao;
        public int _maxBalas,_totalBalas, _tamanhoPente, _balasPorDisparo;
        private bool _segurarGatilho;
        private int _balasRestantes, _disparosAEfetuar;

        //ações

        private bool _atirando, _prontoParaAtirar, _recarregando, _attMelee, _meleePronto;

        //Referencia
        public Transform canoDaArma;
        public BulletScript MeleeHitBox;
        public BulletScript _bala;
        public GameObject arma;

        //Grafico
        private PlayerAnimationManager _playerAnimationManager;



        private void Start()
        {
                _playerAnimationManager = GetComponentInChildren<PlayerAnimationManager>();
                _dano = _specsArma.dano;
                _dispersao = _specsArma.dispersao;
                _balasPorDisparo = _specsArma.balasPorDisparo;
                _tempoRecarga = _specsArma.tempoRecarga;
                _segurarGatilho = _specsArma.segurarGatilho;
                _tamanhoPente = _specsArma.tamanhoPente;
                _tempoEntreDisparos = _specsArma.tempoEntreDisparos;
                _balasRestantes = _tamanhoPente;
                _maxBalas = _specsArma.totalBalas;
                _totalBalas = _maxBalas;
                _prontoParaAtirar = true;
                _meleePronto = true;
                GameObject armaStart = Instantiate(_specsArma.modelo3d, arma.transform.position, _specsArma.modelo3d.transform.rotation);
                armaStart.transform.parent = arma.transform;
        }

        public void SetGunStatus(ScObGunSpecs gun)
        {
                _specsArma = gun;
        }

private void Update()
        {
                if (!_attMelee && _prontoParaAtirar && _atirando && !_recarregando && _balasRestantes > 0)
                {
                        _disparosAEfetuar = _balasPorDisparo;
                        Atirar();
                }
        }


        private void melee()
        {
                _meleePronto = false;
                BulletScript _hitboxMelee = Instantiate(MeleeHitBox, canoDaArma.position, canoDaArma.rotation);
                _hitboxMelee.SetDamage(danoMelee);
                _playerAnimationManager.setAttack();
                Invoke("ResetarMelee", _tempoEntreMelee);
                
                if (_balasRestantes > 1)
                {
                        Invoke("ResetarTiro", 2);
                }

                _attMelee = false;

        }
        private void Atirar()
        {
                _prontoParaAtirar = false;
                //calcular direção dos tiros com a dispersão de bala
                float y = Random.Range(-_dispersao, _dispersao);
                Vector3 auxVector = canoDaArma.rotation.eulerAngles;
                Quaternion dispersaoCalculada = Quaternion.Euler(auxVector.x, auxVector.y + y, auxVector.z);
                
                //Spawn da bala
                BulletScript bala = Instantiate(_bala, canoDaArma.transform.position, dispersaoCalculada);
                bala.SetDamage(_dano);
                _balasRestantes--; 
                _disparosAEfetuar--;
                Invoke("ResetarTiro", _tempoEntreDisparos);
                if (_disparosAEfetuar > 0 && _balasRestantes > 0){
                        Invoke("Atirar", _tempoEntreDisparos);
                }


        }

        private void Recarregar()
        {
                if(_totalBalas > 0 && _balasRestantes < _tamanhoPente){
                        _prontoParaAtirar = false;
                        _recarregando = true;
                        Invoke("ReloadTerminado", _tempoRecarga);
                }
        }

        public void setProntoparaAtirar(bool aux)
        {
                _prontoParaAtirar = aux;
        }
        private void ReloadTerminado()
        {
                Debug.Log("Recarregado");
                if(_totalBalas >= _tamanhoPente)
                {
                        _totalBalas -= _tamanhoPente;
                        _balasRestantes = _tamanhoPente;
                }
                else if(_totalBalas < _tamanhoPente && _totalBalas > 0)
                {
                        _balasRestantes = _totalBalas;
                        _totalBalas = 0;
                }
                _recarregando = false;
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



        public void AuxShootPress(InputAction.CallbackContext ctx)
        {
                switch (ctx.phase)
                {
                        case InputActionPhase.Performed:
                                if(_segurarGatilho)
                                        _atirando = true;
                                else{
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

        public void AuxMelee()
        {
                if (_meleePronto)
                {
                        _attMelee = true;
                        if (_recarregando)
                        {
                                _recarregando = false;
                        }
                        melee();

                }
        }
        
        
        public int GetAtualAmmo()
        {
                return _totalBalas;
        }
        
        public int GetMaxBalas()
        {
                return _maxBalas;
        }
        public void ReceiveAmmo(int ammo)
        {
                PlayerStats status = GetComponent<PlayerStats>();
                if (!status.verifyDown() && !status.verifyDeath())
                        _totalBalas += ammo;
                if (_totalBalas > _maxBalas)
                        _totalBalas = _maxBalas;
        }
        public void AuxReload()
        {
                if(_balasRestantes< _tamanhoPente && !_recarregando && !_attMelee)
                        Recarregar();
        }
}
