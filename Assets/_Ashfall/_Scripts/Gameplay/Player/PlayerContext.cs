using UnityEngine;

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
        public readonly Rigidbody       Rb;
        public readonly Transform       Transform;
        public readonly Animator        Animator;
        public readonly PlayerInputHandler Input;
        public readonly PlayerStats     Stats;
        public readonly CapsuleCollider  Collider;

        // ── Shared Runtime State ──────────────────────────────────────────

        /// <summary>Current facing direction: +1 = right, -1 = left.</summary>
        public float FacingDirection { get; set; } = 1f;
        public bool IsCrouching { get; set; }
        public bool IsGrounded { get; set; }
        public float CoyoteTimeCounter { get; set; }
        public float CoyoteTimeDuration { get; set; }
        public bool IsGroundedOrCoyote => IsGrounded || CoyoteTimeCounter > 0f;
        public bool IsTouchingWall { get; set; }
        public int JumpsUsed { get; set; }
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
            Rigidbody       rb,
            Transform       transform,
            Animator        animator,
            PlayerInputHandler input,
            PlayerStats     stats,
            CapsuleCollider collider)
        {
            Rb        = rb;
            Transform = transform;
            Animator  = animator;
            Input     = input;
            Stats     = stats;
            Collider  = collider;
        }
    }
}