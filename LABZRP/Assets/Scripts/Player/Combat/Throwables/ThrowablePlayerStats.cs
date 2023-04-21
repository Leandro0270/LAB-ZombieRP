using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ThrowablePlayerStats : MonoBehaviour
{
    [SerializeField] private DecalProjector explosionArea;
    [SerializeField] private GameObject ThrowerHand;
    [SerializeField] private GameObject DecalSpawnPoint;
    private int maxCapacity;
    private List<ScObThrowableSpecs> throwableInventory = new List<ScObThrowableSpecs>();
    private int itemIndex = 0;
    private float maxThrowDistance;
    private bool isAiming;
    public GameObject throwableItemPrefab;
    private GameObject decalObject;
    private bool canChangeItem = true;
    private bool canThrowItem = false;
    private bool cancelThrow = false;
    private float currentThrowDistance = 0f;


    private void Update()
    {
        if (isAiming)
        {
            ControlDecalDistance();
        }
    }

    public bool addThrowable(ScObThrowableSpecs throwable)
    {
        if (throwableInventory.Count < maxCapacity)
        {
            throwableInventory.Add(throwable);
            maxThrowDistance = throwableInventory[itemIndex].maxDistance;
            canThrowItem = true;
            return true;
        }
        else
        {
            return false;
        }
    }

    public void changeToNextItem()
    {
        if (throwableInventory.Count == 0)
            return;

        if (itemIndex < throwableInventory.Count - 1)
        {
            itemIndex++;
        }
        else
        {
            itemIndex = 0;
        }
        maxThrowDistance = throwableInventory[itemIndex].maxDistance;
    }

    private Vector3 CalculaTrajetoriaParabolica(Vector3 origem, Vector3 destino, float alturaMaxima)
    {
        Vector3 direcao = destino - origem;
        float distHorizontal = Mathf.Sqrt(direcao.x * direcao.x + direcao.z * direcao.z);
        float distVertical = destino.y - origem.y;

        float alturaAdicional = Mathf.Clamp(alturaMaxima, 0f, alturaMaxima - distVertical);

        float t = Mathf.Sqrt(-2 * alturaAdicional / Physics.gravity.y);
        float velocidadeVertical = -Physics.gravity.y * t;

        t += Mathf.Sqrt(2 * (distVertical - alturaAdicional) / Physics.gravity.y);
        float velocidadeHorizontal = distHorizontal / t;

        Vector3 velocidade = new Vector3(direcao.x / distHorizontal * velocidadeHorizontal, velocidadeVertical,
            direcao.z / distHorizontal * velocidadeHorizontal);

        return velocidade;
    }

    private void ControlDecalDistance()
    {
        if(decalObject == null){
            decalObject = Instantiate(explosionArea.gameObject, DecalSpawnPoint.transform.position, DecalSpawnPoint.transform.rotation);
            decalObject.transform.SetParent(DecalSpawnPoint.transform);
            DecalProjector decalProjector = decalObject.GetComponent<DecalProjector>();
            decalProjector.size = new Vector3(throwableInventory[itemIndex].radius * 2, throwableInventory[itemIndex].radius * 2, decalProjector.size.z);

        }

        if (currentThrowDistance < maxThrowDistance)
        {
            currentThrowDistance += Time.deltaTime * 5;
        }
     
        Vector3 offset = new Vector3(-currentThrowDistance,0,-0.1f);
            decalObject.transform.localPosition = offset;
            
    }

    //Getters and Setters
    public void setMaxCapacity(int maxCapacity)
    {
        this.maxCapacity = maxCapacity;
    }
    
    public void setAiming(bool isAiming)
    {
        if (canThrowItem)
        {
            if(isAiming)
            {
                this.isAiming = isAiming;
                ControlDecalDistance();
            }
            
            else
            {
                this.isAiming = isAiming;
                ThrowItem();
            }
        }
    }

    public void ThrowItem()
    {
            GameObject throwableItemInstance = Instantiate(throwableItemPrefab, ThrowerHand.transform.position, Quaternion.identity);
            Rigidbody rb = throwableItemInstance.GetComponent<Rigidbody>();
            throwableItemInstance.GetComponent<ThrowableItem>().setThrowableSpecs(throwableInventory[itemIndex]);
            Vector3 trajetoria =
                CalculaTrajetoriaParabolica(ThrowerHand.transform.position, decalObject.transform.position,
                    9);
            rb.AddForce(trajetoria, ForceMode.VelocityChange);
            throwableInventory.Remove(throwableInventory[itemIndex]);
            changeToNextItem();
            if(throwableInventory.Count == 0)
                canThrowItem = false;
            currentThrowDistance = 0f;
            Destroy(decalObject);
    }
    
    public void cancelThrowAction()
    {
        canThrowItem = false;
        Destroy(decalObject);
        canThrowItem = true;
    }
}