using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ChallengeCoffeeMachine : MonoBehaviourPunCallbacks, IPunObservable
{
    private ChallengeManager _challengeManager;
    [SerializeField] private GameObject orangeMug;
    [SerializeField] private GameObject redMug;
    [SerializeField] private GameObject greenMug;
    [SerializeField] private GameObject mugExplosionAreaEffect;
    [SerializeField] private  GameObject effectSpawnPoint;
    private bool challengeStarted = false;
    private bool isGreenUp = true;
    private bool isRedUp = true;
    private bool isOrangeUp = true;
    [SerializeField] private GameObject damageEffect;
    [SerializeField] private float StartLife = 100f;
    private float currentLife = 100f;
    [SerializeField] private bool isOnline = false;

    private void Start()
    {
        currentLife = StartLife;
    }

    public void setChallengeManager(ChallengeManager challengeManager)
    {
        _challengeManager = challengeManager;
    }

    public void takeHit(float damage)
    {
        if (challengeStarted)
        {
            GameObject effect = Instantiate(damageEffect, transform.position, transform.rotation);
            Destroy(effect, 2f);
            currentLife -= damage;
            if (currentLife <= 0)
            {
                explodeMug(redMug);
                _challengeManager.destroyCoffeeMachine();
            }
            else
            {
                if ((currentLife <= ((StartLife / 3) * 2)) && isGreenUp)
                {
                    isGreenUp = false;
                    explodeMug(greenMug);
                }
                else if ((currentLife <= (StartLife / 3)) && isOrangeUp)
                {
                    isOrangeUp = false;
                    explodeMug(orangeMug);

                }
            }
        }
    }
    
    public void explodeMug(GameObject mug)
    {
        Destroy(mug);
    }
    
    public void startChallenge()
    {
        challengeStarted = true;
    }
    
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(currentLife);
            
            
        }
        else
        {
            currentLife = (float)stream.ReceiveNext();
           
        }
    }

}
