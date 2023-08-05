using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class VendingMachine : MonoBehaviourPunCallbacks
{
    //Define Selling itens
    [SerializeField] private ScObItem[] itens;
    [SerializeField] private ScObGunSpecs[] guns;
    [SerializeField] private ScObItem[] granades;
    private List<ScObGunSpecs> gunsTier1 = new List<ScObGunSpecs>();
    private List<ScObGunSpecs> gunsTier2 = new List<ScObGunSpecs>();
    private List<ScObGunSpecs> gunsTier3 = new List<ScObGunSpecs>();
    private List<ScObGunSpecs> gunsTier4 = new List<ScObGunSpecs>();
    private List<ScObGunSpecs> gunsTier5 = new List<ScObGunSpecs>();


    //Settings
    [FormerlySerializedAs("hordeItemUpgrade")] [SerializeField] private int hordeGunUpgrade = 3;
    [SerializeField] private int firstGranadeAppearHorde = 5;
    [SerializeField] private GameObject errorText;
    [SerializeField] private GameObject errorPlane;
    [SerializeField] private int timeShowingError = 3;
    [SerializeField] private float buyCooldown = 5f;

    private int currentHorde = 0;
    private bool isOnCooldown = false;
    [SerializeField] private bool canBuyOnlyInHordeCooldown = false;
    private bool isOnHordeCooldown = false;
    private int itemType;
    private bool isDisplayingError = false;
    private int _gunIndex = 0;
    private int _granadeIndex = 0;
    private int _itemIndex = 0;
    private int _randomizeType = 0;
    private int price = 0;

    private List<ScObGunSpecs> _AvailableSpawnGunSpecs = new List<ScObGunSpecs>();

    
    
    //In game objects
    [SerializeField] private GameObject ItemSpawnPoint;
    [SerializeField] private GameObject ItemShowHolder;
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
        foreach (ScObGunSpecs selectedGun in guns)
        {
            if (selectedGun.tier == 1)
            {
                gunsTier1.Add(selectedGun);
            }
            if (selectedGun.tier == 2)
            {
                gunsTier2.Add(selectedGun);
            }
            if (selectedGun.tier == 3)
            {
                gunsTier3.Add(selectedGun);
            }
            if (selectedGun.tier == 4)
            {
                gunsTier4.Add(selectedGun);
            }
            if (selectedGun.tier == 5)
            {
                gunsTier5.Add(selectedGun);
            }
        }
        if (PhotonNetwork.IsMasterClient)
        {
            isMasterClient = true;
            setRandomItem();
        } else if (!isOnline)
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

        if(itemType == 0)
        {
            if (pontosPlayerAtual < price)
            {
                if (isOnline)
                    photonView.RPC("ShowError", RpcTarget.All, "Você não tem pontos suficientes!"); 
                else
                    ShowError("Você não tem pontos suficientes!");
                return;
            }
            HandleItemTypeZero(other.gameObject, playerStats);
        }
        else if (itemType == 1)
        {
            if (pontosPlayerAtual < price)
            {
                if (isOnline)
                    photonView.RPC("ShowError", RpcTarget.All, "Você não tem pontos suficientes!");
                else
                    ShowError("Você não tem pontos suficientes!");
                return;
            }
            HandleItemTypeOne(other.gameObject, playerStats);

        }
        else
        {
            if (pontosPlayerAtual < price)
            {
                if(isOnline)
                    photonView.RPC("ShowError", RpcTarget.All, "Você não tem pontos suficientes!");
                else
                    ShowError("Você não tem pontos suficientes!");
                return;
            }
            HandleItemTypeTwo(other.gameObject, playerStats);
            
        }
    }

    [PunRPC]
    public void HandleBuyItemOnlineRPC()
    {
        isOnCooldown = true;
        ScreenPoints.text = " ";
        Destroy(StartItem);
        if (isMasterClient)
        {
            StartCoroutine(VendingMachineItemCoolDown());

        }
    }
 
    
    private void HandleItemTypeZero(GameObject player, PlayerStats playerStats)
    {
        
        isOnCooldown = true;
        itemSpawn(player);
        playerStats.getPlayerPoints().removePoints(itens[_itemIndex].Price);
        if(isOnline)
            photonView.RPC("HandleBuyItemOnlineRPC", RpcTarget.All);
        else
        {
            Destroy(StartItem);
            StartCoroutine(VendingMachineItemCoolDown());
            ScreenPoints.text = " ";
        }
    }
    
    private void HandleItemTypeOne(GameObject player, PlayerStats playerStats)
    {
        isOnCooldown = true;
        itemSpawn(player);
        playerStats.getPlayerPoints().removePoints(_AvailableSpawnGunSpecs[_gunIndex].Price);
        if(isOnline)
            photonView.RPC("HandleBuyItemOnlineRPC", RpcTarget.All);
        else
        {
            Destroy(StartItem);
            StartCoroutine(VendingMachineItemCoolDown());
            ScreenPoints.text = " ";
        }
        
    }
    private void HandleItemTypeTwo(GameObject player, PlayerStats playerStats)
    {
        
        isOnCooldown = true;
        itemSpawn(player);
        playerStats.getPlayerPoints().removePoints(granades[_granadeIndex].Price);
        if(isOnline)
            photonView.RPC("HandleBuyItemOnlineRPC", RpcTarget.All);
        else
        {
            Destroy(StartItem);
            StartCoroutine(VendingMachineItemCoolDown());
            ScreenPoints.text = " ";
        }
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

            if (_randomizeType == 0 || _randomizeType == 2)
            {
                GameObject item;
                if (!isOnline)
                {
                    item = Instantiate(itemHolder, ItemSpawnPoint.transform.position,
                        ItemSpawnPoint.transform.rotation);
                    if (_randomizeType == 0)
                        item.GetComponent<Item>().setItem(itens[_itemIndex]);
                    else
                        item.GetComponent<Item>().setItem(granades[_granadeIndex]);
                    
                }
                else
                {
                    item = PhotonNetwork.Instantiate("itemHolder", ItemSpawnPoint.transform.position,
                        ItemSpawnPoint.transform.rotation);
                    int itemHolderPhotonViewID = item.GetComponent<PhotonView>().ViewID;
                    if (_randomizeType == 0)
                        photonView.RPC("setItemSpecsOnline", RpcTarget.All, _randomizeType, itemHolderPhotonViewID, _itemIndex);
                    else
                        photonView.RPC("setGranadeSpecsOnline", RpcTarget.All, _randomizeType, itemHolderPhotonViewID, _granadeIndex);
                    
                }
            }
            else 
            {
                if (isOnline)
                {
                    int playerPhotonViewID = playerBuyer.GetComponent<PhotonView>().ViewID;
                    photonView.RPC("changePlayerBuyerGunOnline", RpcTarget.All, _gunIndex, playerPhotonViewID);
                }
                else
                    playerBuyer.GetComponent<WeaponSystem>().ChangeGun(_AvailableSpawnGunSpecs[_gunIndex]);
            }
    }
    [PunRPC]
    public void changePlayerBuyerGunOnline(int gunIndex, int playerViewId)
    {
        GameObject playerBuyer = PhotonView.Find(playerViewId).gameObject;
        playerBuyer.GetComponent<WeaponSystem>().ChangeGun(_AvailableSpawnGunSpecs[gunIndex]);
    }
    
    [PunRPC]
    public void setItemSpecsOnline(int itemIndex, int itemHolderViewId)
    {
        _itemIndex = itemIndex;
        GameObject itemHolder = PhotonView.Find(itemHolderViewId).gameObject;
        Item item = itemHolder.GetComponent<Item>();
        item.setIsOnline(true);
        item.setItem(itens[_itemIndex]);
    }

    [PunRPC]
    public void setGranadeSpecsOnline(int granadeIndex, int itemHolderViewId)
    {
        _granadeIndex = granadeIndex;
        GameObject itemholder = PhotonView.Find(itemHolderViewId).gameObject;
        Item item = itemHolder.GetComponent<Item>();
        item.setIsOnline(true);
        item.setItem(granades[_granadeIndex]);
    }

    private void setRandomItem()
    {
        if(isOnline && !isMasterClient) return;
        itemType = -1;
        _itemIndex = -1;
        _gunIndex = -1;
        _granadeIndex = -1;
        price = 0;
        
        var rotation = transform.rotation;

        if (currentHorde >= firstGranadeAppearHorde)
            _randomizeType = Random.Range(0, 3);
        else
            _randomizeType = Random.Range(0, 2);
        
        itemType = _randomizeType;
        
        if (_randomizeType == 0)
        {
            _itemIndex = Random.Range(0, itens.Length);
            if (isOnline)
                photonView.RPC("setStartItemRPC", RpcTarget.All, _itemIndex);
            else
            {
                ScreenPoints.text = itens[_itemIndex].Price.ToString();
                StartItem = Instantiate(itens[_itemIndex].modelo3dVendingMachine,
                    ItemShowHolder.transform.position,
                    rotation);
                StartItem.transform.parent = transform;
                price = itens[_itemIndex].Price;
            }
        }
        else if (_randomizeType == 1)
        {
            //Dependendo da horda vai spawnar armas de diferentes tiers=================================================================================================================================================================
            if (currentHorde == 0 || currentHorde < (hordeGunUpgrade * 1))
            {
                //Adicionar todos os ScObGunSpecs da lista de guns tier 1
                foreach (ScObGunSpecs gun in gunsTier1)
                {
                    _AvailableSpawnGunSpecs.Add(gun);
                }
            } else if (currentHorde == (hordeGunUpgrade * 1) || currentHorde < (hordeGunUpgrade * 2))
            {
                foreach (ScObGunSpecs gun in gunsTier1)
                {
                    _AvailableSpawnGunSpecs.Add(gun);
                }
                foreach (ScObGunSpecs gun in gunsTier2)
                {
                    _AvailableSpawnGunSpecs.Add(gun);
                }
            } else if(currentHorde == (hordeGunUpgrade * 2) || currentHorde < (hordeGunUpgrade * 3))
            {
                foreach (ScObGunSpecs gun in gunsTier2)
                {
                    _AvailableSpawnGunSpecs.Add(gun);
                }
                foreach (ScObGunSpecs gun in gunsTier3)
                {
                    _AvailableSpawnGunSpecs.Add(gun);
                }
            } else if (currentHorde == (hordeGunUpgrade * 3) || currentHorde < (hordeGunUpgrade * 4))
            {
                foreach (ScObGunSpecs gun in gunsTier3)
                {
                    _AvailableSpawnGunSpecs.Add(gun);
                }
                foreach (ScObGunSpecs gun in gunsTier4)
                {
                    _AvailableSpawnGunSpecs.Add(gun);
                }
            } else if (currentHorde == (hordeGunUpgrade * 4) || currentHorde < (hordeGunUpgrade * 5))
            {
                foreach (ScObGunSpecs gun in gunsTier4)
                {
                    _AvailableSpawnGunSpecs.Add(gun);
                }
                foreach (ScObGunSpecs gun in gunsTier5)
                {
                    _AvailableSpawnGunSpecs.Add(gun);
                }
            }
            else
            {
                foreach (ScObGunSpecs gun in gunsTier5)
                {
                    _AvailableSpawnGunSpecs.Add(gun);
                }
            }
            //==================================================================================================================================================================
            int randomizeGun = Random.Range(0, _AvailableSpawnGunSpecs.Count-1);
            _gunIndex = randomizeGun;
            if (isOnline)
            { 
                int[] availableSpawnGunSpecsIds = new int[_AvailableSpawnGunSpecs.Count];
                foreach (ScObGunSpecs availableGun in _AvailableSpawnGunSpecs)
                {
                    availableSpawnGunSpecsIds.Append(availableGun.id);
                }
                photonView.RPC("setStartGunRPC", RpcTarget.All, _AvailableSpawnGunSpecs[randomizeGun], availableSpawnGunSpecsIds);
            }
            else
            {
                ScreenPoints.text = _AvailableSpawnGunSpecs[_gunIndex].Price.ToString();
                StartItem = Instantiate(_AvailableSpawnGunSpecs[randomizeGun].modelo3dVendingMachine,
                    ItemShowHolder.transform.position,
                    rotation);
                StartItem.transform.parent = transform;
                price = _AvailableSpawnGunSpecs[_gunIndex].Price;
            }
        }
        else
        {
            _granadeIndex = Random.Range(0, granades.Length-1);
            if (isOnline)
            {
                photonView.RPC("setStartGranadeRPC", RpcTarget.All, _granadeIndex);
            }
            else
            {
                ScreenPoints.text = granades[_granadeIndex].Price.ToString();
                StartItem = Instantiate(granades[_granadeIndex].modelo3dVendingMachine,
                    ItemShowHolder.transform.position,
                    rotation);
                StartItem.transform.parent = transform;
                price = granades[_granadeIndex].Price;
            }
        }
    }
    
    [PunRPC]
    public void setStartGranadeRPC(int granadeIndex)
    {
        //Passa o array de avaibleSpawnGunSpecs para a lista de avaibleSpawnGunSpecs
        _randomizeType = 2;
        var rotation = transform.rotation;
        StartItem = Instantiate(granades[granadeIndex].modelo3dVendingMachine, ItemShowHolder.transform.position,
            rotation);
        StartItem.transform.parent = ItemShowHolder.transform;
        price = granades[granadeIndex].Price;
        ScreenPoints.text = price.ToString();
    }
    
    [PunRPC]
    public void setStartGunRPC(int gunIndex, int[] availableSpawnGunId)
    {
        _AvailableSpawnGunSpecs.Clear();
        int i = 0;
        foreach (ScObGunSpecs gun in guns)
        {
            foreach (int id in availableSpawnGunId)
            {
                if (gun.id == id)
                {
                    _AvailableSpawnGunSpecs.Add(gun);
                    if(id == gunIndex)
                        _gunIndex = i;
                    else
                        i++;
                    
                }
            }
        }
        _randomizeType = 1;
        var rotation = transform.rotation;
        StartItem = Instantiate(_AvailableSpawnGunSpecs[gunIndex].modelo3dVendingMachine, ItemShowHolder.transform.position,
            rotation);
        StartItem.transform.parent = ItemShowHolder.transform;
        price = _AvailableSpawnGunSpecs[gunIndex].Price;
        ScreenPoints.text = price.ToString();
    }
    
    [PunRPC]
    public void setStartItemRPC(int itemIndex)
    {
        _randomizeType = 0;
        var rotation = transform.rotation;
        StartItem = Instantiate(granades[itemIndex].modelo3dVendingMachine, ItemShowHolder.transform.position,
            rotation);
        StartItem.transform.parent = ItemShowHolder.transform;
        price = granades[itemIndex].Price;
        ScreenPoints.text = price.ToString();
    }
    
    IEnumerator VendingMachineItemCoolDown()
    {
        yield return new WaitForSeconds(buyCooldown);
        isOnCooldown = false;
        setRandomItem();
        
    }
    
    
    public void setIsOnHorderCooldown(bool isOnHordeCooldown, int currentHorde)
    {
        if (isOnline)
        {
            photonView.RPC("setInOnHordeCooldownOnline", RpcTarget.All, isOnHordeCooldown, currentHorde);
        }
        else
        {
            this.isOnHordeCooldown = isOnHordeCooldown;
            this.currentHorde = currentHorde;
        }
    }
    
    
    [PunRPC]
    public void setInOnHordeCooldownOnline(bool isOnCooldown, int currentHorde)
    {
        this.isOnHordeCooldown = isOnCooldown;
        this.currentHorde = currentHorde;
    }

    IEnumerator restartError()
    {
        yield return new WaitForSeconds(timeShowingError);
        isDisplayingError = false;
        if(_randomizeType == 0){
            ScreenPoints.text = itens[_itemIndex].Price.ToString();
        }
        else if (_randomizeType ==1)
        {
            ScreenPoints.text = _AvailableSpawnGunSpecs[_gunIndex].Price.ToString();
        }
        else if (_randomizeType == 2)
        {
            ScreenPoints.text = granades[_granadeIndex].Price.ToString();
        }
       
        errorPlane.SetActive(false);
        errorText.SetActive(false);
    }

}
