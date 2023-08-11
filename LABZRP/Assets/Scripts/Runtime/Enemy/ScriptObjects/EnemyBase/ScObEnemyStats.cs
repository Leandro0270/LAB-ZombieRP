using UnityEngine;

namespace Runtime.Enemy.ScriptObjects.EnemyBase
{
        [CreateAssetMenu(menuName = "Enemy")]
        public class ScObEnemyStats : ScriptableObject
        {
    
                public float health;
                public float speed;
                public float damage;
                public bool isSpecial;
                public float burnDamagePerSecond;


        }
}
