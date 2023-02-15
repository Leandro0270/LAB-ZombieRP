using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gun")]
public class ScObGunSpecs : ScriptableObject
{
    public int dano;
    public float tempoEntreDisparos, tempoRecarga, dispersao;
    public int totalBalas, tamanhoPente, balasPorDisparo;
    public bool segurarGatilho;
    public GameObject modelo3d;
}
