using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// Shared data container passed to every Player state.
    /// State read and write through this context instead of referencing PlayerController directly
    /// This keeps states decoupled and testable
    /// </summary>
    public class PlayerContext
    {
        // Core Components
        public readonly Rigidbody Rb;
        public readonly Transform Transform;
        public readonly Animator Animator;
        public readonly PlayerInputHandler Input;
        public readonly PlayerStats Stats;
        
        // Shared Runtime State
        public float FacingDirection { get; set; } = 1f;
        public bool IsGrounded { get; set; }
        public float CoyoteTimeCounter { get; set; }
        public float CoyoteTimeDuration { get; set; }
        public bool IsGroundedOrCoyote => IsGrounded || CoyoteTimeCounter > 0f;
        public bool IsTouchingWall { get; set; }
        public int JumpsUsed { get; set; }
        public bool JustLanded { get; set; }
        
        // Dash Runtime
        public bool IsDashOnCooldown { get; set; }
        public float DashCooldownTimer { get; set; }
        
        // Constructor

        public PlayerContext(
            Rigidbody rb,
            Transform transform,
            Animator animator,
            PlayerInputHandler input,
            PlayerStats stats
        )
        {
            Rb  = rb;
            Transform = transform;
            Animator = animator;
            Input = input;
            Stats = stats;
        }
    }
}