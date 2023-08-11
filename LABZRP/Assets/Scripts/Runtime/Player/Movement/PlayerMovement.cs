using Runtime.Player.Combat.PlayerStatus;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Runtime.Player.Movement
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(PlayerStats))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private PlayerStats status;
        [SerializeField] private CharacterController controller;
        private Vector3 _inputMovement;
        private bool _canMove = true;
        private float _speed;
        private PlayerDirection _direction;
        private bool _isLookingRight;
        private bool _isLookingLeft = true;
        private bool _isLookingForward;
        private bool _isLookingBack;
        private bool _setup;
        private bool _isAiming;
        private bool _isRotated;
        private bool _isEffectSpeedSlowed;
        private float _effectSpeedSlowPercentage;
        private float _rotationSlowPercentage;
        private float _aimingSlowPercentage;

        public enum PlayerDirection
        {
            Forward,
            Back,
            Left,
            Right,
            Standing
        }

        private void Start()
        {
            status = GetComponent<PlayerStats>();
            _speed = status.GetSpeed(); 
        }

        public void SetInputMovement(Vector3 valor)
        {
            if(_canMove)
                _inputMovement = valor;
        }

        private void FixedUpdate()
        {
            if (_speed == 0 && !_setup)
            {
                status.InitializePlayerMovementSpeed();
                _setup = true;
            }

            float newSpeed = _speed;
            if (_isRotated)
                newSpeed *= _rotationSlowPercentage;
            if(_isAiming)
                newSpeed *= _aimingSlowPercentage;
            if(_isEffectSpeedSlowed)
                newSpeed *= _effectSpeedSlowPercentage;
            
            Vector3 auxVector2 = _inputMovement.normalized * (newSpeed * Time.deltaTime);
            Vector3 auxVector3 = new Vector3(auxVector2.x, 0, auxVector2.y);
            if (_canMove && auxVector3 != Vector3.zero)
            {
                controller.Move(auxVector3);
                SetLookingDirection(transform.rotation.eulerAngles.y);
                UpdateDirection(auxVector3);
            }
            else
            {
                _direction = PlayerDirection.Standing;
            }
            status.SetMovementAnimationStats(_direction);
        }

        void UpdateDirection(Vector3 moveVector)
        {
            if(moveVector.x > 0)
            {
                if(_isLookingForward)
                    _direction = PlayerDirection.Right;
                if(_isLookingBack)
                    _direction = PlayerDirection.Left;
                if(_isLookingRight)
                    _direction = PlayerDirection.Forward;
                if(_isLookingLeft)
                    _direction = PlayerDirection.Back;
            }
            else if(moveVector.x < 0)
            {
                if(_isLookingForward)
                    _direction = PlayerDirection.Left;
                if(_isLookingBack)
                    _direction = PlayerDirection.Right;
                if(_isLookingLeft)
                    _direction = PlayerDirection.Forward;
                if(_isLookingRight)
                    _direction = PlayerDirection.Back;
            }
            else if(moveVector.z > 0)
            {
                if(_isLookingForward)
                    _direction = PlayerDirection.Forward;
                if(_isLookingBack)
                    _direction = PlayerDirection.Back;
                if(_isLookingLeft)
                    _direction = PlayerDirection.Right;
                if(_isLookingRight)
                    _direction = PlayerDirection.Left;
            }
            else if(moveVector.z < 0)
            {
                if(_isLookingForward)
                    _direction = PlayerDirection.Back;
                if(_isLookingBack)
                    _direction = PlayerDirection.Forward;
                if(_isLookingRight)
                    _direction = PlayerDirection.Right;
                if(_isLookingLeft)
                    _direction = PlayerDirection.Left;
            }
        }
        
    
        public void SetCanMove(bool valor)
        {
            _canMove = valor;
        }
    

        private void SetLookingDirection(float lookingAngle)
        { switch (lookingAngle)
            {
                case >= 45f and < 135f:
                    _isLookingForward = true;
                    _isLookingBack = false;
                    _isLookingLeft = false;
                    _isLookingRight = false;
                    break;
                case >= 135f and < 225f:
                    _isLookingRight = true;
                    _isLookingBack = false;
                    _isLookingForward = false;
                    _isLookingLeft = false;
                    break;
                case >= 225f and < 315f:
                    _isLookingBack = true;
                    _isLookingForward = false;
                    _isLookingLeft = false;
                    _isLookingRight = false;
                    break;
                case >=315f and < 360f:
                    _isLookingLeft = true;
                    _isLookingBack = false;
                    _isLookingForward = false;
                    _isLookingRight = false;
                    break;
            }
        }
    
        public void SetEffectSpeedSlowPercentage(float percentageSlow, bool isEffectSpeedSlowed)
        {
            _effectSpeedSlowPercentage = percentageSlow;
            _isEffectSpeedSlowed = isEffectSpeedSlowed;
        }
        public void SetAiming(float aimingSlow, bool isAiming)
        {
            _isAiming = isAiming;
            _aimingSlowPercentage = aimingSlow;
        }
        public void SetRotationSlowPercentage(float percentageSlow, bool isRotated)
        {
            _rotationSlowPercentage = percentageSlow;
            _isRotated = isRotated;
        }
    
        public void SetSpeed(float speed)
        {
            _speed = speed;
        }


   
    }
}
