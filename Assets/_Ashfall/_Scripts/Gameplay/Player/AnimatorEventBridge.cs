using _Ashfall._Scripts.Gameplay.Player;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// Bridge component sitting on the Model child (where the Animator lives).
    /// Animator Events cannot reach scripts on parent GameObjects directly,
    /// so this script receives them and forwards to PlayerController on the root.
    ///
    /// Setup: Add this component to the same GameObject as the Animator (M_Fighter).
    /// All Animator Events should point to this component's methods.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class AnimatorEventBridge : MonoBehaviour
    {
        private PlayerController _controller;

        private void Awake()
        {
            // Walk up the hierarchy to find PlayerController on root
            _controller = GetComponentInParent<PlayerController>();

            if (!_controller)
                Debug.LogWarning("[AnimatorEventBridge] PlayerController not found in parent!", this);
        }

        // ── Attack Events ─────────────────────────────────────────────────

        /// <summary>
        /// Fire at the impact frame of each attack animation.
        /// Used to activate hitbox at the right moment.
        /// </summary>
        public void OnAttackHit()  => _controller?.OnAttackHit();

        /// <summary>
        /// Fire at the last frame of each attack animation.
        /// Tells AttackState the current hit is done — advance combo or end.
        /// </summary>
        public void OnAttackEnd()  => _controller?.OnAttackEnd();

        // ── Movement Events ───────────────────────────────────────────────

        /// <summary>
        /// Optional: fire at the foot-plant frame of walk/run
        /// for footstep audio or VFX triggers.
        /// </summary>
        public void OnFootstep()
        {
            // TODO: AudioManager.Instance.PlaySfx("footstep");
        }

        // ── Death / Respawn Events ────────────────────────────────────────

        /// <summary>Fire when death animation reaches the "settled" frame.</summary>
        public void OnDeathSettled() => _controller?.OnDeathSettled();
    }
}