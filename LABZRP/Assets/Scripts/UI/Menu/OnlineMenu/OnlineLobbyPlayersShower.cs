using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class OnlineLobbyPlayersShower : MonoBehaviourPunCallbacks
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
        playerNameText.text = PhotonNetwork.PlayerList[index].NickName;
        PlayerIndex = index;
    }

    public void setIsReady(bool isReady)
    {
        this.isReady = isReady;
        playerModel.SetActive(this.isReady);
            playerStatusText.gameObject.SetActive(!this.isReady);
            playerReadyText.gameObject.SetActive(this.isReady);
        
    }
    
    public void setPlayerName(string name)
    {
        playerNameText.text = name;
        this.name = name;
    }

    public void removePlayer()
    {
        if (isReady)
        {
            playerModel.SetActive(false);
            playerStatusText.gameObject.SetActive(true);
            playerReadyText.gameObject.SetActive(false);
        }
        playerStatusText.text = "Waiting for Player";
        name = "Player";
        playerNameText.text = name;
        playerNameText.gameObject.SetActive(false);
        PlayerIndex = -1;
        isReady = false;
    }



    public int getPlayerIndex()
    {
        return PlayerIndex;
    }
    
    public bool getIsReady()
    {
        return isReady;
    }
    
    public string getPlayerName()
    {
        return name;
    }
}