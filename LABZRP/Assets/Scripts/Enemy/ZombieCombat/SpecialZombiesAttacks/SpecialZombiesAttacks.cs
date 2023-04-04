using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class SpecialZombiesAttacks : MonoBehaviour
{
    private float zombieLife;
    private EnemyStatus zumbi;
    private GameObject[] players;
    private GameObject alvo;
    private NavMeshAgent navMeshAgent;

    [SerializeField] private GameObject Effect;
    [SerializeField] private float damage = 10f;
    [SerializeField] private GameObject CoffeProjectile;
    [SerializeField] private Transform pontoDeLançamento;
    [SerializeField] private float distanciaAtivacaoPoder = 10f;
    [SerializeField] private float tempoEntreAtaques = 5f;
    
    
    private EnemyFollow enemyFollow;
    private float tempoAtaqueAtual;
    private bool PlayerIsInRange = false;
    private PlayerStats playerStats;
    private bool canAttack;
    private bool downedPlayer;

    private enum SpecialType
    {
        Arremesso,
        Frog,
        Booomer,
        Hunter,
        Smoker
    }
    
    [SerializeField] private SpecialType tipoDePoder;
    
    
    private void Awake()
    {
        enemyFollow = GetComponent<EnemyFollow>();
        players = GameObject.FindGameObjectsWithTag("Player");
        zumbi = GetComponent<EnemyStatus>();
        // Substitua com a lógica para obter a referência do jogador ou alvo.
        tempoAtaqueAtual = tempoEntreAtaques;
        alvo = GetTarget(players);
        navMeshAgent = GetComponent<NavMeshAgent>();
        zombieLife = zumbi.get_life();

    }
    
    
    private void Update()
    {
        if (!playerStats)
        {
            playerStats = alvo.GetComponent<PlayerStats>();
        }
        if (Vector3.Distance(transform.position, alvo.transform.position) < distanciaAtivacaoPoder)
        {
            PlayerIsInRange = true;
        }
        else
        {
            PlayerIsInRange = false;
        }
        
        
        if(tempoAtaqueAtual>0)
            tempoAtaqueAtual -= Time.deltaTime;
        else
            canAttack = true;
        
        
            switch (tipoDePoder)
            {
                    
                        //COFFEE CLASS ================================================================
                        case SpecialType.Arremesso:
                            if (PlayerIsInRange && canAttack)
                            {
                                GameObject projétil = Instantiate(CoffeProjectile, pontoDeLançamento.position,
                                    Quaternion.identity);
                                Rigidbody rb = projétil.GetComponent<Rigidbody>();
                                Vector3 trajetoria =
                                    CalculaTrajetoriaParabolica(pontoDeLançamento.position, alvo.transform.position,
                                        40);
                                rb.AddForce(trajetoria, ForceMode.VelocityChange);
                                canAttack = false;
                                tempoAtaqueAtual = tempoEntreAtaques;
                                enemyFollow.AttackDelay(tempoEntreAtaques);
                            }

                            break; 




                //FROG CLASS ============================================================================================================================================
                
                case SpecialType.Frog:
                    if (PlayerIsInRange && canAttack)
                    {
                        if (!downedPlayer && !playerStats.getIsIncapacitated())
                        {
                            playerStats.IncapacitatePlayer();
                            playerStats.takeDamage(damage);
                            alvo.transform.SetParent(transform);
                            alvo.transform.localPosition = Vector3.zero;
                            downedPlayer = true;
                            alvo.GetComponent<CharacterController>().enabled = false;

                            zumbi.getEnemyFollow().setFollowPlayers(false);
                        }
                        else
                        {
                            navMeshAgent.speed = 5;
                            if (!zumbi.isDeadEnemy())
                            {
                                if (canAttack)
                                {
                                    playerStats.takeDamage(damage);
                                    tempoAtaqueAtual = tempoEntreAtaques;
                                    canAttack = false;
                                }

                                if (playerStats.verifyDown())
                                {
                                    alvo.transform.SetParent(null);
                                    downedPlayer = false;
                                    alvo.transform.position = new Vector3(alvo.transform.position.x, 59,
                                        alvo.transform.position.z);
                                    zumbi.getEnemyFollow().setFollowPlayers(true);
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

                                navMeshAgent.SetDestination(posicaoFuga);

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

                    break;
                
                
                
                //BOOOMER CLASS ================================================================
                case SpecialType.Booomer:
                    Debug.Log(zombieLife);
                    if (zombieLife <= 0)
                    {
                        Explode();
                    }
                    else
                    {
                        Vector3 pontoMedio = PontoMedioJogadoresProximos();
                        if (pontoMedio != Vector3.zero)
                        {
                            Debug.Log("Entrou no ponto medio");
                            enemyFollow.setFollowPlayers(false);
                            navMeshAgent.SetDestination(pontoMedio);
                            if (Vector3.Distance(transform.position, pontoMedio) <= distanciaAtivacaoPoder)
                            {
                                Explode();
                            }
                        }
                        else
                        {
                            enemyFollow.setFollowPlayers(true);
                        }
                        if (Vector3.Distance(transform.position, alvo.transform.position) <= distanciaAtivacaoPoder)
                        {
                            enemyFollow.setFollowPlayers(true);
                            Explode();
                        }
                    }
                    
                    
                    
                    
                    
                    
                    
                    
                    break;
                
                
                
                
                //HUNTER CLASS ================================================================
                case SpecialType.Hunter:
                    break;
                //SMOKER CLASS ================================================================
                case SpecialType.Smoker:
                    break;
                
                
                
                
                //Error ================================================================
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    


    //--------------------------------------Aux----------------------------------------
    
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

        playerStats = target.GetComponent<PlayerStats>();
        return target;
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

        Vector3 velocidade = new Vector3(direcao.x / distHorizontal * velocidadeHorizontal, velocidadeVertical, direcao.z / distHorizontal * velocidadeHorizontal);

        return velocidade;
    }

    private void Explode()
    {
        Instantiate(CoffeProjectile, transform.position, Quaternion.identity);
        Instantiate(Effect, transform.position, Quaternion.identity);
        Destroy(Effect, 2f);
        Destroy(gameObject);
    }
    
    private Vector3 PontoMedioJogadoresProximos()
    { 
        Vector3 somaPosicoes = Vector3.zero;
        int contador = 0;

        foreach (GameObject jogador in players)
        {
            if (Vector3.Distance(alvo.transform.position, jogador.transform.position) <=10)
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
    
    
    
    
    
    
    
    
//Getters and Setters ================================================================
    public void setLife(float life)
    {
        zombieLife = life;
    }
}

    




