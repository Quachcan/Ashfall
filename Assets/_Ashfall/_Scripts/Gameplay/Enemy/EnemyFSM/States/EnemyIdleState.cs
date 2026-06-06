using _Ashfall._Scripts.Core.StateMachineCore;

namespace _Ashfall._Scripts.Gameplay.Enemy.EnemyFSM.States
{
    public class EnemyIdleState : IState
    {
        private readonly EnemyController _controller;
        private readonly EnemyContext    _ctx;

        public EnemyIdleState(EnemyController controller, EnemyContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter() { }
        public void Exit() { }
        public void Tick() { }
        public void FixedTick() { }
    }
}