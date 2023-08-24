using System.Collections.Generic;
using Photon.Pun;
using Runtime.Challenges;
using Runtime.Enemy.Animation;
using Runtime.Player.Combat.PlayerStatus;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Runtime.Enemy.ZombieCombat.ZombieBehaviour
{
    public class EnemyNavMeshFollow : MonoBehaviourPunCallbacks
    {
        [FormerlySerializedAs("enemy")] [SerializeField] private NavMeshAgent EnemyNavMeshAgent;
        [SerializeField] private bool isOnline = false;
        [SerializeField] private ZombieAnimationController animationController;

        private PlayerStats _targetPlayer;
        private bool _canWalk = true;
        private bool _isAlive = true;
        private readonly bool _followPlayers = true;
        private bool _isSpecial = false;
        private bool _canAttack = true;
        private bool _isCoffeeMachineEvent = false;
        private GameObject[] _coffeeMachines;
        private GameObject _targetCoffeeMachine;
        private List<PlayerStats> _players = new List<PlayerStats>();
        private EnemyStatus.EnemyStatus _enemyStatus; // Cache para EnemyStatus

        private void Awake()
        {
            if (animationController == null)
                animationController = GetComponentInChildren<ZombieAnimationController>();

            _enemyStatus = GetComponent<EnemyStatus.EnemyStatus>(); // Cache do componente
        }

        private void Start()
        {
            if (!PhotonNetwork.IsMasterClient && PhotonNetwork.IsConnected) return;
            UpdateTarget();
            animationController.setTarget(true);
        }


        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient && isOnline) return;
            if (_followPlayers && _isAlive)
            {
                if (!_isCoffeeMachineEvent)
                    HandlePlayerFollow();
                else
                    HandleCoffeeMachineEvent();
            }
            else
            {
                EnemyNavMeshAgent.isStopped = true;
            }
        }

        private void HandlePlayerFollow()
        {
            // Atualiza o alvo se o alvo atual está "down" e o zumbi não é especial
            if (_targetPlayer.GetIsDown() && !_isSpecial)
                UpdateTarget();

            MoveTowardsTarget();

            float distance = Vector3.Distance(_targetPlayer.transform.position, transform.position);
            if (distance < 4f && _canWalk && _canAttack)
            {
                ExecuteAttack(_targetPlayer);
            }
        }

        private void HandleCoffeeMachineEvent()
        {
            // Atualizamos o alvo, pois a máquina de café não tem estados como "down" (ao contrário dos jogadores).
            UpdateTarget();

            MoveTowardsTarget();

            ChallengeCoffeeMachine challengeCoffeeMachine = _targetCoffeeMachine.GetComponent<ChallengeCoffeeMachine>();
            float distance = Vector3.Distance(_targetCoffeeMachine.transform.position, transform.position);
            if (distance < 4f && _canWalk && _canAttack)
            {
                ExecuteCoffeeMachineAttack(challengeCoffeeMachine);
            }
        }



        private void ExecuteAttack(PlayerStats playerStats)
        {
            animationController.setAttack();
            _canWalk = false;

            float damage = _enemyStatus.GetDamage();
            if (isOnline)
                playerStats.TakeOnlineDamage(damage, false);
            else
                playerStats.TakeDamage(damage, false);

            // Se o jogador alvo estiver "down" após o ataque, atualizamos o alvo
            if (playerStats.GetIsDown())
                UpdateTarget();

            Invoke("resetCanWalk", 1f);
        }


        private void ExecuteCoffeeMachineAttack(ChallengeCoffeeMachine challengeCoffeeMachine)
        {
            animationController.setAttack();
            _canWalk = false;

            float damage = _enemyStatus.GetDamage();
            if (isOnline)
            {
                int photonIdCoffeeMachineTarget = challengeCoffeeMachine.GetComponent<PhotonView>().ViewID;
                photonView.RPC("CoffeeMachineTakeHit", RpcTarget.All, photonIdCoffeeMachineTarget);
            }
            else
            {
                challengeCoffeeMachine.takeHit(damage);
            }

            Invoke("resetCanWalk", 1f);
        }



        private void MoveTowardsTarget()
        {
            if (_canWalk)
            {
                EnemyNavMeshAgent.isStopped = false;
                EnemyNavMeshAgent.SetDestination(_targetPlayer.transform.position);
            }
            else
            {
                EnemyNavMeshAgent.isStopped = true;
            }
        }




        private void RefreshCoffeeMachineList()
        {
            _coffeeMachines = GameObject.FindGameObjectsWithTag("CoffeeMachine");
        }



        private void UpdateTarget()
        {
            if (_isCoffeeMachineEvent)
            {
                _targetCoffeeMachine = GetClosestCoffeeMachine();
            }
            else
            {
                PlayerStats newTarget = GetClosestPlayer();
                if (newTarget is null)
                {
                    _canWalk = false;
                    _isAlive = false;
                    EnemyNavMeshAgent.isStopped = true;
                    animationController.setTarget(false);
                }
                else
                {
                    _targetPlayer = newTarget;
                }
            }
        }



        private PlayerStats GetClosestPlayer()
        {
            PlayerStats closestTarget = null;
            float minDist = Mathf.Infinity;
            Vector3 currentPos = transform.position;

            foreach (PlayerStats player in _players)
            {
                if (player.GetIsDown()) // Ignora jogadores que estão "down"
                    continue;

                float dist = Vector3.Distance(player.transform.position, currentPos);
                if (dist < minDist)
                {
                    closestTarget = player;
                    minDist = dist;
                }
            }
            return closestTarget;
        }
        
        private GameObject GetClosestCoffeeMachine()
        {
            GameObject closestTarget = null;
            float minDist = Mathf.Infinity;
            Vector3 currentPos = transform.position;

            foreach (GameObject coffeeMachine in _coffeeMachines)
            {
                float dist = Vector3.Distance(coffeeMachine.transform.position, currentPos);
                if (dist < minDist)
                {
                    closestTarget = coffeeMachine;
                    minDist = dist;
                }
            }
            return closestTarget;
        }


        public void AddPlayer(GameObject player)
        {
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if(!_players.Contains(playerStats))
                _players.Add(playerStats);
        }

        public void RemovePlayer(PlayerStats player)
        {
            if(_players.Contains(player))
                _players.Remove(player);
        }

        [PunRPC]
        public void CoffeeMachineTakeHit(int photonIdCoffeeMachineTarget)
        {
            PhotonView.Find(photonIdCoffeeMachineTarget).GetComponent<ChallengeCoffeeMachine>().takeHit(GetComponent<EnemyStatus.EnemyStatus>().GetDamage());
        }

        public void setCanWalk(bool canWalk)
        {
            this._canWalk = canWalk;
        }
    
        public void setIsOnline(bool isOnline)
        {
            this.isOnline = isOnline;
        }

        public void setIsAlive(bool isAlive)
        {
            this._isAlive = isAlive;
        }
    
        public NavMeshAgent getEnemy()
        {
            return EnemyNavMeshAgent;
        }
    
        public void setFollowPlayers(bool followPlayers)
        {
            _canWalk = followPlayers;
        }
    
        public void setIsStoped(bool isStoped)
        {
            EnemyNavMeshAgent.isStopped = isStoped;
        }

        public void AttackDelay(float time)
        {
            _canWalk = false;
            Invoke("resetCanWalk", time);
        }
    
        public void setTarget(GameObject target)
        {
            if (target.GetComponent<PlayerStats>())
            {
                _targetPlayer = target.GetComponent<PlayerStats>();
            }
            else
            {
                _targetCoffeeMachine = target;
            }
        }


        public void setIsSpecial(bool isSpecial)
        {
            this._isSpecial = isSpecial;
        }
    
        public void setCanAttack(bool canAttack)
        {
            this._canAttack = canAttack;
        }
    
        public void setSpeed(float speed)
        {
            EnemyNavMeshAgent.speed = speed;
        }
    
        public void setNewDestination(Vector3 destination)
        {
            EnemyNavMeshAgent.SetDestination(destination);
        }

        public void setNearPlayerDestination()
        {
            _targetPlayer = GetClosestPlayer();
            EnemyNavMeshAgent.SetDestination(_targetPlayer.transform.position);
        }
    
        public void setCoffeeMachineEvent(bool isCoffeMachineEvent)
        {
            this._isCoffeeMachineEvent = isCoffeMachineEvent;
        }

        private void resetCanWalk()
        {
            _canWalk = true;
        }
    }
}
