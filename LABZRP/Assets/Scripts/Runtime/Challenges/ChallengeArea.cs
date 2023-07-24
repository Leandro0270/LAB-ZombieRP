using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ChallengeArea : MonoBehaviourPunCallbacks
{
    
    [SerializeField] private bool isOnline = false;
    private void OnTriggerEnter(Collider other)
    {
        WeaponSystem weaponSystem = other.GetComponent<WeaponSystem>();
        if (weaponSystem)
        {
            
            if(isOnline)
                photonView.RPC("setIsInArea", RpcTarget.All, true, other.gameObject.GetPhotonView().ViewID);
            else
                weaponSystem.SetIsInArea(true);
            
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        WeaponSystem weaponSystem = other.GetComponent<WeaponSystem>();
        if (weaponSystem)
        {
            if(isOnline)
                photonView.RPC("setIsInArea", RpcTarget.All, false, other.gameObject.GetPhotonView().ViewID);
            else
                weaponSystem.SetIsInArea(false);
        }
    }
    
    
    [PunRPC]
    public void setIsInArea(bool isInArea, int photonId)
    {
        GameObject obj = PhotonView.Find(photonId).gameObject;
        WeaponSystem weaponSystem = obj.GetComponent<WeaponSystem>();
        if (weaponSystem)
        {
            weaponSystem.SetIsInArea(isInArea);
        }
    }
}
