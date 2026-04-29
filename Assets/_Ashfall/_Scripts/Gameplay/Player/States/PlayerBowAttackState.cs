using _Ashfall._Scripts.Core.StateMachineCore;
using _Ashfall._Scripts.Gameplay.Combat;
using _Ashfall._Scripts.Gameplay.Weapons;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    public class PlayerBowAttackState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        private enum BowPhase { Drawing, Releasing }
        private BowPhase _phase;

        private float _chargeTimer;
        
        private float _releaseTimer;
        
        private bool _arrowFired;

        public PlayerBowAttackState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        public void Enter()
        {
            var weapon = _ctx.Weapon?.Current;
            if (weapon == null || weapon.weaponType != WeaponType.Bow ||
                weapon.ComboLength == 0)
            {
                Debug.LogError("[PlayerBowAttackState] Invalid weapon or no AttackData assigned.", _ctx.Rb);
                _controller.ChangeState(PlayerState.Idle);
                return;
            }

            _ctx.Input.ConsumeAttack();

            _chargeTimer  = 0f;
            _releaseTimer = 0f;
            _arrowFired   = false;
            _phase        = BowPhase.Drawing;

            if (_ctx.Animator != null)
            {
                _ctx.Animator.SetFloat(AnimHash.BowChargeProgress, 0f);
                _ctx.Animator.CrossFade(AnimHash.BowDrawState, 0.1f, 0);
            }

            _ctx.AnimMoveSpeed = 0f;
        }

        public void Exit()
        {
            _chargeTimer  = 0f;
            _releaseTimer = 0f;
            _arrowFired   = false;
            _phase        = BowPhase.Drawing;

            if (_ctx.Animator != null)
                _ctx.Animator.SetFloat(AnimHash.BowChargeProgress, 0f);

            _ctx.AnimMoveSpeed = 0f;
        }

        public void Tick()
        {
            var weapon = _ctx.Weapon?.Current;
            if (weapon == null) { EndAttack(); return; }

            switch (_phase)
            {
                case BowPhase.Drawing:
                    TickDrawing(weapon);
                    break;

                case BowPhase.Releasing:
                    TickReleasing();
                    break;
            }
        }

        public void FixedTick() { }

        public void OnBowRelease()
        {
            if (_phase != BowPhase.Releasing || _arrowFired) return;
            SpawnArrow();
            _releaseTimer = 0f;
        }

        private void TickDrawing(WeaponData weapon)
        {
            if (_ctx.Input.DashPressed)
            {
                _ctx.Input.ConsumeDash();
                _controller.ChangeState(PlayerState.Dash);
                return;
            }
            if (_ctx.Input.JumpPressed && _ctx.IsGroundedOrCoyote)
            {
                _controller.ForceJump();
                return;
            }

            if (_ctx.Input.AttackHeld)
            {
                _chargeTimer = Mathf.MoveTowards(_chargeTimer, weapon.chargeTime, Time.deltaTime);

                float progress = weapon.chargeTime > 0f ? _chargeTimer / weapon.chargeTime : 1f;
                if (_ctx.Animator != null)
                    _ctx.Animator.SetFloat(AnimHash.BowChargeProgress, progress);
            }
            else
            {
                BeginRelease(weapon);
            }

            ApplyDrawMovement(weapon);
        }

        private void TickReleasing()
        {
            if (_arrowFired)
            {
                _releaseTimer -= Time.deltaTime;
                if (_releaseTimer <= 0f)
                    EndAttack();
                return;
            }

            _releaseTimer -= Time.deltaTime;
            if (_releaseTimer <= 0f)
            {
                SpawnArrow();
                _releaseTimer = 0.05f;
            }
        }

        private void BeginRelease(WeaponData weapon)
        {
            _phase = BowPhase.Releasing;

            var quickShotData  = weapon.comboAttacks[0];
            _releaseTimer      = quickShotData?.attackDuration ?? 0.4f;

            if (_ctx.Animator != null)
                _ctx.Animator.CrossFade(AnimHash.BowReleaseState, 0.05f, 0);
        }

        private void SpawnArrow()
        {
            if (_arrowFired) return;
            _arrowFired = true;

            var weapon  = _ctx.Weapon?.Current;
            var visuals = _ctx.Weapon?.CurrentVisuals;

            if (weapon == null || visuals == null)
            {
                Debug.LogWarning("[PlayerBowAttackState] Weapon or Visuals is null — cannot spawn arrow.");
                return;
            }

            if (visuals.arrowPrefab == null)
            {
                Debug.LogWarning("[PlayerBowAttackState] arrowPrefab is not assigned in WeaponVisuals_Bow.");
                return;
            }

            bool isCharged = _chargeTimer >= weapon.chargeTime && weapon.ComboLength > 1;
            AttackData data = isCharged ? weapon.comboAttacks[1] : weapon.comboAttacks[0];

            _ctx.Stamina.TrySpend(weapon.attackStaminaCost);

            Transform spawnPoint = _ctx.ArrowSpawnPoint != null
                ? _ctx.ArrowSpawnPoint
                : _ctx.Transform;

            var arrowGO = Object.Instantiate(visuals.arrowPrefab, spawnPoint.position, Quaternion.identity);
            var arrow   = arrowGO.GetComponent<ArrowProjectile>();

            if (arrow == null)
            {
                Debug.LogError("[PlayerBowAttackState] arrowPrefab is missing the ArrowProjectile component!", arrowGO);
                Object.Destroy(arrowGO);
                return;
            }

            arrow.Init(data, _ctx.Transform.gameObject, weapon.arrowSpeed, _ctx.FacingDirection);

            string shotType = isCharged ? "CHARGED" : "quick";
            Debug.Log($"[BowAttackState] Fired [{shotType}] — {data?.attackName ?? "null"}" +
                      $" charge={_chargeTimer:F2}/{weapon.chargeTime:F2}s");
        }

        private void EndAttack()
        {
            bool hasInput = Mathf.Abs(_ctx.Input.MoveX) > 0.1f;
            _controller.ChangeState(hasInput ? PlayerState.Run : PlayerState.Idle);
        }

        private void ApplyDrawMovement(WeaponData weapon)
        {
            float scale   = weapon.ComboLength > 0 ? (weapon.comboAttacks[0]?.attackMoveScale ?? 0f) : 0f;
            float input   = _ctx.Input.MoveX;
            float targetX = input * _ctx.Stats.moveSpeed * scale;
            float current = _ctx.Rb.linearVelocity.x;

            float newX = Mathf.MoveTowards(current, targetX, _ctx.Stats.acceleration * Time.deltaTime);
            Vector3 v  = _ctx.Rb.linearVelocity;
            v.x        = newX;
            _ctx.Rb.linearVelocity = v;

            _ctx.AnimMoveSpeed = input * scale;
        }
    }
}
