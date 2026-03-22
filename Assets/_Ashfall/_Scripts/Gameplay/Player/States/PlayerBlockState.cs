using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Player is holding block — Sekiro-style.
    ///
    /// Flow:
    ///   Enter → open parry window (parryWindowTime seconds)
    ///   Hit received during window → perfect parry → ChangeState(Parry) for animation
    ///   Hit received after window  → normal block → spend stamina, play BlockHit anim
    ///   Stamina depleted on block  → GuardBreak
    ///   Release button             → Idle/Run
    ///
    /// No separate "tap for parry" — just hold block and time it like Sekiro.
    /// </summary>
    public class PlayerBlockState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        private float _parryWindowTimer;
        private bool  _isParryWindowOpen;

        public PlayerBlockState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            // Open parry window immediately on block
            _parryWindowTimer  = _ctx.Stats.parryWindowTime;
            _isParryWindowOpen = true;

            _ctx.AnimMoveSpeed = 0f;
            _ctx.Animator?.SetBool(AnimHash.IsBlocking, true);
            // TODO: _ctx.Hurtbox.SetBlocking(true);
        }

        public void Exit()
        {
            _isParryWindowOpen = false;
            _ctx.Animator?.SetBool(AnimHash.IsBlocking, false);
            // TODO: _ctx.Hurtbox.SetBlocking(false);
        }

        public void Tick()
        {
            // Tick parry window
            if (_isParryWindowOpen)
            {
                _parryWindowTimer -= Time.deltaTime;
                if (_parryWindowTimer <= 0f)
                    _isParryWindowOpen = false;
            }

            // Release block → exit
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

            UpdateBlockMovement();
        }

        public void FixedTick()
        {
            if (!_ctx.IsGroundedOrCoyote)
            {
                _controller.ChangeState(PlayerState.Fall);
                return;
            }

            ApplyBlockMovement();
        }

        // ── Hit Detection ─────────────────────────────────────────────────

        /// <summary>
        /// Called by HurtboxController when a hit is received while blocking.
        /// Checks parry window first — if open, perfect parry.
        /// Otherwise normal block — spend stamina.
        /// </summary>
        public void OnBlockHit(GameObject attacker = null)
        {
            if (_isParryWindowOpen)
            {
                // Perfect parry — transition to Parry state for animation + stagger
                _controller.ChangeState(PlayerState.Parry);

                // Pass attacker reference so ParryState can stagger them
                if (_controller.TryGetParryState(out var parryState))
                    parryState.SetAttacker(attacker);

                return;
            }

            // Normal block
            bool blocked = _ctx.Stamina.TrySpendBlock();
            if (!blocked)
            {
                _controller.ChangeState(PlayerState.GuardBreak);
                return;
            }

            _ctx.Animator?.SetTrigger(AnimHash.BlockHit);
            // TODO: raise EventHub event for block VFX/SFX
        }

        // ── Private ───────────────────────────────────────────────────────

        private void UpdateBlockMovement()
        {
            float input = _ctx.Input.MoveX;
            if (Mathf.Abs(input) < 0.1f) { _ctx.AnimMoveSpeed = 0f; return; }

            float dot = input * _ctx.FacingDirection;
            _ctx.AnimMoveSpeed = dot > 0f ? 1f : -1f;
        }

        private void ApplyBlockMovement()
        {
            float input = _ctx.Input.MoveX;

            if (Mathf.Abs(input) < 0.1f)
            {
                Vector3 v = _ctx.Rb.linearVelocity;
                v.x = Mathf.MoveTowards(v.x, 0f, _ctx.Stats.deceleration * Time.fixedDeltaTime);
                _ctx.Rb.linearVelocity = v;
                return;
            }

            float targetVelX  = input * _ctx.Stats.crouchSpeed;
            float currentVelX = _ctx.Rb.linearVelocity.x;
            float newVelX     = Mathf.MoveTowards(currentVelX, targetVelX,
                                    _ctx.Stats.acceleration * Time.fixedDeltaTime);

            Vector3 vel = _ctx.Rb.linearVelocity;
            vel.x = newVelX;
            _ctx.Rb.linearVelocity = vel;
        }

        private void ExitBlock()
        {
            bool hasInput = Mathf.Abs(_ctx.Input.MoveX) > 0.1f;
            _controller.ChangeState(hasInput ? PlayerState.Run : PlayerState.Idle);
        }
    }
}