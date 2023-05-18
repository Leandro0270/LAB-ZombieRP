using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPoints : MonoBehaviour
{
    [SerializeField] private int pointsPerNormalZombie = 10;
    [SerializeField] private float pointsMultiplier = 1.5f;
    [SerializeField] private bool isMultiplierActive = false;
    private Points_UI pointsUI;
    private int totalPointsInGame = 0;
    private int points = 0;

    public void addPointsNormalZombieKilled()
    {
        if(isMultiplierActive){
            points += (int)(pointsPerNormalZombie * pointsMultiplier);
            totalPointsInGame += (int)(pointsPerNormalZombie * pointsMultiplier);
        }
        else{
            points += pointsPerNormalZombie;
            totalPointsInGame += pointsPerNormalZombie;
        }
        pointsUI.setPoints(points);
    }

    public void addPointsSpecialZombiesKilled(int points)
    {
        if(isMultiplierActive){
            this.points += (int)(points * pointsMultiplier);
            totalPointsInGame += (int)(points * pointsMultiplier);
        }else{
            
            totalPointsInGame += points;
            this.points += points;
        }
        pointsUI.setPoints(this.points);
    }

    public void removePoints(int points)
    {
        this.points -= points;
        pointsUI.setPoints(this.points);
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
        pointsUI.setPoints(this.points);
    }
    
    public void setPointsUI(Points_UI pointsUI)
    {
        this.pointsUI = pointsUI;
    }
    
    public int getTotalPointsInGame()
    {
        return totalPointsInGame;
    }

}
