using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

[RequireComponent(typeof(Rigidbody))]
public class bulletScript : MonoBehaviour
{
    public Rigidbody _rb;
    private float damage;
    public float speedBullet = 20f;
    
    
    private void Awake()
    {
        StartCoroutine(waiter());
    }
    

    void FixedUpdate()
    {
 
        //Nessa variavel está sendo feito o calculo da rotação necessária para o player utilizando o lerp para suavizar
        _rb.MovePosition(_rb.position + transform.forward * (speedBullet * Time.deltaTime));


    }

    IEnumerator waiter()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }

void OnTriggerEnter(Collider objetoDeColisao)
    {
        Debug.Log(damage);
        Destroy(gameObject.GetComponent<BoxCollider>());
        EnemyStatus _status = objetoDeColisao.GetComponent<EnemyStatus>();
        if(_status != null)
        {
            Debug.Log("Chega aqui");
            _status.takeDamage(damage);
        }
        Destroy(gameObject);
    }

public void SetDamage(float damage)
{
    this.damage = damage;
}

}
