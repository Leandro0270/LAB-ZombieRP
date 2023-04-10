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
    public bool isShotgun;
    public bool isSniper;
    public float range;
    public float speedBullet;
    public int hitableEnemies;
    [Range(1f, 100f)]
    public float reducaoDispersaoMirando;
    [Range(1f, 100f)]
    public float slowWhileAimingPercent;
}
