using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class WeaponSystem : MonoBehaviour
{

        //input system

        //melee ou a distância
        public bool isCloseCombat;
        
        //Status das armas
        public int dano;
        public float tempoEntreDisparos, tempoRecarga, dispersao;
        public int tamanhoPente, balasPorDisparo;
        public bool segurarGatilho;
        private int _balasRestantes, _disparosAEfetuar;

        //ações

        private bool _atirando, _prontoParaAtirar, _recarregando;
        
        //Referencia
        public GameObject canoDaArma;
        public bulletScript bala;

        //Grafico
        public GameObject claraoTiro, buracoParede;
        
        private void Start()
        {
                _balasRestantes = tamanhoPente;
                _prontoParaAtirar = true;
        }

        public void OnShootTap(InputAction.CallbackContext ctx)
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

        public void OnShootPress(InputAction.CallbackContext ctx)
        {
                switch (ctx.phase)
                {
                        case InputActionPhase.Performed:
                                if(segurarGatilho)
                                        _atirando = true;
                                break;
                        case InputActionPhase.Started:
                                break;
                        case InputActionPhase.Canceled:
                                _atirando = false;
                                break;
                }
        }

        public void onReload(InputAction.CallbackContext ctx)
        {
                if(_balasRestantes< tamanhoPente && !_recarregando)
                        Recarregar();
        }
        private void Update()
        {
                bala.SetDamage(dano);
                if(Input.GetKeyDown(KeyCode.R) && _balasRestantes < tamanhoPente && !_recarregando) Recarregar();
                
                if (_prontoParaAtirar && _atirando && !_recarregando && _balasRestantes > 0)
                {
                        _disparosAEfetuar = balasPorDisparo;
                        Atirar();
                }
        }

        private void Atirar()
        {
                _prontoParaAtirar = false;
                
                //dispersão

                //calcular direção dos tiros com a dispersão de bala
                float y = Random.Range(-dispersao, dispersao);
                Vector3 auxVector = transform.rotation.eulerAngles;
                Quaternion dispersaoCalculada = Quaternion.Euler(auxVector.x, auxVector.y + y, auxVector.z);
                
                //Spawn da bala
                bulletScript _bala =Instantiate(bala, canoDaArma.transform.position, dispersaoCalculada);
                _bala.SetDamage(dano);
                _balasRestantes--; 
                        _disparosAEfetuar--;
                

        Invoke("ResetarTiro", tempoEntreDisparos);

                if (_disparosAEfetuar > 0 && _balasRestantes > 0)
                {
                        Invoke("Atirar", tempoEntreDisparos);
                }


        }

        private void Recarregar()
        {
                Debug.Log("Recarregando");
                _prontoParaAtirar = false;
                _recarregando = true;
                Invoke("ReloadTerminado", tempoRecarga);
        }

        private void ReloadTerminado()
        {
                Debug.Log("Recarregado");
                _balasRestantes = tamanhoPente;
                _recarregando = false;
                ResetarTiro();
        }
        

        private void ResetarTiro()
        {
                _prontoParaAtirar = true;
        }
        
 
}
