using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUiHandler : MonoBehaviour
{
    private bool setup = true;
    private GameObject _player;
    private PlayerStats _playerStats;
    private WeaponSystem _weaponSystem;
    private HealthBar_UI _healthBarUi;
    private BULLETS_UI _bulletsUi;
    public GameObject BulletText;
    public GameObject SliderComponentGameObject;


    private void Awake()
    {
        _healthBarUi = SliderComponentGameObject.GetComponent<HealthBar_UI>();
        _bulletsUi = BulletText.GetComponent<BULLETS_UI>();
    }

    public HealthBar_UI GetHealthBarUI()
    {
        return SliderComponentGameObject.GetComponent<HealthBar_UI>();
    }
    
    public BULLETS_UI getBulletsUI()
    {
        return BulletText.GetComponent<BULLETS_UI>();
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

                if (_weaponSystem == null)
                {
                    _weaponSystem = _player.GetComponent<WeaponSystem>();
                    _weaponSystem.setBullets_UI(_bulletsUi);
                }

                if (_playerStats != null && _weaponSystem != null)
                    setup = false;
            }
        }
        
    }
}
