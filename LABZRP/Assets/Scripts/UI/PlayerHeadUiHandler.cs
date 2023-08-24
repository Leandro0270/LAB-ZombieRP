using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

namespace UI
{
    public class PlayerHeadUiHandler : MonoBehaviourPunCallbacks
    {
        [SerializeField] private int playerHeadIndex;
        [SerializeField] private Texture outPutImage;
        [SerializeField] private GameObject idleEyes;
        [SerializeField] private MeshRenderer idleEyesMeshRenderer;
        [SerializeField] private GameObject damageEyes;
        [SerializeField] private MeshRenderer damageEyesMeshRenderer;
        [SerializeField] private GameObject deadEyes;
        [SerializeField] private MeshRenderer deadEyesMeshRenderer;
        [SerializeField] private MeshRenderer playerSkinMeshRenderer;
        [SerializeField] private List<GameObject> availablePlayerHairs;
        [SerializeField] private List<GameObject> availablePlayerAccessories;
        [SerializeField] private float timeToResetDamageEyes = 1f;
        private int _playerIndex;
        private GameObject _selectedPlayerHair;
        private MeshRenderer _selectedPlayerHairMeshRenderer;
        private List<GameObject> _selectedPlayerAccessories;
        private Coroutine _damageCoroutine;
        private static bool IsOnline => PhotonNetwork.IsConnected;

        public Texture GetOutPutImage()
        {
            return outPutImage;
        }
    
        [PunRPC]
        public void TakeDamage()
        {
            if(IsOnline && photonView.IsMine)
                photonView.RPC("TakeDamage", RpcTarget.Others);
        
            if(_damageCoroutine != null)
            {
                StopCoroutine(_damageCoroutine);
            }

            _damageCoroutine = StartCoroutine(ShowDamageEyes());
        }

        private IEnumerator ShowDamageEyes()
        {
            idleEyes.SetActive(false);
            damageEyes.SetActive(true);

            yield return new WaitForSeconds(timeToResetDamageEyes);

            damageEyes.SetActive(false);
            idleEyes.SetActive(true);
        }

        [PunRPC]
        public void DownPlayer(bool downed)
        {
            if(IsOnline && photonView.IsMine)
                photonView.RPC("DownPlayer", RpcTarget.Others, downed);
        
            if (downed)
            {
                if (_damageCoroutine != null)
                    StopCoroutine(_damageCoroutine);

                idleEyes.SetActive(false);
                damageEyes.SetActive(false);
                deadEyes.SetActive(true);
            }
            else
            {
                StopCoroutine(_damageCoroutine);
                idleEyes.SetActive(true);
                damageEyes.SetActive(false);
                deadEyes.SetActive(false);
            }
        }
    
        public void SetPlayerSkinSpecs(ScObPlayerCustom playerCustom)
        {
            idleEyesMeshRenderer.material = playerCustom.Eyes;
            damageEyesMeshRenderer.material = playerCustom.Eyes;
            deadEyesMeshRenderer.material = playerCustom.Eyes;
            playerSkinMeshRenderer.material = playerCustom.Skin;
            //Ainda n temos cabelo, mas quando tivermos trabalhar neste ponto ===================================================================================
            // _selectedPlayerHairMeshRenderer.material = playerCustom.Hair;
            // foreach (var accessory in _selectedPlayerAccessories)
            // {
            //     accessory.SetActive(false);
            // }
            //
            // foreach (var accessory in playerCustom.Accessories)
            // {
            //     accessory.SetActive(true);
            // }
            // _selectedPlayerAccessories = playerCustom.Accessories;
        }
    }
}
