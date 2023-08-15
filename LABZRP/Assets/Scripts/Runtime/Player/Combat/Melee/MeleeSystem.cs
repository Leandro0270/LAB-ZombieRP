using System;
using Photon.Pun;
using Runtime.Enemy.ZombieCombat.EnemyStatus;
using Runtime.environment;
using Runtime.Player.Combat.Gun;
using Runtime.Player.Combat.PlayerStatus;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Player.Combat.Melee
{
    public class MeleeSystem : MonoBehaviourPunCallbacks
    {
      [SerializeField] private WeaponSystem weaponSystem;
      [SerializeField] private BoxCollider meleeCollider;
      [SerializeField] private string playerTag = "Player";
      [SerializeField] private string enemyTag = "Enemy";
      [SerializeField] private string explosiveBarrelTag = "ExplosiveBarrel";
      [SerializeField] private bool isOnline;
      private int _hittableObjects = 0;
      private int _hitObjects = 0;
      private float _range = 1f;
      private float _horizontalArea = 1f;
      private float _verticalArea = 1f;
      private float _timeAttacking;
      private bool _attacking;
      private float _currentTimeAttacking;
      private float _damage;
      private bool _haveCriticalChance;
      private float _currentDamage;
      private bool _haveKnockBack;
      private float _criticalChance;
      private float _knockBackForce;
      private bool _isCritical;
      private float _criticalDamagePercentage;

      private void Start()
      {
          if(meleeCollider == null)
              meleeCollider = GetComponent<BoxCollider>();
          meleeCollider.enabled = false;
          _attacking = false;
      }

      private void Update()
      {
          if (_attacking)
          {

              if(_currentTimeAttacking >= _timeAttacking)
              {
                  _attacking = false;
                  meleeCollider.enabled = false;
              }
              else
              {
                  _currentTimeAttacking += Time.deltaTime;
              }
          }
          
      }

      public void ApplyMeleeAttackStats(float damage, bool haveCriticalChance, float timeAttacking, float horizontalRange, float verticalRange, float range, float criticalDamagePercentage,float criticalChance, int hittableObjects, bool haveKnockBack, float knockBackForce)
      {
          _damage = damage;
          _currentDamage = damage;
          _haveCriticalChance = haveCriticalChance;
          _timeAttacking = timeAttacking;
          _horizontalArea = horizontalRange;
          _verticalArea = verticalRange;
          _range = range;
          _hittableObjects = hittableObjects;
          _criticalDamagePercentage = criticalDamagePercentage;
            _haveKnockBack = haveKnockBack;
            _criticalChance = criticalChance;
          if(haveKnockBack)
                _knockBackForce = knockBackForce;
          var currentMeleeColliderCenter = meleeCollider.center;
          Vector3 newMeleeColliderSize = new Vector3(_horizontalArea, _verticalArea, _range);
          Vector3 newMeleeColliderCenter = new Vector3(currentMeleeColliderCenter.x, currentMeleeColliderCenter.y,currentMeleeColliderCenter.z + (range/2));
          meleeCollider.center = newMeleeColliderCenter;
          meleeCollider.size = newMeleeColliderSize;
      }
        
      
      public void Attack()
      {
          _currentDamage = _damage;
          _hitObjects = 0;
          if (_haveCriticalChance)
          {
              float random = Random.Range(0, 100);
              if (random <= _criticalChance)
              {
                  _currentDamage = _damage + (_damage * _criticalDamagePercentage / 100);
                  _isCritical = true;
              }
          }
          meleeCollider.enabled = true;
            _attacking = true;
            _currentTimeAttacking = 0;
      }

      private void OnTriggerEnter(Collider other)
      {
          
          if((isOnline && !photonView.IsMine) || !_attacking)
              return;
          var collidedTag = other.gameObject.tag;
          if (collidedTag == playerTag)
          {
              var targetPlayerStats = other.gameObject.GetComponent<PlayerStats>();
              targetPlayerStats.TakeDamage(_currentDamage,false);
              _hitObjects++;
          }
          else if (collidedTag == enemyTag)
          {
              var enemyStatus = other.gameObject.GetComponent<EnemyStatus>();
              enemyStatus.TakeDamage(_currentDamage, weaponSystem, false, true, _isCritical);
              _hitObjects++;
              if (_hitObjects >= _hittableObjects)
              {
                  meleeCollider.enabled = false;
                  _attacking = false;
              }
              var rb = other.gameObject.GetComponent<Rigidbody>();
              if (!rb) return;
              if (_haveKnockBack)
                  rb.AddForce(transform.forward * _knockBackForce, ForceMode.Impulse);

          }
          else if (collidedTag == explosiveBarrelTag)
          {
              var explosiveBarrels = other.gameObject.GetComponent<ExplosiveBarrels>();
              explosiveBarrels.takeDamage(_damage);
              _hitObjects++;

          }
      }
    }
}
