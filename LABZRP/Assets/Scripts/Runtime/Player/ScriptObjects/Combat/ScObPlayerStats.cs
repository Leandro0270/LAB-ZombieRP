using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Player")]
public class ScObPlayerStats : ScriptableObject
{
    public int classIndex;
    [FormerlySerializedAs("name")] public String _nickName;
    public int maxThrowableCapacity;
    public int maxAuxiliaryCapacity;
    public int maxGunCapacity;
    public float health;
    public float speed;
    public float revivalSpeed;
    public float timeBeteweenMelee;
    public float meleeDamage;
    public ScObGunSpecs startGun;
    public Color MainColor;
    public Material PlayerIndicator;
    public float burnDamagePerSecond;
}
