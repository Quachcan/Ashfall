using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Player is crouching and standing still.
    /// Transitions:
    ///   move input      → CrouchWalk
    ///   crouch toggle   → Idle
    ///   jump            → Jump (cancels crouch)
    ///   fall off ledge  → Fall
    /// </summary>
    public class PlayerCrouchIdleState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        public PlayerCrouchIdleState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            SetHorizontalVelocity(0f);
            _ctx.AnimMoveSpeed = 0f;
            _ctx.Animator?.SetBool(AnimHash.IsCrouching, true);
        }

        public void Exit()
        {
            _ctx.Animator?.SetBool(AnimHash.IsCrouching, false);
        }

        public void Tick()
        {
            // Toggle crouch off → stand up
            if (_ctx.Input.CrouchToggled)
            {
                _ctx.Input.ConsumeCrouch();
                _controller.ChangeState(PlayerState.Idle);
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
            
            if (_ctx.Input.DashPressed && !_ctx.IsDashOnCooldown &&   _ctx.Stamina.Has(_ctx.Stats.dashStaminaCost))
            {
                _ctx.Input.ConsumeDash();
                _controller.ChangeState(PlayerState.Dash);
                return;
            }

            // Move input → CrouchWalk
            if (Mathf.Abs(_ctx.Input.MoveX) > 0.1f)
            {
                _controller.ChangeState(PlayerState.CrouchWalk);
                return;
            }
        }

        public void FixedTick()
        {
            if (!_ctx.IsGroundedOrCoyote)
                _controller.ChangeState(PlayerState.Fall);
        }

        private void SetHorizontalVelocity(float x)
        {
            Vector3 v = _ctx.Rb.linearVelocity;
            v.x = x;
            _ctx.Rb.linearVelocity = v;
        }
    }
}