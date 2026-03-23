using System;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    /// <summary>
    /// Pure C# health system — no MonoBehaviour, no Update overhead.
    /// Owned and ticked by PlayerController or EnemyBase each frame.
    /// No auto-regen — HP only restored at Grace Point or via item.
    /// </summary>
    public class HealthSystem
    {
        // ── Config ────────────────────────────────────────────────────────

        private float _maxHp;

        // ── Runtime ───────────────────────────────────────────────────────

        public float CurrentHp  { get; private set; }
        public float MaxHp      => _maxHp;
        public float Normalized => _maxHp > 0f ? CurrentHp / _maxHp : 0f;
        public bool  IsDead     { get; private set; }

        // ── Events ────────────────────────────────────────────────────────

        /// <summary>Raised when damage is taken. float = final damage amount.</summary>
        public event Action<float> OnDamageTaken;

        /// <summary>Raised when HP is restored. float = amount restored.</summary>
        public event Action<float> OnHealed;

        /// <summary>Raised when HP reaches 0.</summary>
        public event Action OnDeath;

        // ── Constructor ───────────────────────────────────────────────────

        public HealthSystem(float maxHp)
        {
            _maxHp    = maxHp;
            CurrentHp = maxHp;
            IsDead    = false;
        }

        // ── Damage ────────────────────────────────────────────────────────

        /// <summary>
        /// Apply damage. Called by HurtboxController after ATK/DEF calculation.
        /// </summary>
        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f) return;

            CurrentHp = Mathf.Max(0f, CurrentHp - amount);
            OnDamageTaken?.Invoke(amount);

            if (CurrentHp <= 0f)
                Die();
        }

        // ── Heal ──────────────────────────────────────────────────────────

        /// <summary>Restore HP to full — called by Grace Point.</summary>
        public void RestoreFull()
        {
            if (IsDead) return;
            float amount = _maxHp - CurrentHp;
            if (amount <= 0f) return;

            CurrentHp = _maxHp;
            OnHealed?.Invoke(amount);
        }

        /// <summary>Restore a specific amount — called by healing item.</summary>
        public void Restore(float amount)
        {
            if (IsDead || amount <= 0f) return;

            float actual = Mathf.Min(amount, _maxHp - CurrentHp);
            CurrentHp   += actual;
            OnHealed?.Invoke(actual);
        }

        /// <summary>
        /// Update max HP — called by StatSystem on level up or equipment change.
        /// Scales current HP proportionally.
        /// </summary>
        public void SetMaxHp(float newMax, bool scaleCurrentHp = true)
        {
            if (newMax <= 0f) return;

            float ratio = scaleCurrentHp ? (CurrentHp / _maxHp) : 1f;
            _maxHp      = newMax;

            CurrentHp = scaleCurrentHp
                ? Mathf.Min(_maxHp * ratio, _maxHp)
                : Mathf.Min(CurrentHp, _maxHp);
        }

        /// <summary>Revive with a given HP — called by Grace Point respawn.</summary>
        public void Revive(float hp = -1f)
        {
            IsDead    = false;
            CurrentHp = hp > 0f ? Mathf.Min(hp, _maxHp) : _maxHp;
        }

        // ── Private ───────────────────────────────────────────────────────

        private void Die()
        {
            if (IsDead) return;
            IsDead    = true;
            CurrentHp = 0f;
            OnDeath?.Invoke();
        }
    }
}