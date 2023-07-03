using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HordeModeGameOverManager : MonoBehaviour
{
    [SerializeField] private GameObject playerGameOverUI;
    [SerializeField] GameObject gameOverUI;
    [SerializeField] MainGameManager mainGameManager;
    private GameObject GameInstanceConfiguration;
    private List<GameObject> players = new List<GameObject>();
    private int lastHorde = 0;
    [SerializeField] private TextMeshProUGUI _titleText;

    public void gameOver()
    {
        GameInstanceConfiguration = mainGameManager.getPlayerConfigurationManager();
        lastHorde = mainGameManager.getHordeManager().getCurrentHorde();
        players = mainGameManager.getPlayers();
        _titleText.text = "Você sobreviveu até a horda " + (lastHorde+1);
        foreach (var player in players)
        {
            GameObject IntantiatedPlayerStats = Instantiate(playerGameOverUI, gameOverUI.transform);
            HordeModeGameOverPlayer hordeModeGameOverPlayer = IntantiatedPlayerStats.GetComponent<HordeModeGameOverPlayer>();
            hordeModeGameOverPlayer.name.text = player.GetComponent<PlayerStats>().getPlayerName();
            hordeModeGameOverPlayer.points.text = player.GetComponent<PlayerPoints>().getTotalPointsInGame().ToString();
            hordeModeGameOverPlayer.kills.text = player.GetComponent<WeaponSystem>().getTotalKilledZombies().ToString();
            hordeModeGameOverPlayer.downs.text = player.GetComponent<ReviveScript>().getDownCount().ToString();
            hordeModeGameOverPlayer.revives.text = player.GetComponent<ReviveScript>().getReviveCount().ToString();
        }
    }

    public void backToMenu()
    {
        Destroy(GameInstanceConfiguration);
        SceneManager.LoadScene("PlayerSetupLocalMultiplayer");

    }
}
