using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        private int _tamanhoPente, _balasPorDisparo;
        private bool _segurarGatilho;
        private int _balasRestantes, _disparosAEfetuar;

        //ações

        private bool _atirando, _prontoParaAtirar, _recarregando, _attMelee, _meleePronto;

        //Referencia
        public GameObject canoDaArma;
        public bulletScript MeleeHitBox;
        public bulletScript bala;

        //Grafico
        [SerializeField] private GameObject VisualArma;
        public GameObject claraoTiro, buracoParede;



        private void Start()
        {
                _dano = _specsArma.dano;
                _dispersao = _specsArma.dispersao;
                _balasPorDisparo = _specsArma.balasPorDisparo;
                _tempoRecarga = _specsArma.tempoRecarga;
                _segurarGatilho = _specsArma.segurarGatilho;
                _tamanhoPente = _specsArma.tamanhoPente;
                _tempoEntreDisparos = _specsArma.tempoEntreDisparos;
                _balasRestantes = _tamanhoPente;
                _prontoParaAtirar = true;
                _meleePronto = true;
                VisualArma.GetComponent<MeshFilter>().mesh = _specsArma.modelo3d;
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
                bulletScript _hitboxMelee =Instantiate(MeleeHitBox, canoDaArma.transform.position, canoDaArma.transform.rotation);
                _hitboxMelee.SetDamage(danoMelee);
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
                Vector3 auxVector = canoDaArma.transform.rotation.eulerAngles;
                Quaternion dispersaoCalculada = Quaternion.Euler(auxVector.x, auxVector.y + y, auxVector.z);
                
                //Spawn da bala
                bulletScript _bala =Instantiate(bala, canoDaArma.transform.position, dispersaoCalculada);
                _bala.SetDamage(_dano);
                _balasRestantes--; 
                _disparosAEfetuar--;
                Invoke("ResetarTiro", _tempoEntreDisparos);
                if (_disparosAEfetuar > 0 && _balasRestantes > 0){
                        Invoke("Atirar", _tempoEntreDisparos);
                }


        }

        private void Recarregar()
        {
                        _prontoParaAtirar = false;
                        _recarregando = true;
                        Invoke("ReloadTerminado", _tempoRecarga);
                
        }

        private void ReloadTerminado()
        {
                Debug.Log("Recarregado");
                _balasRestantes = _tamanhoPente;
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

        public void AuxShootTap(InputAction.CallbackContext ctx)
        {
                switch (ctx.phase)
                {
                        case InputActionPhase.Performed:
                                _atirando = false;
                                break;
                        case InputActionPhase.Started:
                                _atirando = true;
                                break;
                        case InputActionPhase.Canceled:
                                break;
                }
        }

        public void AuxShootPress(InputAction.CallbackContext ctx)
        {
                switch (ctx.phase)
                {
                        case InputActionPhase.Performed:
                                if(_segurarGatilho)
                                        _atirando = true;
                                break;
                        case InputActionPhase.Started:
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
        public void AuxReload()
        {
                if(_balasRestantes< _tamanhoPente && !_recarregando && !_attMelee)
                        Recarregar();
        }
}
