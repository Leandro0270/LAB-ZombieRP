using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;


public class SpecialZombiesAttacks : MonoBehaviourPunCallbacks
{
    private float zombieLife;
    private EnemyStatus zumbi;
    private GameObject[] players;
    private GameObject alvo;
    private NavMeshAgent navMeshAgent;
    private Rigidbody rb;
    [SerializeField] private PhotonView _photonView;
    [SerializeField] private GameObject Effect;
    [SerializeField] private float damage = 10f;
    [SerializeField] private GameObject CoffeProjectile;
    [SerializeField] private Transform pontoDeLançamento;
    [SerializeField] private float distanciaAtivacaoPoder = 10f;
    [SerializeField] private float distanciaFuga = 20f;
    [SerializeField] private float tempoEntrePoderes = 5f;
    [SerializeField] private float tempoEntreAtaques = 2f;
    [SerializeField] private float delayLocknShoot = 2f;
    [SerializeField] private int points = 40;

    private EnemyFollow enemyFollow;
    private float tempoPoderesAtual;
    private float tempoAtaqueAtual;
    private bool PlayerIsInRange = false;
    private PlayerStats playerStats;
    private bool canSpecialAttack = true;
    private bool canAttack = true;
    private bool downedPlayer;
    private bool auxBool = false;
    [SerializeField] private bool isOnline = false;
    private int targetPhotonViewID;

    private enum SpecialType
    {
        Arremesso,
        Frog,
        Booomer,
        Jumper,
        Fisher
    }

    [SerializeField] private SpecialType tipoDePoder;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemyFollow = GetComponent<EnemyFollow>();
        zumbi = GetComponent<EnemyStatus>();
        players = GameObject.FindGameObjectsWithTag("Player");
        
