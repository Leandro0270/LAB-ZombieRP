using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OnlineGameManager : MonoBehaviourPunCallbacks
{
    private string LocalplayerNickname;
    private string ConnectedRoomCode;

    
    
    public void ConnectToRoom()
    {
        PhotonNetwork.LocalPlayer.NickName = LocalplayerNickname;
        PhotonNetwork.ConnectUsingSettings();
        if(PhotonNetwork.InLobby == false)
            PhotonNetwork.JoinLobby();
        PhotonNetwork.JoinOrCreateRoom(ConnectedRoomCode, new Photon.Realtime.RoomOptions {MaxPlayers = 4}, null);
        DontDestroyOnLoad(this);


    }
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public override void OnConnectedToMaster()
    {
        if(PhotonNetwork.InLobby == false)
            PhotonNetwork.JoinLobby();
        
    }
    
    public override void OnJoinedLobby()
    {
        PhotonNetwork.JoinOrCreateRoom(ConnectedRoomCode, new Photon.Realtime.RoomOptions {MaxPlayers = 4}, null);
        DontDestroyOnLoad(this);
        LoadScene("PlayerSetupOnline"); 

    }
    
    public void setLocalPlayerNickname(string nickname)
    {
        LocalplayerNickname = nickname;
    }
    
    public void setConnectedRoomCode(string roomCode)
    {
        ConnectedRoomCode = roomCode;
    }
    
    
    
    
}
