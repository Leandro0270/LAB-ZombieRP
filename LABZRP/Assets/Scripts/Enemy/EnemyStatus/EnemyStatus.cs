using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStatus : MonoBehaviour
{
    public ScObEnemyStats _status;
    private GameObject hordeManager;
    public GameObject blood1;
    public GameObject blood2;
    private bool isDead = false;
    private float totalLife;
    private float _life;
    private float _speed;
    private float _damage;
    private bool isSpecial = false;
    [SerializeField] private ZombieAnimationController _animator;
    private EnemyFollow _enemyFollow;


    // Start is called before the first frame update
    void Awake()
    {
        //Armazena um objeto que estiver com a tag horderManager na variavel hordermManager
        _enemyFollow = GetComponent<EnemyFollow>();
        hordeManager = GameObject.FindGameObjectWithTag("HorderManager");
        totalLife = _status.health;
        _life = totalLife;
        _speed = _status.speed;
        _damage = _status.damage;
    }

    // Update is called once per frame

    private void Start()
    {
        _enemyFollow.getEnemy().speed = _speed;
    }

    public void takeDamage(float damage)
    {
        _life -= damage;
        //instancia o objeto blood1 na posição do inimigo
        GameObject NewBloodParticle= Instantiate(blood1, new Vector3(transform.position.x, 57, transform.position.z), blood1.transform.rotation);
        //destroi o objeto blood1 depois de 4 segundos
        Destroy(NewBloodParticle, 8f);
    }

    public void killEnemy()
    {
        Destroy(GetComponent<CapsuleCollider>());
        GameObject.Find("GameManager").GetComponent<MainGameManager>().removeEnemy(gameObject);
        GetComponent<BoxCollider>().enabled = true;
        isDead = true;
        GameObject NewBloodParticle= Instantiate(blood2, new Vector3(transform.position.x, 57, transform.position.z), blood2.transform.rotation);
        Destroy(NewBloodParticle, 8f);
        _animator.setTarget(false);
        _animator.triggerDown();
        GetComponent<EnemyFollow>().setIsAlive(false);
        hordeManager.GetComponent<HordeManager>().decrementZombiesAlive();
        StartCoroutine(waiter());

    }
    
    IEnumerator waiter()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }

    
    public bool getIsSpecial()
    {
        return isSpecial;
    }
    
    public float get_life()
    {
        return _life;
    }
    
    public bool isDeadEnemy()
    {
        return isDead;
    }

    public float getDamage()
    {
        return _damage;
    }
}
