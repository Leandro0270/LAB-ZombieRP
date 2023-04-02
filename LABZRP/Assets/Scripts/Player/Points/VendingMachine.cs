using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class VendingMachine : MonoBehaviour
{

    public TextMeshPro ScreenPoints;
    public GameObject itemShow;
    private BoxCollider buyArea;
    [SerializeField] private ScObItem[] itens;
    public GameObject itemHolder;
    private int randomItemIndex;
    
    [SerializeField] private float buyCooldown = 5f;
    private bool isOnCooldown = false;
    
    
    //In game objects
    private GameObject ItemShowHolder;
    private GameObject StartItem;
    

    private void Awake()
    {

        setRandomItem();
        buyArea = GetComponent<BoxCollider>();

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
                    itemSpawn();
                    playerStats.getPlayerPoints().removePoints(itens[randomItemIndex].Price);
                    Destroy(StartItem);
                    Destroy(ItemShowHolder);
                    StartCoroutine(VendingMachineItemCoolDown());
                    ScreenPoints.text = " ";

                }
            }
        }

    }

    private void itemSpawn()
    {
        var position = transform.position;
        GameObject item = Instantiate(itemHolder, new Vector3(position.x, position.y, position.z + -5), transform.rotation);
        item.GetComponent<Item>().setItem(itens[randomItemIndex]);
    }

    private void setRandomItem()
    {
        randomItemIndex = Random.Range(0, itens.Length);
        var rotation = transform.rotation;
        var position = transform.position;
        ItemShowHolder = Instantiate(itemShow,
            new Vector3(position.x, (position.y + 7), position.z - 1),
            rotation);
        StartItem = Instantiate(itens[randomItemIndex].modelo3d, ItemShowHolder.transform.position,
            rotation);
        StartItem.transform.parent = ItemShowHolder.transform;
        ScreenPoints.text = itens[randomItemIndex].Price.ToString();
    }
    
    IEnumerator VendingMachineItemCoolDown()
    {
        yield return new WaitForSeconds(buyCooldown);
        isOnCooldown = false;
        setRandomItem();
        
    }

}
