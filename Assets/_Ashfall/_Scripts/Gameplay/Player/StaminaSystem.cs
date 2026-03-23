using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// Pure C# stamina system — no MonoBehaviour, no Update overhead.
    /// Owned and ticked by PlayerController each frame via Tick(dt).
    /// All config values live in PlayerStats SO.
    ///
    /// Drain sources:
    ///   Dash    : flat cost on dash start
    ///   Sprint  : per-second drain while sprinting
    ///   Attack  : flat cost per attack
    ///   Block   : flat cost per blocked hit
    /// Regen: starts after Stats.staminaRegenDelay seconds of no drain.
    /// </summary>
    public class StaminaSystem
    {
        // ── Config ────────────────────────────────────────────────────────

        private PlayerStats _stats;

        // ── Runtime ───────────────────────────────────────────────────────

        public float Current     { get; private set; }
        public float Normalized  => _stats != null ? Current / _stats.maxStamina : 1f;
        public bool  IsExhausted { get; private set; }

        private float _regenDelayTimer;

        // ── Events ────────────────────────────────────────────────────────

        public event System.Action OnExhausted;
        public event System.Action OnRecovered;

        // ── Constructor ───────────────────────────────────────────────────

        public StaminaSystem(PlayerStats stats)
        {
            _stats  = stats;
            Current = stats.maxStamina;
        }

        // ── Tick ──────────────────────────────────────────────────────────

        /// <summary>Called by PlayerController every Update — no MonoBehaviour needed.</summary>
        public void Tick(float dt)
        {
            if (_stats == null) return;
            TickRegenDelay(dt);
            TickRegen(dt);
            UpdateExhaustedState();
        }

        // ── Drain API ─────────────────────────────────────────────────────

        public bool TrySpendDash()   => TrySpend(_stats.dashStaminaCost);
        public bool TrySpendAttack() => TrySpend(_stats.attackStaminaCost);
        public bool TrySpendBlock()  => TrySpend(_stats.blockStaminaCost);

        /// <summary>
        /// Drain stamina per second for sprint.
        /// Returns false when stamina runs out — caller should exit sprint.
        /// </summary>
        public bool DrainSprint(float dt)
        {
            if (IsExhausted) return false;
            Drain(_stats.sprintStaminaDrain * dt);
            return Current > 0f;
        }

        public bool Has(float cost) => !IsExhausted && Current >= cost;

        // ── Restore API ───────────────────────────────────────────────────

        public void RestoreFull()
        {
            Current          = _stats.maxStamina;
            _regenDelayTimer = 0f;
            IsExhausted      = false;
        }

        public void Restore(float amount)
        {
            Current = Mathf.Min(Current + amount, _stats.maxStamina);
        }

        // ── Private ───────────────────────────────────────────────────────

        private bool TrySpend(float cost)
        {
            if (IsExhausted || Current < cost) return false;
            Drain(cost);
            return true;
        }

        private void Drain(float amount)
        {
            Current          = Mathf.Max(0f, Current - amount);
            _regenDelayTimer = _stats.staminaRegenDelay;
        }

        private void TickRegenDelay(float dt)
        {
            if (_regenDelayTimer > 0f)
                _regenDelayTimer -= dt;
        }

        private void TickRegen(float dt)
        {
            if (_regenDelayTimer > 0f)         return;
            if (Current >= _stats.maxStamina)  return;

            Current = Mathf.Min(
                Current + _stats.staminaRegenRate * dt,
                _stats.maxStamina);
        }

        private void UpdateExhaustedState()
        {
            if (IsExhausted)
            {
                if (Current >= _stats.maxStamina * 0.25f)
                {
                    IsExhausted = false;
                    OnRecovered?.Invoke();
                }
            }
            else if (Current <= 0f)
            {
                IsExhausted = true;
                OnExhausted?.Invoke();
            }
        }
    }
}