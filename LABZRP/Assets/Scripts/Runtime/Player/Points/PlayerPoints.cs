using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PlayerPoints : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private int pointsPerNormalZombie = 10;
    [SerializeField] private float pointsMultiplier = 1.5f;
    [SerializeField] private bool isMultiplierActive = false;
    private Points_UI pointsUI;
    private int totalPointsInGame = 0;
    private int points = 0;
    [SerializeField] private bool isOnline = false;
    [SerializeField] private PhotonView photonView;

    public void addPointsNormalZombieKilled()
    {
        if (!isOnline || photonView.IsMine)
        {
            if (isMultiplierActive)
            {
                points += (int)(pointsPerNormalZombie * pointsMultiplier);
                totalPointsInGame += (int)(pointsPerNormalZombie * pointsMultiplier);
            }
            else
            {
                points += pointsPerNormalZombie;
                totalPointsInGame += pointsPerNormalZombie;
            }
        }
        pointsUI.setPoints(points);

    }

    public void addPointsSpecialZombiesKilled(int points)
    {
        if (!isOnline || photonView.IsMine)
        {
            if (isMultiplierActive)
            {
                this.points += (int)(points * pointsMultiplier);
                totalPointsInGame += (int)(points * pointsMultiplier);
            }
            else
            {
                totalPointsInGame += points;
                this.points += points;
            }
            pointsUI.setPoints(this.points);
        }
    }

    public void removePoints(int points)
    {

        if (!isOnline || photonView.IsMine)
        {
            this.points -= points;
            pointsUI.setPoints(this.points);
        }
    }
    

    public int getPoints()
    {
        return points;
    }
    
    public void setMultiplier(bool isMultiplierActive)
    {
        this.isMultiplierActive = isMultiplierActive;
    }
    
    
    public void addChallengePoints(int points)
    {
        this.points += points;
        pointsUI.setPoints(points);
        
    }
    
    public void setPointsUI(Points_UI pointsUI)
    {
        this.pointsUI = pointsUI;
    }
    
    public int getTotalPointsInGame()
    {
        return totalPointsInGame;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(points);
            stream.SendNext(totalPointsInGame);
        }
        else
        {
            points = (int)stream.ReceiveNext();
            totalPointsInGame = (int)stream.ReceiveNext();
        }
    }
}
