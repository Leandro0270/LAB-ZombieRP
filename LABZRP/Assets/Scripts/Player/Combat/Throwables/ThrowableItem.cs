using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableItem : MonoBehaviour
{
    [SerializeField] private GameObject explosionArea;
    private ScObThrowableSpecs _throwableSpecs;
    private ScObThrowableSpecs.Type _throwableType;
    private GameObject _throwablePrefab3DModel;
    private bool _explodeOnImpact;
    private float _radius;
    private bool _affectEnemies;
    private bool _affectAllies;
    private float _timeToExplode;
    private float _damage;
    private float _slowDown;
    private float _slowDownDuration;
    private float _health;
    private float _effectDuration;
    private float _maxDistance;
    private bool setup = false;
    private GameObject Model;
    private float _currentTime;
    private bool _attractEnemies;
    private float _attactionRadius;
    private bool _affectCamera;
    private float _cameraShakeAmount;
    private float _cameraShakeDuration;

    // Update is called once per frame
    void Update()
    {
        if (setup)
        {


            if (!_explodeOnImpact)
            {
                _currentTime += Time.deltaTime;
                if (_currentTime >= _timeToExplode)
                {
                    Explode();
                }
            }
            
            if(_attractEnemies)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, _attactionRadius);
                foreach (var hitCollider in hitColliders)
                {
                    if (hitCollider.gameObject.CompareTag("Enemy"))
                    {
                        hitCollider.gameObject.GetComponent<EnemyFollow>().setNewDestination(transform.position);
                    }
                }
            }
            

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_explodeOnImpact && (other.gameObject.CompareTag("Floor")|| other.gameObject.CompareTag("Enemy") || other.gameObject.CompareTag("Player")))
        {
            Explode();
        }
    }


    private void Explode()
        {
            if (explosionArea != null)
            {
                GameObject explosion = Instantiate(explosionArea, transform.position, transform.rotation);
                explosion.GetComponent<ExplosionArea>().Setup(_throwableSpecs);
                SphereCollider _sphereCollider = explosion.GetComponent<SphereCollider>();
                _sphereCollider.radius = _radius;
            }

            Destroy(gameObject);

        }


        public void setThrowableSpecs(ScObThrowableSpecs throwableSpecs)
        {
            _throwableSpecs = throwableSpecs;
            _throwablePrefab3DModel = throwableSpecs.throwablePrefab3DModel;
            _explodeOnImpact = throwableSpecs.explodeOnImpact;
            _radius = throwableSpecs.radius;
            _timeToExplode = throwableSpecs.timeToExplode;
            _attactionRadius = throwableSpecs.attactionRadius;
            _attractEnemies = throwableSpecs.attractEnemies;
            _affectCamera = throwableSpecs.affectCamera;
            _cameraShakeAmount = throwableSpecs.cameraShakeAmount;
            _cameraShakeDuration = throwableSpecs.cameraShakeDuration;
            Model = Instantiate(_throwablePrefab3DModel, transform.position, transform.rotation);
            Model.transform.parent = transform;
            setup = true;
        }
    
}
