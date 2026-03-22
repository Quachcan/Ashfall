using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Player is holding block.
    /// Reduces incoming damage, costs stamina per hit.
    /// Running out of stamina while blocking triggers GuardBreak.
    ///
    /// Transitions:
    ///   Release block button → Idle/Run
    ///   Stamina depleted on hit → GuardBreak
    ///   Jump → Jump (cancel block)
    ///   Dash → Dash (cancel block)
    /// </summary>
    public class PlayerBlockState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        // Track how long block button has been held
        private float _holdTimer;

        public PlayerBlockState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            _holdTimer     = 0f;
            _ctx.AnimMoveSpeed = 0f;
            _ctx.Animator?.SetBool(AnimHash.IsBlocking, true);
            // TODO: _ctx.Hurtbox.SetBlocking(true);
        }

        public void Exit()
        {
            _ctx.Animator?.SetBool(AnimHash.IsBlocking, false);
            // TODO: _ctx.Hurtbox.SetBlocking(false);
        }

        public void Tick()
        {
            _holdTimer += Time.deltaTime;

            // Released before parry window expired → Parry
            if (_ctx.Input.BlockReleased)
            {
                if (_holdTimer <= _ctx.Stats.parryWindowTime)
                {
                    _ctx.Animator?.SetBool(AnimHash.IsBlocking, false);
                    _controller.ChangeState(PlayerState.Parry);
                }
                else
                {
                    ExitBlock();
                }
                return;
            }

            // Still holding → Block
            if (!_ctx.Input.BlockHeld)
            {
                ExitBlock();
                return;
            }

            // Jump cancels block
            if (_ctx.Input.JumpPressed)
            {
                _ctx.Input.ConsumeJump();
                _controller.ChangeState(PlayerState.Jump);
                return;
            }

            // Dash cancels block
            if (_ctx.Input.DashPressed && !_ctx.IsDashOnCooldown
                && _ctx.Stamina.Has(_ctx.Stats.dashStaminaCost))
            {
                _ctx.Input.ConsumeDash();
                _controller.ChangeState(PlayerState.Dash);
                return;
            }
        }

        public void FixedTick()
        {
            // Fall off ledge while blocking
            if (!_ctx.IsGroundedOrCoyote)
            {
                _controller.ChangeState(PlayerState.Fall);
            }
        }

        /// <summary>
        /// Called by HurtboxController when a hit is received while blocking.
        /// Spends stamina — if depleted, triggers guard break.
        /// </summary>
        public void OnBlockHit()
        {
            bool blocked = _ctx.Stamina.TrySpendBlock();
            if (!blocked)
            {
                // Out of stamina — guard break
                _controller.ChangeState(PlayerState.GuardBreak);
                return;
            }

            // Play block hit reaction animation
            _ctx.Animator?.SetTrigger(AnimHash.BlockHit);

            // TODO: raise EventHub event for block VFX/SFX
            // _eventHub.playerEvents.onPlayerBlocked.Raise();
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void ExitBlock()
        {
            bool hasInput = Mathf.Abs(_ctx.Input.MoveX) > 0.1f;
            _controller.ChangeState(hasInput ? PlayerState.Run : PlayerState.Idle);
        }
    }
}