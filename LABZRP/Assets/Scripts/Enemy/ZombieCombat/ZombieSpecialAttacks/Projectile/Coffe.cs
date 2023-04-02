using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coffe : MonoBehaviour
{


    [SerializeField] private GameObject AreaEffect;


    private void OnTriggerEnter(Collider other)
    {
        //se o objeto que colidiu com o objeto que tem esse script estiver na layer de ground vai instanciar AreaEffect
        if (other.gameObject.layer == 3)
        {
            Instantiate(AreaEffect, new Vector3(transform.position.x, transform.position.y + 2, transform.position.z), transform.rotation);
            Destroy(gameObject);
        }
    }
}
