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

        public void Enter() { }

        public void Exit()
        {
            _ctx.AnimMoveSpeed = 0f;
        }

        public void Tick()
        {
            // Flip sprite to face movement direction
            _controller.SetFacingDirection(_ctx.Input.MoveX);

            // Crouch toggle while running → CrouchWalk
            if (_ctx.Input.CrouchToggled)
            {
                _ctx.Input.ConsumeCrouch();
                _controller.ChangeState(PlayerState.CrouchWalk);
                return;
            }

            // Block/Parry — Fighter only
            if (_ctx.CanBlock && _ctx.Input.BlockPressed)
            {
                _ctx.Input.ConsumeBlock();
                // Will resolve tap vs hold in BlockState based on timing
                _controller.ChangeState(PlayerState.Block);
                return;
            }

            // Jump input
            if (_ctx.Input.JumpPressed)
            {
                _ctx.Input.ConsumeJump();
                _controller.ChangeState(PlayerState.Jump);
                return;
            }

            // Dash input — only triggers on tap, not hold (handled in InputHandler)
            if (_ctx.Input.DashPressed && !_ctx.IsDashOnCooldown && _ctx.Stamina.Has(_ctx.Stats.dashStaminaCost))
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

            // Crouch toggle → CrouchWalk or CrouchIdle
            if (_ctx.Input.IsCrouching)
            {
                _controller.ChangeState(
                    Mathf.Abs(_ctx.Input.MoveX) > 0.1f
                        ? PlayerState.CrouchWalk
                        : PlayerState.CrouchIdle);
                return;
            }

            // No input → Idle
            if (Mathf.Abs(_ctx.Input.MoveX) < 0.1f)
            {
                // If was sprinting, consume sprint on stop
                if (_ctx.Input.SprintHeld)
                    _ctx.Input.ConsumeSprint();

                _controller.ChangeState(PlayerState.Idle);
                return;
            }
        }

        public void FixedTick()
        {
            // Fall off ledge — use coyote window to avoid instant Fall on edge
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
            float input       = _ctx.Input.MoveX;

            // Sprint requires stamina — stop sprinting if exhausted mid-sprint
            bool  isSprinting = _ctx.Input.SprintHeld && Mathf.Abs(input) > 0.1f;
            if (isSprinting)
            {
                bool hasStamina = _ctx.Stamina.DrainSprint(Time.fixedDeltaTime);
                if (!hasStamina)
                {
                    // Ran out of stamina — force exit sprint
                    _ctx.Input.ConsumeSprint();
                    isSprinting = false;
                }
            }

            // Sprint uses moveSpeed as target, run uses runSpeed as target
            float topSpeed   = isSprinting ? _ctx.Stats.moveSpeed : _ctx.Stats.runSpeed;
            float targetVelX = input * topSpeed;
            float currentVelX = _ctx.Rb.linearVelocity.x;

            float rate;
            if (Mathf.Abs(input) < 0.1f)
                rate = _ctx.Stats.deceleration;
            else if (isSprinting && Mathf.Abs(currentVelX) >= _ctx.Stats.runSpeed)
                rate = _ctx.Stats.sprintAcceleration; // slower ramp-up into sprint
            else
                rate = _ctx.Stats.acceleration;

            float newVelX = Mathf.MoveTowards(currentVelX, targetVelX, rate * Time.fixedDeltaTime);

            Vector3 v = _ctx.Rb.linearVelocity;
            v.x = newVelX;
            _ctx.Rb.linearVelocity = v;

            // Map actual speed → Blend Tree value
            // 0 = Idle | 1 = Walk | 2 = Run | 3 = Sprint
            float absSpeed = Mathf.Abs(newVelX);
            float animSpeed;

            if (absSpeed < 0.1f)
                animSpeed = 0f;
            else if (absSpeed < _ctx.Stats.walkSpeed)
                animSpeed = Mathf.InverseLerp(0f, _ctx.Stats.walkSpeed, absSpeed);
            else if (absSpeed < _ctx.Stats.runSpeed)
                animSpeed = 1f + Mathf.InverseLerp(_ctx.Stats.walkSpeed, _ctx.Stats.runSpeed, absSpeed);
            else
                animSpeed = 2f + Mathf.InverseLerp(_ctx.Stats.runSpeed, _ctx.Stats.moveSpeed, absSpeed);

            _ctx.AnimMoveSpeed = Mathf.Clamp(animSpeed, 0f, 3f);
        }
    }
}