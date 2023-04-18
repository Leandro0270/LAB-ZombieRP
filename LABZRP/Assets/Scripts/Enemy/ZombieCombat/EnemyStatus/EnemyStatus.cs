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
    private SpecialZombiesAttacks _specialZombiesAttacks;
    private int points;



    // Start is called before the first frame update
    void Awake()
    {
        //Armazena um objeto que estiver com a tag horderManager na variavel hordermManager
        _enemyFollow = GetComponent<EnemyFollow>();
        hordeManager = GameObject.FindGameObjectWithTag("HorderManager");
        _specialZombiesAttacks = GetComponent<SpecialZombiesAttacks>();
        totalLife = _status.health;
        _life = totalLife;
        _speed = _status.speed;
        _damage = _status.damage;
        isSpecial = _status.isSpecial;
    }

    // Update is called once per frame

    private void Start()
    {
        _enemyFollow.getEnemy().speed = _speed;
        if(isSpecial)
            points = _specialZombiesAttacks.getPoints();
        else
            points = 10;

    }

    public void takeDamage(float damage)
    {
        _life -= damage;
        if (_specialZombiesAttacks)
        {
            _specialZombiesAttacks.setLife(_life);
        }
        //instancia o objeto blood1 na posição do inimigo
        GameObject NewBloodParticle = Instantiate(blood1, new Vector3(transform.position.x, 57, transform.position.z),
            blood1.transform.rotation);
        //destroi o objeto blood1 depois de 4 segundos
        Destroy(NewBloodParticle, 8f);
    }

    public void killEnemy()
    {
        Destroy(GetComponent<CapsuleCollider>());
        GameObject.Find("GameManager").GetComponent<MainGameManager>().removeEnemy(gameObject);
        GetComponent<BoxCollider>().enabled = true;
        isDead = true;
        GameObject NewBloodParticle = Instantiate(blood2, new Vector3(transform.position.x, 57, transform.position.z),
            blood2.transform.rotation);
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
    
    public EnemyFollow getEnemyFollow()
    {
        return _enemyFollow;
    }


    public void ReceiveTemporaryDamage(float time, float damage)
    {

        float updatedDamage = _damage + damage;
        float baseDamage = _damage;
        _damage = updatedDamage;
        StartCoroutine(resetTemporaryDamage(time, baseDamage));
    }

    private IEnumerator resetTemporaryDamage(float time, float baseDamage)
    {
        yield return new WaitForSeconds(time);
        _damage = baseDamage;
    }

    public void ReceiveTemporarySpeed(float time, float speed)
    {
        float updatedSpeed = _speed + speed;
        float baseSpeed = _speed;
        _enemyFollow.getEnemy().speed = updatedSpeed;
        StartCoroutine(resetTemporarySpeed(time, baseSpeed));
    }
    private IEnumerator resetTemporarySpeed(float time, float baseSpeed)
    {
          yield return new WaitForSeconds(time);
            _speed = baseSpeed;
            _enemyFollow.getEnemy().speed = _speed;
    }
    
    public void PermanentDamage(float damage)
    {
        _damage += damage;
    }
    
    public void PermanentSpeed(float speed)
    {
        _speed += speed;
        _enemyFollow.getEnemy().speed = _speed;
    }
    
    public void receiveLife(float life)
    {
        _life += life;
        if (_life > totalLife)
        {
            _life = totalLife;
        }
    }
    
    public SpecialZombiesAttacks getSpecialZombieAttack()
    {
        return _specialZombiesAttacks;
    }
    public int getPoints()
    {
        return points;
    }
    
}
