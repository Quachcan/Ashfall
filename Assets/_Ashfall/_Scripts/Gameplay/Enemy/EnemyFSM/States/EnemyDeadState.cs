using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Enemy.EnemyFSM.States
{
    public class EnemyDeadState : IState
    {
        private readonly EnemyController _controller;
        private readonly EnemyContext    _ctx;
        
        private static readonly int DeadHash = Animator.StringToHash("Dead");

        public EnemyDeadState(EnemyController controller, EnemyContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            _ctx.Animator?.SetTrigger(DeadHash);
        }

        public void Exit() { }
        public void Tick() { }
        public void FixedTick() { }
    }
}