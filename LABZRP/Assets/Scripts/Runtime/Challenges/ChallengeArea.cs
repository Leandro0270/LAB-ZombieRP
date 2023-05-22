using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        WeaponSystem weaponSystem = other.GetComponent<WeaponSystem>();
        if (weaponSystem)
        {
            weaponSystem.SetIsInArea(true);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        WeaponSystem weaponSystem = other.GetComponent<WeaponSystem>();
        if (weaponSystem)
        {
            weaponSystem.SetIsInArea(false);
        }
    }
}
