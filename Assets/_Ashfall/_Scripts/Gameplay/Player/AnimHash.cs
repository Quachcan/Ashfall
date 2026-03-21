using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// Caches all Animator parameter hashes.
    /// Using integer hashes instead of strings eliminates per-frame
    /// string comparisons in the Animator — a common Unity perf pitfall.
    /// </summary>
    public static class AnimHash
    {
        // ── Float Parameters ──────────────────────────────────────────────

        /// <summary>
        /// Drives the Locomotion Blend Tree.
        /// 0 = Idle, 1 = Walk, 2 = Run, 3 = Sprint
        /// </summary>
        public static readonly int MoveSpeed       = Animator.StringToHash("MoveSpeed");

        // ── Bool Parameters ───────────────────────────────────────────────

        /// <summary>True when the player is on the ground — switches Grounded ↔ Airborne SSM.</summary>
        public static readonly int IsGrounded      = Animator.StringToHash("IsGrounded");
        
        /// <summary>True when the player is crouching — switches Grounded SSM → Crouch SSM.</summary>
        public static readonly int IsCrouching     = Animator.StringToHash("IsCrouching");

        // ── Int Parameters ────────────────────────────────────────────────

        /// <summary>Current attack index in the combo sequence (0, 1, 2).</summary>
        public static readonly int ComboIndex      = Animator.StringToHash("ComboIndex");

        // ── Trigger Parameters ────────────────────────────────────────────

        // Movement
        public static readonly int Jump            = Animator.StringToHash("Jump");
        public static readonly int Dash            = Animator.StringToHash("Dash");

        // Combat
        public static readonly int Attack          = Animator.StringToHash("Attack");
        public static readonly int HitReact_Head   = Animator.StringToHash("HitReact_Head");
        public static readonly int HitReact_Torso  = Animator.StringToHash("HitReact_Torso");
        public static readonly int HitReact_Legs   = Animator.StringToHash("HitReact_Legs");
        public static readonly int Block           = Animator.StringToHash("Block");

        // State
        public static readonly int Dead            = Animator.StringToHash("Dead");
        public static readonly int Revive          = Animator.StringToHash("Revive");
    }
}