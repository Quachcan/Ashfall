using _Ashfall._Scripts.Gameplay.Stats;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Enemy
{
    public class EnemyContext
    {
        public readonly Animator Animator;
        public readonly EnemyStats Stats;
        public readonly HealthSystem Health;

        public EnemyContext(Animator animator, EnemyStats stats, HealthSystem health)
        {
            Animator = animator;
            Stats = stats;
            Health = health;
        }
    }
}