using System.Collections;
using System.Collections.Generic;
using Runtime.Player.ScriptObjects.Combat;
using UnityEngine;
[CreateAssetMenu(menuName = "Item")]
public class ScObItem : ScriptableObject
{
    public string nome;
    public float life;
    public int Balas;
    public GameObject modelo3d;
    public GameObject modelo3dVendingMachine;
    public ScObThrowableSpecs throwable;
    public int Price;
}
