using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class OnlineLobbyPlayersShower : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI playerStatusText;
    [SerializeField] private TextMeshProUGUI playerReadyText;
    [SerializeField] private CustomizeSkinMenu playerPrefab;
    [SerializeField] private GameObject playerModel;
    [SerializeField] private Animator playerModelAnimator;
    private ScObPlayerCustom playerCustom;
    

    private int PlayerIndex;
    private bool isReady;
    private string name;
    
    public void setPlayerCustom(ScObPlayerCustom playerCustom)
    {
        this.playerCustom = playerCustom;
        playerPrefab.SetEyesMaterial(playerCustom.Eyes);
        playerPrefab.SetPantsMaterial(playerCustom.pants);
        playerPrefab.SetShoesMaterial(playerCustom.Shoes);
        playerPrefab.SetSkinMaterial(playerCustom.Skin);
        playerPrefab.SetTshirtMaterial(playerCustom.tshirt);
    }
    public void setPlayerIndex(int index)
    {
        playerNameText.gameObject.SetActive(true);
        playerStatusText.text = "Not Ready!";
        PlayerIndex = index;
    }

    public void setIsReady(bool isReady)
    {
        this.isReady = isReady;
        if (isReady)
        {
            playerModel.SetActive(true);
            playerStatusText.gameObject.SetActive(false);
            playerReadyText.gameObject.SetActive(true);
            playerModelAnimator.SetBool("isReady", true);
        }
    }
    
    public void setPlayerName(string name)
    {
        playerNameText.text = name;
        this.name = name;
    }
}
