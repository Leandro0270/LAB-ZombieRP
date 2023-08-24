using System;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using Cinemachine;
using UnityEngine.Serialization;

namespace Runtime.Câmera.MainCamera
{
    public class CmGameplay : MonoBehaviourPunCallbacks
    {
        [Header("Cinemachine Configuration")]
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        [SerializeField] private CinemachineTargetGroup targetGroup;
        private CinemachineFramingTransposer _framingTransposer;
        [Header("Camera Configuration")]
        [SerializeField] private float minOffsetY = 60f;
        [SerializeField] private float maxOffsetY = 1000f;
        [SerializeField] private float smoothTime = 0.5f;
        [SerializeField] private float minFov = 60f;
        [SerializeField] private float maxFov = 90f;
        private List<Transform> _currentTargets = new List<Transform>();
        private List<Transform> _players = new List<Transform>();
        private Transform _clientPlayerTransform;
        private static bool IsOnline => PhotonNetwork.IsConnected;

        private void Start()
        {
            _framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if(_framingTransposer == null) throw new Exception("Framing Transposer is null");
            _framingTransposer.m_MinimumFOV = minFov;
            _framingTransposer.m_MaximumFOV = maxFov;
            _framingTransposer.m_MinimumDistance = minOffsetY;
            _framingTransposer.m_MaximumDistance = maxOffsetY;
            _framingTransposer.m_XDamping = smoothTime;
            _framingTransposer.m_YDamping = smoothTime;
            _framingTransposer.m_ZDamping = smoothTime;
            
        }

        public void StartMatchAddPlayer(Transform player)
        {
            _players.Add(player);
            if (IsOnline)
            {
                if (!player.gameObject.GetPhotonView().IsMine) return;
                _clientPlayerTransform = player;
                _currentTargets.Add(_clientPlayerTransform);
                targetGroup.AddMember(_clientPlayerTransform, 1, 2);

            }
            else
            {
                _currentTargets.Add(player);
                targetGroup.AddMember(player, 1, 2);
            }
        }

        public void RemoveTargetPlayer(Transform player)
        {
            _currentTargets.Remove(player);
            targetGroup.RemoveMember(player);
            
            if (!IsOnline) return;
            
            if (player != _clientPlayerTransform) return;
            
            foreach (var playerToTarget in _players)
            {
                if (playerToTarget == _clientPlayerTransform) continue;
                _currentTargets.Add(playerToTarget);
                targetGroup.AddMember(playerToTarget, 1, 2);
            }

        }
        
        public void AddTargetPlayer(Transform player)
        {
            if (IsOnline)
            {
                if(player != _clientPlayerTransform) return;
                foreach (var playerTargetToRemove in _currentTargets)
                {
                    _currentTargets.Remove(playerTargetToRemove);
                    targetGroup.RemoveMember(playerTargetToRemove);
                }
            }
            _currentTargets.Add(player);
            targetGroup.AddMember(player, 1, 2);
        }
        
    }
}
