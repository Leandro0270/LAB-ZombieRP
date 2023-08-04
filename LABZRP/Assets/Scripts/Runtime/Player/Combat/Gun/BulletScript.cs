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
    [SerializeField] private bool isWallBang = false;
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

    
    [SerializeField] private BoxCollider _collider;
    [SerializeField] private UniversalAdditionalLightData _UAlightD;
    [SerializeField] private Light _light;
    [SerializeField] private MeshRenderer _meshRenderer;
    [SerializeField] private ParticleSystem _particleSystem;
    

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
        if(_isBulletOwner && _isOnline)
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
            if(_isBulletOwner && _isOnline)
                PhotonNetwork.Destroy(gameObject);

        }
        EnemyStatus _status = objetoDeColisao.GetComponent<EnemyStatus>();
        if (_status != null)
        {
            enemiesHitted++;
            GameObject NewBloodParticle = Instantiate(bloodParticle, objetoDeColisao.transform.position,
                objetoDeColisao.transform.rotation);
            Destroy(NewBloodParticle, 4f);
            if (!_status.isDeadEnemy())
            {
                
                hitted = true;
                if (_isBulletOwner)
                {
                    _status.takeDamage(damage, PlayerShooter,_isAiming,isMelee);
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
                    if(_isOnline && _isBulletOwner)
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
            if (!status.verifyDown())
            {
                enemiesHitted++;
                if (_isOnline)
                {
                    if(_isBulletOwner)
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
                    if (_isOnline)
                    {
                        if (photonView.IsMine)
                            PhotonNetwork.Destroy(gameObject);
                    }
                    else
                        Destroy(gameObject, 2f);
                }

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
        if (hitableEnemies > 1)
        {
            isWallBang = true;
        }
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
            _isBulletOwner = true;
            PlayerShooter = shooter;
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


    [PunRPC]
    public void destroyOnlineBullet()
    {
        _collider.enabled = false;
        if (!isMelee)
        {
            _UAlightD.enabled = false;
            _light.enabled = false;
            _meshRenderer.enabled = false;
            _particleSystem.Stop();
        }
    }
    private void destroyBullet()
    {
        if (_isOnline)
        {
            if(photonView.IsMine)
                photonView.RPC("destroyOnlineBullet", RpcTarget.All);
        }
        else
        {

            Destroy(gameObject.GetComponent<BoxCollider>());
            if (!isMelee)
            {
                Destroy(gameObject.GetComponent<MeshFilter>());
                Destroy(gameObject.GetComponent<UniversalAdditionalLightData>());
                Destroy(gameObject.GetComponent<Light>());
                Destroy(gameObject.GetComponentInChildren<ParticleSystem>());
            }
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
