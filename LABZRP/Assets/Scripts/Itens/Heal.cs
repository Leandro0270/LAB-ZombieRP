using System;
using UnityEngine;

public class Heal : MonoBehaviour
{
    public ScObHealItem healItem;
    
    public void Awake()
    {
        if (healItem == null)
        {
            throw new Exception("HealItem não foi definido");
        }
        else
        {
            GameObject healStart = Instantiate(healItem.modelo3d, transform.position, transform.rotation);
            healStart.transform.parent = transform;
        }
    }
    void OnTriggerEnter(Collider objetoDeColisao)
    {
        PlayerStats status = objetoDeColisao.GetComponent<PlayerStats>();
        if (status != null && status.GetLife() < status.GetTotalLife())
        {
            status.ReceiveHeal(healItem.heal);
            Destroy(gameObject);
        }
        
    }
}
