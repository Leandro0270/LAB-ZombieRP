using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class VendingMachine : MonoBehaviourPunCallbacks
{
    //Define Selling itens
    [SerializeField] private ScObItem[] itens;
    [SerializeField] private ScObGunSpecs[] guns;


    //Settings
    [SerializeField] private GameObject errorText;
    [SerializeField] private GameObject errorPlane;
    [SerializeField] private int timeShowingError = 3;
    [SerializeField] private float buyCooldown = 5f;
    
    private int randomItemIndex;
    
    private bool isOnCooldown = false;
    [SerializeField] private bool canBuyOnlyInHordeCooldown = false;
    private bool isOnHordeCooldown = false;
    private int itemType;
    private bool isDisplayingError = false;

    
    
    //In game objects
    [SerializeField] private GameObject ItemSpawnPoint;
    [SerializeField]
    private GameObject ItemShowHolder;
    public GameObject itemHolder;
    public GameObject itemShow;
    private GameObject StartItem;
    public TextMeshPro ScreenPoints;
    
    [SerializeField] private bool isOnline = false;
    [SerializeField] private bool isMasterClient = false;
    [SerializeField] private PhotonView photonView;
    //debug
    [SerializeField] private bool changeItem = false;

    
    private void Update()
    {
        if(isOnline && !isMasterClient) return;
        if (changeItem)
        {
            Destroy(StartItem);
            setRandomItem();
            changeItem = false;
        }
    }

    private void Awake()
    {

        setRandomItem();
    }
    
    public void setIsMasterClient(bool isMasterClient)
    {
        this.isMasterClient = isMasterClient;
    }

    private void OnTriggerStay(Collider other)
    {
        if(isOnline && !isMasterClient) return;
        
        if(isOnCooldown) return;
        PlayerStats playerStats = other.GetComponent<PlayerStats>();
        if (playerStats == null || !playerStats.getInteracting()) return;
        if (canBuyOnlyInHordeCooldown && !isOnHordeCooldown)
        {
            if (isOnline)
            {
                photonView.RPC("ShowError", RpcTarget.All, "Termine a horda para comprar!"); }
            else
            {
                ShowError("Termine a horda para comprar!");
            }
            return;
        }
        int pontosPlayerAtual = playerStats.getPlayerPoints().getPoints();

        if (itemType <= (itens.Length - 1))
        {
            if (pontosPlayerAtual < itens[randomItemIndex].Price)
            {
                if (isOnline)
                {
                    photonView.RPC("ShowError", RpcTarget.All, "Você não tem pontos suficientes!"); }
                else
                {
                    ShowError("Você não tem pontos suficientes!");
                }
                return;
            }
            HandleItemTypeZero(other.gameObject, playerStats);
        }
        else if (itemType <= (guns.Length + itens.Length - 1))
        {
            if (pontosPlayerAtual < guns[randomItemIndex].Price)
            {
                if (isOnline)
                {
                    photonView.RPC("ShowError", RpcTarget.All, "Você não tem pontos suficientes!");
                }
                else
                {
                    ShowError("Você não tem pontos suficientes!");
                }
                return;
            }
            HandleItemTypeOne(other.gameObject, playerStats);
        }
    }

    private void HandleItemTypeZero(GameObject player, PlayerStats playerStats)
    {
        isOnCooldown = true;
        itemSpawn(player);
        playerStats.getPlayerPoints().removePoints(itens[randomItemIndex].Price);
        Destroy(StartItem);
        Destroy(ItemShowHolder);
        StartCoroutine(VendingMachineItemCoolDown());
        ScreenPoints.text = " ";
    }

    private void HandleItemTypeOne(GameObject player, PlayerStats playerStats)
    {
        isOnCooldown = true;
            itemSpawn(player);
            playerStats.getPlayerPoints().removePoints(guns[randomItemIndex].Price);
            Destroy(StartItem);
            Destroy(ItemShowHolder);
            StartCoroutine(VendingMachineItemCoolDown());
            ScreenPoints.text = " ";
        
    }

    
    [PunRPC]
    private void ShowError(string message)
    {
        if (isDisplayingError) return;
        isDisplayingError = true;
        errorPlane.SetActive(true);
        errorText.SetActive(true);
        errorText.GetComponent<TextMeshPro>().text = message;
        ScreenPoints.text = "!";
        StartCoroutine(restartError());
    }
    
    

    private void itemSpawn(GameObject playerBuyer)
    {

            if (itemType <= (itens.Length - 1))
            {
                GameObject item;
                if (isOnline)
                {
                    item = PhotonNetwork.Instantiate("itemHolder", ItemSpawnPoint.transform.position,
                        ItemSpawnPoint.transform.rotation);
                    int itemHolderPhotonViewID = item.GetComponent<PhotonView>().ViewID;
                    photonView.RPC("setItemSpecsOnline", RpcTarget.All, randomItemIndex, itemHolderPhotonViewID);
                }
                else
                {
                    item = Instantiate(itemHolder, ItemSpawnPoint.transform.position,
                        ItemSpawnPoint.transform.rotation);
                    item.GetComponent<Item>().setItem(itens[randomItemIndex]);
                }

            }
            else if (itemType <= (guns.Length + itens.Length - 1))
            {
                if (isOnline)
                {
                    int playerPhotonViewID = playerBuyer.GetComponent<PhotonView>().ViewID;
                    photonView.RPC("changePlayerBuyerGunOnline", RpcTarget.All, randomItemIndex, playerPhotonViewID);
                }
                else
                    playerBuyer.GetComponent<WeaponSystem>().ChangeGun(guns[randomItemIndex]);
            }
    }
    [PunRPC]
    public void changePlayerBuyerGunOnline(int gunIndex, int playerViewId)
    {
        GameObject playerBuyer = PhotonView.Find(playerViewId).gameObject;
        playerBuyer.GetComponent<WeaponSystem>().ChangeGun(guns[gunIndex]);
    }
    
    [PunRPC]
    public void setItemSpecsOnline(int itemIndex, int itemHolderViewId)
    {
        GameObject itemHolder = PhotonView.Find(itemHolderViewId).gameObject;
        itemHolder.GetComponent<Item>().setItem(itens[itemIndex]);
    }

    private void setRandomItem()
    {
        
        if(isOnline && !isMasterClient) return;
        //Randomizar se vai ser arma ou item
        var rotation = transform.rotation;
        itemType = Random.Range(0, guns.Length + itens.Length);
        if (itemType <= (itens.Length - 1))
        {
            randomItemIndex = Random.Range(0, itens.Length);
            
            if (isOnline)
            {
                photonView.RPC("setStartItemRPC", RpcTarget.All, randomItemIndex, false);
            }
            else
            {
                ScreenPoints.text = itens[randomItemIndex].Price.ToString();
                StartItem = Instantiate(itens[randomItemIndex].modelo3dVendingMachine,
                    ItemShowHolder.transform.position,
                    rotation);
                StartItem.transform.parent = transform;
            }

        }
        else if(itemType <= (guns.Length + itens.Length - 1))
        {
            randomItemIndex = Random.Range(0, guns.Length);

            if (isOnline)
            {
                photonView.RPC("setStartItemRPC", RpcTarget.All, randomItemIndex, true);

            }
            else
            {
                randomItemIndex = Random.Range(0, guns.Length);
                ScreenPoints.text = guns[randomItemIndex].Price.ToString();
                StartItem = Instantiate(guns[randomItemIndex].modelo3dVendingMachine, ItemShowHolder.transform.position,
                    rotation);
                StartItem.transform.parent = ItemShowHolder.transform;
            }
        } 
    }
    
    [PunRPC]
    public void setStartItemRPC(int itemIndex, bool isGun)
    {
        var rotation = transform.rotation;
        if (!isGun)
        {
            StartItem = Instantiate(itens[itemIndex].modelo3dVendingMachine, ItemShowHolder.transform.position,
                rotation);
            StartItem.transform.parent = transform;
            ScreenPoints.text = itens[itemIndex].Price.ToString();
        }
        else
        {
            ScreenPoints.text = guns[itemIndex].Price.ToString();
            StartItem = Instantiate(guns[itemIndex].modelo3dVendingMachine, ItemShowHolder.transform.position,
                rotation);
            StartItem.transform.parent = ItemShowHolder.transform;
            ScreenPoints.text = guns[itemIndex].Price.ToString();
        }
    }
    
    IEnumerator VendingMachineItemCoolDown()
    {
        yield return new WaitForSeconds(buyCooldown);
        isOnCooldown = false;
        setRandomItem();
        
    }
    
    
    public void setIsOnHorderCooldown(bool isOnHordeCooldown)
    {
        this.isOnHordeCooldown = isOnHordeCooldown;
    }

    IEnumerator restartError()
    {
        yield return new WaitForSeconds(timeShowingError);
        isDisplayingError = false;
        if (itemType <= (itens.Length - 1))
        {
            ScreenPoints.text = itens[randomItemIndex].Price.ToString();
        }
        else if (itemType <= (guns.Length + itens.Length - 1))
        {
            ScreenPoints.text = guns[randomItemIndex].Price.ToString();
        }
        errorPlane.SetActive(false);
        errorText.SetActive(false);
    }

}
