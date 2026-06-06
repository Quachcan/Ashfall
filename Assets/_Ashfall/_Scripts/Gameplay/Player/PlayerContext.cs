using UnityEngine;
using _Ashfall._Scripts.Gameplay.Combat;
using _Ashfall._Scripts.Gameplay.Stats;
using _Ashfall._Scripts.Gameplay.Weapons;

namespace _Ashfall._Scripts.Gameplay.Player
{
    public class PlayerContext
    {
        // ── Core Components ───────────────────────────────────────────────
        public readonly Rigidbody          Rb;
        public readonly Transform          Transform;
        public readonly Animator           Animator;
        public readonly PlayerStats        Stats;
        public readonly CapsuleCollider    Collider;
        public readonly HealthSystem       Health;

        // ── Shared Runtime State ──────────────────────────────────────────
        public float FacingDirection { get; set; } = 1f;
        public bool  CanBlock { get; set; }
        public float AnimMoveSpeed { get; set; }

        // ── Equipment ─────────────────────────────────────────────────────
        public WeaponHandler Weapon { get; set; }
        public Transform ArrowSpawnPoint { get; set; }

        public PlayerContext(
            Rigidbody          rb,
            Transform          transform,
            Animator           animator,
            PlayerStats        stats,
            CapsuleCollider    collider,
            HealthSystem       health)
        {
            Rb        = rb;
            Transform = transform;
            Animator  = animator;
            Stats     = stats;
            Collider  = collider;
            Health    = health;
        }
    }
}