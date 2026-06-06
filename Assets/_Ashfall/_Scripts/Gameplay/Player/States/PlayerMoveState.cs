using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;
using DG.Tweening;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    public class PlayerMoveState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        public Vector3 TargetPosition;
        public float   MoveDuration = 1.5f;

        public PlayerMoveState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            _ctx.AnimMoveSpeed = 2f;

            float dir = Mathf.Sign(TargetPosition.x - _ctx.Transform.position.x);
            _controller.SetFacingDirection(dir);

            _ctx.Transform.DOMove(TargetPosition, MoveDuration)
                .SetEase(Ease.Linear)
                .OnComplete(OnMoveComplete);
        }

        public void Exit() { }
        public void Tick() { }
        public void FixedTick() { }

        private void OnMoveComplete()
        {
            _ctx.AnimMoveSpeed = 0f;
            _controller.ChangeState(PlayerState.Idle);
        }
    }
}