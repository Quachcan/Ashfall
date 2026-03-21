using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// Caches Animator parameter hashes.
    /// Using integer hashes instead of strings eliminates per-frame string comparisons
    /// in the Animator, which is a common Unity performance pitfall.
    /// </summary>
    public static class AnimHash
    {
        // ── Booleans ─────────────────────────────────────────────────────
        public static readonly int IsRunning  = Animator.StringToHash("IsRunning");
        public static readonly int IsGrounded = Animator.StringToHash("IsGrounded");

        // ── Triggers ─────────────────────────────────────────────────────
        public static readonly int Jump   = Animator.StringToHash("Jump");
        public static readonly int Fall   = Animator.StringToHash("Fall");
        public static readonly int Land   = Animator.StringToHash("Land");
        public static readonly int Dash   = Animator.StringToHash("Dash");
        public static readonly int Attack = Animator.StringToHash("Attack");
        public static readonly int Dead   = Animator.StringToHash("Dead");
        public static readonly int Revive = Animator.StringToHash("Revive");
    }
}