using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Coffee : MonoBehaviourPunCallbacks
{


    [SerializeField] private GameObject AreaEffect;
    [SerializeField] private string AreaEffectName;
    private bool isOnline = false;


    private void Start()
    {
        if(PhotonNetwork.IsConnected)
            isOnline = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        //se o objeto que colidiu com o objeto que tem esse script estiver na layer de ground vai instanciar AreaEffect
        if (other.gameObject.layer == 3)
        {
            if (!isOnline)
            {
                Instantiate(AreaEffect, new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), transform.rotation);
                Destroy(gameObject);
            }
            else
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    PhotonNetwork.Instantiate(AreaEffectName, new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), transform.rotation);
                    PhotonNetwork.Destroy(gameObject);
                }
            }
            
        }
    }
}
