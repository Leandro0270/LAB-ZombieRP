using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using Random = UnityEngine.Random;

public class PlayerStats : MonoBehaviour
{
    private PlayerAnimationManager _playerAnimationManager;
    private ScObPlayerStats playerStatus;
    private bool isDown;
    private bool isDead ;
    public float totalLife;
    public float life;
    private float downLife = 100f;
    private float speed;
    public float dispersaoSangue = 2;
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
        
        speed = playerStatus.speed;
        totalLife = playerStatus.health;
        life = totalLife;
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


    public void ReceiveHeal(float heal)
    {
        if (!isDown && !isDead)
            life += heal;
        if (life > totalLife)
            life = totalLife;
    }

    public void setPlayerStats(ScObPlayerStats stats)
    {
        playerStatus = stats;
    }
    public float getSpeed()
    {
        return speed;
    }

    public void takeDamage(float damage)
    {
        if (!isDown && !isDead)
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
            isDown = true;
            GetComponent<CapsuleCollider>().enabled = false;
            GetComponent<BoxCollider>().enabled = true;
            _weaponSystem.setProntoparaAtirar(false);
            _camera.removePlayer(gameObject);
            _playerMovement.setCanMove(false);
            _playerRotation.setCanRotate(false);
            _playerAnimationManager.setDowning();
            _playerAnimationManager.setDown(true);
            deadTimer();
        }
    }

    public void deadTimer()
    {
        if(verifyDown() && downLife >1)
            downLife -= Time.deltaTime;
        else
        {
            _itemHorderGenerator.removePlayer(gameObject);
            isDead = true;
        }
    }



    public void revived()
    {
        if (isDown && !isDead)
        {
            _weaponSystem.setProntoparaAtirar(true);
            GetComponent<CapsuleCollider>().enabled = true;
            GetComponent<BoxCollider>().enabled = false;
            _playerRotation.setCanRotate(true);
            _playerMovement.setCanMove(true);
            isDown = false;
            _playerAnimationManager.setDown(false);
            life = totalLife * 0.3f;
            _camera.addPlayer(gameObject);
        }
    }

    public bool verifyDown()
    {
        if (!isDead && life <= 0)
        {
            isDown = true;
            _camera.removePlayer(gameObject);
        }

        return isDown;
    }

    public bool verifyDeath()
    {
        if (isDown && downLife < 1)
            isDead = true;

        return isDead;
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
        _playerMovement.setSpeed(speed);
    }

}
