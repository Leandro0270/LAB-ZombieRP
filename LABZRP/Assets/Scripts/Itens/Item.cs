using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Item : MonoBehaviour
{
    private ScObItem ItemScOB;
    private GameObject StartItem;
    public void Start()
    {
        
            StartItem = instanceItem();
        
    }

    public void Update()
    {
        if(StartItem == null)
        {
            instanceItem();
        }
    }

    void OnTriggerEnter(Collider objetoDeColisao)
    {
        WeaponSystem playerAmmo = objetoDeColisao.GetComponent<WeaponSystem>();
        PlayerStats status = objetoDeColisao.GetComponent<PlayerStats>();
        if (ItemScOB.Balas > 0)
        {
            if (ItemScOB.life > 0)
            {
                if (playerAmmo != null && status != null)
                {
                    if (playerAmmo.GetAtualAmmo() < playerAmmo.GetMaxBalas() ||
                        status.GetLife() < status.GetTotalLife())
                    {
                        playerAmmo.ReceiveAmmo(ItemScOB.Balas);
                        status.ReceiveHeal(ItemScOB.life);
                        Destroy(gameObject);
                    }
                }

                {
                }
            }
        }

        if (ItemScOB.Balas == 0)
        {
            if (ItemScOB.life > 0)
            {
                if (status != null)
                {
                    if (status.GetLife() < status.GetTotalLife())
                    {
                        status.ReceiveHeal(ItemScOB.life);
                        Destroy(gameObject);
                    }
                }
            }
        }

        if (ItemScOB.life == 0)
        {
            if (ItemScOB.Balas > 0)
            {
                if (playerAmmo != null)
                {
                    if (playerAmmo.GetAtualAmmo() < playerAmmo.GetMaxBalas())
                    {
                        playerAmmo.ReceiveAmmo(ItemScOB.Balas);
                        Destroy(gameObject);
                    }
                }
            }
        }

    }

    public void setItem(ScObItem item)
    {
        ItemScOB = item;
    }

    private GameObject instanceItem()
    {
        StartItem = Instantiate(ItemScOB.modelo3d, transform.position, transform.rotation);
        StartItem.transform.parent = transform;
        return StartItem;
    }

}
