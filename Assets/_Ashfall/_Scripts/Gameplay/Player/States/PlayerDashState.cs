using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Player performs a quick dash in the facing direction.
    /// Freezes vertical velocity during the dash for a clean horizontal feel.
    /// Includes an invincibility frame window (i-frame) hook for the combat system later.
    /// Transitions: dash ends + grounded → Idle/Run | dash ends + airborne → Fall
    /// </summary>
    public class PlayerDashState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        private float _dashTimer;

        public PlayerDashState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            _dashTimer = _ctx.Stats.dashDuration;

            // Arm cooldown immediately so it ticks even if the state exits early
            _ctx.IsDashOnCooldown  = true;
            _ctx.DashCooldownTimer = _ctx.Stats.dashCooldown;

            // Apply dash velocity — freeze Y so the dash is purely horizontal
            Vector3 v = _ctx.Rb.linearVelocity;
            v.x = _ctx.FacingDirection * _ctx.Stats.dashSpeed;
            v.y = 0f;
            _ctx.Rb.linearVelocity = v;

            // Disable gravity during dash for consistent distance
            _ctx.Rb.useGravity = false;

            _ctx.Animator?.SetTrigger(AnimHash.Dash);

            // TODO: enable i-frames via combat system
            // _ctx.Combat.SetInvincible(true, _ctx.Stats.dashDuration);
        }

        public void Exit()
        {
            // Re-enable gravity when dash ends
            _ctx.Rb.useGravity = true;

            // TODO: disable i-frames
            // _ctx.Combat.SetInvincible(false);
        }

        public void Tick()
        {
            _dashTimer -= Time.deltaTime;

            if (_dashTimer <= 0f)
                EndDash();
        }

        public void FixedTick() { }

        // ── Helpers ───────────────────────────────────────────────────────

        private void EndDash()
        {
            // Bleed off dash speed so landing doesn't feel floaty
            Vector3 v = _ctx.Rb.linearVelocity;
            v.x = _ctx.FacingDirection * _ctx.Stats.moveSpeed;
            _ctx.Rb.linearVelocity = v;

            if (_ctx.IsGrounded)
            {
                bool hasInput = Mathf.Abs(_ctx.Input.MoveX) > 0.1f;
                _controller.ChangeState(hasInput ? PlayerState.Run : PlayerState.Idle);
            }
            else
            {
                _controller.ChangeState(PlayerState.Fall);
            }
        }
    }
}