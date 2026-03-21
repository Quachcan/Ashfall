using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Player is falling downward (either jumped and peaked, or walked off a ledge).
    /// Applies extra gravity for a heavier, more satisfying fall feel.
    /// Transitions: land → Idle | double jump → Jump | dash → Dash | attack → Attack
    /// </summary>
    public class PlayerFallState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        public PlayerFallState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            // VerticalVelocity float drives the Airborne SSM blend automatically
            // No trigger needed — IsGrounded bool handles SSM transition
        }

        public void Exit() { }

        public void Tick()
        {
            // Air horizontal flip
            _controller.SetFacingDirection(_ctx.Input.MoveX);

            // Double jump while falling
            if (_ctx.Input.JumpPressed && _ctx.JumpsUsed < _ctx.Stats.maxJumps)
            {
                _ctx.Input.ConsumeJump();
                _controller.ChangeState(PlayerState.Jump);
                return;
            }

            _ctx.Input.ConsumeJump();

            // Dash in air
            if (_ctx.Input.DashPressed && !_ctx.IsDashOnCooldown &&  _ctx.Stamina.Has(_ctx.Stats.dashStaminaCost))
            {
                _ctx.Input.ConsumeDash();
                _controller.ChangeState(PlayerState.Dash);
                return;
            }

            // Attack in air
            if (_ctx.Input.AttackPressed)
            {
                _ctx.Input.ConsumeAttack();
                _controller.ChangeState(PlayerState.Attack);
                return;
            }
        }

        public void FixedTick()
        {
            // Landed
            if (_ctx.IsGrounded)
            {
                bool hasInput = Mathf.Abs(_ctx.Input.MoveX) > 0.1f;
                _controller.ChangeState(hasInput ? PlayerState.Run : PlayerState.Idle);
                return;
            }

            ApplyAirMovement();
            ApplyFallGravity();
        }

        // ── Physics ───────────────────────────────────────────────────────

        private void ApplyAirMovement()
        {
            float input       = _ctx.Input.MoveX;
            float targetVelX  = input * _ctx.Stats.moveSpeed;
            float currentVelX = _ctx.Rb.linearVelocity.x;

            float newVelX = Mathf.MoveTowards(
                currentVelX,
                targetVelX,
                _ctx.Stats.acceleration * Time.fixedDeltaTime);

            Vector3 v = _ctx.Rb.linearVelocity;
            v.x = newVelX;
            _ctx.Rb.linearVelocity = v;
        }

        private void ApplyFallGravity()
        {
            // Extra downward force to make falling feel heavier than rising
            _ctx.Rb.linearVelocity += Vector3.up
                * (Physics.gravity.y * (_ctx.Stats.fallGravityMultiplier - 1f) * Time.fixedDeltaTime);
        }
    }
}