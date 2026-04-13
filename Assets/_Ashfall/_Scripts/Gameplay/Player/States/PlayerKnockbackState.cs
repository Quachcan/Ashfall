using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Player was hit by a strong attack and is knocked back.
    /// Locks all input for the duration — physics impulse is already
    /// applied by HurtboxController.ApplyKnockback before this state enters.
    ///
    /// Transitions: timer ends + grounded → Idle | timer ends + airborne → Fall
    /// </summary>
    public class PlayerKnockbackState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        private float _timer;

        public PlayerKnockbackState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            _timer = _ctx.PendingKnockbackDuration;

            _ctx.Animator?.SetTrigger(AnimHash.HitReact);

            // Cancel any attack hitbox that may be active
            _ctx.Hitbox?.SetActive(false);
        }

        public void Exit() { }

        public void Tick()
        {
            _timer -= Time.deltaTime;

            if (_timer > 0f) return;

            _controller.ChangeState(_ctx.IsGrounded ? PlayerState.Idle : PlayerState.Fall);
        }

        public void FixedTick() { }
    }
}
