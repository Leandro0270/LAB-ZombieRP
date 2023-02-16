using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CâmeraMovement : MonoBehaviour
{
    public List<Transform> targets; // Lista de objetos a seguir
    public Vector3 offset; // Distância da câmera aos objetos
    public float smoothTime = 0.5f; // Suavização do movimento da câmera

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

        // Calcule a nova posição da câmera com base na posição média
        Vector3 targetPosition = center + offset;
        targetPosition.y = transform.position.y;

        // Suavize o movimento da câmera
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}