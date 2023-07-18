using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

public class BulletScript : MonoBehaviourPunCallbacks, IPunObservable
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
    private bool isCritical = false;
    private bool _isAiming = false;
    private bool hitted = false;
    [SerializeField] private bool _isOnline = false;
    private bool _isBulletOwner = false;


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
        if (!hitted && _isBulletOwner)
            PlayerShooter.missedShot();
        if(_isBulletOwner)
            PhotonNetwork.Destroy(gameObject);
        else if(!_isOnline)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider objetoDeColisao)
    {
        if (objetoDeColisao.gameObject.CompareTag("WALL"))
        {
            GameObject NewBulletHole = Instantiate(BulletHole,
                objetoDeColisao.ClosestPointOnBounds(transform.position),
                transform.rotation);
            Destroy(NewBulletHole, 20f);
            destroyBullet();
            if(_isBulletOwner)
                PhotonNetwork.Destroy(gameObject);

        }


        EnemyStatus _status = objetoDeColisao.GetComponent<EnemyStatus>();


        if (_status != null)
        {
            if (!_status.isDeadEnemy())
            {
                hitted = true;
                enemiesHitted++;
                if (isCritical)
                {
                    Debug.Log("Critico!");
                }

                GameObject NewBloodParticle = Instantiate(bloodParticle, objetoDeColisao.transform.position,
                    objetoDeColisao.transform.rotation);
                Destroy(NewBloodParticle, 4f);

                //A função takeDamage retorna true se o zumbi morreu
                if (_status.takeDamage(damage))
                {
                    if (_isAiming)
                    {
                        Debug.Log("Matou mirando");
                        PlayerShooter.addKilledZombieWithAim();

                    }

                    else if (isMelee)
                    {
                        Debug.Log("Matou com melee");
                        PlayerShooter.addKilledZombieWithMelee();


                    }

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
                    if(_isOnline)
                        PhotonNetwork.Destroy(gameObject);
                    else if(!_isOnline)
                        Destroy(gameObject, 2f);
                }

            }
        }
        else
        {
            PlayerShooter.missedShot();
        }


        PlayerStats status = objetoDeColisao.GetComponent<PlayerStats>();
        if (status != null)
        {
            if (_isOnline)
            {
                status.takeOnlineDamage(damage * 0.5f);
            }
            else
            {
                status.takeDamage(damage * 0.5f);
            }
            if (enemiesHitted < hitableEnemies)
            {
                enemiesHitted++;
            }
            else
            {
                destroyBullet();
                if(_isBulletOwner)
                    PhotonNetwork.Destroy(gameObject);
                else if(!_isOnline)
                    Destroy(gameObject, 2f);
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
        if (_isOnline && photonView.IsMine)
        {
            PlayerShooter = shooter;
            _isBulletOwner = true;
            int shooterID = shooter.gameObject.GetComponent<PhotonView>().ViewID;
            photonView.RPC("setShooterOnline", RpcTarget.All, shooterID);
        }
        else if (!_isOnline)
        {
            PlayerShooter = shooter;
            _isBulletOwner = true;
        }
    }
    
    
    [PunRPC]
    public void setShooterOnline(int shooterID)
    {
        GameObject shooter = PhotonView.Find(shooterID).gameObject;
        PlayerShooter = shooter.GetComponent<WeaponSystem>();
    }

    public void setMelee(bool melee)
    {
        isMelee = melee;
    }

    public void setHaveKnockback(bool knockback)
    {
        haveKnockback = knockback;
    }

    public void setIsOnline(bool online)
    {
        _isOnline = online;
    }

    public void setIsAiming(bool aiming)
    {
        _isAiming = aiming;
    }

    public void setIsCritical(bool critical)
    {
        isCritical = critical;
    }

    private void destroyBullet()
    {
        if (_isOnline)
        {
            photonView.RPC("disableBulletOnline", RpcTarget.All, PlayerShooter.gameObject.GetComponent<PhotonView>().ViewID);
        }
        else
        {

            Destroy(gameObject.GetComponent<MeshFilter>());
            Destroy(gameObject.GetComponent<UniversalAdditionalLightData>());
            Destroy(gameObject.GetComponent<BoxCollider>());
            Destroy(gameObject.GetComponent<Light>());
            Destroy(gameObject.GetComponentInChildren<ParticleSystem>());
        }
    }

    [PunRPC]
    public void disableBulletOnline(int shooterID)
    {
        // Para desabilitar MeshFilter (alterando a visibilidade)
        MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.enabled = false;
        }

        // Para desabilitar BoxCollider
        BoxCollider boxCollider = gameObject.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        // Para desabilitar UniversalAdditionalLightData
        UniversalAdditionalLightData lightData = gameObject.GetComponent<UniversalAdditionalLightData>();
        if (lightData != null)
        {
            lightData.enabled = false;
        }

        // Para desabilitar Light
        Light light = gameObject.GetComponent<Light>();
        if (light != null)
        {
            light.enabled = false;
        }

        // Para desabilitar ParticleSystem
        ParticleSystem particleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Stop();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.IsWriting)
        {
            stream.SendNext(haveKnockback);
            stream.SendNext(knockbackForce);
            stream.SendNext(isMelee);
            stream.SendNext(damage);
            stream.SendNext(distancia);
            stream.SendNext(velocidadeBala);
            stream.SendNext(hitableEnemies);
            stream.SendNext(enemiesHitted);
            stream.SendNext(isCritical);
            stream.SendNext(_isAiming);
            stream.SendNext(hitted);

        }
        else
        {
            haveKnockback = (bool)stream.ReceiveNext();
            knockbackForce = (float)stream.ReceiveNext();
            isMelee = (bool)stream.ReceiveNext();
            damage = (float)stream.ReceiveNext();
            distancia = (float)stream.ReceiveNext();
            velocidadeBala = (float)stream.ReceiveNext();
            hitableEnemies = (int)stream.ReceiveNext();
            enemiesHitted = (int)stream.ReceiveNext();
            isCritical = (bool)stream.ReceiveNext();
            _isAiming = (bool)stream.ReceiveNext();
            hitted = (bool)stream.ReceiveNext();

        }
    }
}
