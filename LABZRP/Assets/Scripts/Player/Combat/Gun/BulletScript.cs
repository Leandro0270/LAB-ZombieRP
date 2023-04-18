using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class BulletScript : MonoBehaviour
{
    [SerializeField] private bool haveKnockback = false;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private bool isMelee = false;
    private WeaponSystem PlayerShooter;
    public GameObject BulletHole;
    public GameObject bloodParticle;
    private Rigidbody _rb;
    private float damage;
    public float distancia;
    public float velocidadeBala = 20f;
    public int hitableEnemies = 1;
    private int enemiesHitted = 0;

    
    

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        StartCoroutine(waiter());
        
    }


    void FixedUpdate()
    {

        //Nessa variavel está sendo feito o calculo da rotação necessária para o player utilizando o lerp para suavizar
        _rb.MovePosition(_rb.position + transform.forward * (velocidadeBala * Time.deltaTime));


    }

    IEnumerator waiter()
    {
        float tempo = distancia / velocidadeBala;
        yield return new WaitForSeconds(tempo);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider objetoDeColisao)
    {
        //Verifica se o objeto colidido tem a tag "WALL"
        if (!isMelee)
        {
            if (objetoDeColisao.gameObject.CompareTag("WALL"))
            {
                GameObject NewBulletHole = Instantiate(BulletHole,
                    objetoDeColisao.ClosestPointOnBounds(transform.position),
                    transform.rotation);
                Destroy(NewBulletHole, 20f);
                destroyBullet();
                Destroy(gameObject);

            }
        }

        EnemyStatus _status = objetoDeColisao.GetComponent<EnemyStatus>();
        if (_status != null)
        {
            if (!_status.isDeadEnemy())
            {
                
                        enemiesHitted++;
                        _status.takeDamage(damage);
                        GameObject NewBloodParticle = Instantiate(bloodParticle, objetoDeColisao.transform.position,
                            objetoDeColisao.transform.rotation);
                        Destroy(NewBloodParticle, 4f);
                        if (_status.get_life() < 1)
                        {
                            _status.killEnemy();
                                if (_status.getIsSpecial())
                                {
                                    int points = _status.getPoints();
                                    PlayerShooter.addKilledSpecialZombie(points);
                                }
                                else
                                    PlayerShooter.addKilledNormalZombie();
                        }
                        else
                        {
                            if (haveKnockback)
                            {
                                Rigidbody rb = objetoDeColisao.GetComponent<Rigidbody>();
                                rb.AddForce(transform.forward * knockbackForce, ForceMode.Impulse);
                            }
                        }
                        if (enemiesHitted < hitableEnemies)
                        {
                            enemiesHitted++;
                        }
                        else
                        {
                            destroyBullet();
                            Destroy(gameObject,2f);
                        }
                        
            }
        }
        

        PlayerStats status = objetoDeColisao.GetComponent<PlayerStats>();
        if (status != null)
        {
            status.takeDamage(damage * 0.5f);
            if (enemiesHitted< hitableEnemies)
            {
                enemiesHitted++;
            }
            else
            {
                destroyBullet();
                Destroy(gameObject);
            }
            

        }
        

    }
    
    public void setIsKnockback(bool knockback)
    {
        haveKnockback = knockback;
    }
    
    public void setKnockbackForce(float knockbackForce)
    {
        this.knockbackForce = knockbackForce;
    }
    public void setDistancia(float distancia)
    {
        this.distancia = distancia;
    }
    public void setVelocidadeBalas(float velocidadeBala)
    {
        this.velocidadeBala = velocidadeBala;
    }
    public void setHitableEnemies(int hitableEnemies)
    {
        this.hitableEnemies = hitableEnemies;
    }
    public void SetDamage(float damage)
    {
        this.damage = damage;
    }


    public void setShooter(WeaponSystem shooter)
    {
        PlayerShooter = shooter;
    }
    
    public void setMelee(bool melee)
    {
        isMelee = melee;
    }

    private void destroyBullet()
    {
        Destroy(gameObject.GetComponent<MeshFilter>());
        Destroy(gameObject.GetComponent<UniversalAdditionalLightData>());
        Destroy(gameObject.GetComponent<BoxCollider>());
        Destroy(gameObject.GetComponent<Light>());
        Destroy(gameObject.GetComponentInChildren<ParticleSystem>());
    }
}
