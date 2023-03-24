using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAreaEffect : MonoBehaviour
{
    
    private bool setup = false;
    private BoxCollider boxCollider;
    public ScObEnemyAreaEffect enemyAreaEffect;
    private float radiusAreaEffect = 5f;
    private bool EffectIsPermanent = false;
    private bool addEnemyDamage = false;
    public float EnemyDamage;
    private bool addEnemySpeed = false;
    public float EnemySpeed;
    private bool addEnemyHealth = false;
    public float EnemyHealth;
    private bool isDamagePlayerSplash = false;
    private float DamageSplashPlayer;
    private bool isDamagePlayerOverTime = false;
    private float DamagheOverTimePlayer;
    private bool isPlayerSpeedSlower = false;
    private float PlayerSpeedSlower;
    private float TimeEffect = 0f;
    private float AreaEffectTime = 0f;
    private GameObject ParticlesAreaEffect;

    private void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        boxCollider.size = new Vector3(radiusAreaEffect, 0, radiusAreaEffect);
    }
    private void Update()
    {
        if (enemyAreaEffect && !setup)
        {
            radiusAreaEffect = enemyAreaEffect.radiusAreaEffect;
            addEnemyDamage = enemyAreaEffect.addEnemyDamage;
            addEnemySpeed = enemyAreaEffect.addEnemySpeed;
            addEnemyHealth = enemyAreaEffect.addEnemyHealth;
            isDamagePlayerSplash = enemyAreaEffect.isDamagePlayerSplash;
            isDamagePlayerOverTime = enemyAreaEffect.isDamagePlayerOverTime;
            TimeEffect = enemyAreaEffect.TimeEffect;
            DamageSplashPlayer = enemyAreaEffect.PlayerSplashDamage;
            DamagheOverTimePlayer = enemyAreaEffect.PlayerOverTimeDamage;
            EffectIsPermanent = enemyAreaEffect.EffectIsPermanent;
            EnemyDamage = enemyAreaEffect.EnemyDamage;
            EnemySpeed = enemyAreaEffect.EnemySpeed;
            EnemyHealth = enemyAreaEffect.EnemyHealth;
            AreaEffectTime = enemyAreaEffect.areaEffectTime;
            ParticlesAreaEffect = enemyAreaEffect.ParticlesAreaEffect;
            isPlayerSpeedSlower = enemyAreaEffect.isPlayerSpeedSlower;
            PlayerSpeedSlower = enemyAreaEffect.PlayerSpeedSlower;
            Destroy(Instantiate(ParticlesAreaEffect, transform.position, transform.rotation), AreaEffectTime);
            setup = true;
        }
        
        if(setup)
        {
            AreaEffectTime -= Time.deltaTime;
            if(AreaEffectTime<=0)
            {
                Destroy(gameObject);
            }
        }
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (setup)
        {
            EnemyStatus Estats = other.GetComponent<EnemyStatus>();
            PlayerStats Pstats = other.GetComponent<PlayerStats>();
            if (Pstats && !Pstats.verifyDown())
            {
                if (isDamagePlayerSplash)
                    Pstats.takeDamage(DamageSplashPlayer);
                
                if(isPlayerSpeedSlower)
                    Pstats.ReceiveTemporarySlow(TimeEffect, PlayerSpeedSlower);

            }

            if (Estats)
            {
                if (EffectIsPermanent)
                {
                    if (addEnemySpeed)
                        Estats.PermanentSpeed(EnemySpeed);
                    if (addEnemyDamage)
                        Estats.PermanentDamage(EnemyDamage);
                }
                else
                {
                    if(addEnemySpeed)
                        Estats.ReceiveTemporarySpeed(TimeEffect, EnemySpeed);
                    if(addEnemyDamage)
                        Estats.ReceiveTemporaryDamage(TimeEffect, EnemyDamage);
                }
                
                if(addEnemyHealth)
                    Estats.receiveLife(EnemyHealth);
            }

        }
    }
    

    private void OnTriggerStay(Collider other)
    {
        if (setup)
        {
            PlayerStats Pstats = other.GetComponent<PlayerStats>();
            if (Pstats && !Pstats.verifyDown())
            {
                if (isDamagePlayerOverTime)
                    Pstats.takeDamage(DamagheOverTimePlayer * Time.deltaTime);
                
                
            }
        }
    }


    private void setEnemyAreaEffect(ScObEnemyAreaEffect SCOBenemyAreaEffect)
    {
        enemyAreaEffect = SCOBenemyAreaEffect;
    }

}
