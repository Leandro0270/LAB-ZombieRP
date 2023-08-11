using System.Collections.Generic;
using Photon.Pun;
using Runtime.Player.Combat.PlayerStatus;
using Runtime.Player.Points;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Runtime.Enemy.HorderMode
{
    public class HordeModeGameOverManager : MonoBehaviourPunCallbacks
    {
        [SerializeField] private Button returnMenuButton;
        [SerializeField] private GameObject returnMasterClientText;
        [SerializeField] private GameObject playerGameOverUI;
        [SerializeField] GameObject gameOverUI;
        [SerializeField] MainGameManager mainGameManager;
        private GameObject GameInstanceConfiguration;
        private List<GameObject> players = new List<GameObject>();
        private int lastHorde = 0;
        private bool isOnline = false;
        [SerializeField] private TextMeshProUGUI _titleText;

        public void gameOver()
        {
            GameInstanceConfiguration = mainGameManager.getPlayerConfigurationManager();
            lastHorde = mainGameManager.getHordeManager().getCurrentHorde();
            players = mainGameManager.getPlayers();
            string text = "Você sobreviveu até a horda " + (lastHorde+1);
            _titleText.text = text;
            if (isOnline && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("updateHordeText", RpcTarget.Others, text);
            }
            foreach (var player in players)
            {
                GameObject IntantiatedPlayerStats = Instantiate(playerGameOverUI, gameOverUI.transform);
                HordeModeGameOverPlayer hordeModeGameOverPlayer = IntantiatedPlayerStats.GetComponent<HordeModeGameOverPlayer>();
                hordeModeGameOverPlayer.nickname.text = player.GetComponent<PlayerStats>().GetPlayerName();
                hordeModeGameOverPlayer.points.text = player.GetComponent<PlayerPoints>().getTotalPointsInGame().ToString();
                hordeModeGameOverPlayer.kills.text = player.GetComponent<WeaponSystem>().getTotalKilledZombies().ToString();
                hordeModeGameOverPlayer.downs.text = player.GetComponent<ReviveScript>().getDownCount().ToString();
                hordeModeGameOverPlayer.revives.text = player.GetComponent<ReviveScript>().getReviveCount().ToString();
            }

            if (!isOnline || PhotonNetwork.IsMasterClient)
            {
                returnMenuButton.gameObject.SetActive(true);
                returnMasterClientText.SetActive(false);
                returnMenuButton.Select();
            }
            else
            {
                returnMenuButton.gameObject.SetActive(false);
                returnMasterClientText.SetActive(true);
            }

            if (isOnline)
            {
                PhotonNetwork.Destroy(GameInstanceConfiguration);
            }
        

        }

        public void backToMenu()
        {
            if (isOnline)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (GameInstanceConfiguration != null)
                        Destroy(GameInstanceConfiguration);
                    SceneManager.LoadScene("PlayerSetupOnline");
                }
                else
                {
                    photonView.RPC("backToMenuRPC", RpcTarget.Others);
                }
            }
            else
            {
                Destroy(GameInstanceConfiguration);
                SceneManager.LoadScene("PlayerSetupLocalMultiplayer");

            }
        }


        [PunRPC]
        public void updateHordeText(string text)
        {
            _titleText.text = text;
        }
        [PunRPC]
        public void backToMenuRPC()
        {
            if (GameInstanceConfiguration != null)
                Destroy(GameInstanceConfiguration);
            SceneManager.LoadScene("PlayerSetupOnline");

        }
    
        public void setIsOnline(bool isOnline)
        {
            this.isOnline = isOnline;
        }
    }
}
