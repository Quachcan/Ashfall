using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player
{
    public static class AnimHash
    {
        // ── Parameters ────────────────────────────────────────────────────
        public static readonly int MoveSpeed    = Animator.StringToHash("MoveSpeed");
        public static readonly int IsGrounded   = Animator.StringToHash("IsGrounded");
        public static readonly int IsBlocking   = Animator.StringToHash("IsBlocking");
        public static readonly int ComboIndex   = Animator.StringToHash("ComboIndex");

        // ── Triggers ──────────────────────────────────────────────────────
        public static readonly int Dash         = Animator.StringToHash("Dash");
        public static readonly int Attack       = Animator.StringToHash("Attack");
        public static readonly int BlockHit     = Animator.StringToHash("BlockHit");
        public static readonly int GuardBreak   = Animator.StringToHash("GuardBreak");
        public static readonly int HitReact     = Animator.StringToHash("HitReact");
        public static readonly int Dead         = Animator.StringToHash("Dead");

        // ── Dynamic Helpers ───────────────────────────────────────────────
        public static int GetAttackStateHash(int index)
            => Animator.StringToHash($"Attack_{index}");
    }
}