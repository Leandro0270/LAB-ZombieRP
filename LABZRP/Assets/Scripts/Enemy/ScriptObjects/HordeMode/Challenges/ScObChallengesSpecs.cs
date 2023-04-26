using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Challenge")]
public class ScObChallengesSpecs : ScriptableObject
{   public enum Type
    {
        NoHit,
        ExplosiveEnemies,
        NoThrowable,
        KillInArea,
        KillInTime,
        KillWithAim,
        KillWithMelee,
        DefendTheCoffeeMachine,
        Sharpshooter
    }
    public bool havePenalty;
    public Type ChallengeType;
    public String ChallengeName;
    public String ChallengeDescription;
    public int ChallengeReward;
    public int ChallengeDifficulty;
    public float ChallengeTime;
    public GameObject Model3dChallengeMachine;
    
    
    //Kill in time
    public int zombiesToKill;
    
}
