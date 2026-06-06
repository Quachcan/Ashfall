using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;
using DG.Tweening;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    public class PlayerDashState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        public bool IsDashingToEnemy;
        private const float ENGAGE_DISTANCE = 1.5f; 
        private Vector3 _homePosition;

        public PlayerDashState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
            _homePosition = _ctx.Transform.position;
        }

        public void Enter()
        {
            if (_homePosition == Vector3.zero) _homePosition = _ctx.Transform.position;

            _ctx.AnimMoveSpeed = 0f;
            _ctx.Animator?.SetTrigger(AnimHash.Dash);

            Vector3 targetPos = CalculateTargetPosition();

            _ctx.Transform.DOMove(targetPos, _ctx.Stats.dashDuration)
                .SetEase(Ease.OutCubic)
                .OnComplete(OnDashComplete);
        }

        public void Exit() { }
        public void Tick() { }
        public void FixedTick() { }

        private Vector3 CalculateTargetPosition()
        {
            if (!IsDashingToEnemy) return _homePosition;

            Vector3 enemyPos = new Vector3(5f, _homePosition.y, _homePosition.z);
            float dirToPlayer = Mathf.Sign(_homePosition.x - enemyPos.x);

            Vector3 engagePos = enemyPos;
            engagePos.x += dirToPlayer * ENGAGE_DISTANCE;
            
            return engagePos;
        }

        private void OnDashComplete()
        {
            _controller.ChangeState(IsDashingToEnemy ? PlayerState.Attack : PlayerState.Idle);
        }
    }
}