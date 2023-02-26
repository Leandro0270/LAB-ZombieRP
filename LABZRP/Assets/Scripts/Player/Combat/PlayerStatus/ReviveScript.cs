using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReviveScript : MonoBehaviour
{
    private PlayerStats _playerStats;
    private float _timeToRevive;
    
    // Start is called before the first frame update
    void Start()
    {
        _playerStats = GetComponent<PlayerStats>();
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider ctx)
    {
        if (ctx.GetComponent<PlayerStats>() != null)
        {
            _timeToRevive = ctx.GetComponent<PlayerStats>().getRevivalSpeed();
        }
    }
    
    private void OnTriggerStay(Collider ctx)
    {
        if (ctx.GetComponent<PlayerStats>() != null)
        {
            bool pressing = ctx.GetComponent<PlayerStats>().getInteracting();
            
            if (pressing && !_playerStats.GetisDead())
            {
                _playerStats.stopDeathCounting(true);
                _timeToRevive -= Time.deltaTime;
                if (_timeToRevive <= 0)
                {
                    _playerStats.revived();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerStats>() != null)
        {
            _playerStats.stopDeathCounting(false);
        }
    }
}
