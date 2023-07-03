
using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OnlineMenuManager : MonoBehaviourPunCallbacks
{
    public GameObject inputPanel;
    public Button selectNickButton;
    public TMP_InputField nickInput;
    public TMP_InputField codeInput;
    public TextMeshProUGUI ConnectionFeedbackText;
    public OnlineGameManager onlineGameManager;
    
    public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    private void Start()
    {
        selectNickButton.interactable = false;
    }

    public void verifyInput()
      {
          bool isNickEmpty = string.IsNullOrEmpty(nickInput.text.Trim());
          bool isCodeEmpty = string.IsNullOrEmpty(codeInput.text.Trim());
          if (!isNickEmpty && !isCodeEmpty)
          {
              selectNickButton.interactable = true;
          }
          else
          {
              selectNickButton.interactable = false;

          }
      }


    public void connectToLobby()
    {
        string playerNick = nickInput.text;
        string roomCode = codeInput.text;
        inputPanel.SetActive(false);
        ConnectionFeedbackText.gameObject.SetActive(true);
        ConnectionFeedbackText.text = "Conectando...";
        
        onlineGameManager.setLocalPlayerNickname(playerNick);
        onlineGameManager.setConnectedRoomCode(roomCode);
        onlineGameManager.ConnectToRoom();
        
        PhotonNetwork.LocalPlayer.NickName = playerNick;
        PhotonNetwork.ConnectUsingSettings();
    }


    public override void OnConnectedToMaster()
    {
        ConnectionFeedbackText.text = "Conectado!";
    }
    
    public override void OnJoinedLobby()
    {
        ConnectionFeedbackText.text = "Entrando na sala..."; 
        enabled = false;
    }
   
}
