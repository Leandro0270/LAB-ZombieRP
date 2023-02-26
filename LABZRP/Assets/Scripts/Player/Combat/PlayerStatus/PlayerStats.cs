using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using Random = UnityEngine.Random;

public class PlayerStats : MonoBehaviour
{
    private PlayerAnimationManager _playerAnimationManager;
    private ScObPlayerStats _playerStatus;
    private bool _isDown;
    private bool _isDead ;
    public float totalLife;
    public float life;
    private float _downLife = 100f;
    private float _speed;
    public float dispersaoSangue = 2;
    private float _revivalSpeed;
    private float _timeBetweenMelee;
    private float _meleeDamage;
    private bool _interacting;
    private bool _stopDeathLife = false;
    private CâmeraMovement _camera;
    private PlayerMovement _playerMovement;
    private PlayerRotation _playerRotation;
    private WeaponSystem _weaponSystem;
    private ItemHorderGenerator _itemHorderGenerator;
    public GameObject blood1;
    public GameObject bloodSplash;
    public GameObject blood2;

    private void Start()
    {
        
        _speed = _playerStatus.speed;
        totalLife = _playerStatus.health;
        life = totalLife;
        _revivalSpeed = _playerStatus.revivalSpeed;
        _timeBetweenMelee = _playerStatus.timeBeteweenMelee;
        _meleeDamage = _playerStatus.meleeDamage;
        _itemHorderGenerator = GameObject.FindGameObjectWithTag("HorderManager").GetComponent<ItemHorderGenerator>();
        _itemHorderGenerator.addPlayer(gameObject);
        _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CâmeraMovement>();
        _camera.addPlayer(gameObject);
        _playerAnimationManager = GetComponentInChildren<PlayerAnimationManager>();
        _weaponSystem = GetComponent<WeaponSystem>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerRotation = GetComponent<PlayerRotation>();
    }

    // Update is called once per frame

    private void Update()
    {
        if (_isDown && !_isDead && !_stopDeathLife)
        {
            _downLife -= Time.deltaTime;
        }

        if (_downLife <= 0)
        {
            PlayerDeath();
        }
    }
    

    public void ReceiveHeal(float heal)
    {
        if (!_isDown && !_isDead)
            life += heal;
        if (life > totalLife)
            life = totalLife;
    }

    public void setPlayerStats(ScObPlayerStats stats)
    {
        _playerStatus = stats;
    }
    public float getSpeed()
    {
        return _speed;
    }

    public void takeDamage(float damage)
    {
        if (!_isDown && !_isDead)
        {
            float y = Random.Range(-dispersaoSangue, dispersaoSangue);
            float x = Random.Range(-dispersaoSangue, dispersaoSangue);
            Vector3 spawnPosition = new Vector3(transform.position.x + y, transform.position.y - 0.27f, transform.position.z +x);
            GameObject _blood1 = Instantiate(blood1, spawnPosition, blood1.transform.rotation);
            Destroy(_blood1, 15f);
            GameObject _bloodSplash = Instantiate(bloodSplash, transform.position, transform.rotation);
            Destroy(_bloodSplash, 2f);
            life -= damage;
        }

        if (life < 1)
        {
            GameObject _blood2 = Instantiate(blood2, transform.position, blood2.transform.rotation);
            Destroy(_blood2, 15f);
            _isDown = true;
            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<BoxCollider>().enabled = true;
            _weaponSystem.setProntoparaAtirar(false);
            _camera.removePlayer(gameObject);
            _playerMovement.setCanMove(false);
            _playerRotation.setCanRotate(false);
            _playerAnimationManager.setDowning();
            _playerAnimationManager.setDown(true);
        }
    }

    public void PlayerDeath()
    {
        
            _itemHorderGenerator.removePlayer(gameObject);
            _isDead = true;
        
    }



    public void revived()
    {
        if (_isDown && !_isDead)
        {
            _weaponSystem.setProntoparaAtirar(true);
            GetComponent<CapsuleCollider>().enabled = true;
            GetComponent<BoxCollider>().enabled = false;
            _playerRotation.setCanRotate(true);
            _playerMovement.setCanMove(true);
            _isDown = false;
            _playerAnimationManager.setDown(false);
            life = totalLife * 0.3f;
            _camera.addPlayer(gameObject);
        }
    }

    public bool verifyDown()
    {
        return _isDown;
    }

    public bool verifyDeath()
    {
        return _isDead;
    }
    
    public float GetLife()
    {
        return life;
    }
    
    public float GetTotalLife()
    {
        return totalLife;
    }

    public void updateSpeedMovement()
    {
        _playerMovement.setSpeed(_speed);
    }

    public float getMeleeDamage()
    {
        return _meleeDamage;
    }
    
    public float getTimeBetweenMelee()
    {
        return _timeBetweenMelee;
    }
    
    public float getRevivalSpeed()
    {
        return _revivalSpeed;
    }
    
    public void setInteracting(bool value)
    {
        _interacting = value;
    }

    public bool GetisDead()
    {
        return _isDead;
    }
    public bool getInteracting()
    {
        return _interacting;
    }
    
    public void stopDeathCounting(bool value)
    {
        _stopDeathLife = value;
    }
    
}
