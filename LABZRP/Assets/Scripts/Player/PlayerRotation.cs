using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Esse comando faz com que seja necessario o objeto em que o script for aplicado tenha o componente RIGIDBODY
[RequireComponent(typeof(Rigidbody))]
public class PlayerRotation : MonoBehaviour
{
    private bool _usingController;
    private Vector3 _inputRotation;
    
    
    private Rigidbody _rb;
    //Variavel que vai armazenar onde o raycast está batendo
    private RaycastHit _hit;
    //Variavel que vai definir onde que o raycast vai poder bater, fazendo com que tudo que não esteja nessa layer seja ignorado
    public LayerMask ground;
    void Start()
    {
        //_rb está recebendo o componente Rigidbody de onde o script está sendo aplicado
        _rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        //Estes comandos estão localizados na própria unity Project Settings>InputManager
        _inputRotation.x = Input.GetAxisRaw("Joystick X");
        _inputRotation.z = Input.GetAxisRaw("Joystick Y") * -1;
        if (_inputRotation.x != 0 &&  _inputRotation.z != 0)
        {
            _usingController = true;
        }
        else
        {
            _usingController = false;
        }

    //O movimento vertical não será utilizado, por isso está sendo zerado
        _inputRotation.y = 0;    }

    //Para uso de componentes envolvendo fisicas (Nesse caso o RigidBody) é recomendado utilizar o fixed update
    void FixedUpdate()
    {
        if(_usingController == false){
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out _hit, 100, ground))
            {
                //Nessa variavel está sendo feito a distância do player para onde o raycast está batendo
                Vector3 playerToMouse = _hit.point - (transform.position);
                playerToMouse.y = 0;
                //Nessa variavel está sendo feito o calculo da rotação necessária para o player utilizando o lerp para suavizar
                Quaternion newRotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(playerToMouse), 0.2f);
                _rb.MoveRotation(newRotation);
            }
        }
        else
        {
            _inputRotation = _inputRotation.normalized * Time.deltaTime;
            Quaternion newRotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_inputRotation), 0.2f);
            _rb.MoveRotation(newRotation);
        }
    }
}
