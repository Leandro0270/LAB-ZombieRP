using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Rendering.Universal;
using Random = UnityEngine.Random;

public class PlayerStats : MonoBehaviour
{
    
    //VISUAL
    public GameObject blood1;
    public GameObject bloodSplash;
    public GameObject blood2;
    private PlayerAnimationManager _playerAnimationManager;
    public float dispersaoSangue = 2;
    
    //Player Specs
   
    private ScObPlayerStats _playerStatus;
    private bool _isDown;
    private bool _isDead;
    public float totalLife;
    public float life;
    private float _downLife = 100f;
    private float _speed;
    private float _revivalSpeed;
    private float _timeBetweenMelee;
    private float _meleeDamage;
    private bool _interacting;
    private bool _stopDeathLife = false;
    private bool _SetupColorComplete = false;
    
    //UI
    private HealthBar_UI _healthBarUi;
    public GameObject PlayerUI;
    private Color _characterColor;

    //Script components
    public DecalProjector _playerIndicator;
    private CâmeraMovement _camera;
    private PlayerMovement _playerMovement;
    private PlayerRotation _playerRotation;
    private WeaponSystem _weaponSystem;
    private ItemHorderGenerator _itemHorderGenerator;
    private PlayerPoints _playerPoints;





//======================================================================================================
//Unity base functions

    private void Start()
    {
        _InitializePlayerSpecs();
    }
    

        private void Update()
    {

        if(!_SetupColorComplete)
        {
            if (_healthBarUi.getColor() != _characterColor)
            {
                
                _healthBarUi.setColor(_characterColor);
                _SetupColorComplete = true;
            }
        }
        if (_isDown && !_isDead && !_stopDeathLife)
        {
            _healthBarUi.setColor(Color.gray);
            _healthBarUi.SetHealth((int)_downLife);
            _downLife -= Time.deltaTime;
            
        }
        if (_downLife <= 0)
        {
            PlayerDeath();
        }
        
        if(_isDown)
            if(_healthBarUi.getColor() != _characterColor)
                _healthBarUi.setColor(Color.gray);
    }

//======================================================================================================
//Main functions
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
            _healthBarUi.SetHealth((int)life);
        }

        if (life < 1)
        {
            GameObject _blood2 = Instantiate(blood2, transform.position, blood2.transform.rotation);
            Destroy(_blood2, 15f);
            _isDown = true;
            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<BoxCollider>().enabled = true;
            _weaponSystem.SetProntoparaAtirar(false);
            _camera.removePlayer(gameObject);
            _playerMovement.setCanMove(false);
            _playerRotation.setCanRotate(false);
            _playerAnimationManager.setDowning();
            _playerAnimationManager.setDown(true);
            _weaponSystem.SetGunVisable(false);
            _healthBarUi.setColor(Color.gray);
            
        }
    }

    public void revived()
    {
        if (_isDown && !_isDead)
        {
            _weaponSystem.SetProntoparaAtirar(true);
            GetComponent<CapsuleCollider>().enabled = true;
            GetComponent<BoxCollider>().enabled = false;
            _playerRotation.setCanRotate(true);
            _playerMovement.setCanMove(true);
            _isDown = false;
            _playerAnimationManager.setDown(false);
            life = totalLife * 0.3f;
            _camera.addPlayer(gameObject);
            _weaponSystem.SetGunVisable(true);
            _healthBarUi.setColor(_characterColor);
            _healthBarUi.SetHealth((int)life);

        }
    }

    public void PlayerDeath()
    {
        
        _itemHorderGenerator.removePlayer(gameObject);
        _isDead = true;
        
    }
    
    public void ReceiveHeal(float heal)
    {
        if (!_isDown && !_isDead)
            life += heal;
        if (life > totalLife)
            life = totalLife;
        _healthBarUi.SetHealth((int)life);

    }
    
    private void _InitializePlayerSpecs()
    {
        _weaponSystem = GetComponent<WeaponSystem>();
        _playerMovement = GetComponent<PlayerMovement>();
        _playerRotation = GetComponent<PlayerRotation>();
        _characterColor = _playerStatus.MainColor;
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
        GameObject findCanvaHud = GameObject.FindGameObjectWithTag("PlayersUI");
        if (findCanvaHud == null)
            Debug.LogError("Não foi encontrado o Canvas HUD, posicione ele na cena");
        PlayerUiHandler playerUiConfig = Instantiate(PlayerUI, findCanvaHud.transform).GetComponent<PlayerUiHandler>();
        playerUiConfig.transform.parent = findCanvaHud.transform;
        playerUiConfig.setPlayer(this.gameObject);
        _playerIndicator.material = _playerStatus.PlayerIndicator;
    }

    //================================================================================================
    //Getters and Setters
    
    
    public void sethealthBarUi(HealthBar_UI healthBarUi)
    {
        _healthBarUi = healthBarUi;
        healthBarUi.setColor(_characterColor);
        healthBarUi.setMaxHealth((int)totalLife);
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
    
    public bool getInteracting()
    {
        return _interacting;
    }
    
    public void stopDeathCounting(bool value)
    {
        _stopDeathLife = value;
    }
    
    public void setPlayerStats(ScObPlayerStats stats)
    {
        _playerStatus = stats;
    }
    public float getSpeed()
    {
        return _speed;
    }
}
