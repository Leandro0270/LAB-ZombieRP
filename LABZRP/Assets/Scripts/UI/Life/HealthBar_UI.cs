using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI.Life
{
    public class HealthBar_UI : MonoBehaviourPunCallbacks, IPunObservable
    {
        private Color _playerColor;
        [SerializeField] private Color downLifeColor;
        [FormerlySerializedAs("_slider")] [SerializeField] private Slider slider;
        [FormerlySerializedAs("_fill")] [SerializeField] private Image fill;

        public void SetMaxHealth(float health)
        {
            slider.maxValue = health;
            slider.value = health;
        }
        public void SetHealth(float health)
        {
            slider.value = health;
        }

        public void RevivePlayer()
        {
            fill.color = _playerColor;
        }
        public void DownPlayer()
        {
            fill.color = downLifeColor;
        }
        public void SetupPlayerColor(Color color)
        {
            _playerColor = color;
            fill.color = color;
        }
    
    
        public Color GetPlayerSetupColor()
        {
            return _playerColor;
        }
    
    
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(slider.maxValue);
                stream.SendNext(slider.value);
            }
            else
            {
                slider.maxValue = (float)stream.ReceiveNext();
                slider.value = (float)stream.ReceiveNext();
            

            }
        }
    }
}
