using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    public class PlayerAttackState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        public PlayerAttackState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            if (_ctx.Weapon?.Current == null)
            {
                _controller.ChangeState(PlayerState.Idle);
                return;
            }

            _ctx.AnimMoveSpeed = 0f;

            int stateHash = AnimHash.GetAttackStateHash(1);
            _ctx.Animator.CrossFade(stateHash, 0.05f, 0);
        }

        public void Exit() { }
        public void Tick() { }
        public void FixedTick() { }

        public void OnAttackHit()
        {
        }

        public void OnAttackEnd()
        {
            var dashState = _controller.GetState<PlayerDashState>(PlayerState.Dash);
            if (dashState != null)
            {
                dashState.IsDashingToEnemy = false;
                _controller.ChangeState(PlayerState.Dash);
            }
            else
            {
                _controller.ChangeState(PlayerState.Idle);
            }
        }
    }
}