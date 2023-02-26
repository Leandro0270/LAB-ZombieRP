using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player")]
public class ScObPlayerStats : ScriptableObject
{
    public float health;
    public float speed;
    public float revivalSpeed;
    public float timeBeteweenMelee;
    public float meleeDamage;
    public ScObGunSpecs startGun;
}
