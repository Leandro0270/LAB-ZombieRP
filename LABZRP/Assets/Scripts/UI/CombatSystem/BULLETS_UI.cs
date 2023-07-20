using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BULLETS_UI : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private TextMeshProUGUI texto;
    private int balasPente;
    private int balasTotal;
    private bool isShotgun = false;




    public void initializeHud(int balasPente, int balasTotal, bool isShotgun)
    {
        this.balasPente = balasPente;
        this.balasTotal = balasTotal;
        this.isShotgun = isShotgun;
        updateText();
    }

    public void setBalasPente(int balasPente)
    {
        this.balasPente = balasPente;
        updateText();
    }

    public void setBalasTotal(int balasTotal)
    {
        this.balasTotal = balasTotal;
        updateText();
    }

    public void updateText()
    {
        if (isShotgun)
            texto.text = "|" + (balasPente / 6) + " / " + (balasTotal / 6);
        else
            texto.text = "|" + balasPente + " / " + balasTotal;
    }



    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(texto.text);
        }
        else
        {
            texto.text = (string)stream.ReceiveNext();


        }
    }
}
