using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Player performs a parry — randomly picks one of the parry clips,
    /// then opens an active window where any incoming hit is perfectly deflected.
    ///
    /// Triggered by: tapping Block (press + release within parryWindowTime).
    ///
    /// Flow:
    ///   Enter → swap random clip → CrossFade to Parry state
    ///   Animator Event OnParryWindowOpen → _windowOpen = true
    ///   If hit arrives while _windowOpen → stagger enemy, no damage
    ///   parryActiveDuration expires → exit to Idle
    /// </summary>
    public class PlayerParryState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        // Use centralized hash from AnimHash
        private static readonly int ParryStateHash = AnimHash.ParryState;

        private float _parryTimer;
        private bool  _parrySuccess;
        private bool  _windowOpen;

        public PlayerParryState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            _parryTimer   = _ctx.Stats.parryActiveDuration;
            _parrySuccess = false;
            _windowOpen   = false;

            _ctx.AnimMoveSpeed = 0f;

            // Swap to a random parry clip then CrossFade
            _controller.SwapParryClip();
            _ctx.Animator?.CrossFade(ParryStateHash, 0.05f, 0);

            // TODO: _ctx.Hurtbox.SetBlocking(true);
        }

        public void Exit()
        {
            // TODO: _ctx.Hurtbox.SetBlocking(false);
        }

        public void Tick()
        {
            _parryTimer -= Time.deltaTime;

            if (_parryTimer <= 0f)
                _controller.ChangeState(PlayerState.Idle); // window expired — whiff
        }

        public void FixedTick() { }

        // ── Animator Event Callbacks ──────────────────────────────────────

        /// <summary>
        /// Called by AnimatorEventBridge when animation reaches the active parry frame.
        /// Opens the deflection window — hits arriving after this are perfect parries.
        /// </summary>
        public void OnParryWindowOpen() => _windowOpen = true;

        // ── Hit Detection ─────────────────────────────────────────────────

        /// <summary>
        /// Called by HurtboxController when a hit arrives.
        /// Only deflects if active window is open.
        /// </summary>
        public void OnHitReceived(GameObject attacker)
        {
            if (!_windowOpen) return; // hit before active frame — not a parry

            _parrySuccess = true;

            // TODO: stagger enemy
            // attacker.GetComponent<EnemyBase>()?.TriggerStagger();

            // TODO: raise EventHub event for parry VFX/SFX
            // _eventHub.playerEvents.onPlayerParried.Raise();

            _controller.ChangeState(PlayerState.Idle);
        }
    }
}