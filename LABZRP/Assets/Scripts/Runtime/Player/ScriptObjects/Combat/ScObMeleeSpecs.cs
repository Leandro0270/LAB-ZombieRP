using UnityEngine;

namespace Runtime.Player.ScriptObjects.Combat
{
    [CreateAssetMenu(menuName = "NewMeleeSpecs")]

    public class ScObMeleeSpecs : ScriptableObject
    {
        public int meleeId;                     //id da arma
        public string meleeName = "New Melee";  //nome da arma
        public bool haveMesh;                 //Possui algum modelo 3d?
        public GameObject meleePrefab3DModel; //Modelo 3d da arma
        
        [Header("Stats")]
        public int damage;                     //Dano da arma
        public float delayBetweenAttacks;      //Tempo entre ataques
        public float attackDuration;           //Duração do ataque
        public float attackRange;              //Alcance do ataque
        public float horizontalArea;           //Área horizontal do ataque
        public float verticalArea;             //Área vertical do ataque
        public bool haveCriticalChance;       //Possui chance de crítico?
        public float criticalChance;          //Chance de crítico
        public float criticalDamagePercentage; //Porcentagem de dano crítico
        public bool haveKnockBack;             //Possui knockback?
        public float knockBackForce;           //Força do knockback
        public int hittableEnemies;            //Quantidade de inimigos atingidos por ataque
        
        [Header("Audio/Visual")]
        public AudioClip attackSound;           // Som de ataque da arma
        public ParticleSystem attackEffect;     // Efeito visual ao atacar
        

        
        
        
        
    }
}
