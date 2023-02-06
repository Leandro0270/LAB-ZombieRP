using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Gun")]
public class ScObGunSpecs : ScriptableObject
{
    public int dano;
    public float tempoEntreDisparos, tempoRecarga, dispersao;
    public int tamanhoPente, balasPorDisparo;
    public bool segurarGatilho;
    public Mesh modelo3d;
}
