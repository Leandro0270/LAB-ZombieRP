using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class PlayerStats : MonoBehaviour
{
    public ScObPlayerStats playerStatus;
    private bool isDown;
    private bool isDead ;
    private float totalLife;
    private float life;
    private float downLife = 100f;
    private float speed;


    private void Awake()
    {
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

    public float getSpeed()
    {
        return speed;
    }

    public void takeDamage(float damage)
    {
        if (!isDown && !isDead)
            life =- damage;
    }

    public void takeHeal(float heal)
    {
        if (!isDown && !isDead)
            life =+ heal;
        if (life > totalLife)
            life = totalLife;
    }

    public void revived()
    {
        if (isDown && !isDead)
        {
            isDown = false;
            life = totalLife * 0.3f;
        }
    }

    public bool verifyDown()
    {
        if (!isDead && life <= 0)
            isDown = true;
        return isDown;
    }

    public bool verifyDeath()
    {
        if (isDown && downLife < 1)
            isDead = true;

        return isDead;
    }

}
