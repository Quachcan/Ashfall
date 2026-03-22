using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Plays the parry animation after a perfect block is detected in BlockState.
    /// This state is purely visual — the parry detection logic lives in BlockState.
    ///
    /// Flow:
    ///   BlockState detects hit during parry window
    ///   → ChangeState(Parry)
    ///   → SwapParryClip (random) + CrossFade
    ///   → Animator Event OnParryWindowOpen (optional — for future counter-attack)
    ///   → Animation ends → return to Idle
    /// </summary>
    public class PlayerParryState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        private static readonly int ParryStateHash = AnimHash.ParryState;

        private GameObject _attacker;
        private bool       _windowOpen;
        private float      _exitTimer;
        private const float ExitNormalizedTime = 0.85f; // exit at 85% of animation

        public PlayerParryState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            _windowOpen        = false;
            _ctx.AnimMoveSpeed = 0f;

            // Stagger the attacker immediately
            if (_attacker != null)
            {
                // TODO: _attacker.GetComponent<EnemyBase>()?.TriggerStagger();
                _attacker = null;
            }

            // Swap to random parry clip then play
            _controller.SwapParryClip();
            _ctx.Animator?.CrossFade(ParryStateHash, 0.05f, 0);

            // TODO: raise EventHub — parry VFX, SFX, optional slow-motion
            // _eventHub.playerEvents.onPlayerParried.Raise();
        }

        public void Exit() { }

        public void Tick()
        {
            if (_ctx.Animator == null) return;

            // Check if parry animation is near completion
            // More reliable than Animation Events on SO clips
            var stateInfo = _ctx.Animator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.shortNameHash == AnimHash.ParryState
                && stateInfo.normalizedTime >= ExitNormalizedTime)
            {
                _controller.ChangeState(PlayerState.Idle);
            }
        }

        public void FixedTick() { }

        // ── External API ──────────────────────────────────────────────────

        /// <summary>Set attacker reference before entering — called by BlockState.</summary>
        public void SetAttacker(GameObject attacker) => _attacker = attacker;

        /// <summary>
        /// Called by AnimatorEventBridge at the active parry frame.
        /// Opens counter-attack window — future feature.
        /// </summary>
        public void OnParryWindowOpen() => _windowOpen = true;

        // ── Animator Event ────────────────────────────────────────────────

        /// <summary>Called by AnimatorEventBridge when parry animation ends.</summary>
        public void OnParryEnd()
        {
            _controller.ChangeState(PlayerState.Idle);
        }
    }
}