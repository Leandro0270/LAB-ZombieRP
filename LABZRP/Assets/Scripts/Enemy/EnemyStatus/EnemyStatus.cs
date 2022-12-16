using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    public ScObEnemyStats _status;
    private bool isDead = false;
    private float totalLife;
    private float _life;
    private float _speed;
    private float _damage;


    // Start is called before the first frame update
    void Awake()
    {
        totalLife = _status.health;
        _life = totalLife;
        _speed = _status.speed;
        _damage = _status.damage;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(_life);
        if (_life < 1)
        {
            killEnemy();
        }
    }

    public void takeDamage(float damage)
    {
        _life -= damage;
    }

    public void killEnemy()
    {
        Destroy(GetComponent<BoxCollider>());
        isDead = true;
        StartCoroutine(waiter());

    }
    
    IEnumerator waiter()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }
}
