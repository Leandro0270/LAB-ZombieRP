using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

//Esse comando faz com que seja necessario o objeto em que o script for aplicado tenha o componente RIGIDBODY
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerMovement : MonoBehaviour
{
    private PlayerStats _status;
    private Rigidbody _rb;
    private Vector3 _inputMovimento;
    //!!! O atributo speed será modificado em breve para comportar modificações por scriptObject
    private float _speed;

    void Start()
    {
        _status = GetComponent<PlayerStats>();
        _speed = _status.getSpeed();
        //_rb está recebendo o componente Rigidbody de onde o script está sendo aplicado
        _rb = GetComponent<Rigidbody>();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _inputMovimento = context.ReadValue<Vector2>();
        //O movimento vertical não será utilizado, por isso está sendo zerado
    }

    
    //Para uso de componentes envolvendo fisicas (Nesse caso o RigidBody) é recomendado utilizar o fixed update
    void FixedUpdate()
    {
        //Time.deltaTime normaliza a atualização de comandos independente da quantidade de frames
        _inputMovimento = _inputMovimento.normalized * (_speed * Time.deltaTime);
        Vector3 auxVector3 = new Vector3(_inputMovimento.x, 0, _inputMovimento.y);
        if(!_status.verifyDown() && !_status.verifyDeath())
        _rb.MovePosition(transform.position + auxVector3);
    }
}
