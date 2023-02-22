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


    // Start is called before the first frame update
    void Awake()
    {
        //Armazena um objeto que estiver com a tag horderManager na variavel hordermManager
        hordeManager = GameObject.FindGameObjectWithTag("HorderManager");
        totalLife = _status.health;
        _life = totalLife;
        _speed = _status.speed;
        _damage = _status.damage;
    }

    // Update is called once per frame
  

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
        GetComponent<BoxCollider>().size = new Vector3(1,0.07f,1);
        GetComponent<BoxCollider>().center = new Vector3(0,-0.5f,0);
        isDead = true;
        GameObject NewBloodParticle= Instantiate(blood2, new Vector3(transform.position.x, 57, transform.position.z), blood2.transform.rotation);
        Destroy(NewBloodParticle, 8f);
        hordeManager.GetComponent<HordeManager>().decrementZombiesAlive();
        StartCoroutine(waiter());

    }
    
    IEnumerator waiter()
    {
        yield return new WaitForSeconds(3);
        Destroy(gameObject);
    }

    public float get_life()
    {
        return _life;
    }
    
    public bool isDeadEnemy()
    {
        return isDead;
    }
}
