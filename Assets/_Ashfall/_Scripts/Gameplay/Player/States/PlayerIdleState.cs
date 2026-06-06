using _Ashfall._Scripts.Core.StateMachineCore;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
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
            _ctx.AnimMoveSpeed = 0f;
        }

        public void Exit() { }
        public void Tick() { }
        public void FixedTick() { }
    }
}