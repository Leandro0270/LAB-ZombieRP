using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "EnemyAreaEffect")]

public class ScObEnemyAreaEffect : ScriptableObject
{
    public bool EffectIsPermanent;
    public float areaEffectTime;
    public float radiusAreaEffect; 
    public bool addEnemyDamage;
    public float EnemyDamage;
    public bool addEnemySpeed;
    public float EnemySpeed;
    public bool addEnemyHealth;
    public float EnemyHealth;
    public bool isDamagePlayerSplash;
    public bool isDamagePlayerOverTime;
    public bool isPlayerSpeedSlower;
    public float PlayerSpeedSlower;
    public GameObject ParticlesAreaEffect;
    public float PlayerSplashDamage;
    public float PlayerOverTimeDamage;
    public float TimeEffect;
    
}
