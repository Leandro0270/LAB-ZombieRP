using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Runtime.Player.Combat.PlayerStatus;
using UnityEngine;
using UnityEngine.UI;

public class ReviveScript : MonoBehaviourPunCallbacks, IPunObservable
{
    private PlayerStats _playerStats;
    private float _timeToRevive;
    public Slider _RevivalSlider;
    private bool _isReviving = false;
    private Slider _RevivalUIInstance;
    private float MaxRevivalSpeed;
    private int _revives = 0;
    private int _downs = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        _playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if(_isReviving)
        {
            if (_RevivalUIInstance != null)
            {
                _RevivalUIInstance.value = MaxRevivalSpeed - _timeToRevive;
            }
        }
    }


    // Update is called once per frame
    void OnTriggerEnter(Collider ctx)
    {
        if (ctx.GetComponent<PlayerStats>() != null)
        {
            if (!_isReviving)
            {
                _timeToRevive = ctx.GetComponent<PlayerStats>().GetRevivalSpeed();
                MaxRevivalSpeed = _timeToRevive;
                
            }
        }
    }
    
    private void OnTriggerStay(Collider ctx)
    {
        if (ctx.GetComponent<PlayerStats>() != null)
        {
             bool pressing = ctx.GetComponent<PlayerStats>().GetInteracting();

            if (pressing && _playerStats.GetIsDown() && !_playerStats.GetIsDead())
            {
                _isReviving = true;
                instantiateRevivalUI();
                _RevivalUIInstance.transform.position = Camera.main.WorldToScreenPoint(ctx.gameObject.transform.position + new Vector3(0, 6, 0));
                _RevivalUIInstance.maxValue = MaxRevivalSpeed;
                _playerStats.StopDeathCounting(true);
                _timeToRevive -= Time.deltaTime;
                if (_timeToRevive <= 0)
                {
                    _isReviving = false;
                    Destroy(_RevivalUIInstance.gameObject);
                    _playerStats.Revived();
                    ctx.GetComponent<ReviveScript>().addReviveCount();
                }
            }
            
            if (ctx.GetComponent<PlayerStats>().GetInteracting() == false && _playerStats.GetIsDown())
            {
                if (_RevivalUIInstance != null)
                {
                    _playerStats.StopDeathCounting(false);
                    _isReviving = false;
                    _timeToRevive = MaxRevivalSpeed;
                    Destroy(_RevivalUIInstance.gameObject);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerStats>() != null)
        {
            _playerStats.StopDeathCounting(false);
            _isReviving = false;
            if(_RevivalUIInstance != null)
                Destroy(_RevivalUIInstance.gameObject);
        }
    }

    private void instantiateRevivalUI()
    {
        if (_RevivalUIInstance == null)
        {
            GameObject canva = GameObject.FindGameObjectWithTag("Canva");
            _RevivalUIInstance = Instantiate(_RevivalSlider, (transform.position), Quaternion.identity);
            _RevivalUIInstance.gameObject.transform.SetParent(canva.transform);
        }
    }

    public void addReviveCount()
    {
        _revives++;
    }
    
    public int getReviveCount()
    {
        return _revives;
    }
    
    public void addDownCount()
    {
        _downs++;
    }
    
    public int getDownCount()
    {
        return _downs++;
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_downs);
            stream.SendNext(_revives);
        }
        else
        {
            _downs = (int)stream.ReceiveNext();
            _revives = (int)stream.ReceiveNext();
        }
    }
}
