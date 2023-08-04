using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Gun")]
[System.Serializable]

public class ScObGunSpecs : ScriptableObject
{
    [Header("Identificação")]
    public int id;
    [Range(1, 5), Tooltip("Nível da arma")]
    public int tier = 1;

    [Header("Status Básicos")]
    public int dano;
    [Tooltip("Balas disparadas por minuto")]
    public float BalasPorMinuto;
    [Tooltip("Tempo de recarga")]
    public float tempoRecarga;
    [Tooltip("Dispersão da arma")]
    public float dispersao;

    [Header("Disparos")]
    public float tempoEntreDisparos;
    public int totalBalas;
    [Tooltip("Tamanho do carregador")]
    public int tamanhoPente;
    public int balasPorDisparo;
    [Tooltip("A arma continua atirando enquanto o gatilho está pressionado?")]
    public bool segurarGatilho;

    [Header("Modelos")]
    public GameObject modelo3d;
    public GameObject modelo3dVendingMachine;

    [Header("Tipo de Arma")]
    public bool isShotgun;
    public bool isSniper;

    [Header("Balística")]
    public float range;
    public float speedBullet;
    [Tooltip("A arma tem efeito de recuo?")]
    public bool haveKnockback;
    public float knockbackForce;
    [Range(1, 10)]
    public int hitableEnemies = 1;

    [Header("Mira")]
    [Range(1f, 100f)]
    public float reducaoDispersaoMirando = 1f;
    [Range(1f, 100f)]
    public float slowWhileAimingPercent = 1f;

    [Header("Chance de Crítico")]
    public bool haveCriticalChance;
    public float criticalChanceIncrementalPerBullet;
    public float criticalDamagePercentage;
    [Range(1f, 100f)]
    public float criticalBaseChancePercentage;

    [Header("Preço")]
    public int Price = 100;

}
