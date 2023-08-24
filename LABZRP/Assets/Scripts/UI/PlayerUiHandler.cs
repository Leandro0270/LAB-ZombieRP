using Photon.Pun;
using Runtime.Player.Combat.Gun;
using Runtime.Player.Combat.PlayerStatus;
using Runtime.Player.Points;
using UI.Life;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    public class PlayerUiHandler : MonoBehaviourPunCallbacks
    {
        private bool setup = true;
        private GameObject _player;
        private PlayerStats _playerStats;
        private WeaponSystem _weaponSystem;
        private PlayerPoints _playerPoints;
        [FormerlySerializedAs("_healthBarUi")] [SerializeField] private HealthBar_UI healthBarUi;
        [FormerlySerializedAs("_pointsUI")] [SerializeField] private Points_UI pointsUI;
        [FormerlySerializedAs("_bulletsUi")] [SerializeField] private BULLETS_UI bulletsUi;
        [SerializeField] private GameObject BulletText;
        [SerializeField] private GameObject SliderComponentGameObject;
        [SerializeField] private GameObject PointsText;
        [SerializeField] private RawImage playerHeadImage;
        private PlayerHeadUiHandler _playerHeadUiHandler;


        private void Awake()
        {
            if(pointsUI == null)
                pointsUI = PointsText.GetComponent<Points_UI>();
            if(healthBarUi == null)
                healthBarUi = SliderComponentGameObject.GetComponent<HealthBar_UI>();
            if(bulletsUi == null)
                bulletsUi = BulletText.GetComponent<BULLETS_UI>();
        }

        public HealthBar_UI GetHealthBarUI()
        {
            return SliderComponentGameObject.GetComponent<HealthBar_UI>();
        }
    
        public Points_UI getPointsUI()
        {
            return PointsText.GetComponent<Points_UI>();
        }
    
        public BULLETS_UI getBulletsUI()
        {
            return BulletText.GetComponent<BULLETS_UI>();
        }

        [PunRPC]
        public void setOnlinePlayerRPC(int PhotonViewId)
        {
            GameObject player = PhotonView.Find(PhotonViewId).gameObject;
            _player = player;
        }
        public void setOnlinePlayer(GameObject player)
        {
            int photonViewId = player.GetComponent<PhotonView>().ViewID;
            photonView.RPC("setOnlinePlayerRPC", RpcTarget.All, photonViewId);
        }
        public void setPlayer(GameObject player)
        {
            _player = player;
        }

        private void Update()
        {
            if (!setup) return;
            if (!_player) return;
            
            if (_playerStats == null)
            {
                _playerStats = _player.GetComponent<PlayerStats>();
                _playerStats.SetHealthBarUi(healthBarUi);
            }
                
            if(_playerPoints == null)
            {
                _playerPoints = _player.GetComponent<PlayerPoints>();
                _playerPoints.setPointsUI(pointsUI);
            }
            if (_weaponSystem == null)
            {
                _weaponSystem = _player.GetComponent<WeaponSystem>();
                _weaponSystem.setBullets_UI(bulletsUi);
            }
            if (_playerHeadUiHandler == null)
            {
                _playerHeadUiHandler = _playerStats.GetPlayerHeadUiHandler();
                playerHeadImage.texture = _playerHeadUiHandler.GetOutPutImage();
            }
            

            if (_playerStats != null && _weaponSystem != null && _playerPoints != null )
                setup = false;

        }
    }
}
