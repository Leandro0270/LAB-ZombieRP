using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Esse comando faz com que seja necessario o objeto em que o script for aplicado tenha o componente RIGIDBODY
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody _rb;
    private Vector3 _inputMovimento;
    //!!! O atributo speed será modificado em breve para comportar modificações por scriptObject
    public int speed = 10;
    void Start()
    {
        //_rb está recebendo o componente Rigidbody de onde o script está sendo aplicado
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        //Estes comandos estão localizados na própria unity Project Settings>InputManager
        _inputMovimento.x = Input.GetAxisRaw("Horizontal");
        _inputMovimento.z = Input.GetAxisRaw("Vertical");
        //O movimento vertical não será utilizado, por isso está sendo zerado
        _inputMovimento.y = 0;
    }
    
    //Para uso de componentes envolvendo fisicas (Nesse caso o RigidBody) é recomendado utilizar o fixed update
    void FixedUpdate()
    {
        //Time.deltaTime normaliza a atualização de comandos independente da quantidade de frames
        _inputMovimento = _inputMovimento.normalized * (speed * Time.deltaTime);
        _rb.MovePosition(transform.position + _inputMovimento);
    }
}
