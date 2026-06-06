using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorEventBridge : MonoBehaviour
    {
        private PlayerController _controller;

        private void Awake()
        {
            _controller = GetComponentInParent<PlayerController>();

            if (!_controller)
                Debug.LogWarning("[AnimatorEventBridge] PlayerController not found in parent!", this);
        }

        public void OnAttackHit() => _controller?.OnAttackHit();
        
        public void OnAttackEnd() => _controller?.OnAttackEnd();
        
        public void OnDeathSettled() => _controller?.OnDeathSettled();
    }
}