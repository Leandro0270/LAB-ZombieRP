using System;
using Photon.Pun;
using Runtime.Player.Combat.PlayerStatus;
using UnityEngine;
using UnityEngine.Serialization;

public class Item : MonoBehaviourPunCallbacks
{
    [SerializeField] private bool isOnline = false;
    public ScObItem ItemScOB;
    private GameObject StartItem;

    private void OnTriggerEnter(Collider objetoDeColisao)
    {
        if(StartItem){
            PlayerStats status = objetoDeColisao.GetComponent<PlayerStats>();
            if (status)
            {
                bool isMine = false;
                if(isOnline)
                    isMine = objetoDeColisao.GetComponent<PhotonView>().IsMine;
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
                                    if (!isOnline || isMine)
                                    {
                                        playerAmmo.ReceiveAmmo(ItemScOB.Balas);
                                        status.ReceiveHeal(ItemScOB.life);
                                        if (isOnline)
                                        {
                                            photonView.RPC("MasterClientDestroyItemHolder", RpcTarget.MasterClient);
                                        }
                                        else
                                        {
                                            GameObject.Find("GameManager").GetComponent<MainGameManager>()
                                                .removeItem(gameObject);
                                            Destroy(gameObject);
                                        }
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
                                    if (!isOnline || isMine)
                                    {
                                        status.ReceiveHeal(ItemScOB.life);
                                        if (isOnline)
                                        {
                                            photonView.RPC("MasterClientDestroyItemHolder", RpcTarget.MasterClient);
                                        }
                                        else
                                        {
                                            GameObject.Find("GameManager").GetComponent<MainGameManager>()
                                                .removeItem(gameObject);
                                            Destroy(gameObject);
                                        }
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
                                    if (!isOnline || isMine)
                                    {
                                        playerAmmo.ReceiveAmmo(ItemScOB.Balas);
                                        if (isOnline)
                                        {
                                            photonView.RPC("MasterClientDestroyItemHolder", RpcTarget.MasterClient);
                                        }
                                        else
                                        {
                                            GameObject.Find("GameManager").GetComponent<MainGameManager>()
                                                .removeItem(gameObject);
                                            Destroy(gameObject);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (!isOnline || isMine)
                    {
                        if (status.AddItemThrowable(ItemScOB.throwable))
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
        
        }
    }

    public void setItem(ScObItem item)
    {
        ItemScOB = item;
        instanceItem();
    }

    private void instanceItem()
    {
        if (ItemScOB != null){
            if(ItemScOB.throwable == null){
                StartItem = Instantiate(ItemScOB.modelo3d, transform.position, transform.rotation);
                StartItem.transform.parent = transform;
            }
            else{
                StartItem = Instantiate(ItemScOB.modelo3d, transform.position, transform.rotation);
                StartItem.transform.parent = transform;
            }
        }
    }


    [PunRPC]
    public void MasterClientDestroyItemHolder()
    {
        GameObject.Find("GameManager").GetComponent<MainGameManager>().removeItem(gameObject);
        PhotonNetwork.Destroy(this.gameObject);
    }
    
    public void setIsOnline(bool isOnline)
    {
        this.isOnline = isOnline;
    }
    
}
