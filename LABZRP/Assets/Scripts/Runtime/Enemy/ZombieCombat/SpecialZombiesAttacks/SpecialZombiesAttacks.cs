using System;
using System.Collections;
using Photon.Pun;
using Runtime.Enemy.Animation;
using Runtime.Enemy.ZombieCombat.ZombieBehaviour;
using Runtime.Player.Combat.PlayerStatus;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

namespace Runtime.Enemy.ZombieCombat.SpecialZombiesAttacks
{
    public class SpecialZombiesAttacks : MonoBehaviourPunCallbacks, IPunObservable
    {
        private float _zombieLife;
        [FormerlySerializedAs("ZombieStatus")] [SerializeField] private EnemyStatus.EnemyStatus zombieStatus;
        private GameObject[] _players;
        private GameObject _target;
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float timeToSinglePlayerBreakFree = 5f;
        [SerializeField] private ZombieAnimationController animationController;
        [SerializeField] private GameObject visualEffect;
        [SerializeField] private float damage = 10f;
        [SerializeField] private GameObject coffeeProjectile;
        [SerializeField] private Transform launchPoint;
        [SerializeField] private float specialActivationDistance = 10f;
        [SerializeField] private float runDistance = 20f;
        [SerializeField] private float specialAttacksDelay = 5f;
        [SerializeField] private float attackDelay = 2f;
        [SerializeField] private float delayLockNShoot = 2f;
        [SerializeField] private int points = 40;
        [SerializeField] private LineRenderer fisherHook;
        [SerializeField] private bool isOnline;
        [SerializeField] private EnemyNavMeshFollow enemyNavMeshFollow;
        private bool _isSinglePlayer = false;
        private float _currentBreakFreeTime;
        private float _currentSpecialAttacksDelay;
        private float _currentAttackDelay;
        private bool _playerIsInRange;
        private PlayerStats _playerStats;
        private bool _canSpecialAttack = true;
        private bool _canAttack = true;
        private bool _downedPlayer;
        private bool _jumpedIntoPlayer;
        private int _targetPhotonViewID;

        private enum SpecialType
        {
            Arremesso,
            Frog,
            Booomer,
            Jumper,
            Fisher
        }

        [SerializeField] private SpecialType _tipoDePoder;


        private void Awake()
        {
            _players = GameObject.FindGameObjectsWithTag("Player");
            _currentSpecialAttacksDelay = specialAttacksDelay;
            _target = GetTarget(_players);
            _zombieLife = zombieStatus.getBaseLife();
            enemyNavMeshFollow.setIsSpecial(true);

        }


