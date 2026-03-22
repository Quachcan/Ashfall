using _Ashfall._Scripts.Core.StateMachineCore;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player.States
{
    /// <summary>
    /// Handles the 3-hit attack combo.
    ///
    /// Combo flow:
    ///   - Enter with ComboIndex 1 → play Attack_1
    ///   - Press attack during current hit → queue next hit (nextAttackQueued = true)
    ///   - When current hit finishes → if queued, advance to next index
    ///   - If no input within comboWindowTime → return to Idle
    ///   - After final hit (comboLength) → return to Idle
    ///   - Taking damage cancels combo (handled by HurtboxController calling ChangeState)
    ///
    /// Animation setup:
    ///   Animator needs an Attack SSM with states Attack_1, Attack_2, Attack_3.
    ///   Each state reads ComboIndex int to decide which clip to play.
    ///   Wire Animator events OnAttackHit() and OnAttackEnd() on each clip.
    /// </summary>
    public class PlayerAttackState : IState
    {
        private readonly PlayerController _controller;
        private readonly PlayerContext    _ctx;

        // ── Combo State ───────────────────────────────────────────────────

        /// <summary>Current hit index in the combo (1-based: 1, 2, 3).</summary>
        private int   _comboIndex;

        /// <summary>True when player pressed attack during current hit — advance on hit end.</summary>
        private bool  _nextAttackQueued;

        /// <summary>Countdown timer for combo window — resets on each hit start.</summary>
        private float _comboWindowTimer;

        /// <summary>Fallback timer in case Animator events are not wired yet.</summary>
        private float _attackTimer;

        /// <summary>True while the current hit animation is still playing.</summary>
        private bool  _hitInProgress;

        public PlayerAttackState(PlayerController controller, PlayerContext ctx)
        {
            _controller = controller;
            _ctx        = ctx;
        }

        // ── IState ────────────────────────────────────────────────────────

        public void Enter()
        {
            // Always start or re-enter at index 1
            _comboIndex        = 1;
            _nextAttackQueued  = false;

            // Spend stamina for first hit
            _ctx.Stamina.TrySpendAttack();

            StartHit(_comboIndex);
        }

        public void Exit()
        {
            _comboIndex       = 0;
            _nextAttackQueued = false;
            _hitInProgress    = false;
            _ctx.AnimMoveSpeed = 0f;
        }

        public void Tick()
        {
            // Queue next attack if input received during current hit
            if (_ctx.Input.AttackPressed)
            {
                _ctx.Input.ConsumeAttack();

                bool canContinue = _comboIndex < _ctx.Stats.ComboLength;
                bool hasStamina  = _ctx.Stamina.Has(_ctx.Stats.attackStaminaCost);

                if (canContinue && hasStamina)
                    _nextAttackQueued = true;
            }

            // Tick fallback timer
            if (_hitInProgress)
            {
                _attackTimer -= Time.deltaTime;
                if (_attackTimer <= 0f)
                    OnAttackEnd(); // fallback if no Animator event
            }
            else
            {
                // Between hits — tick combo window
                _comboWindowTimer -= Time.deltaTime;
                if (_comboWindowTimer <= 0f)
                    EndCombo(); // window expired, no input
            }

            // Allow limited movement during attack
            ApplyAttackMovement();
        }

        public void FixedTick() { }

        // ── Animator Event Callbacks ──────────────────────────────────────

        /// <summary>
        /// Called by Animator event at the moment the hit lands.
        /// Wire this on each Attack_N animation clip at the impact frame.
        /// Used later for hitbox activation.
        /// </summary>
        public void OnAttackHit()
        {
            // TODO: activate hitbox for this frame
            // _ctx.Combat.ActivateHitbox(_comboIndex);
        }

        /// <summary>
        /// Called by Animator event when the attack animation finishes.
        /// Wire this at the last frame of each Attack_N clip.
        /// </summary>
        public void OnAttackEnd()
        {
            _hitInProgress = false;
            _attackTimer   = 0f;

            if (_nextAttackQueued && _comboIndex < _ctx.Stats.ComboLength)
            {
                // Advance to next hit
                _nextAttackQueued = false;
                _comboIndex++;
                _ctx.Stamina.TrySpendAttack();
                StartHit(_comboIndex);
            }
            else
            {
                // No queued input — open combo window and wait
                _comboWindowTimer = _ctx.Stats.comboWindowTime;
            }
        }

        // ── Private ───────────────────────────────────────────────────────

        private void StartHit(int index)
        {
            _hitInProgress    = true;
            _nextAttackQueued = false;
            _attackTimer      = _ctx.Stats.attackDuration;

            if (_ctx.Animator == null) return;
            if (index <= 0 || index > _ctx.Stats.ComboLength) return;

            // Generate state hash from naming convention "Attack_1", "Attack_2"...
            // No hardcoded array — comboLength in SO controls how many hits exist
            _ctx.Animator.CrossFade(AnimHash.GetAttackStateHash(index), 0.05f, 0);
        }

        private void EndCombo()
        {
            bool hasInput = Mathf.Abs(_ctx.Input.MoveX) > 0.1f;
            _controller.ChangeState(hasInput ? PlayerState.Run : PlayerState.Idle);
        }

        private void ApplyAttackMovement()
        {
            // Slow movement during attack — not completely stopped
            // so the player still has some control feel
            float input   = _ctx.Input.MoveX;
            float targetX = input * _ctx.Stats.moveSpeed * _ctx.Stats.attackMoveScale;
            float current = _ctx.Rb.linearVelocity.x;

            float newX = Mathf.MoveTowards(current, targetX,
                _ctx.Stats.acceleration * Time.deltaTime);

            Vector3 v = _ctx.Rb.linearVelocity;
            v.x = newX;
            _ctx.Rb.linearVelocity = v;
        }
    }
}