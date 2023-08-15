using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Runtime.Player.ScriptObjects.Combat
{
    [CreateAssetMenu(menuName = "Player")]
    public class ScObPlayerStats : ScriptableObject
    {
        public int classIndex;
        [FormerlySerializedAs("name")] public String nickName;
        public int maxThrowableCapacity;
        public int maxAuxiliaryCapacity;
        public int maxGunCapacity;
        public float health;
        public float speed;
        public float revivalSpeed;
        public ScObMeleeSpecs startMeleeSpecs;
        public ScObGunSpecs startGun;
        [FormerlySerializedAs("MainColor")] public Color mainColor;
        [FormerlySerializedAs("PlayerIndicator")] public Material playerIndicator;
        public float burnDamagePerSecond;
    }
}
