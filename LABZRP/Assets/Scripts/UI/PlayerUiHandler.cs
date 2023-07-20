using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUiHandler : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView photonView;
    private bool setup = true;
    private GameObject _player;
    private PlayerStats _playerStats;
    private WeaponSystem _weaponSystem;
    private PlayerPoints _playerPoints;
    [SerializeField] private HealthBar_UI _healthBarUi;
    [SerializeField] private Points_UI _pointsUI;
    [SerializeField] private BULLETS_UI _bulletsUi;
    public GameObject BulletText;
    public GameObject SliderComponentGameObject;
    public GameObject PointsText;
    [SerializeField] private GameObject _playerHeadSpawnPostion;
    private GameObject _InstantiatePlayerHead;


    private void Awake()
    {
        _pointsUI = PointsText.GetComponent<Points_UI>();
        _healthBarUi = SliderComponentGameObject.GetComponent<HealthBar_UI>();
        _bulletsUi = BulletText.GetComponent<BULLETS_UI>();
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
        if (setup)
        {
            if (_player)
            {
                if (_playerStats == null)
                {
                    _playerStats = _player.GetComponent<PlayerStats>();
                    _playerStats.sethealthBarUi(_healthBarUi);
                }
                
                if(_playerPoints == null)
                {
                    _playerPoints = _player.GetComponent<PlayerPoints>();
                    _playerPoints.setPointsUI(_pointsUI);
                }
                if (_weaponSystem == null)
                {
                    _weaponSystem = _player.GetComponent<WeaponSystem>();
                    _weaponSystem.setBullets_UI(_bulletsUi);
                }
                
                if(_InstantiatePlayerHead == null)
                {
                    GameObject originalPlayerHead = _playerStats.getPlayerHead();
                    _InstantiatePlayerHead = Instantiate(originalPlayerHead, _playerHeadSpawnPostion.transform.position, _playerHeadSpawnPostion.transform.rotation);
                    _InstantiatePlayerHead = Instantiate(_playerStats.getPlayerHead(), _playerHeadSpawnPostion.transform.position, _playerHeadSpawnPostion.transform.rotation);
                    _InstantiatePlayerHead.transform.SetParent(_playerHeadSpawnPostion.transform);
                }

                if (_playerStats != null && _weaponSystem != null && _playerPoints != null && _playerHeadSpawnPostion != null)
                    setup = false;
            }
        }
        
    }
}