        private void Update()
        {
            if (PhotonNetwork.IsMasterClient || !isOnline)
            {
                if (_target == null)
                {
                    _downedPlayer = false;
                    _playerIsInRange = false;
                    _canSpecialAttack = false;
                    _canAttack = false;
                    _jumpedIntoPlayer = false;
                    _players = GameObject.FindGameObjectsWithTag("Player");
                    FindNewTarget();
                    enemyNavMeshFollow.setTarget(_target);
                }
            
                if (!_playerStats)
                    _playerStats = _target.GetComponent<PlayerStats>();

                if (Vector3.Distance(transform.position, _target.transform.position) < specialActivationDistance)
                    _playerIsInRange = true;
                else
                    _playerIsInRange = false;


                if (_currentAttackDelay > 0)
                    _currentAttackDelay -= Time.deltaTime;
                else
                    _canAttack = true;

                if (_currentSpecialAttacksDelay > 0)
                    _currentSpecialAttacksDelay -= Time.deltaTime;
                else
                    _canSpecialAttack = true;


                if ((_playerStats.GetIsIncapacitated() && !_downedPlayer) || _playerStats.GetIsDown())
                {
                    FindNewTarget();
                    enemyNavMeshFollow.setTarget(_target);
                }




                switch (_tipoDePoder)
                {

                    //COFFEE CLASS ================================================================
                    case SpecialType.Arremesso:
                        if (!zombieStatus.IsDeadEnemy())
                        {
                            if (!_playerIsInRange)
                            {
                                enemyNavMeshFollow.setIsStoped(false);
                                enemyNavMeshFollow.setFollowPlayers(true);

                                enemyNavMeshFollow.setIsStoped(true);
                                animationController.setAttack();
                                enemyNavMeshFollow.setFollowPlayers(false);
                                if (_canSpecialAttack && !zombieStatus.IsDeadEnemy())
                                {
                                    _currentSpecialAttacksDelay = specialAttacksDelay;
                                    _canSpecialAttack = false;
                                    StartCoroutine(StopBeforeSpecialAttack());
                                }
                            }
                            else
                            {
                                if (Vector3.Distance(transform.position, _target.transform.position) > runDistance)
                                {
                                    enemyNavMeshFollow.setIsStoped(true);
                                    enemyNavMeshFollow.setFollowPlayers(false);
                                    if (_canSpecialAttack && !zombieStatus.IsDeadEnemy())
                                    {
                                        _currentSpecialAttacksDelay = specialAttacksDelay;
                                        _canSpecialAttack = false;
                                        StartCoroutine(StopBeforeSpecialAttack());
                                    }
                                }
                                else
                                {
                                    enemyNavMeshFollow.setIsStoped(false);
                                    navMeshAgent.SetDestination(PosicaoFuga());
                                }
                            }
                        }
                        else
                        {
                            enemyNavMeshFollow.setIsStoped(true);
                            enemyNavMeshFollow.setFollowPlayers(false);
                        }

                        break;

                    //FROG CLASS ============================================================================================================================================

                    case SpecialType.Frog:
                        if (_playerIsInRange && _canSpecialAttack)
                        {
                            if (!_downedPlayer && !_playerStats.GetIsIncapacitated())
                            {
                                if (isOnline)
                                {
                                    _playerStats.incapacitateOnline(photonView.ViewID);
                                    _playerStats.TakeOnlineDamage(damage, false);
                                }
                                else
                                {
                                    _playerStats.IncapacitatePlayer(zombieStatus);
                                    _playerStats.TakeDamage(damage,false);
                                }

                                if (isOnline)
                                {
                                    _playerStats.ChangeTransformParent(photonView.ViewID, true, false);
                                }
                                else
                                {
                                    _target.transform.SetParent(transform);
                                    _target.transform.localPosition = Vector3.zero;
                                }
                                _downedPlayer = true;
                                if(isOnline)
                                    _playerStats.OnlineCharacterController(false);
                                else
                                    _target.GetComponent<CharacterController>().enabled = false;

                                enemyNavMeshFollow.setFollowPlayers(false);
                            }
                            else
                            {
                                navMeshAgent.speed = 5;
                                if (!zombieStatus.IsDeadEnemy())
                                {
                                    if (_isSinglePlayer)
                                    {
                                        if (_currentBreakFreeTime < timeToSinglePlayerBreakFree)
                                        {
                                            _currentBreakFreeTime += Time.deltaTime;
                                        }
                                        else
                                        {
                                            _target.transform.SetParent(null);
                                            Vector3 targetPosition = _target.transform.position;
                                            _target.transform.position = new Vector3(targetPosition.x, 59,
                                                targetPosition.z);
                                            _playerStats.CapacitatePlayer();
                                            enemyNavMeshFollow.setFollowPlayers(true);
                                            enemyNavMeshFollow.setIsStoped(false);
                                            enemyNavMeshFollow.setIsAlive(true);
                                            enemyNavMeshFollow.setTarget(_target);
                                            enemyNavMeshFollow.setIsSpecial(false);
                                            enabled = false;
                                        }
                                    }
                                    if (_canAttack)
                                    {
                                        if (isOnline)
                                        {
                                            _playerStats.TakeOnlineDamage(damage, false);
                                        }
                                        else
                                        {
                                            _playerStats.TakeDamage(damage, false);
                                        }

                                        _currentAttackDelay = attackDelay;
                                        _canAttack = false;
                                    }

                                    if (_playerStats.GetIsDown())
                                    {
                                        if (isOnline)
                                        {
                                            _playerStats.ChangeTransformParent(0,false,true);
                                        }
                                        else
                                        {
                                            _target.transform.SetParent(null);
                                            _downedPlayer = false;
                                            Vector3 targetPostion = _target.transform.position;
                                            _target.transform.position = new Vector3(targetPostion.x, 59,
                                                targetPostion.z);
                                        }
                                    
                                        enemyNavMeshFollow.setFollowPlayers(true);
                                        GameObject[] aux = new GameObject[_players.Length - 1];
                                        foreach (GameObject player in _players)
                                        {
                                            int i = 0;
                                            if (player != _target)
                                            {
                                                aux[i] = player;
                                                i++;
                                            }
                                        }

                                        _target = GetTarget(aux);

                                    }
                                    navMeshAgent.SetDestination(PosicaoFuga());
                                }
                                else
                                {
                                    if (isOnline)
                                    {
                                        _playerStats.ChangeTransformParent(0,false,true);
                                        _playerStats.CapacitateOnlinePlayer();

                                    }
                                    else
                                    {
                                        _target.transform.SetParent(null);
                                        _target.transform.position = new Vector3(_target.transform.position.x, 59,
                                            _target.transform.position.z);
                                        _playerStats.CapacitatePlayer();
                                    }
                                }
                            }
                        }

                        break;



                    //BOOMER CLASS ================================================================
                    case SpecialType.Booomer:
                        enemyNavMeshFollow.setIsStoped(false);
                        if (_zombieLife <= 0)
                        {
                            Explode();
                        }
                        else
                        {
                            Vector3 pontoMedio = PontoMedioJogadoresProximos();
                            Vector3 posFinal = Vector3.zero;
                            if (pontoMedio != Vector3.zero)
                            {
                                posFinal = pontoMedio;
                                enemyNavMeshFollow.setFollowPlayers(false);

                                navMeshAgent.SetDestination(pontoMedio);
                            }
                            else
                            {
                                posFinal = _target.transform.position;
                                enemyNavMeshFollow.setFollowPlayers(true);
                            }

                            if (Vector3.Distance(transform.position, posFinal) <= specialActivationDistance)
                            {
                                Explode();
                            }
                        }

                        break;




                    //HUNTER CLASS ================================================================
                    case SpecialType.Jumper:

                        if (!zombieStatus.IsDeadEnemy())
                        {

                            if (!_jumpedIntoPlayer && !_playerStats.GetIsIncapacitated() && !_downedPlayer && _canSpecialAttack)
                            {
                                if (_playerIsInRange)
                                {
                                    enemyNavMeshFollow.enabled = false;
                                    StartCoroutine(StopBeforeSpecialAttack());
                                }
                            }

                            if (_downedPlayer)
                            {
                                if (_playerStats.GetIsDown())
                                {
                                    _downedPlayer = false;
                                    StartCoroutine(ReativarNavMeshAgentComAtraso(1f));
                                }
                                else
                                {

                                    if (_canAttack)
                                    {
                                        if (isOnline)
                                        {
                                            _playerStats.TakeOnlineDamage(damage, false);
                                        }
                                        else
                                        {
                                            _playerStats.TakeDamage(damage, false);

                                        }
                                        _currentAttackDelay = attackDelay;
                                        _canAttack = false;
                                    }
                                }
                            }

                        }

                        else
                        {
                            if (_downedPlayer && !_playerStats.GetIsDown())
                                if (isOnline)
                                {
                                    _playerStats.CapacitateOnlinePlayer();
                                }
                                else
                                {
                                    _playerStats.CapacitatePlayer();
                                }
                        }

                        break;
                    //SMOKER CLASS ================================================================
                    case SpecialType.Fisher:
                        if (!_downedPlayer)
                        {
                            if (!_playerStats.GetIsIncapacitated())
                            {
                                if (PosicaoVisivel())
                                {
                                    enemyNavMeshFollow.setFollowPlayers(false);
                                    enemyNavMeshFollow.setIsStoped(true);
                                    StartCoroutine(StopBeforeSpecialAttack());
                                }
                            }

                        }
                        else
                        {
                            if (_zombieLife > 0)
                            {
                                if (isOnline)
                                {
                                    _target.GetComponent<PhotonView>().RPC("PuxarJogadorRPC", RpcTarget.All, transform.position, 0.2f);
                                }
                                else
                                {
                                    PuxarJogador(0.2f);
                                }

                                if (_canSpecialAttack)
                                {
                                    if(isOnline)
                                        _playerStats.TakeOnlineDamage(damage, false);
                                    else
                                        _playerStats.TakeDamage(damage, false);
                                    if (_playerStats.GetIsDown())
                                    {
                                        enemyNavMeshFollow.setIsStoped(false);
                                        _downedPlayer = false;
                                    }

                                    _currentSpecialAttacksDelay = specialAttacksDelay;
                                    _canSpecialAttack = false;
                                }
                            }
                            else
                            { 
                                if(isOnline)
                                    _playerStats.CapacitateOnlinePlayer();
                                else
                                    _playerStats.CapacitatePlayer();
                            }
                        }

                        break;


                    //Error ================================================================
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }



        //--------------------------------------Aux----------------------------------------


        GameObject GetTarget(GameObject[] players)
        {
            GameObject target = null;
            float minDist = Mathf.Infinity;
            Vector3 currentPos = transform.position;
            foreach (GameObject t in players)
            {
                float dist = Vector3.Distance(t.transform.position, currentPos);
                if (dist < minDist)
                {
                    target = t;
                    minDist = dist;
                }
            }

            _playerStats = target.GetComponent<PlayerStats>();
            enemyNavMeshFollow.setTarget(target);
            if (isOnline && PhotonNetwork.IsMasterClient)
            {
                _targetPhotonViewID = target.GetComponent<PhotonView>().ViewID;
                photonView.RPC("SetManualTarget", RpcTarget.Others, _targetPhotonViewID);
            }
            return target;
        }

        [PunRPC]
        public void SetManualTarget(int targetID)
        {
            GameObject target = PhotonView.Find(targetID).gameObject;
            _playerStats = target.GetComponent<PlayerStats>();
            _targetPhotonViewID = target.GetComponent<PhotonView>().ViewID;
        }

        private Vector3 PosicaoFuga()
        {
            Vector3 posicaoFuga = transform.position;
            float maiorDistancia = 0f;
            foreach (GameObject jogador in _players)
            {
                if (jogador != _target)
                {
                    Vector3 direcaoFuga = (transform.position - jogador.transform.position)
                        .normalized;
                    Vector3 pontoFugaCandidato = transform.position + direcaoFuga * 20;

                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(pontoFugaCandidato, out hit, 20, NavMesh.AllAreas))
                    {
                        float distancia = Vector3.Distance(hit.position,
                            jogador.transform.position);
                        if (distancia > maiorDistancia)
                        {
                            maiorDistancia = distancia;
                            posicaoFuga = hit.position;
                        }
                    }
                }
            }

            return posicaoFuga;
        }

        private void PularNoJogador()
        {
            rb.mass = 0;
            navMeshAgent.enabled = false;
            rb.isKinematic = false;
            Vector3 velocidade = CalculaTrajetoriaParabolica(transform.position, _target.transform.position, 10);
            rb.AddForce(velocidade, ForceMode.VelocityChange);
            _jumpedIntoPlayer = true;
        
        }

        private void Explode()
        {        
            zombieStatus.KillEnemy();
            var position = transform.position;
            if (isOnline)
            {
                PhotonNetwork.Instantiate("bombZombieExplosion", position, Quaternion.identity);
                photonView.RPC("instantiateExplosionEffect", RpcTarget.All, position);
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                Instantiate(coffeeProjectile, position, Quaternion.identity);
                GameObject effectInstantiate = Instantiate(visualEffect, position, Quaternion.identity);
                Destroy(effectInstantiate, 2f);
                Destroy(gameObject);
            }
        
        }

        private Vector3 PontoMedioJogadoresProximos()
        {
            Vector3 somaPosicoes = Vector3.zero;
            int contador = 0;

            foreach (GameObject jogador in _players)
            {
                if (Vector3.Distance(_target.transform.position, jogador.transform.position) <= 15)
                {
                    somaPosicoes += jogador.transform.position;
                    contador++;
                }
            }

            if (contador > 0)
            {
                return somaPosicoes / contador;
            }

            return Vector3.zero;
        }



        private void PuxarJogador(float velocidadePuxar)
        {
            if(fisherHook.gameObject.activeSelf == false)
                fisherHook.gameObject.SetActive(true);
            fisherHook.SetPosition(0, launchPoint.transform.position);
            fisherHook.SetPosition(1, _target.transform.position);
        
            float distanciaJogador = Vector3.Distance(transform.position, _target.transform.position);
            if (distanciaJogador > 4f)
                _target.transform.position = Vector3.Lerp(_target.transform.position,
                    new Vector3(transform.position.x, _target.transform.position.y, transform.position.z),
                    Time.deltaTime * velocidadePuxar);
        }

        private bool PosicaoVisivel()
        {
            RaycastHit hit;
            Vector3 direcao = (_target.transform.position - transform.position).normalized;
            int layerMask = ~LayerMask.GetMask("Enemy");
            if (Physics.Raycast(transform.position, direcao, out hit, specialActivationDistance, layerMask))
            {
                if (hit.collider.gameObject == _target)
                {
                    return true;
                }
            }

            return false;
        }

        [PunRPC]
        void PuxarJogadorRPC(float velocidadePuxar)
        {
        
            if(fisherHook.gameObject.activeSelf == false)
                fisherHook.gameObject.SetActive(true);
            fisherHook.SetPosition(0, launchPoint.transform.position);
            fisherHook.SetPosition(1, _target.transform.position);
        
            if (_target.GetComponent<PhotonView>().IsMine)
            {
                float distanciaJogador = Vector3.Distance(transform.position, _target.transform.position);
                if (distanciaJogador > 4f)
                    _target.transform.position = Vector3.Lerp(_target.transform.position,
                        new Vector3(transform.position.x, _target.transform.position.y, transform.position.z),
                        Time.deltaTime * velocidadePuxar);
            }
        }
        private void FindNewTarget()
        {
            {
                //usa uma lista auxiliar sem o player que morreu para definir um novo target
                GameObject[] aux = new GameObject[_players.Length - 1];
                foreach (GameObject player in _players)
                {
                    int i = 0;
                    PlayerStats statusPlayer = player.GetComponent<PlayerStats>();
                    if (player != _target || !statusPlayer.GetIsIncapacitated() || !statusPlayer.GetIsDown())
                    {
                        aux[i] = player;
                        i++;
                    }
                }
                _target = GetTarget(aux);
            }
        }


        [PunRPC]
        public void instantiateExplosionEffect(Vector3 position)
        {
            GameObject effectInstantiate = Instantiate(visualEffect, position, Quaternion.identity);
            Destroy(effectInstantiate, 2f);
        }
        void OnTriggerEnter(Collider objetoDeColisao)
        {
            switch (_tipoDePoder)
            {
                case SpecialType.Jumper:
                    if (_jumpedIntoPlayer)
                    {

                        if (objetoDeColisao.gameObject == _target)
                        {
                            _downedPlayer = true;
                            if(isOnline)
                                _playerStats.incapacitateOnline(photonView.ViewID);
                            else
                                _playerStats.IncapacitatePlayer(zombieStatus);
                        }
                        else
                        {
                            rb.mass = 4;
                            StartCoroutine(ReativarNavMeshAgentComAtraso(1f));
                        }

                    }

                    break;
            }
        }
    
        private Vector3 CalculaTrajetoriaParabolica(Vector3 origem, Vector3 destino, float alturaMaxima)
        {
            Vector3 direcao = destino - origem;
            float distHorizontal = Mathf.Sqrt(direcao.x * direcao.x + direcao.z * direcao.z);
            float distVertical = destino.y - origem.y;

            float alturaAdicional = Mathf.Clamp(alturaMaxima, 0f, alturaMaxima - distVertical);

            float t = Mathf.Sqrt(-2 * alturaAdicional / Physics.gravity.y);
            float velocidadeVertical = -Physics.gravity.y * t;

            t += Mathf.Sqrt(2 * (distVertical - alturaAdicional) / Physics.gravity.y);
            float velocidadeHorizontal = distHorizontal / t;

            Vector3 velocidade = new Vector3(direcao.x / distHorizontal * velocidadeHorizontal, velocidadeVertical,
                direcao.z / distHorizontal * velocidadeHorizontal);

            return velocidade;
        }


        private IEnumerator ReativarNavMeshAgentComAtraso(float atraso)
        {
            yield return new WaitForSeconds(atraso);
            rb.isKinematic = true;
            _jumpedIntoPlayer = false;
            navMeshAgent.enabled = true;
            enemyNavMeshFollow.enabled = true;
            _currentSpecialAttacksDelay = specialAttacksDelay;
            _canSpecialAttack = false;
        }
    
        private IEnumerator StopBeforeSpecialAttack()
        {
            yield return new WaitForSeconds(delayLockNShoot);
            if (zombieStatus.IsDeadEnemy() == false)
            {
                switch (_tipoDePoder)
                { 
                    case SpecialType.Arremesso:
                        GameObject projétil;
                        if (isOnline)
                        {
                            projétil = PhotonNetwork.Instantiate("CoffeeProjectile", launchPoint.position, Quaternion.identity);
                        }
                        else
                        {
                            projétil = Instantiate(coffeeProjectile, launchPoint.position,
                                Quaternion.identity);
                        }

                        Rigidbody localRigidBody = projétil.GetComponent<Rigidbody>();
                        Vector3 trajetoria =
                            CalculaTrajetoriaParabolica(launchPoint.position, _target.transform.position,
                                40);
                        localRigidBody.AddForce(trajetoria, ForceMode.VelocityChange);
                        _currentSpecialAttacksDelay = specialAttacksDelay;
                        _canSpecialAttack = false;
                        break;


                    case SpecialType.Fisher:
                        if (_playerIsInRange)
                        {
                            _downedPlayer = true;
                            if(isOnline)
                                _playerStats.incapacitateOnline(photonView.ViewID);
                            else
                                _playerStats.IncapacitatePlayer(zombieStatus);
                        }
                        else
                        {
                            enemyNavMeshFollow.setFollowPlayers(true);
                            enemyNavMeshFollow.setIsStoped(false);
                        }

                        break;

                    case SpecialType.Jumper:
                        PularNoJogador();
                        break;
                }
            }

        }


//Getters and Setters ================================================================
        public void SetLife(float life)
        {
            _zombieLife = life;
        }

        public int GetPoints()
        {
            return points;
        }
    
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_zombieLife);
                stream.SendNext(damage);
                stream.SendNext(specialActivationDistance);
                stream.SendNext(runDistance);
                stream.SendNext(specialAttacksDelay);
                stream.SendNext(attackDelay);
                stream.SendNext(delayLockNShoot);
                stream.SendNext(points);
                stream.SendNext(_currentSpecialAttacksDelay);
                stream.SendNext(_currentAttackDelay);
                stream.SendNext(_playerIsInRange);
                stream.SendNext(_canSpecialAttack);
                stream.SendNext(_downedPlayer);
                stream.SendNext(_canAttack);
                stream.SendNext(_jumpedIntoPlayer);
            
            
            
            }
            else
            {
                _zombieLife = (float) stream.ReceiveNext();
                damage = (float) stream.ReceiveNext();
                specialActivationDistance = (float) stream.ReceiveNext();
                runDistance = (float) stream.ReceiveNext();
                specialAttacksDelay = (float) stream.ReceiveNext();
                attackDelay = (float) stream.ReceiveNext();
                delayLockNShoot = (float) stream.ReceiveNext();
                points = (int) stream.ReceiveNext();
                _currentSpecialAttacksDelay = (float) stream.ReceiveNext();
                _currentAttackDelay = (float) stream.ReceiveNext();
                _playerIsInRange = (bool) stream.ReceiveNext();
                _canSpecialAttack = (bool) stream.ReceiveNext();
                _downedPlayer = (bool) stream.ReceiveNext();
                _canAttack = (bool) stream.ReceiveNext();
                _jumpedIntoPlayer = (bool) stream.ReceiveNext();
            }
        }
    
  
    }
}




