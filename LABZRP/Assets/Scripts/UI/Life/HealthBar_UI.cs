using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;
public class HealthBar_UI : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] PhotonView photonView;
    [SerializeField] private bool isOnline = false;
    private Color _color;
    public GameObject Fill;
    [SerializeField] private Slider _slider;
    [SerializeField] private Image _fill;

    public void setMaxHealth(int health)
    {
        _slider.maxValue = health;
        _slider.value = health;
    }
    public void SetHealth(int health)
    {
        _slider.value = health;
    }

    public void setColor(Color color)
    {
        _fill.color = color;
    }
    
    
    public Color getColor()
    {
        return _fill.color;
    }
    
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_slider.maxValue);
            stream.SendNext(_slider.value);
        }
        else
        {
            _slider.maxValue = (float)stream.ReceiveNext();
            _slider.value = (float)stream.ReceiveNext();
            

        }
    }
}
