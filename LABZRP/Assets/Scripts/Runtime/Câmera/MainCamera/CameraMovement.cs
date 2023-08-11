using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace Runtime.Câmera.MainCamera
{
    public class CameraMovement : MonoBehaviourPunCallbacks
    {
        public List<Transform> targets; // Lista de objetos a seguir
        public Vector3 offset; // Distância da câmera aos objetos
        public float smoothTime = 0.5f; // Suavização do movimento da câmera
        public float defaultCenterY = 10f; // Altura padrão do centro Y
        public float defaultCenterYOnline = 20f;
        public float distanceThreshold = 10f; // Distância mínima entre os jogadores para aumentar o centro Y
        public float heightIncrease = 5f; // Altura a ser adicionada ao centro Y se os jogadores estiverem distantes
        public bool isOnline = false;
        public bool followOnlyClientPlayer = false;
        private bool showingOnlyMyPlayer = false;
        private Transform clientPlayer;
        private Vector3 velocity;

    
        private void FixedUpdate()
        {
            // Se não há alvos, saia da função
            if (targets.Count == 0) return;

            if (!showingOnlyMyPlayer)
            {
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
                    if (isOnline)
                    {
                        center.y = defaultCenterYOnline + heightIncrease;

                    }
                    else
                    {
                        center.y = defaultCenterY + heightIncrease;

                    }
                }
                else
                {
                    if (isOnline)
                    {
                        center.y = defaultCenterYOnline;

                    }
                    else
                    {
                        center.y = defaultCenterY;

                    }
                }

                // Calcule a nova posição da câmera com base na posição média
                Vector3 targetPosition = center + offset;

                // Suavize o movimento da câmera
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            }
            else
            {
                Vector3 targetPosition = clientPlayer.position + offset;
                transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
            }
        }
    
    
        public void addPlayer (GameObject player)
        {
            targets.Add(player.transform);
            if (PhotonNetwork.IsConnected)
            {
                if (clientPlayer == null)
                {
                    if (player.GetComponent<PhotonView>().IsMine)
                    {
                        clientPlayer = player.transform;
                    }
                }
                else
                {
                    if(followOnlyClientPlayer)
                        showingOnlyMyPlayer = true;
                }
            }
        }
    
        public void removePlayer (GameObject player)
        {
            if (PhotonNetwork.IsConnected)
            {
                if (showingOnlyMyPlayer)
                {
                    if (player.transform == clientPlayer.transform)
                    {
                        showingOnlyMyPlayer = false;
                    }
                }
            }
            targets.Remove(player.transform);
        }

        public void setIsOnline(bool isOnline)
        {
            this.isOnline = isOnline;
        }

    }
}