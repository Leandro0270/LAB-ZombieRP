using UnityEngine;

namespace Runtime.Enemy.ScriptObjects.EnemySpecial
{
    [CreateAssetMenu(menuName = "EnemySpecial")]
    public class ScObSpecialZombies : ScriptableObject
    {
    
        public float health;
        public float speed;
        public float damage;
        public int points;

    }
}
