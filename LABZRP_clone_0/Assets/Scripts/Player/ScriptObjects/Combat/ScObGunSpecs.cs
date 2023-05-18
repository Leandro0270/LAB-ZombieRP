using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Gun")]
public class ScObGunSpecs : ScriptableObject
{
    public int dano;
    public float tempoEntreDisparos, tempoRecarga, dispersao;
    public int totalBalas, tamanhoPente, balasPorDisparo;
    public bool segurarGatilho;
    public GameObject modelo3d;
    public GameObject modelo3dVendingMachine;
    public bool isShotgun;
    public bool isSniper;
    public float range;
    public float speedBullet;
    public bool haveKnockback;
    public float knockbackForce;
    [Range(1f, 10f)]
    public int hitableEnemies = 1;
    [Range(1f, 100f)]
    public float reducaoDispersaoMirando = 1f;
    [Range(1f, 100f)]
    public float slowWhileAimingPercent = 1f;
    public bool haveCriticalChance;
    
    public float criticalChanceIncrementalPerBullet;
    public float criticalDamagePercentage;
    [Range(1f, 100f)]
    public float criticalBaseChancePercentage;
    public int Price = 100;

}
