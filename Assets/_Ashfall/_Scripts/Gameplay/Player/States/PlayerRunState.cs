using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Player is moving horizontally on the ground.
    /// Uses acceleration/deceleration for responsive, non-slippery feel.
    /// Transitions: no input → Idle | jump → Jump | fall off ledge → Fall | dash → Dash | attack → Attack
    /// </summary>
    public class PlayerRunState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        public PlayerRunState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            _ctx.Animator?.SetBool(AnimHash.IsRunning, true);
            _ctx.Animator?.SetBool(AnimHash.IsGrounded, true);
        }

        public void Exit()
        {
            _ctx.Animator?.SetBool(AnimHash.IsRunning, false);
        }

        public void Tick()
        {
            // Flip sprite to face movement direction
            _controller.SetFacingDirection(_ctx.Input.MoveX);

            // Jump input
            if (_ctx.Input.JumpPressed)
            {
                _ctx.Input.ConsumeJump();
                _controller.ChangeState(PlayerState.Jump);
                return;
            }

            // Dash input
            if (_ctx.Input.DashPressed && !_ctx.IsDashOnCooldown)
            {
                _ctx.Input.ConsumeDash();
                _controller.ChangeState(PlayerState.Dash);
                return;
            }

            // Attack input
            if (_ctx.Input.AttackPressed)
            {
                _ctx.Input.ConsumeAttack();
                _controller.ChangeState(PlayerState.Attack);
                return;
            }

            // No input → Idle
            if (Mathf.Abs(_ctx.Input.MoveX) < 0.1f)
            {
                _controller.ChangeState(PlayerState.Idle);
                return;
            }
        }

        public void FixedTick()
        {
            // Fall off ledge
            if (!_ctx.IsGroundedOrCoyote)
            {
                _controller.ChangeState(PlayerState.Fall);
                return;
            }

            ApplyMovement();
        }

        // ── Movement ──────────────────────────────────────────────────────

        private void ApplyMovement()
        {
            float input      = _ctx.Input.MoveX;
            float targetVelX = input * _ctx.Stats.moveSpeed;
            float currentVelX = _ctx.Rb.linearVelocity.x;

            // Use acceleration when there's input, deceleration when stopping
            float rate = Mathf.Abs(input) > 0.1f
                ? _ctx.Stats.acceleration
                : _ctx.Stats.deceleration;

            float newVelX = Mathf.MoveTowards(currentVelX, targetVelX, rate * Time.fixedDeltaTime);

            Vector3 v = _ctx.Rb.linearVelocity;
            v.x = newVelX;
            _ctx.Rb.linearVelocity = v;
        }
    }
}