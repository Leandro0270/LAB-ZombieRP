using System;
using UnityEngine;

namespace Runtime.Enemy.ScriptObjects.HordeMode.Challenges
{
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
        public float TimeToStartChallenge;
        public float ChallengeTime;
        public GameObject Model3dChallengeMachine;
    
    
        //Kill in time
        public int zombiesToKill;
    
    }
}
