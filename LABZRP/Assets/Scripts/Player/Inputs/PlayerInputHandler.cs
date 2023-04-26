using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerController _controls;
    private PauseMenu _pause;
    private PlayerConfiguration _playerConfig;
    private PlayerMovement _move;
    private PlayerRotation _rotate;
    private WeaponSystem _attack;
    private PlayerStats _status;
    private CustomizePlayerInGame _customize;
    private ThrowablePlayerStats _throwableStats;
    private MainGameManager _mainGameManager;
    public float delay = 2f;
    private float delayTimer = 0f;

    private void Awake()
    {
        _move = GetComponent<PlayerMovement>();
        _rotate = GetComponent<PlayerRotation>();
        _attack = GetComponent<WeaponSystem>();
        _status = GetComponent<PlayerStats>();
        _customize = GetComponent<CustomizePlayerInGame>();
        _controls = new PlayerController();
        _throwableStats = GetComponent<ThrowablePlayerStats>();
    }

    private void Start()
    {
        _mainGameManager =GameObject.Find("GameManager").GetComponent<MainGameManager>();
        _mainGameManager.addPlayer(gameObject);
        
    }

    private void Update()
    {
        if (delayTimer > 0)
        {
            delayTimer -= Time.deltaTime;
        }
    }

    public void InitializePlayer(PlayerConfiguration pc)
    {
        _playerConfig = pc;
        _playerConfig.Input.onActionTriggered += Input_onActionTriggered;
        if (_status != null)
            _status.setPlayerStats(pc.playerStats);
        

        if (_attack != null)
            _attack.SetGunStatus(pc.playerStats.startGun);
        if (_customize != null)
            _customize.SetSkin(pc.playerCustom);
        //Pega o dispositivo que o jogador está usando
        if (_rotate != null)
            if (_playerConfig.Input.devices[0].device is Gamepad)
                _rotate.SetGamepadValidation(true);

    }

    private void Input_onActionTriggered(CallbackContext obj)
    {
        if (obj.action.name == _controls.Gameplay.Move.name)
        {
            OnMove(obj);
        }

        if (obj.action.name == _controls.Gameplay.Rotation.name)
        {
            OnJoinRotation(obj);
        }

        if (obj.action.name == _controls.Gameplay.Reload.name)
        {
            OnReload();
        }

        if (obj.action.name == _controls.Gameplay.Melee.name)
        {
            if (delayTimer <= 0)
            {
                OnMelee();
                delayTimer = delay;
            }

        }

        if (obj.action.name == _controls.Gameplay.ShootHold.name)
        {

            OnShootPress(obj);
        }

        if (obj.action.name == _controls.Gameplay.Aim.name)
        {
            OnAimPress(obj);
        }

        if (obj.action.name == _controls.Gameplay.ChangeThrowable.name)
        {
            onChangeThrowable();
        }

        if (obj.action.name == _controls.Gameplay.Select.name)
        {
            switch (obj.phase)
            {
                case InputActionPhase.Started:
                    OnSelect(true);
                    break;
                case InputActionPhase.Canceled:
                    OnSelect(false);
                    break;
            }
        }

        if (obj.action.name == _controls.Gameplay.Throwable.name)
        {
            switch (obj.phase)
            {
                case InputActionPhase.Started:
                    onAimThrowable(true);
                    break;
                case InputActionPhase.Canceled:
                    onAimThrowable(false);
                    break;
            }

        }
        
        if (obj.action.name == _controls.Gameplay.CancelAction.name)
        {

            cancelActions();
        }
    }




    private void OnMove(CallbackContext context)
    {
        if (_move != null)
        {
            _move.SetInputMovimento(context.ReadValue<Vector2>());
        }
    }

    private void OnJoinRotation(CallbackContext ctx)
    {
        if (_rotate != null)
        {

            if (ctx.ReadValue<Vector2>() != new Vector2(0, 0))
                _rotate.setRotationInput(ctx.ReadValue<Vector2>());
        }
    }

    private void OnReload()
    {
        _attack.AuxReload();

    }

    private void OnMelee()
    {
        _attack.AuxMelee();

    }

    private void OnShootPress(CallbackContext ctx)
    {
        _attack.AuxShootPress(ctx);

    }

    private void OnSelect(bool value)
    {
        _status.setInteracting(value);

    }

    private void OnAimPress(CallbackContext ctx)
    {
        _attack.AuxAimPress(ctx);
    }

    private void onAimThrowable(bool isAiming)
    {
        _throwableStats.setAiming(isAiming);
    }
    
    private void onChangeThrowable()
    {
        _throwableStats.changeToNextItem();
        _throwableStats.cancelThrowAction();

    }

    private void cancelActions()
    {
        _throwableStats.cancelThrowAction();
    }

}
