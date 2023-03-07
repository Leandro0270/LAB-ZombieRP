using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyFollow : MonoBehaviour
{
    public NavMeshAgent enemy;
    private GameObject[] players;
    private GameObject target;

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
        enemy.SetDestination(target.transform.position);
        transform.LookAt(target.transform.position);
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

}



