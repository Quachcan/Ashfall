using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Player has died. Freezes all input and movement.
    /// Triggers the death animation and notifies the EventHub.
    /// The Grace Point / Respawn system will call ChangeState(Idle) after the respawn sequence.
    /// </summary>
    public class PlayerDeadState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        public PlayerDeadState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            // Stop all movement
            _ctx.Rb.linearVelocity = Vector3.zero;
            _ctx.Rb.isKinematic    = true;

            _ctx.Animator?.SetTrigger(AnimHash.Dead);

            // TODO: raise EventHub event
            // _eventHub.playerEvents.onPlayerDead.Raise();
        }

        public void Exit()
        {
            // Re-enable physics for respawn
            _ctx.Rb.isKinematic = false;
            _ctx.JumpsUsed      = 0;
        }

        /// <summary>Dead state ignores all input.</summary>
        public void Tick() { }

        public void FixedTick() { }
    }
}