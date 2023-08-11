using Photon.Pun;
using Runtime.Player.Combat.PlayerStatus;
using Runtime.Player.Combat.Throwables;
using Runtime.Player.Movement;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace Runtime.Player.Inputs
{
    public class PlayerInputHandler : MonoBehaviourPunCallbacks
    {
    
    
        private PlayerController _controls;
        private PauseMenu _pause;
        private OnlinePlayerConfiguration _onlinePlayerConfig;
        private PlayerConfiguration _playerConfig;
        private bool _gameIsPaused;
        [SerializeField] private PlayerMovement move;
        [SerializeField] private PlayerRotation rotate;
        [SerializeField] private WeaponSystem attack;
        [SerializeField] private PlayerStats status;
        [SerializeField] private CustomizePlayerInGame customize;
        [SerializeField] private ThrowablePlayerStats throwableStats;
        [SerializeField] private PlayerInput onlinePlayerInput;
        private MainGameManager _mainGameManager;
        public float delay = 2f;
        private float _delayTimer;
    
        private void Start()
        {
            _controls = new PlayerController();
            _mainGameManager =GameObject.Find("GameManager").GetComponent<MainGameManager>();
            _mainGameManager.addPlayer(gameObject);
            _pause = _mainGameManager.getPauseMenu();
            _pause.addPlayer(this);

        }

        private void Update()
        {
            if (_delayTimer > 0)
            {
                _delayTimer -= Time.deltaTime;
            }
        }
    
        public void InitializeOnlinePlayer(OnlinePlayerConfiguration pc)
        {
            _onlinePlayerConfig = pc;
            if (photonView.IsMine)
            {
                onlinePlayerInput.onActionTriggered += Input_onActionTriggered;
            }
            else
            {
                onlinePlayerInput.enabled = false;
                rotate.setIsOnlinePlayer(true);
            }

            if (status != null)
            {
                status.setIsOnline(true);
                status.SetPlayerStats(_onlinePlayerConfig.playerStats);
            }

            if (attack != null)
            {
                attack.setIsOnline(true);
                attack.SetGunStatus(_onlinePlayerConfig.playerStats.startGun);
            }

            if (customize != null)
            {
                customize.SetSkin(_onlinePlayerConfig.playerCustom);
            }
        }
        
    
    
        public void InitializePlayer(PlayerConfiguration pc)
        {
            _playerConfig = pc;
            _playerConfig.Input.onActionTriggered += Input_onActionTriggered;
            if (status != null)
                status.SetPlayerStats(pc.playerStats);
            if (attack != null)
                attack.SetGunStatus(pc.playerStats.startGun);
            if (customize != null)
                customize.SetSkin(pc.playerCustom);
        }

        private void Input_onActionTriggered(CallbackContext obj)
        {
            if (obj.action.name == _controls.Gameplay.Move.name)
            {
                if(!_gameIsPaused)
                    OnMove(obj);
            }

            if (obj.action.name == _controls.Gameplay.Rotation.name)
            {
                if(!_gameIsPaused)
                    OnJoinRotation(obj);
            }
            if (obj.action.name == _controls.Gameplay.Reload.name)
            {
                if(!_gameIsPaused)
                    OnReload();
            }

            if (obj.action.name == _controls.Gameplay.Melee.name)
            {
                if (!_gameIsPaused)
                {
                    if (_delayTimer <= 0)
                    {
                        OnMelee();
                        _delayTimer = delay;
                    }
                }

            }

            if (obj.action.name == _controls.Gameplay.ShootHold.name)
            {
                if(!_gameIsPaused)
                    OnShootPress(obj);
            }

            if (obj.action.name == _controls.Gameplay.Aim.name)
            {
                if(!_gameIsPaused)
                    OnAimPress(obj);
            }

            if (obj.action.name == _controls.Gameplay.ChangeThrowable.name)
            {
                if(!_gameIsPaused)
                    OnChangeThrowable();
            }

            if (obj.action.name == _controls.Gameplay.Pause.name)
            {
                if (_delayTimer <= 0)
                {
                    OnPause();
                    _delayTimer = delay;
                }
            }

            if (obj.action.name == _controls.Gameplay.Select.name)
            {
                if (!_gameIsPaused)
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
                if (!_gameIsPaused)
                {
                    switch (obj.phase)
                    {
                        case InputActionPhase.Started:
                            OnAimThrowable(true);
                            break;
                        case InputActionPhase.Canceled:
                            OnAimThrowable(false);
                            break;
                    }
                }

            }
        
            if (obj.action.name == _controls.Gameplay.CancelAction.name)
            {

                CancelActions();
            }
        }



        private void OnPause()
        {
            _pause.gameObject.SetActive(true);
            _gameIsPaused = _pause.EscButton();
        }
    
        private void OnMove(CallbackContext context)
        {
            if (move != null)
            {
                move.SetInputMovement(context.ReadValue<Vector2>());
            }
        }

        private void OnJoinRotation(CallbackContext ctx)
        {
            if (rotate != null)
            {
                PlayerInput playerInput;
                if(_onlinePlayerConfig != null)
                    playerInput = onlinePlayerInput;
                else
                    playerInput = _playerConfig.Input;

                if (ctx.ReadValue<Vector2>() != new Vector2(0, 0))
                    rotate.SetRotationInput(ctx.ReadValue<Vector2>(),playerInput.devices[0] is Gamepad);
            }
        }

        private void OnReload()
        {
            attack.AuxReload();

        }

        private void OnMelee()
        {
            attack.AuxMelee();

        }

        private void OnShootPress(CallbackContext ctx)
        {
            attack.AuxShootPress(ctx);

        }

        private void OnSelect(bool value)
        {
            status.SetInteracting(value);

        }

        private void OnAimPress(CallbackContext ctx)
        {
            attack.AuxAimPress(ctx);
        }

        private void OnAimThrowable(bool isAiming)
        {
            throwableStats.setAiming(isAiming);
        }
        private void OnChangeThrowable()
        {
            throwableStats.changeToNextItem();
            throwableStats.cancelThrowAction();

        }
        private void CancelActions()
        {
            throwableStats.cancelThrowAction();
        }
        public void SetGameIsPaused(bool value)
        {
            _gameIsPaused = value;
        }
    }
}
