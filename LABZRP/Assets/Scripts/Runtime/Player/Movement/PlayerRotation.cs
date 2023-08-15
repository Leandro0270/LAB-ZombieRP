using Runtime.Player.Combat.PlayerStatus;
using UnityEngine;
using UnityEngine.Serialization;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Runtime.Player.Movement
{
    [RequireComponent(typeof(PlayerStats))]

//Esse comando faz com que seja necessario o objeto em que o script for aplicado tenha o componente RIGIDBODY
    public class PlayerRotation : MonoBehaviour
    {
        private bool _isOnlinePlayer;
        [FormerlySerializedAs("_status")] [SerializeField] private PlayerStats status;
        private Vector3 _inputRotation;
        private Vector3 _inputMouse;
        private Vector3 _lateInputRotation;
        private bool _canRotate = true;

        public void SetRotationInput(Vector2 auxRotation, bool isGamepad)
        {
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



        //Para uso de componentes envolvendo fisicas (Nesse caso o RigidBody) é recomendado utilizar o fixed update
        void FixedUpdate(){
            if(_inputRotation != Vector3.zero && !_isOnlinePlayer){
                _inputRotation = _inputRotation.normalized * Time.deltaTime;
                Quaternion newRotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(_inputRotation), 0.2f);
                if (_lateInputRotation != _inputRotation)
                {
                    if (!status.GetIsDown() && !status.GetIsDead() && _canRotate)
                    {
                        transform.rotation = newRotation;
                    }
                }
            }

        }
    
        public void SetCanRotate(bool valor)
        {
            _canRotate = valor;
        }
    
        public void setIsOnlinePlayer(bool valor)
        {
            _isOnlinePlayer = valor;
        }
    }
}
    