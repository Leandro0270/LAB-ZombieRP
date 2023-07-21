
using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;

public class OnlinePlayerConfiguration
    {
        
        public OnlinePlayerConfiguration(Player joinedPlayer, int playerIndex)
        {
            isMasterClient = joinedPlayer.IsMasterClient;
            PlayerIndex = playerIndex;
            player = joinedPlayer;
            playerName = joinedPlayer.NickName;
            isLocal = PhotonNetwork.LocalPlayer.ActorNumber == player.ActorNumber;
        }
    
        public Player player { get; set; }
        public int PlayerIndex { get; set; }
        
        public string playerName { get; set; }
        public bool isLocal { get; set; }

        public OnlineLobbyPlayersShower lobbyPlayersShower { get; set; }
        public bool isReady { get; set; }
        public ScObPlayerStats playerStats { get; set; }
        public bool isMasterClient { get; set; }
        public ScObPlayerCustom playerCustom { get; set; }
    }

