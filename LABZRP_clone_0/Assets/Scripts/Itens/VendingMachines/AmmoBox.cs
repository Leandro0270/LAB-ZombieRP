using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    [SerializeField] private int price = 200;
    public GameObject priceText;
    public GameObject lid; // Referência à tampa da caixa
    public float openSpeed = 2f; // Velocidade de abertura da tampa
    public Vector3 openPosition = new Vector3(0f, 0.271f, 0.2152298f);
    public Vector3 openRotation = new Vector3(-39.335f, 0f, 0f);

    private Vector3 closedPosition;
    private Vector3 closedRotation;
    private bool isOpen = false;
    private bool isAnimating = false;
    private int playerCount = 0; // Contador de jogadores dentro do trigger

    private void Start()
    {
        closedPosition = lid.transform.localPosition;
        closedRotation = lid.transform.localEulerAngles;
        priceText.GetComponent<TextMeshPro>().SetText("$"+price);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCount++;

            if (!isOpen && !isAnimating)
            {
                priceText.SetActive(true);
                StartCoroutine(OpenLid());
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        PlayerStats playerStats = other.GetComponent<PlayerStats>();

        if (playerStats)
        {
            WeaponSystem weapon = playerStats.getWeaponSystem();
            PlayerPoints playerPoints = playerStats.getPlayerPoints();
            bool isInteracting = playerStats.getInteracting();
            bool haveLessAmmo = (weapon.GetAtualAmmo()<weapon.GetMaxBalas());
            bool haveMoney = (playerPoints.getPoints() >= price);
            if(isInteracting && haveLessAmmo && haveMoney)
            {
                playerPoints.removePoints(price);
                weapon.ReceiveAmmo(weapon.GetMaxBalas());
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerCount--;

            if (playerCount <= 0 && isOpen && !isAnimating)
            {
                StartCoroutine(CloseLid());
            }
        }
    }

    private IEnumerator OpenLid()
    {
        isAnimating = true;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;

            lid.transform.localPosition = Vector3.Lerp(closedPosition, openPosition, t);
            lid.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(closedRotation), Quaternion.Euler(openRotation), t);

            yield return null;
        }

        isOpen = true;
        isAnimating = false;
    }

    private IEnumerator CloseLid()
    {
        isAnimating = true;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * openSpeed;

            lid.transform.localPosition = Vector3.Lerp(openPosition, closedPosition, t);
            lid.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(openRotation), Quaternion.Euler(closedRotation), t);

            yield return null;
        }

        isOpen = false;
        isAnimating = false;
        priceText.SetActive(false);

    }
}