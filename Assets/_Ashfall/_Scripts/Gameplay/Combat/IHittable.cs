using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    public interface IHittable
    {
        /// <summary>
        /// Receive a hit with the given data.
        /// </summary>
        /// <param name="data">Damage, knockback, poise damage values.</param>
        /// <param name="hitPoint">World position where the hit landed.</param>
        /// <param name="hitDirection">Normalized directions from attacker to target.</param>
        /// <param name="attacker">The GameObject that landed the hit.</param>
        void TakeHit(HitData data, Vector3 hitPoint, Vector3 hitDirection, GameObject attacker);
        
        /// <summary>True if currently invincible (i-frame, dead, cutscene, etc.).</summary>
        bool IsInvincible { get; }
    }
}