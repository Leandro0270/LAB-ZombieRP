using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnlineLobbyPlayersShower : MonoBehaviour
{
    private int PlayerIndex;
    private bool isReady;
    private string name;
    
    public void setPlayerIndex(int index)
    {
        PlayerIndex = index;
    }

    public void setIsReady(bool isReady)
    {
        this.isReady = isReady;
    }
    
    public void setPlayerName(string name)
    {
        this.name = name;
    }
}
