using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

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
    private PlayerDirection _direction;

    public enum PlayerDirection
    {
        FORWARD,
        BACK,
        LEFT,
        RIGHT,
        STANDING
    }

    void Start()
    {
        _animationManager = GetComponentInChildren<PlayerAnimationManager>();
        _status = GetComponent<PlayerStats>();
        _speed = _status.getSpeed();
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

        Vector3 auxVecto2 = _inputMovimento.normalized * (_speed * Time.deltaTime);
        Vector3 auxVector3 = new Vector3(auxVecto2.x, 0, auxVecto2.y);
        if (_canMove && auxVector3 != Vector3.zero)
        {
            _controller.Move(auxVector3);
            UpdateDirection(auxVector3);
        }
        else
        {
            _direction = PlayerDirection.STANDING;
        }
        _status.setMovementAnimationStats(_direction);
    }

    void UpdateDirection(Vector3 moveVector)
    {
        if(moveVector.x > 0)
        {
            _direction = PlayerDirection.RIGHT;
        }
        else if(moveVector.x < 0)
        {
            _direction = PlayerDirection.LEFT;
        }
        else if(moveVector.z > 0)
        {
            _direction = PlayerDirection.FORWARD;
        }
        else if(moveVector.z < 0)
        {
            _direction = PlayerDirection.BACK;
        }
    }
    
    public PlayerDirection getDirection()
    {
        return _direction;
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
