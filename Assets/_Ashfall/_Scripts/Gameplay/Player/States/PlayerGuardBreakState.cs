using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Player's guard is broken — stamina depleted while blocking.
    /// Player is briefly staggered and vulnerable.
    /// Cannot block or attack during this state.
    ///
    /// Transitions:
    ///   Duration expires → Idle
    /// </summary>
    public class PlayerGuardBreakState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        private float _timer;

        public PlayerGuardBreakState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            _timer = _ctx.Stats.guardBreakDuration;

            // Stop movement
            var v = _ctx.Rb.linearVelocity;
            v.x = 0f;
            _ctx.Rb.linearVelocity = v;

            _ctx.AnimMoveSpeed = 0f;
            _ctx.Animator?.SetTrigger(AnimHash.GuardBreak);

            // TODO: raise EventHub event
            // _eventHub.playerEvents.onGuardBreak.Raise();
        }

        public void Exit() { }

        public void Tick()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
                _controller.ChangeState(PlayerState.Idle);
        }

        public void FixedTick() { }
    }
}