using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    public class PlayerTakeHitState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;
        private float _timer;

        public PlayerTakeHitState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            _timer = 0.4f;
            _ctx.Animator?.SetTrigger(AnimHash.HitReact);
        }

        public void Exit() { }

        public void Tick()
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f) _controller.ChangeState(PlayerState.Idle);
        }

        public void FixedTick() { }
    }
}