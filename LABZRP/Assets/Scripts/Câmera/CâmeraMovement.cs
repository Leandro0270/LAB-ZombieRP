using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CâmeraMovement : MonoBehaviour
{
    public List<Transform> targets; // Lista de objetos a seguir
    public Vector3 offset; // Distância da câmera aos objetos
    public float smoothTime = 0.5f; // Suavização do movimento da câmera
    public float defaultCenterY = 10f; // Altura padrão do centro Y
    public float distanceThreshold = 10f; // Distância mínima entre os jogadores para aumentar o centro Y
    public float heightIncrease = 5f; // Altura a ser adicionada ao centro Y se os jogadores estiverem distantes

    private Vector3 velocity;

    private void Start()
    {
        // Encontre todos os objetos com a tag "Player" e adicione à lista de alvos
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            targets.Add(player.transform);
        }
    }

    private void FixedUpdate()
    {
        // Se não há alvos, saia da função
        if (targets.Count == 0) return;

        // Calcule a posição média dos alvos
        Vector3 center = Vector3.zero;
        foreach (Transform target in targets)
        {
            center += target.position;
        }
        center /= targets.Count;

        // Verifique se os jogadores estão distantes
        bool playersAreFar = false;
        foreach (Transform target in targets)
        {
            if (Vector3.Distance(target.position, center) > distanceThreshold)
            {
                playersAreFar = true;
                break;
            }
        }

        // Se os jogadores estiverem distantes, aumente o centro Y
        if (playersAreFar)
        {
            center.y = defaultCenterY + heightIncrease;
        }
        else
        {
            center.y = defaultCenterY;
        }

        // Calcule a nova posição da câmera com base na posição média
        Vector3 targetPosition = center + offset;

        // Suavize o movimento da câmera
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
    
    
    public void addPlayer (GameObject player)
    {
        targets.Add(player.transform);
    }
    
    public void removePlayer (GameObject player)
    {
        targets.Remove(player.transform);
    }
    

}