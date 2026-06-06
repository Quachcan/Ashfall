using _Ashfall._Scripts.Core.StateMachineCore;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
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
            _ctx.Animator?.SetTrigger(AnimHash.Dead);
        }

        public void Exit() { }
        public void Tick() { }
        public void FixedTick() { }
    }
}