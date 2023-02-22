using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
[RequireComponent(typeof(Rigidbody))]
public class BulletScript : MonoBehaviour
{
    public Rigidbody _rb;
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
        Destroy(gameObject.GetComponent<BoxCollider>());
        EnemyStatus status = objetoDeColisao.GetComponent<EnemyStatus>();
        if(status != null)
        {
            Debug.Log("Chega aqui");
            status.takeDamage(damage);
        }
        PlayerStats statusPlayer = objetoDeColisao.GetComponent<PlayerStats>();
        if (statusPlayer != null)
        {
            Debug.Log("Deu colisão");
            statusPlayer.takeDamage(damage*0.5f);
        }
        Destroy(gameObject);
    }

public void SetDamage(float damage)
{
    this.damage = damage;
}

}
