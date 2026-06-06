using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    public interface IHittable
    {
        /// <summary>
        /// Receives an attack. Called purely via code logic, bypassing physics colliders.
        /// </summary>
        /// <param name="data">The attack configuration containing multipliers and types.</param>
        /// <param name="attackerStats">The source stats used to calculate final damage.</param>
        /// <param name="attacker">The game object initiating the attack.</param>
        void TakeHit(AttackData data, ICombatStats attackerStats, GameObject attacker);

        bool IsInvincible { get; }
    }
}