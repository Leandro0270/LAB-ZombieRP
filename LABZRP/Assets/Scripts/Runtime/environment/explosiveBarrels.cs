using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class explosiveBarrels : MonoBehaviourPunCallbacks
{
    [SerializeField] private string explosionEffectName = "Explosion";
    
    [SerializeField] private GameObject FireEffect;
    [SerializeField] private GameObject ExplosionEffect;
    [SerializeField] private float barrelLife =  100;
    [SerializeField] private float fireDamageTick = 2;
    [SerializeField] private float fireDamage = 8;
    [Range(0,1)]
    [SerializeField] private float startBurnPercentage = 0.7f;
    private float currentBarrelLife;
    private float currentFireDamageTickTime = 0;
    private bool isOnline = false;
    private bool isBurning = false;
    // Start is called before the first frame update
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            isOnline = true;
        }
        currentBarrelLife = barrelLife;
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient || !isOnline)
        {
            if (isBurning)
            {
                if (currentFireDamageTickTime >= fireDamageTick)
                {
                    currentFireDamageTickTime = 0;
                    takeDamage(fireDamage);
                }
                else
                    currentFireDamageTickTime += Time.deltaTime;
                
            }
        }
    }
    [PunRPC]
    public void explode()
    {
        if (isOnline)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Instantiate(explosionEffectName, transform.position, Quaternion.identity);
                PhotonNetwork.Destroy(gameObject);
            }
        }
        else
        {
            Instantiate(ExplosionEffect,transform.position,Quaternion.identity);
            Destroy(gameObject);
        }
    }


    [PunRPC]
    public void takeDamage(float damage)
    {
        if (isOnline && !PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("TakeDamage", RpcTarget.MasterClient, damage);
            return;
        }

        currentBarrelLife -= damage;
        
        
        if(currentBarrelLife <= barrelLife*startBurnPercentage && !isBurning)
        {
            visualFireEffect();
        }

        if (currentBarrelLife <= 0)
        {
            explode();
        }
    }

    [PunRPC]
    public void visualFireEffect()
    {
        if (isOnline && PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("visualFireEffect", RpcTarget.Others);
            
        }
        isBurning = true;
        FireEffect.SetActive(true);
    }
}
