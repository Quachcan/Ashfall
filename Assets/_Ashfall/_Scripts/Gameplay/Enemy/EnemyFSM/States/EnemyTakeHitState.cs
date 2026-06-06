using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Enemy.EnemyFSM.States
{
    public class EnemyTakeHitState : IState
    {
        private readonly EnemyController _controller;
        private readonly EnemyContext    _ctx;
        private float _timer;

        private static readonly int HitReactHash = Animator.StringToHash("HitReact");

        public EnemyTakeHitState(EnemyController controller, EnemyContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            _timer = 0.4f; // Hit reaction duration
            _ctx.Animator?.SetTrigger(HitReactHash);
        }

        public void Exit() { }

        public void Tick()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f) _controller.ChangeState(EnemyState.Idle);
        }

        public void FixedTick() { }
    }
}