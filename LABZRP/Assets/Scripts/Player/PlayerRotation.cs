using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine.InputSystem;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(PlayerStats))]

//Esse comando faz com que seja necessario o objeto em que o script for aplicado tenha o componente RIGIDBODY
[RequireComponent(typeof(Rigidbody))]
public class PlayerRotation : MonoBehaviour
{

    private PlayerStats _status;
    private Vector3 _inputRotation;

    private Vector3 _inputMouse;

    private Vector3 _lateInputRotation;


    private Rigidbody _rb;
    //Variavel que vai armazenar onde o raycast está batendo


    private RaycastHit _hit;



    //Variavel que vai definir onde que o raycast vai poder bater, fazendo com que tudo que não esteja nessa layer seja ignorado
    public LayerMask ground;

    private bool isGamepad;



    public void SetGamepadValidation(bool validation)
    {
        isGamepad = validation;
    }

    public void setRotationInput(Vector3 auxRotation)
    {
        _inputRotation.z = -auxRotation.x;
        _inputRotation.x = auxRotation.y;
        //O movimento vertical não será utilizado, por isso está sendo zerado
        _inputRotation.y = 0;
    }

void Start()
    {
        _status = GetComponent<PlayerStats>();
        //_rb está recebendo o componente Rigidbody de onde o script está sendo aplicado
        _rb = GetComponent<Rigidbody>();
    }
    
    

    //Para uso de componentes envolvendo fisicas (Nesse caso o RigidBody) é recomendado utilizar o fixed update
    void FixedUpdate(){
        if(isGamepad){
            _inputRotation = _inputRotation.normalized * Time.deltaTime;
            Quaternion newRotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_inputRotation), 0.2f);
            if(_lateInputRotation != _inputRotation){
                if(!_status.verifyDown() && !_status.verifyDeath())
                    _rb.MoveRotation(newRotation);
            }
        }else{
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit, 100, ground))
            {
                    //Nessa variavel está sendo feito a distância do player para onde o raycast está batendo
                    Vector3 playerToMouse = _hit.point - (transform.position);
                    Vector3 FixRotation = new Vector3(playerToMouse.z, 0, -playerToMouse.x);
                    //Nessa variavel está sendo feito o calculo da rotação necessária para o player utilizando o lerp para suavizar
                    Quaternion newRotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(FixRotation), 0.2f);
                    if(!_status.verifyDown() && !_status.verifyDeath())
                        _rb.MoveRotation(newRotation);
            }
        }

    }
}
    