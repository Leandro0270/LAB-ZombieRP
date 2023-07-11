using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

//Esse comando faz com que seja necessario o objeto em que o script for aplicado tenha o componente RIGIDBODY
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerMovement : MonoBehaviour
{
    private PlayerStats _status;
    private CharacterController _controller;
    private PlayerAnimationManager _animationManager;
    private Vector3 _inputMovimento;
    private bool _canMove = true;
    private float _speed;

    void Start()
    {
        _animationManager = GetComponentInChildren<PlayerAnimationManager>();
        _status = GetComponent<PlayerStats>();
        _speed = _status.getSpeed();
        //_rb está recebendo o componente Rigidbody de onde o script está sendo aplicado
        _controller = GetComponent<CharacterController>();
    }

    public void SetInputMovimento(Vector3 valor)
    {
        _inputMovimento = valor;
    }


    void FixedUpdate()
    {
        if (_speed == 0)
        {
            _status.updateSpeedMovement();
        }
        //Time.deltaTime normaliza a atualização de comandos independente da quantidade de frames
        Vector3 auxVecto2 = _inputMovimento.normalized * (_speed * Time.deltaTime);
        Vector3 auxVector3 = new Vector3(auxVecto2.x, 0, auxVecto2.y);
        if (_canMove && auxVector3 != Vector3.zero)
        {
            _controller.Move(auxVector3);
            _status.setIsWalking(true);
        }
        else
        {
            _status.setIsWalking(false);
        }

    }
    
    
    public void setCanMove(bool valor)
    {
        _canMove = valor;
    }
    
    public void setSpeed(float valor)
    {
        _speed = valor;
    }
    
}
