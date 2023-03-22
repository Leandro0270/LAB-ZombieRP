using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPoints : MonoBehaviour
{
    [SerializeField] private int pointsPerNormalZombie = 10;
    private int points = 0;
    
    
    public void addPoints()
    {
        Debug.Log(points);
        points += pointsPerNormalZombie;
    }
    
    public void addPoints(int points)
    {
        this.points += points;
    }
    
}
