using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    public NavMeshAgent enemy;
    
    private GameObject[] players;
    private GameObject target;
    private bool canWalk = true;
    private bool isAlive = true;

    public ZombieAnimationController animation;
    // Start is called before the first frame update
    void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        target = GetTarget(players);
        animation.setTarget(true);
    }

    // Update is called once per frame
    void Update()
    {
        
       
        if (isAlive)
        {
            PlayerStats _playerstats = target.GetComponent<PlayerStats>();
            if (_playerstats.verifyDown())
            {
                //usa uma lista auxiliar sem o player que morreu para definir um novo target
                GameObject[] aux = new GameObject[players.Length - 1];
                foreach (GameObject player in players)
                {
                    int i = 0;
                    if (player != target)
                    {
                        aux[i] = player;
                        i++;
                    }
                }

                target = GetTarget(aux);
            }
            if (canWalk)
            {
                enemy.isStopped = false;
                enemy.SetDestination(target.transform.position);
            }
            else
            {
                enemy.isStopped = true;
            }
            
            float distance = Vector3.Distance(target.transform.position, transform.position);
            if (distance < 4f && canWalk)
            {

                animation.setAttack();
                canWalk = false;
                _playerstats.takeDamage(GetComponent<EnemyStatus>().getDamage());
                Invoke("resetCanWalk", 1f);
                if (_playerstats.verifyDown())
                {
                    //usa uma lista auxiliar sem o player que morreu para definir um novo target
                    GameObject[] aux = new GameObject[players.Length - 1];
                    foreach (GameObject player in players)
                    {
                        int i = 0;
                        if (player != target)
                        {
                            aux[i] = player;
                            i++;
                        }
                    }

                    target = GetTarget(aux);
                }
            }



        }
        else
        {
            enemy.isStopped = true;
        }
    }

    GameObject GetTarget (GameObject[] players){
        GameObject target = null;
        float minDist = Mathf.Infinity;
        Vector3 currentPos = transform.position;
        foreach (GameObject t in players){
            float dist = Vector3.Distance(t.transform.position, currentPos);
            if (dist < minDist)
            {
                target = t;
                minDist = dist;
            }
        }
        return target;
    }


    private void resetCanWalk()
    {
        canWalk = true;
    }
    
    public void setIsAlive(bool isAlive)
    {
        this.isAlive = isAlive;
    }

}



