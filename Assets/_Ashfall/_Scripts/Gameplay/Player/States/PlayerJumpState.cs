using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Player is jumping upward.
    /// Supports variable jump height (short tap = low jump, hold = high jump)
    /// and double jump via JumpsUsed counter.
    /// Transitions: velocity goes negative → Fall | dash → Dash | attack → Attack
    /// </summary>
    public class PlayerJumpState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        public PlayerJumpState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            ApplyJumpForce();

            // If jumping during coyote window (was grounded, now falling),
            // reset counter first so it counts as the first jump, not a double jump
            if (!_ctx.IsGrounded && _ctx.CoyoteTimeCounter > 0f)
                _ctx.JumpsUsed = 0;

            _ctx.JumpsUsed++;

            // Consume coyote window so it can't be used again this airborne sequence
            _ctx.CoyoteTimeCounter = 0f;

            _ctx.AnimMoveSpeed = 0f;
            _ctx.Animator?.SetTrigger(AnimHash.Jump);
        }

        public void Exit() { }

        public void Tick()
        {
            // Air horizontal flip
            _controller.SetFacingDirection(_ctx.Input.MoveX);

            // Double jump — buffer keeps JumpPressed true for a short window
            // so rapid button presses are never missed.
            // ForceJump() handles re-entering the same Jump state which
            // StateMachine.ChangeState() would normally block.
            if (_ctx.Input.JumpPressed && _ctx.JumpsUsed < _ctx.Stats.maxJumps)
            {
                _ctx.Input.ConsumeJump();
                _controller.ForceJump();
                return;
            }

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
            ApplyAirMovement();
            ApplyVariableGravity();

            // Once velocity turns negative we are falling
            if (_ctx.Rb.linearVelocity.y < 0f)
            {
                _controller.ChangeState(PlayerState.Fall);
            }
        }

        // ── Physics ───────────────────────────────────────────────────────

        private void ApplyJumpForce()
        {
            // Zero out any existing vertical velocity for consistent jump height
            Vector3 v = _ctx.Rb.linearVelocity;
            v.y = _ctx.Stats.jumpForce;
            _ctx.Rb.linearVelocity = v;
        }

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

        private void ApplyVariableGravity()
        {
            // Short-hop: if player releases jump button while still moving upward,
            // add extra downward force to cut the jump short
            if (!_ctx.Input.JumpHeld && _ctx.Rb.linearVelocity.y > 0f)
            {
                _ctx.Rb.linearVelocity += Vector3.up
                    * (Physics.gravity.y * (_ctx.Stats.lowJumpMultiplier - 1f) * Time.fixedDeltaTime);
            }
        }
    }
}