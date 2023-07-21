using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReviveScript : MonoBehaviour
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
                _timeToRevive = ctx.GetComponent<PlayerStats>().getRevivalSpeed();
                MaxRevivalSpeed = _timeToRevive;
                
            }
        }
    }
    
    private void OnTriggerStay(Collider ctx)
    {
        if (ctx.GetComponent<PlayerStats>() != null)
        {
             bool pressing = ctx.GetComponent<PlayerStats>().getInteracting();

            if (pressing && _playerStats.verifyDown() && !_playerStats.verifyDeath())
            {
                _isReviving = true;
                instantiateRevivalUI();
                _RevivalUIInstance.transform.position = Camera.main.WorldToScreenPoint(ctx.gameObject.transform.position + new Vector3(0, 6, 0));
                _RevivalUIInstance.maxValue = MaxRevivalSpeed;
                _playerStats.stopDeathCounting(true);
                _timeToRevive -= Time.deltaTime;
                if (_timeToRevive <= 0)
                {
                    _isReviving = false;
                    Destroy(_RevivalUIInstance.gameObject);
                    _playerStats.Revived();
                    ctx.GetComponent<ReviveScript>().addReviveCount();
                }
            }
            
            if (ctx.GetComponent<PlayerStats>().getInteracting() == false && _playerStats.verifyDown())
            {
                if (_RevivalUIInstance != null)
                {
                    _playerStats.stopDeathCounting(false);
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
            _playerStats.stopDeathCounting(false);
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
}
