using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class VendingMachine : MonoBehaviour
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
    
    //debug
    [SerializeField] private bool changeItem = false;

    
    private void Update()
    {
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
    

    private void OnTriggerStay(Collider other)
    {
        if(isOnCooldown) return;
        PlayerStats playerStats = other.GetComponent<PlayerStats>();
        if (playerStats == null || !playerStats.getInteracting()) return;
        if (canBuyOnlyInHordeCooldown && !isOnHordeCooldown)
        {
            ShowError("Termine a horda para comprar!");
            return;
        }
        int pontosPlayerAtual = playerStats.getPlayerPoints().getPoints();

        if (itemType <= (itens.Length - 1))
        {
            if (pontosPlayerAtual < itens[randomItemIndex].Price)
            {
                ShowError("Você não tem pontos suficientes!");
                return;
            }else
                HandleItemTypeZero(other.gameObject, playerStats);
        }
        else if (itemType <= (guns.Length + itens.Length - 1))
        {
            if (pontosPlayerAtual < guns[randomItemIndex].Price)
            {
                ShowError("Você não tem pontos suficientes!");
                return;
            }else
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
                GameObject item = Instantiate(itemHolder, ItemSpawnPoint.transform.position, ItemSpawnPoint.transform.rotation);
                item.GetComponent<Item>().setItem(itens[randomItemIndex]);

            }
            else if (itemType <= (guns.Length + itens.Length - 1))
            {
                playerBuyer.GetComponent<WeaponSystem>().ChangeGun(guns[randomItemIndex]);
            }
    }
    

    private void setRandomItem()
    {
        //Randomizar se vai ser arma ou item
        var rotation = transform.rotation;
        itemType = Random.Range(0, guns.Length + itens.Length);
        if (itemType <= (itens.Length - 1))
        {
            randomItemIndex = Random.Range(0, itens.Length);
            ScreenPoints.text = itens[randomItemIndex].Price.ToString();
            StartItem = Instantiate(itens[randomItemIndex].modelo3dVendingMachine, ItemShowHolder.transform.position,
                rotation);
            //Coloca o start item como filho do objeto itemshowholder
            StartItem.transform.parent = transform;
            ScreenPoints.text = itens[randomItemIndex].Price.ToString();

        }
        else if(itemType <= (guns.Length + itens.Length - 1))
        {
            randomItemIndex = Random.Range(0, guns.Length);
            ScreenPoints.text = guns[randomItemIndex].Price.ToString();
            StartItem = Instantiate(guns[randomItemIndex].modelo3dVendingMachine, ItemShowHolder.transform.position,
                rotation);
            StartItem.transform.parent = ItemShowHolder.transform;
            ScreenPoints.text = guns[randomItemIndex].Price.ToString();

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
