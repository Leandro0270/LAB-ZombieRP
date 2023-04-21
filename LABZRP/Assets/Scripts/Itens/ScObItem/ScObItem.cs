using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Item")]
public class ScObItem : ScriptableObject
{
    public string nome;
    public float life;
    public int Balas;
    public GameObject modelo3d;
    public ScObThrowableSpecs throwable;
    public int Price;
}
