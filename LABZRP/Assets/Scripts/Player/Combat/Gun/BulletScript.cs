using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
public class BulletScript : MonoBehaviour
{
    public GameObject bloodParticle;
    private Rigidbody _rb;
    private float damage;
    public float distance;
    public float speedBullet = 20f;
    
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        StartCoroutine(waiter());
    }
    

    void FixedUpdate()
    {
 
        //Nessa variavel está sendo feito o calculo da rotação necessária para o player utilizando o lerp para suavizar
        _rb.MovePosition(_rb.position + transform.forward * (speedBullet * Time.deltaTime));


    }

    IEnumerator waiter()
    {
        float tempo = distance / speedBullet;
        yield return new WaitForSeconds(tempo);
        Destroy(gameObject);
    }

void OnTriggerEnter(Collider objetoDeColisao)
    {
        
        EnemyStatus _status = objetoDeColisao.GetComponent<EnemyStatus>();
        if(_status != null)
        {

            if(!_status.isDeadEnemy()){
                Destroy(gameObject.GetComponent<Rigidbody>());
                Destroy(gameObject.GetComponent<BoxCollider>());
                Destroy(gameObject.GetComponent<Light>());
                Destroy(gameObject.GetComponentInChildren<ParticleSystem>());
                GameObject NewBloodParticle= Instantiate(bloodParticle, objetoDeColisao.transform.position, objetoDeColisao.transform.rotation);
                Destroy(NewBloodParticle, 4f);
                    _status.takeDamage(damage);
                    Destroy(gameObject, 1f);
                if(_status.get_life() < 1)
                    _status.killEnemy();
            }
        }
        PlayerStats status = objetoDeColisao.GetComponent<PlayerStats>();
        if (status != null)
        {
            Destroy(gameObject.GetComponent<Rigidbody>());
            Destroy(gameObject.GetComponent<BoxCollider>());
            Destroy(gameObject.GetComponent<Light>());
            Destroy(gameObject.GetComponentInChildren<ParticleSystem>());
            status.takeDamage(damage*0.5f);
            Destroy(gameObject);

        }
        //Verifica se o objeto colidido tem a tag "WALL"

    if (objetoDeColisao.gameObject.CompareTag("WALL"))
        {
            Destroy(gameObject);
        }

    }
public void SetDamage(float damage)
{
    this.damage = damage;
}
}
