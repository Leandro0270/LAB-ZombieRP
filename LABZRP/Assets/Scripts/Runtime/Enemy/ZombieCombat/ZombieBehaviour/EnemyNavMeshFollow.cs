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
        public NavMeshAgent enemy;
        [SerializeField] private bool isOnline = false;
        private GameObject[] players;
        private GameObject[] CoffeMachines;
        private GameObject target;
        private bool canWalk = true;
        private bool isAlive = true;
        private bool followPlayers = true;
        private bool isSpecial = false;
        private bool canAttack = true;
        private bool isCoffeMachineEvent = false;
        private bool isEvent = false;

        [FormerlySerializedAs("animation")] [SerializeField] private ZombieAnimationController animationController;
        // Start is called before the first frame update
        void Start()
        {
            if (PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected)
            {
                players = GameObject.FindGameObjectsWithTag("Player");
                if (isCoffeMachineEvent)
                {
                    CoffeMachines = GameObject.FindGameObjectsWithTag("CoffeeMachine");
                    target = GetTarget(CoffeMachines);

                }
                else
                {
                    target = GetTarget(players);

                }

                if (animationController == null)
                    animationController = GetComponentInChildren<ZombieAnimationController>();
                animationController.setTarget(true);
            }

        }

        // Update is called once per frame
        void Update()
        {
            if (PhotonNetwork.IsMasterClient || !isOnline)
            {
                if (followPlayers)
                {

                    if (isAlive)
                    {
                        if (!isEvent)
                        {
                            if (target == null || players.Length != PhotonNetwork.PlayerList.Length)
                            {
                                players = GameObject.FindGameObjectsWithTag("Player");
                                target = GetTarget(players);
                            }
                            PlayerStats _playerstats = target.GetComponent<PlayerStats>();
                            if (_playerstats.GetIsDown() && !_playerstats.GetIsIncapacitated() && !isSpecial)
                            {
                                //usa uma lista auxiliar sem o player que morreu para definir um novo target
                                GameObject[] aux = new GameObject[players.Length - 1];
                                foreach (GameObject player in players)
                                {
                                    int i = 0;
                                    if (player != target)
                                    {
                                        aux[i] = player;
                                        i++;
                                    }
                                }

                                target = GetTarget(aux);
                            }

                            if (!isOnline || PhotonNetwork.IsMasterClient)
                            {
                                if (canWalk)
                                {
                                    enemy.isStopped = false;
                                    enemy.SetDestination(target.transform.position);
                                }
                                else
                                    enemy.isStopped = true;
                            }

                            float distance = Vector3.Distance(target.transform.position, transform.position);
                            if (distance < 4f && canWalk && !_playerstats.GetIsIncapacitated() && canAttack)
                            {

                                animationController.setAttack();
                                canWalk = false;
                                if (isOnline)
                                {
                                    _playerstats.TakeOnlineDamage(GetComponent<EnemyStatus.EnemyStatus>().GetDamage(), false);
                                }
                                else
                                {
                                    _playerstats.TakeDamage(GetComponent<EnemyStatus.EnemyStatus>().GetDamage(), false);
                                }

                                Invoke("resetCanWalk", 1f);
                                if (_playerstats.GetIsDown())
                                {
                                    //usa uma lista auxiliar sem o player que morreu para definir um novo target
                                    GameObject[] aux = new GameObject[players.Length - 1];
                                    foreach (GameObject player in players)
                                    {
                                        int i = 0;
                                        if (player != target)
                                        {
                                            aux[i] = player;
                                            i++;
                                        }
                                    }

                                    target = GetTarget(aux);
                                }
                            }



                        }
                        else
                        {
                            if (isCoffeMachineEvent)
                            {
                                CoffeMachines = GameObject.FindGameObjectsWithTag("CoffeeMachine");
                                target = GetTarget(CoffeMachines);
                                ChallengeCoffeeMachine _challengeCoffeeMachine =
                                    target.GetComponent<ChallengeCoffeeMachine>();

                                if (canWalk)
                                {
                                    enemy.isStopped = false;
                                    enemy.SetDestination(target.transform.position);
                                }
                                else
                                {
                                    enemy.isStopped = true;
                                }

                                float distance = Vector3.Distance(target.transform.position, transform.position);
                                if (distance < 4f && canWalk && canAttack)
                                {
                                    animationController.setAttack();
                                    canWalk = false;
                                    if (isOnline)
                                    {
                                        int photonIdCoffeeMachineTarget = _challengeCoffeeMachine.GetComponent<PhotonView>().ViewID;
                                        photonView.RPC("CoffeeMachineTakeHit", RpcTarget.All, photonIdCoffeeMachineTarget);
                                    }
                                    else
                                        _challengeCoffeeMachine.takeHit(GetComponent<EnemyStatus.EnemyStatus>().GetDamage());
                                    Invoke("resetCanWalk", 1f);
                                }
                            }
                        }
                    }
                    else
                    {
                        enemy.isStopped = true;
                    }
                }
            }
        }
    
    
        [PunRPC]
        public void CoffeeMachineTakeHit(int photonIdCoffeeMachineTarget)
        {
            PhotonView.Find(photonIdCoffeeMachineTarget).GetComponent<ChallengeCoffeeMachine>().takeHit(GetComponent<EnemyStatus.EnemyStatus>().GetDamage());
        }
    
    
        GameObject GetTarget (GameObject[] players){
            GameObject target = null;
            float minDist = Mathf.Infinity;
            Vector3 currentPos = transform.position;
            foreach (GameObject t in players){
                float dist = Vector3.Distance(t.transform.position, currentPos);
                if (dist < minDist)
                {
                    target = t;
                    minDist = dist;
                }
            }
            return target;
        }

        public void setCanWalk(bool canWalk)
        {
            this.canWalk = canWalk;
        }
    
        public void setIsOnline(bool isOnline)
        {
            this.isOnline = isOnline;
        }

        private void resetCanWalk()
        {
            canWalk = true;
        }
    
        public void setIsAlive(bool isAlive)
        {
            this.isAlive = isAlive;
        }
    
        public NavMeshAgent getEnemy()
        {
            return enemy;
        }
    
    
        public void setFollowPlayers(bool followPlayers)
        {
            canWalk = followPlayers;
        }
    
        public void setIsStoped(bool isStoped)
        {
            enemy.isStopped = isStoped;
        }


        public void AttackDelay(float time)
        {
            canWalk = false;
            Invoke("resetCanWalk", time);
        }
    
        public void setTarget(GameObject target)
        {
            this.target = target;
        }
        public void setIsSpecial(bool isSpecial)
        {
            this.isSpecial = isSpecial;
        }
    
        public void setCanAttack(bool canAttack)
        {
            this.canAttack = canAttack;
        }
    
    
        public void setSpeed(float speed)
        {
            enemy.speed = speed;
        }
    
        public void setNewDestination(Vector3 destination)
        {
            enemy.SetDestination(destination);
        }

        public void setNearPlayerDestination()
        {
            target = GetTarget(players);
            enemy.SetDestination(target.transform.position);
        }
    
        public void setCoffeeMachineEvent(bool isCoffeMachineEvent)
        {
            isEvent = isCoffeMachineEvent;
            this.isCoffeMachineEvent = isCoffeMachineEvent;
        }
    
    }
}



