using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class PlayerStats : MonoBehaviour
{
    private ScObPlayerStats playerStatus;
    private bool isDown;
    private bool isDead ;
    public float totalLife;
    public float life;
    private float downLife = 100f;
    private float speed;
    private CâmeraMovement _camera;

    private void Start()
    {
        _camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CâmeraMovement>();
        speed = playerStatus.speed;
        totalLife = playerStatus.health;
        life = totalLife;
    }

    // Update is called once per frame
    void Update()
    {
        if (verifyDown()){
            if(downLife > 1)
                downLife =- Time.deltaTime;
            else
            {
                isDead = true;
            }
        }
        
    }
    
    
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
            life -= damage;
    }



    public void revived()
    {
        if (isDown && !isDead)
        {
            isDown = false;
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

}
