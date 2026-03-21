using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Player is performing a basic attack.
    /// This is a stub state — the full attack combo logic will be implemented
    /// in the Combat system task. For now it plays the animation and returns
    /// to Idle/Fall once the animation finishes.
    /// </summary>
    public class PlayerAttackState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        /// <summary>Fallback duration if no animation event is wired up yet.</summary>
        private const float FallbackAttackDuration = 0.4f;

        private float _attackTimer;

        public PlayerAttackState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            _attackTimer = FallbackAttackDuration;

            // Briefly stop horizontal movement during the attack startup
            Vector3 v = _ctx.Rb.linearVelocity;
            v.x = 0f;
            _ctx.Rb.linearVelocity = v;

            _ctx.Animator?.SetTrigger(AnimHash.Attack);
        }

        public void Exit() { }

        public void Tick()
        {
            _attackTimer -= Time.deltaTime;

            if (_attackTimer <= 0f)
                EndAttack();
        }

        public void FixedTick() { }

        // ── Helpers ───────────────────────────────────────────────────────

        private void EndAttack()
        {
            if (_ctx.IsGrounded)
                _controller.ChangeState(PlayerState.Idle);
            else
                _controller.ChangeState(PlayerState.Fall);
        }

        // Called by Animator event when the attack animation finishes
        // Wire this up in the Animation Controller later
        public void OnAttackAnimationEnd() => EndAttack();
    }
}