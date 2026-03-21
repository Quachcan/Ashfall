using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Player is crouching and moving horizontally.
    /// Transitions:
    ///   no input        → CrouchIdle
    ///   crouch toggle   → Run
    ///   jump            → Jump (cancels crouch)
    ///   fall off ledge  → Fall
    /// </summary>
    public class PlayerCrouchWalkState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        public PlayerCrouchWalkState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            _ctx.Animator?.SetBool(AnimHash.IsCrouching, true);
        }

        public void Exit()
        {
            _ctx.Animator?.SetBool(AnimHash.IsCrouching, false);
            _ctx.AnimMoveSpeed = 0f;
        }

        public void Tick()
        {
            _controller.SetFacingDirection(_ctx.Input.MoveX);

            // Toggle crouch off → stand and run
            if (_ctx.Input.CrouchToggled)
            {
                _ctx.Input.ConsumeCrouch();
                _controller.ChangeState(
                    Mathf.Abs(_ctx.Input.MoveX) > 0.1f ? PlayerState.Run : PlayerState.Idle);
                return;
            }

            // Jump cancels crouch
            if (_ctx.Input.JumpPressed)
            {
                _ctx.Input.ConsumeJump();
                _ctx.Input.ClearCrouch();
                _controller.ChangeState(PlayerState.Jump);
                return;
            }
            
            if (_ctx.Input.DashPressed && !_ctx.IsDashOnCooldown)
            {
                _ctx.Input.ConsumeDash();
                _controller.ChangeState(PlayerState.Dash);
                return;
            }

            // No input → CrouchIdle
            if (Mathf.Abs(_ctx.Input.MoveX) < 0.1f)
            {
                _controller.ChangeState(PlayerState.CrouchIdle);
                return;
            }
        }

        public void FixedTick()
        {
            if (!_ctx.IsGroundedOrCoyote)
            {
                _controller.ChangeState(PlayerState.Fall);
                return;
            }

            ApplyCrouchMovement();
        }

        private void ApplyCrouchMovement()
        {
            float input       = _ctx.Input.MoveX;
            float targetVelX  = input * _ctx.Stats.crouchSpeed;
            float currentVelX = _ctx.Rb.linearVelocity.x;

            float newVelX = Mathf.MoveTowards(
                currentVelX,
                targetVelX,
                _ctx.Stats.acceleration * Time.fixedDeltaTime);

            Vector3 v = _ctx.Rb.linearVelocity;
            v.x = newVelX;
            _ctx.Rb.linearVelocity = v;

            // Crouch walk maps to MoveSpeed 0→1 range only (Walk blend in Blend Tree)
            float absSpeed = Mathf.Abs(newVelX);
            _ctx.AnimMoveSpeed = Mathf.Clamp(
                Mathf.InverseLerp(0f, _ctx.Stats.crouchSpeed, absSpeed),
                0f, 1f);
        }
    }
}