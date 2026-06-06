using Sirenix.OdinInspector;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Enemy
{
    [CreateAssetMenu(menuName = "Ashfall/Enemy/EnemyStats", fileName = "EnemyStats_New")]
    public class EnemyStats : ScriptableObject
    {
        [TitleGroup("Health")]
        public float maxHp = 100f;
        
        [TitleGroup("Combat Stats")]
        [Tooltip("Base Physical Attack")]
        public float atk = 10;
        
        [Tooltip("Base Magic Attack")]
        public float mag = 0;

        [Tooltip("Base Defense (Reduce incoming damage")]
        public float def = 5;
    }
}