using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    public interface IHittable
    {
        /// <summary>
        /// Receive a hit with the given data.
        /// </summary>
        /// <param name="data">AttackData SO chứa damage, knockback, crit, v.v.</param>
        /// <param name="hitPoint">World position nơi đòn chạm.</param>
        /// <param name="hitDirection">Hướng normalized từ attacker đến target.</param>
        /// <param name="attacker">GameObject thực hiện đòn đánh.</param>
        void TakeHit(AttackData data, Vector3 hitPoint, Vector3 hitDirection, GameObject attacker);

        /// <summary>True nếu đang invincible (i-frame, dead, cutscene, v.v.).</summary>
        bool IsInvincible { get; }
    }
}
