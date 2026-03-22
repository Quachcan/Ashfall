using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// Caches all Animator parameter and state hashes.
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
        public static readonly int MoveSpeed    = Animator.StringToHash("MoveSpeed");

        // ── Bool Parameters ───────────────────────────────────────────────

        /// <summary>True when grounded — switches Grounded ↔ Airborne SSM.</summary>
        public static readonly int IsGrounded   = Animator.StringToHash("IsGrounded");

        /// <summary>True while crouching — switches Grounded SSM → Crouch SSM.</summary>
        public static readonly int IsCrouching  = Animator.StringToHash("IsCrouching");

        /// <summary>True while holding block — switches Grounded SSM → Block SSM.</summary>
        public static readonly int IsBlocking   = Animator.StringToHash("IsBlocking");

        // ── Int Parameters ────────────────────────────────────────────────

        /// <summary>Current attack index in combo (1, 2, 3...).</summary>
        public static readonly int ComboIndex   = Animator.StringToHash("ComboIndex");

        // ── Trigger Parameters ────────────────────────────────────────────

        // Movement
        public static readonly int Jump         = Animator.StringToHash("Jump");
        public static readonly int Dash         = Animator.StringToHash("Dash");

        // Combat — Attack
        public static readonly int Attack       = Animator.StringToHash("Attack");

        // Combat — Block
        /// <summary>Trigger Block_Hit animation inside Block SSM.</summary>
        public static readonly int BlockHit     = Animator.StringToHash("BlockHit");

        // Combat — GuardBreak
        public static readonly int GuardBreak   = Animator.StringToHash("GuardBreak");

        // Combat — Hit Reactions
        public static readonly int HitReact_Head  = Animator.StringToHash("HitReact_Head");
        public static readonly int HitReact_Torso = Animator.StringToHash("HitReact_Torso");
        public static readonly int HitReact_Legs  = Animator.StringToHash("HitReact_Legs");

        // State
        public static readonly int Dead         = Animator.StringToHash("Dead");
        public static readonly int Revive       = Animator.StringToHash("Revive");

        // ── State Name Hashes (dùng với CrossFade) ────────────────────────

        /// <summary>Parry state hash — used with CrossFade in PlayerParryState.</summary>
        public static readonly int ParryState   = Animator.StringToHash("Parry");

        // ── Dynamic Helpers ───────────────────────────────────────────────

        /// <summary>
        /// Returns state hash for Attack_N (Attack_1, Attack_2...).
        /// Adding more hits = increase comboLength in PlayerStats SO
        /// + add matching Attack_N state in Animator. No code change needed.
        /// </summary>
        public static int GetAttackStateHash(int index)
            => Animator.StringToHash($"Attack_{index}");
    }
}