using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Player is standing still on the ground.
    /// Transitions: move input → Run | jump → Jump | fall off ledge → Fall | dash → Dash | attack → Attack
    /// </summary>
    public class PlayerIdleState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        public PlayerIdleState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            // Stop horizontal movement
            SetHorizontalVelocity(0f);
            _ctx.AnimMoveSpeed = 0f;
        }

        public void Exit() { }

        public void Tick()
        {
            if (_ctx.Input.CrouchToggled)
            {
                _ctx.Input.ConsumeCrouch();
                _controller.ChangeState(PlayerState.CrouchIdle);
                return;
            }
            
            // Jump input
            if (_ctx.Input.JumpPressed)
            {
                _ctx.Input.ConsumeJump();
                _controller.ChangeState(PlayerState.Jump);
                return;
            }

            // Dash input
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

            // Move input → Run
            if (Mathf.Abs(_ctx.Input.MoveX) > 0.1f)
            {
                _controller.ChangeState(PlayerState.Run);
                return;
            }
        }

        public void FixedTick()
        {
            // Transition to Fall if walked off a ledge.
            // IsGroundedOrCoyote gives a small window so we don't
            // flicker into Fall on the very frame we step off an edge.
            if (!_ctx.IsGroundedOrCoyote)
            {
                _controller.ChangeState(PlayerState.Fall);
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void SetHorizontalVelocity(float x)
        {
            Vector3 v = _ctx.Rb.linearVelocity;
            v.x = x;
            _ctx.Rb.linearVelocity = v;
        }
    }
}