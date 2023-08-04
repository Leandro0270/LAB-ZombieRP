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
    private bool isLookingRight = false;
    private bool isLookingLeft = true;
    private bool isLookingForward = false;
    private bool isLookingBack = false;
    private bool setup = false;

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
        if(_canMove)
            _inputMovimento = valor;
    }

    void FixedUpdate()
    {
        if (_speed == 0 && !setup)
        {
            _status.updateSpeedMovement();
            setup = true;
        }

        Vector3 auxVecto2 = _inputMovimento.normalized * (_speed * Time.deltaTime);
        Vector3 auxVector3 = new Vector3(auxVecto2.x, 0, auxVecto2.y);
        if (_canMove && auxVector3 != Vector3.zero)
        {
            _controller.Move(auxVector3);
            setLookigDirection(transform.rotation.eulerAngles.y);
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
            if(isLookingForward)
                _direction = PlayerDirection.RIGHT;
            if(isLookingBack)
                _direction = PlayerDirection.LEFT;
            if(isLookingRight)
                _direction = PlayerDirection.FORWARD;
            if(isLookingLeft)
                _direction = PlayerDirection.BACK;
        }
        else if(moveVector.x < 0)
        {
            if(isLookingForward)
                _direction = PlayerDirection.LEFT;
            if(isLookingBack)
                _direction = PlayerDirection.RIGHT;
            if(isLookingLeft)
                _direction = PlayerDirection.FORWARD;
            if(isLookingRight)
                _direction = PlayerDirection.BACK;
        }
        else if(moveVector.z > 0)
        {
            if(isLookingForward)
                _direction = PlayerDirection.FORWARD;
            if(isLookingBack)
                _direction = PlayerDirection.BACK;
            if(isLookingLeft)
                _direction = PlayerDirection.RIGHT;
            if(isLookingRight)
                _direction = PlayerDirection.LEFT;
        }
        else if(moveVector.z < 0)
        {
            if(isLookingForward)
                _direction = PlayerDirection.BACK;
            if(isLookingBack)
                _direction = PlayerDirection.FORWARD;
            if(isLookingRight)
                _direction = PlayerDirection.RIGHT;
            if(isLookingLeft)
                _direction = PlayerDirection.LEFT;
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

    public void setLookigDirection(float LookingAngle)
    {
        if (LookingAngle >= 315f && LookingAngle < 45f)
        {
            isLookingLeft = true;
            isLookingBack = false;
            isLookingForward = false;
            isLookingRight = false;
        }
        else if(LookingAngle >= 45f && LookingAngle < 135f)
        {
            isLookingForward = true;
            isLookingBack = false;
            isLookingLeft = false;
            isLookingRight = false;
        }
        else if(LookingAngle >= 135f && LookingAngle < 225f)
        {
            isLookingRight = true;
            isLookingBack = false;
            isLookingForward = false;
            isLookingLeft = false;
        }
        else if(LookingAngle >= 225f && LookingAngle < 315f)
        {
            isLookingBack = true;
            isLookingForward = false;
            isLookingLeft = false;
            isLookingRight = false;
        }
    }
}
