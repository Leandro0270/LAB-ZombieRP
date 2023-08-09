using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

public class PlayerInputHandler : MonoBehaviourPunCallbacks
{
    
    [SerializeField] PlayerInput _OnlinePlayerInput;
    [SerializeField] PhotonView _photonView;
    private PlayerController _controls;
    private PauseMenu _pause;
    private OnlinePlayerConfiguration _OnlinePlayerConfig;
    private PlayerConfiguration _playerConfig;
    private bool gameIsPaused = false;
    [SerializeField] private PlayerMovement _move;
    [SerializeField] private PlayerRotation _rotate;
    [SerializeField] private WeaponSystem _attack;
    [SerializeField] private PlayerStats _status;
    [SerializeField] private CustomizePlayerInGame _customize;
    [SerializeField] private ThrowablePlayerStats _throwableStats;
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
        _pause = _mainGameManager.getPauseMenu();
        _pause.addPlayer(this);

    }

    private void Update()
    {
        if (delayTimer > 0)
        {
            delayTimer -= Time.deltaTime;
        }
    }
    
    public void InitializeOnlinePlayer(OnlinePlayerConfiguration pc)
    {
        _OnlinePlayerConfig = pc;
        if (_photonView.IsMine)
        {
            _OnlinePlayerInput.onActionTriggered += Input_onActionTriggered;
        }
        else
        {
            _OnlinePlayerInput.enabled = false;
            _rotate.setIsOnlinePlayer(true);
        }

        if (_status != null)
        {
            _status = GetComponent<PlayerStats>();
            _status.setIsOnline(true);
            _status.setPlayerStats(_OnlinePlayerConfig.playerStats);
        }

        if (_attack != null)
        {
            _attack = GetComponent<WeaponSystem>();
            _attack.setIsOnline(true);
            _attack.SetGunStatus(_OnlinePlayerConfig.playerStats.startGun);
        }

        if (_customize != null)
        {
            _customize = GetComponent<CustomizePlayerInGame>();
            _customize.SetSkin(_OnlinePlayerConfig.playerCustom);
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
            if(!gameIsPaused)
                OnMove(obj);
        }

        if (obj.action.name == _controls.Gameplay.Rotation.name)
        {
            if(!gameIsPaused)
                OnJoinRotation(obj);
        }

        if (obj.action.name == _controls.Gameplay.Reload.name)
        {
            if(!gameIsPaused)
                OnReload();
        }

        if (obj.action.name == _controls.Gameplay.Melee.name)
        {
            if (!gameIsPaused)
            {
                if (delayTimer <= 0)
                {
                    OnMelee();
                    delayTimer = delay;
                }
            }

        }

        if (obj.action.name == _controls.Gameplay.ShootHold.name)
        {
            if(!gameIsPaused)
                OnShootPress(obj);
        }

        if (obj.action.name == _controls.Gameplay.Aim.name)
        {
            if(!gameIsPaused)
                OnAimPress(obj);
        }

        if (obj.action.name == _controls.Gameplay.ChangeThrowable.name)
        {
            if(!gameIsPaused)
                onChangeThrowable();
        }

        if (obj.action.name == _controls.Gameplay.Pause.name)
        {
            if (delayTimer <= 0)
            {
                OnPause();
                delayTimer = delay;
            }
        }

        if (obj.action.name == _controls.Gameplay.Select.name)
        {
            if (!gameIsPaused)
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
        }

        if (obj.action.name == _controls.Gameplay.Throwable.name)
        {
            if (!gameIsPaused)
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

        }
        
        if (obj.action.name == _controls.Gameplay.CancelAction.name)
        {

            cancelActions();
        }
    }



    private void OnPause()
    {
        _pause.gameObject.SetActive(true);
        gameIsPaused = _pause.EscButton();
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
            PlayerInput playerInput;
            if(_OnlinePlayerConfig != null)
                playerInput = _OnlinePlayerInput;
            else
                playerInput = _playerConfig.Input;

            if (ctx.ReadValue<Vector2>() != new Vector2(0, 0))
                _rotate.setRotationInput(ctx.ReadValue<Vector2>(),playerInput.devices[0] is Gamepad);
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

    
    
    public void setGameIsPaused(bool value)
    {
        gameIsPaused = value;
    }
}
