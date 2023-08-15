using System.Collections;
using Photon.Pun;
using Runtime.Enemy.ZombieCombat.ZombieBehaviour;
using Runtime.Player.ScriptObjects.Combat;
using UnityEngine;

namespace Runtime.Player.Combat.Throwables
{
    public class ThrowableItem : MonoBehaviourPunCallbacks
    {
        [SerializeField] private GameObject explosionArea;
        [SerializeField] private bool isOnline = false;
        [SerializeField] private MeshCollider meshCollider;
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
        // private bool _affectCamera;
        // private float _cameraShakeAmount;
        // private float _cameraShakeDuration;

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
                    if (!isOnline || PhotonNetwork.IsMasterClient)
                    {
                        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _attactionRadius);
                        foreach (var hitCollider in hitColliders)
                        {
                            if (hitCollider.gameObject.CompareTag("Enemy"))
                            {
                                EnemyNavMeshFollow enemyNavMeshFollow = hitCollider.gameObject.GetComponent<EnemyNavMeshFollow>();
                                enemyNavMeshFollow.setNewDestination(transform.position);
                                enemyNavMeshFollow.setCanAttack(false);
                                StartCoroutine(resetEnemyTarget(enemyNavMeshFollow));
                            }
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

        [PunRPC]
        public void applyExplosion(int viewId)
        {
            GameObject explosion = PhotonView.Find(viewId).gameObject;
            explosion.GetComponent<ExplosionArea>().Setup(_throwableSpecs);
          
        }

        private void Explode()
        {
            if (explosionArea != null)
            {
                if (isOnline)
                {
                    GameObject explosion = PhotonNetwork.Instantiate("explosionArea", transform.position, transform.rotation);
                    int viewID = explosion.GetComponent<PhotonView>().ViewID;
                    photonView.RPC("applyExplosion", RpcTarget.All, viewID);
                }
                else
                {
                    GameObject explosion = Instantiate(explosionArea, transform.position, transform.rotation);
                    explosion.GetComponent<ExplosionArea>().Setup(_throwableSpecs);
                }
            }

            Destroy(gameObject);

        }

    
        public IEnumerator resetEnemyTarget(EnemyNavMeshFollow enemyNavMeshFollow)
        {
            yield return new WaitForSeconds(_effectDuration+1);
            enemyNavMeshFollow.setCanAttack(true);
            enemyNavMeshFollow.setNearPlayerDestination();
        }

        public void setThrowableSpecs(ScObThrowableSpecs throwableSpecs)
        {
            _throwableSpecs = throwableSpecs;
            _throwablePrefab3DModel = throwableSpecs.throwablePrefab3DModel;
            _explodeOnImpact = throwableSpecs.explodeOnImpact;
            meshCollider.sharedMesh = throwableSpecs.throwablePrefab3DModel.GetComponent<MeshFilter>().sharedMesh;
            _radius = throwableSpecs.radius;
            _timeToExplode = throwableSpecs.timeToExplode;
            _attactionRadius = throwableSpecs.attractionRadius;
            _attractEnemies = throwableSpecs.attractEnemies;
            // _affectCamera = throwableSpecs.affectCamera;
            // _cameraShakeAmount = throwableSpecs.cameraShakeAmount;
            // _cameraShakeDuration = throwableSpecs.cameraShakeDuration;
            Model = Instantiate(_throwablePrefab3DModel, transform.position, transform.rotation);
            Model.transform.parent = transform;
            setup = true;
        }
    
    }
}