        tempoPoderesAtual = tempoEntrePoderes;
        alvo = GetTarget(players);
        navMeshAgent = GetComponent<NavMeshAgent>();
        zombieLife = zumbi.get_life();
        enemyFollow.setIsSpecial(true);

    }


    private void Update()
    {
        if (PhotonNetwork.IsMasterClient || !isOnline)
        {
            if (!playerStats)
                playerStats = alvo.GetComponent<PlayerStats>();


            if (Vector3.Distance(transform.position, alvo.transform.position) < distanciaAtivacaoPoder)
                PlayerIsInRange = true;
            else
                PlayerIsInRange = false;


            if (tempoAtaqueAtual > 0)
                tempoAtaqueAtual -= Time.deltaTime;
            else
                canAttack = true;

            if (tempoPoderesAtual > 0)
                tempoPoderesAtual -= Time.deltaTime;
            else
                canSpecialAttack = true;


            if ((playerStats.getIsIncapacitated() && !downedPlayer) || playerStats.verifyDown())
            {
                FindNewTarget();
                enemyFollow.setTarget(alvo);
            }




            switch (tipoDePoder)
            {

                //COFFEE CLASS ================================================================
                case SpecialType.Arremesso:
                    if (!zumbi.isDeadEnemy())
                    {
                        if (!PlayerIsInRange)
                        {
                            enemyFollow.setIsStoped(false);
                            enemyFollow.setFollowPlayers(true);
                        }
                        else
                        {
                            if (Vector3.Distance(transform.position, alvo.transform.position) > distanciaFuga)
                            {
                                enemyFollow.setIsStoped(true);
                                enemyFollow.setFollowPlayers(false);
                                if (canSpecialAttack && !zumbi.isDeadEnemy())
                                {
                                    tempoPoderesAtual = tempoEntrePoderes;
                                    canSpecialAttack = false;
                                    StartCoroutine(StopBeforeSpecialAttack());
                                }
                            }
                            else
                            {
                                enemyFollow.setIsStoped(false);
                                navMeshAgent.SetDestination(posicaoFuga());
                            }
                        }
                    }
                    else
                    {
                        enemyFollow.setIsStoped(true);
                        enemyFollow.setFollowPlayers(false);
                    }

                    break;

                //FROG CLASS ============================================================================================================================================

                case SpecialType.Frog:
                    if (PlayerIsInRange && canSpecialAttack)
                    {
                        if (!downedPlayer && !playerStats.getIsIncapacitated())
                        {
                            if (isOnline)
                            {
                                playerStats.incapacitateOnline(photonView.ViewID);
                                playerStats.takeOnlineDamage(damage);
                            }
                            else
                            {
                                playerStats.IncapacitatePlayer(gameObject);
                                playerStats.takeDamage(damage);
                            }

                            if (isOnline)
                            {
                                playerStats.changeTransformParent(photonView.ViewID, true, false);
                            }
                            else
                            {
                                alvo.transform.SetParent(transform);
                                alvo.transform.localPosition = Vector3.zero;
                            }
                            downedPlayer = true;
                            if(isOnline)
                                playerStats.OnlineCharacterController(false);
                            else
                                alvo.GetComponent<CharacterController>().enabled = false;

                            enemyFollow.setFollowPlayers(false);
                        }
                        else
                        {
                            navMeshAgent.speed = 5;
                            if (!zumbi.isDeadEnemy())
                            {
                                if (canAttack)
                                {
                                    if (isOnline)
                                    {
                                        playerStats.takeOnlineDamage(damage);
                                    }
                                    else
                                    {
                                        playerStats.takeDamage(damage);
                                    }

                                    tempoAtaqueAtual = tempoEntreAtaques;
                                    canAttack = false;
                                }

                                if (playerStats.verifyDown())
                                {
                                    if (isOnline)
                                    {
                                        playerStats.changeTransformParent(0,false,true);
                                    }
                                    else
                                    {
                                        alvo.transform.SetParent(null);
                                        downedPlayer = false;
                                        alvo.transform.position = new Vector3(alvo.transform.position.x, 59,
                                            alvo.transform.position.z);
                                    }
                                    
                                    enemyFollow.setFollowPlayers(true);
                                    GameObject[] aux = new GameObject[players.Length - 1];
                                    foreach (GameObject player in players)
                                    {
                                        int i = 0;
                                        if (player != alvo)
                                        {
                                            aux[i] = player;
                                            i++;
                                        }
                                    }

                                    alvo = GetTarget(aux);

                                }

                                navMeshAgent.SetDestination(posicaoFuga());
                            }
                            else
                            {
                                if (isOnline)
                                {
                                    playerStats.changeTransformParent(0,false,true);
                                    playerStats.CapacitateOnlinePlayer();

                                }
                                else
                                {
                                    alvo.transform.SetParent(null);
                                    alvo.transform.position = new Vector3(alvo.transform.position.x, 59,
                                        alvo.transform.position.z);
                                    playerStats.CapacitatePlayer();
                                }
                            }
                        }
                    }

                    break;



                //BOOOMER CLASS ================================================================
                case SpecialType.Booomer:
                    enemyFollow.setIsStoped(false);
                    if (zombieLife <= 0)
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
                            enemyFollow.setFollowPlayers(false);

                            navMeshAgent.SetDestination(pontoMedio);
                        }
                        else
                        {
                            posFinal = alvo.transform.position;
                            enemyFollow.setFollowPlayers(true);
                        }

                        if (Vector3.Distance(transform.position, posFinal) <= distanciaAtivacaoPoder)
                        {
                            Explode();
                        }
                    }

                    break;




                //HUNTER CLASS ================================================================
                case SpecialType.Jumper:

                    if (!zumbi.isDeadEnemy())
                    {

                        if (!auxBool && !playerStats.getIsIncapacitated() && !downedPlayer && canSpecialAttack)
                        {
                            if (PlayerIsInRange)
                            {
                                enemyFollow.enabled = false;
                                StartCoroutine(StopBeforeSpecialAttack());
                            }
                        }

                        if (downedPlayer)
                        {
                            if (playerStats.verifyDown())
                            {
                                downedPlayer = false;
                                StartCoroutine(ReativarNavMeshAgentComAtraso(1f));
                            }
                            else
                            {

                                if (canAttack)
                                {
                                    if (isOnline)
                                    {
                                        playerStats.takeOnlineDamage(damage);
                                    }
                                    else
                                    {
                                        playerStats.takeDamage(damage);

                                    }
                                    tempoAtaqueAtual = tempoEntreAtaques;
                                    canAttack = false;
                                }
                            }
                        }

                    }

                    else
                    {
                        if (downedPlayer && !playerStats.verifyDown())
                            if (isOnline)
                            {
                                playerStats.CapacitateOnlinePlayer();
                            }
                            else
                            {
                                playerStats.CapacitatePlayer();
                            }
                    }

                    break;
                //SMOKER CLASS ================================================================
                case SpecialType.Fisher:
                    if (!downedPlayer)
                    {
                        if (!playerStats.getIsIncapacitated())
                        {
                            if (PosicaoVisivel())
                            {
                                enemyFollow.setFollowPlayers(false);
                                enemyFollow.setIsStoped(true);
                                StartCoroutine(StopBeforeSpecialAttack());
                            }
                        }

                    }
                    else
                    {
                        if (zombieLife > 0)
                        {
                            if (isOnline)
                            {
                                alvo.GetComponent<PhotonView>().RPC("PuxarJogadorRPC", RpcTarget.All, transform.position, 0.2f);
                            }
                            else
                            {
                                PuxarJogador(0.2f);

                            }

                            if (canSpecialAttack)
                            {
                                if(isOnline)
                                    playerStats.takeOnlineDamage(damage);
                                else
                                    playerStats.takeDamage(damage);
                                if (playerStats.verifyDown())
                                {
                                    enemyFollow.setIsStoped(false);
                                    downedPlayer = false;
                                }

                                tempoPoderesAtual = tempoEntrePoderes;
                                canSpecialAttack = false;
                            }
                        }
                        else
                        { 
                            if(isOnline)
                                playerStats.CapacitateOnlinePlayer();
                            else
                                playerStats.CapacitatePlayer();
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

        playerStats = target.GetComponent<PlayerStats>();
        enemyFollow.setTarget(target);
        targetPhotonViewID = target.GetComponent<PhotonView>().ViewID;
        return target;
    }


    private Vector3 posicaoFuga()
    {
        Vector3 posicaoFuga = transform.position;
        float maiorDistancia = 0f;
        foreach (GameObject jogador in players)
        {
            if (jogador.transform != alvo)
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
        Vector3 velocidade = CalculaTrajetoriaParabolica(transform.position, alvo.transform.position, 10);
        rb.AddForce(velocidade, ForceMode.VelocityChange);
        auxBool = true;
        
    }

    private void Explode()
    {        
        zumbi.killEnemy();
        var position = transform.position;
        if (isOnline)
        {
            PhotonNetwork.Instantiate("bombZombieExplosion", position, Quaternion.identity);
            GameObject effectInstantiate = PhotonNetwork.Instantiate("bombZombieExplosionEffect", position, Quaternion.identity);
            Destroy(effectInstantiate, 2f);
            PhotonNetwork.Destroy(gameObject);
        }
        else
        {
            Instantiate(CoffeProjectile, position, Quaternion.identity);
            GameObject effectInstantiate = Instantiate(Effect, position, Quaternion.identity);
            Destroy(effectInstantiate, 2f);
            Destroy(gameObject);
        }
        
    }

    private Vector3 PontoMedioJogadoresProximos()
    {
        Vector3 somaPosicoes = Vector3.zero;
        int contador = 0;

        foreach (GameObject jogador in players)
        {
            if (Vector3.Distance(alvo.transform.position, jogador.transform.position) <= 15)
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
        
        float distanciaJogador = Vector3.Distance(transform.position, alvo.transform.position);
        if (distanciaJogador > 4f)
            alvo.transform.position = Vector3.Lerp(alvo.transform.position,
                new Vector3(transform.position.x, alvo.transform.position.y, transform.position.z),
                Time.deltaTime * velocidadePuxar);
    }

    private bool PosicaoVisivel()
    {
        RaycastHit hit;
        Vector3 direcao = (alvo.transform.position - transform.position).normalized;
        int layerMask = ~LayerMask.GetMask("Enemy");
        if (Physics.Raycast(transform.position, direcao, out hit, distanciaAtivacaoPoder, layerMask))
        {
            if (hit.collider.gameObject == alvo)
            {
                return true;
            }
        }

        return false;
    }

    [PunRPC]
    void PuxarJogadorRPC(Vector3 position, float velocidadePuxar)
    {
        float distanciaJogador = Vector3.Distance(transform.position, position);
        if (distanciaJogador > 4f)
            transform.position = Vector3.Lerp(transform.position,
                new Vector3(position.x, transform.position.y, position.z),
                Time.deltaTime * velocidadePuxar);
    }
    private void FindNewTarget()
    {
        {
            //usa uma lista auxiliar sem o player que morreu para definir um novo target
            GameObject[] aux = new GameObject[players.Length - 1];
            foreach (GameObject player in players)
            {
                int i = 0;
                PlayerStats _statusPlayer = player.GetComponent<PlayerStats>();
                if (player != alvo || !_statusPlayer.getIsIncapacitated() || !_statusPlayer.verifyDown())
                {
                    aux[i] = player;
                    i++;
                }
            }

            alvo = GetTarget(aux);
        }
    }


    void OnTriggerEnter(Collider objetoDeColisao)
    {
        switch (tipoDePoder)
        {
            case SpecialType.Jumper:
                if (auxBool)
                {

                    if (objetoDeColisao.gameObject == alvo)
                    {
                        downedPlayer = true;
                        if(isOnline)
                            playerStats.incapacitateOnline(photonView.ViewID);
                        else
                            playerStats.IncapacitatePlayer(gameObject);
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
        auxBool = false;
        navMeshAgent.enabled = true;
        enemyFollow.enabled = true;
        tempoPoderesAtual = tempoEntrePoderes;
        canSpecialAttack = false;
    }
    
    private IEnumerator StopBeforeSpecialAttack()
    {
        yield return new WaitForSeconds(delayLocknShoot);
        if (zumbi.isDeadEnemy() == false)
        {
            switch (tipoDePoder)
            { 
                case SpecialType.Arremesso:
                    GameObject projétil;
                    if (isOnline)
                    {
                        projétil = PhotonNetwork.Instantiate("CoffeeProjectile", pontoDeLançamento.position, Quaternion.identity);
                    }
                    else
                    {
                        projétil = Instantiate(CoffeProjectile, pontoDeLançamento.position,
                            Quaternion.identity);
                    }

                    Rigidbody rb = projétil.GetComponent<Rigidbody>();
                    Vector3 trajetoria =
                        CalculaTrajetoriaParabolica(pontoDeLançamento.position, alvo.transform.position,
                            40);
                    rb.AddForce(trajetoria, ForceMode.VelocityChange);
                    tempoPoderesAtual = tempoEntrePoderes;
                    canSpecialAttack = false;
                    break;


                case SpecialType.Fisher:
                    if (PlayerIsInRange)
                    {
                        downedPlayer = true;
                        if(isOnline)
                            playerStats.incapacitateOnline(photonView.ViewID);
                        else
                            playerStats.IncapacitatePlayer(gameObject);
                    }
                    else
                    {
                        enemyFollow.setFollowPlayers(true);
                        enemyFollow.setIsStoped(false);
                    }

                    break;

                case SpecialType.Jumper:
                    PularNoJogador();
                    break;
            }
        }

    }


//Getters and Setters ================================================================
    public void setLife(float life)
    {
        zombieLife = life;
    }

    public int getPoints()
    {
        return points;
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(zombieLife);
            stream.SendNext(damage);
            stream.SendNext(distanciaAtivacaoPoder);
            stream.SendNext(distanciaFuga);
            stream.SendNext(tempoEntrePoderes);
            stream.SendNext(tempoEntreAtaques);
            stream.SendNext(delayLocknShoot);
            stream.SendNext(points);
            stream.SendNext(tempoPoderesAtual);
            stream.SendNext(tempoAtaqueAtual);
            stream.SendNext(PlayerIsInRange);
            stream.SendNext(canSpecialAttack);
            stream.SendNext(downedPlayer);
            stream.SendNext(canAttack);
            stream.SendNext(auxBool);
            
            
            
        }
        else
        {
            zombieLife = (float) stream.ReceiveNext();
            damage = (float) stream.ReceiveNext();
            distanciaAtivacaoPoder = (float) stream.ReceiveNext();
            distanciaFuga = (float) stream.ReceiveNext();
            tempoEntrePoderes = (float) stream.ReceiveNext();
            tempoEntreAtaques = (float) stream.ReceiveNext();
            delayLocknShoot = (float) stream.ReceiveNext();
            points = (int) stream.ReceiveNext();
            tempoPoderesAtual = (float) stream.ReceiveNext();
            tempoAtaqueAtual = (float) stream.ReceiveNext();
            PlayerIsInRange = (bool) stream.ReceiveNext();
            canSpecialAttack = (bool) stream.ReceiveNext();
            downedPlayer = (bool) stream.ReceiveNext();
            canAttack = (bool) stream.ReceiveNext();
            auxBool = (bool) stream.ReceiveNext();
        }
    }
    
  
}




