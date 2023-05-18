using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player")]
public class ScObPlayerStats : ScriptableObject
{
    public String name;
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
