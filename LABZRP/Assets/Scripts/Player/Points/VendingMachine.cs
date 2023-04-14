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
    [SerializeField] private float buyCooldown = 5f;
    
    private int randomItemIndex;
    
    private bool isOnCooldown = false;
    private bool isOnHordeCooldown = false;
    private int itemType;
    
    
    //In game objects
    [SerializeField]
    private GameObject ItemShowHolder;
    public GameObject itemHolder;
    public GameObject itemShow;

    private GameObject StartItem;
    public TextMeshPro ScreenPoints;

    

    private void Awake()
    {

        setRandomItem();
    }




    private void OnTriggerStay(Collider other)
    {
        PlayerStats playerStats = other.GetComponent<PlayerStats>();
        if (playerStats)
        {
            if (playerStats.getInteracting())
            {
                if (playerStats.getPlayerPoints().getPoints() >= itens[randomItemIndex].Price && !isOnCooldown)
                {
                    isOnCooldown = true;
                    itemSpawn(other.gameObject);
                    playerStats.getPlayerPoints().removePoints(itens[randomItemIndex].Price);
                    Destroy(StartItem);
                    Destroy(ItemShowHolder);
                    StartCoroutine(VendingMachineItemCoolDown());
                    ScreenPoints.text = " ";

                }
            }
        }

    }

    private void itemSpawn(GameObject playerBuyer)
    {
        if (isOnHordeCooldown)
        {
            if (itemType == 0)
            {
                GameObject item = Instantiate(itemHolder, gameObject.transform.position, transform.rotation);
                item.GetComponent<Item>().setItem(itens[randomItemIndex]);

            }
            else if (itemType == 1)
            {
                playerBuyer.GetComponent<WeaponSystem>().ChangeGun(guns[randomItemIndex]);
            }
        }
        else
        {
            return;
        }
    }

    private void setRandomItem()
    {
        //Randomizar se vai ser arma ou item
        var rotation = transform.rotation;
        var position = transform.position;
        itemType = Random.Range(0, 2);
        if (itemType == 0)
        {
            randomItemIndex = Random.Range(0, itens.Length);
            ScreenPoints.text = itens[randomItemIndex].Price.ToString();
            StartItem = Instantiate(itens[randomItemIndex].modelo3d, ItemShowHolder.transform.position,
                rotation);
            ScreenPoints.text = itens[randomItemIndex].Price.ToString();

        }
        else if(itemType == 1)
        {
            randomItemIndex = Random.Range(0, guns.Length);
            ScreenPoints.text = guns[randomItemIndex].Price.ToString();
            StartItem = Instantiate(guns[randomItemIndex].modelo3dVendingMachine, ItemShowHolder.transform.position,
                rotation);
            ScreenPoints.text = guns[randomItemIndex].Price.ToString();

        }
        
        
        ItemShowHolder = Instantiate(itemShow,
            new Vector3(position.x, (position.y + 7), position.z - 1),
            rotation);
        StartItem.transform.parent = ItemShowHolder.transform;
    }
    
    IEnumerator VendingMachineItemCoolDown()
    {
        yield return new WaitForSeconds(buyCooldown);
        isOnCooldown = false;
        setRandomItem();
        
    }

}
