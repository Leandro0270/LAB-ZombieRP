using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class Item : MonoBehaviourPunCallbacks
{
    [SerializeField] private bool isOnline = false;
    [SerializeField] private PhotonView photonView;
    public bool isThrowable;
    public ScObItem ItemScOB;
    private GameObject StartItem;
    public void Start()
    {
        
            StartItem = instanceItem();
        
    }

    public void Update()
    {
        if (!StartItem)
            StartItem = instanceItem();
    }

    void OnTriggerEnter(Collider objetoDeColisao)
    {
        PlayerStats status = objetoDeColisao.GetComponent<PlayerStats>();
        if (status)
        {
            if (ItemScOB.throwable == null)
            {
                WeaponSystem playerAmmo = objetoDeColisao.GetComponent<WeaponSystem>();
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
                                if (isOnline)
                                {
                                    photonView.RPC("MasterClientDestroyItemHolder", RpcTarget.MasterClient);
                                }
                                else
                                {
                                    GameObject.Find("GameManager").GetComponent<MainGameManager>().removeItem(gameObject);
                                    Destroy(gameObject);
                                }
                            }
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
                                if (isOnline)
                                {
                                    photonView.RPC("MasterClientDestroyItemHolder", RpcTarget.MasterClient);
                                }
                                else
                                {
                                    GameObject.Find("GameManager").GetComponent<MainGameManager>().removeItem(gameObject);
                                    Destroy(gameObject);
                                }
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
                                if (isOnline)
                                {
                                    photonView.RPC("MasterClientDestroyItemHolder", RpcTarget.MasterClient);
                                }
                                else
                                {
                                    GameObject.Find("GameManager").GetComponent<MainGameManager>().removeItem(gameObject);
                                    Destroy(gameObject);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (status.addItemThrowable(ItemScOB.throwable))
                {
                    if (isOnline)
                    {
                        photonView.RPC("MasterClientDestroyItemHolder", RpcTarget.MasterClient);
                    }
                    else
                    {
                        GameObject.Find("GameManager").GetComponent<MainGameManager>().removeItem(gameObject);

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
        if(ItemScOB.throwable == null){
            StartItem = Instantiate(ItemScOB.modelo3d, transform.position, transform.rotation);
            StartItem.transform.parent = transform;
            return StartItem;
        }
        else{
            StartItem = Instantiate(ItemScOB.modelo3d, transform.position, transform.rotation);
            StartItem.transform.parent = transform;
            return StartItem;
        }
    }


    [PunRPC]
    public void MasterClientDestroyItemHolder()
    {
        GameObject.Find("GameManager").GetComponent<MainGameManager>().removeItem(gameObject);
        PhotonNetwork.Destroy(this.gameObject);
    }
    
}
