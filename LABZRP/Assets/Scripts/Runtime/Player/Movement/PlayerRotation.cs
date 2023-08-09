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
public class PlayerRotation : MonoBehaviour
{
    [SerializeField] private PlayerMovement _move;
    private bool _canSwitchInputs = true;
    private bool isOnlinePlayer;
    private PlayerStats _status;
    private Vector3 _inputRotation;
    private Vector3 _inputMouse;
    private Vector3 _lateInputRotation;
    private bool _canRotate = true;
    private RaycastHit _hit;
    public LayerMask ground;

    private bool isGamepad;



    public void SetGamepadValidation(bool validation)
    {
        isGamepad = validation;
    }
    
    private void setCanSwitchInputs(bool canSwitch)
    {
        _canSwitchInputs = canSwitch;
    }
    public void setRotationInput(Vector2 auxRotation, bool isGamepad)
    {
        this.isGamepad = isGamepad;
        if (isGamepad)
        {
            _inputRotation.z = -auxRotation.x;
            _inputRotation.x = auxRotation.y;
            _inputRotation.y = 0;
        }
        else
        {
            Vector2 normalizedRotation = new Vector2(
                (auxRotation.x / Screen.width) * 2 - 1,
                (auxRotation.y / Screen.height) * 2 - 1
            );
            _inputRotation.z = -normalizedRotation.x;
            _inputRotation.x = normalizedRotation.y;
            _inputRotation.y = 0;
        }
    }

void Start()
    {
        _status = GetComponent<PlayerStats>();
        //_rb está recebendo o componente Rigidbody de onde o script está sendo aplicado
    }
    
    

    //Para uso de componentes envolvendo fisicas (Nesse caso o RigidBody) é recomendado utilizar o fixed update
    void FixedUpdate(){
        if(_inputRotation != Vector3.zero && !isOnlinePlayer){
            _inputRotation = _inputRotation.normalized * Time.deltaTime;
            Quaternion newRotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_inputRotation), 0.2f);
            if(_lateInputRotation != _inputRotation)
            {
                if (!_status.verifyDown() && !_status.verifyDeath() && _canRotate)
                    transform.rotation = newRotation;
            }
        }

    }
    
    public void setCanRotate(bool valor)
    {
        _canRotate = valor;
    }
    
    public void setIsOnlinePlayer(bool valor)
    {
        isOnlinePlayer = valor;
    }
}
    