using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
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
            _timer = 2f; // Thời gian bị Stagger
            _ctx.AnimMoveSpeed = 0f;
            _ctx.Animator?.SetTrigger(AnimHash.GuardBreak);
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