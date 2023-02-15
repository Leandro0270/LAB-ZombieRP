using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MeshFilter))]
public class Ammo : MonoBehaviour
{
    public ScObAmmoItem ammoItem;

    public void Awake()
    {
        if (ammoItem == null)
        {
            throw new Exception("AmmoItem não foi definido");
        }
        else
        {
            GameObject ammoStart = Instantiate(ammoItem.modelo3d, transform.position, transform.rotation);
            ammoStart.transform.parent = transform;
        }
    }

    void OnTriggerEnter(Collider objetoDeColisao)
    {
        WeaponSystem playerAmmo = objetoDeColisao.GetComponent<WeaponSystem>();
        if (playerAmmo != null && playerAmmo.GetAtualAmmo() < playerAmmo.GetMaxBalas())
        {
            playerAmmo.ReceiveAmmo(ammoItem.quantidade);
            Destroy(gameObject);
        }
        
    }
}
