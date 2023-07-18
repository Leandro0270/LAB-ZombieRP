using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class Points_UI : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private TextMeshProUGUI texto;

    private int points;
    
    public void setPoints(int points)
    {
        this.points = points;
        updateText();
    }
    
    public void updateText()
    {
        texto.text = "$|" + points;
    }
    
    
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(points);
        }
        else
        {
            points = (int) stream.ReceiveNext();
            updateText();
        }
    }
}
