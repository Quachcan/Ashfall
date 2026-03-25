using UnityEngine;
using _Ashfall._Scripts.Gameplay.Combat;

namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// Shared data container passed to every Player state.
    /// States read and write through this context instead of referencing PlayerController directly.
    /// This keeps states decoupled and testable.
    /// </summary>
    public class PlayerContext
    {
        // ── Core Components ───────────────────────────────────────────────
        public readonly Rigidbody          Rb;
        public readonly Transform          Transform;
        public readonly Animator           Animator;
        public readonly PlayerInputHandler Input;
        public readonly PlayerStats        Stats;
        public readonly CapsuleCollider    Collider;
        public readonly StaminaSystem      Stamina;
        public readonly HealthSystem       Health;

        // ── Shared Runtime State ──────────────────────────────────────────

        /// <summary>Current facing direction: +1 = right, -1 = left.</summary>
        public float FacingDirection { get; set; } = 1f;

        /// <summary>True while the player is in a crouch state.</summary>
        public bool IsCrouching { get; set; }

        /// <summary>
        /// True if this class can block/parry.
        /// Set by PlayerController based on PlayerStats — only Fighter has block.
        /// </summary>
        public bool CanBlock { get; set; }

        /// <summary>True when the player is touching the ground this frame.</summary>
        public bool IsGrounded { get; set; }

        /// <summary>
        /// Coyote time counter — counts down after leaving the ground.
        /// States should use IsGrounded || CoyoteTimeCounter > 0 for jump eligibility.
        /// </summary>
        public float CoyoteTimeCounter { get; set; }

        /// <summary>How long coyote time lasts in seconds (set from PlayerStats).</summary>
        public float CoyoteTimeDuration { get; set; }

        /// <summary>True if player is considered "on ground" including coyote window.</summary>
        public bool IsGroundedOrCoyote => IsGrounded || CoyoteTimeCounter > 0f;

        /// <summary>True when the player is touching a wall this frame.</summary>
        public bool IsTouchingWall { get; set; }

        /// <summary>How many jumps have been used in the current airborne sequence.</summary>
        public int JumpsUsed { get; set; }

        /// <summary>True for one FixedUpdate frame after landing (used by states to trigger land FX).</summary>
        public bool JustLanded { get; set; }

        // ── Animator Driver Values ────────────────────────────────────────

        /// <summary>
        /// Current locomotion speed for the Blend Tree.
        /// 0 = Idle, 1 = Walk, 2 = Run, 3 = Sprint.
        /// Set by movement states each frame.
        /// </summary>
        public float AnimMoveSpeed { get; set; }



        // ── Dash Runtime ──────────────────────────────────────────────────

        /// <summary>True while the dash cooldown is active.</summary>
        public bool IsDashOnCooldown { get; set; }

        /// <summary>Remaining cooldown time in seconds.</summary>
        public float DashCooldownTimer { get; set; }

        // ── Constructor ───────────────────────────────────────────────────

        public PlayerContext(
            Rigidbody          rb,
            Transform          transform,
            Animator           animator,
            PlayerInputHandler input,
            PlayerStats        stats,
            CapsuleCollider    collider,
            StaminaSystem      stamina,
            HealthSystem       health)
        {
            Rb        = rb;
            Transform = transform;
            Animator  = animator;
            Input     = input;
            Stats     = stats;
            Collider  = collider;
            Stamina   = stamina;
            Health    = health;
        }
    }
}