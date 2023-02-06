using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerController _controls;
    private PlayerConfiguration _playerConfig;
    private PlayerMovement _move;
    private PlayerRotation _rotate;
    private WeaponSystem _attack;
    private PlayerStats _status;

    private void Awake()
    {
        _move = GetComponent<PlayerMovement>();
        _rotate = GetComponent<PlayerRotation>();
        _attack = GetComponent<WeaponSystem>();
        _status = GetComponent<PlayerStats>();
        _controls = new PlayerController();

    }

    public void InitializePlayer(PlayerConfiguration pc)
    {
        _playerConfig = pc;
        _playerConfig.Input.onActionTriggered += Input_onActionTriggered;
        if (_status != null)
            _status.setPlayerStats(pc.playerStats);
        if(_attack != null)
            _attack.SetGunStatus(pc.playerStats.startGun);
    }

    private void Input_onActionTriggered(CallbackContext obj)
    {
        if (obj.action.name == _controls.Gameplay.Move.name)
            OnMove(obj);

        if (obj.action.name == _controls.Gameplay.Rotation.name)
            OnJoinRotation(obj);

        if (obj.action.name == _controls.Gameplay.Reload.name)
            OnReload();
        
        if(obj.action.name == _controls.Gameplay.Melee.name)
            OnMelee();
        
        if(obj.action.name == _controls.Gameplay.ShootTap.name)
            OnShootTap(obj);
        
        if(obj.action.name == _controls.Gameplay.ShootHold.name)
            OnShootPress(obj);

    }


    public void OnMove(CallbackContext context)
    {
        if (_move != null)
        {
            _move.SetInputMovimento(context.ReadValue<Vector2>());
        }
    }

    public void OnJoinRotation(CallbackContext ctx)
    {
        if (_rotate != null)
        {
            if (ctx.control.device is Gamepad){
                _rotate.SetGamepadValidation(true);
                
                if(ctx.ReadValue<Vector2>() != new Vector2(0,0))
                     _rotate.setRotationInput(ctx.ReadValue<Vector2>());
            }

        }
        
    }

    public void OnReload()
    {
        _attack.AuxReload();
    }

    public void OnMelee()
    {
        _attack.AuxMelee();
    }

    public void OnShootPress(CallbackContext ctx)
    {
        _attack.AuxShootPress(ctx);
    }
    
    public void OnShootTap(CallbackContext ctx)
    {
        _attack.AuxShootTap(ctx);
    }
    
}
