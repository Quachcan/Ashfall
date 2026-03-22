using Sirenix.OdinInspector;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// Manages player stamina runtime — drain, regen, exhaustion.
    /// All config values (costs, rates) live in PlayerStats SO.
    ///
    /// Drain sources:
    ///   Dash    : flat cost on dash start
    ///   Sprint  : per-second drain while sprinting
    ///   Attack  : flat cost per attack (wired by CombatSystem later)
    ///   Block   : flat cost per blocked hit (wired by HurtboxController later)
    ///
    /// Regen: starts after Stats.staminaRegenDelay seconds of no drain.
    /// </summary>
    public class StaminaSystem : MonoBehaviour
    {
        // ── Runtime (Inspector readonly) ──────────────────────────────────

        [TitleGroup("Runtime")]
        [BoxGroup("Runtime/Box")]
        [ProgressBar(0, "_maxStamina", ColorGetter = "StaminaBarColor", Height = 20)]
        [ShowInInspector, ReadOnly, LabelText("Stamina")]
        private float _current;

        [BoxGroup("Runtime/Box")]
        [ShowInInspector, ReadOnly, LabelText("Regen Delay Timer")]
        private float _regenDelayTimer;

        [BoxGroup("Runtime/Box")]
        [ShowInInspector, ReadOnly, LabelText("Is Exhausted")]
        private bool _isExhausted;

#if UNITY_EDITOR
        // Called by Odin every Inspector repaint — requests another repaint
        // so ShowInInspector fields and ProgressBar update continuously in Play Mode

#endif

        // ── Dependencies ──────────────────────────────────────────────────

        private PlayerStats _stats;

        // Cached for ProgressBar max reference
        private float _maxStamina => _stats ? _stats.maxStamina : 100f;

        // ── Public API ────────────────────────────────────────────────────

        public float Current    => _current;
        public float Normalized => _stats ? _current / _stats.maxStamina : 1f;
        public bool  IsExhausted => _isExhausted;

        /// <summary>True if stamina is sufficient for the given cost.</summary>
        public bool Has(float cost) => !_isExhausted && _current >= cost;

        // ── Unity Lifecycle ───────────────────────────────────────────────

        /// <summary>
        /// Called by PlayerController after Stats SO is validated.
        /// </summary>
        public void Initialize(PlayerStats stats)
        {
            _stats   = stats;
            _current = stats.maxStamina;
        }

private void Update()
        {
            if (_stats == null) return;
            TickRegenDelay();
            TickRegen();
            UpdateExhaustedState();

#if UNITY_EDITOR
            if (UnityEditor.Selection.activeGameObject == gameObject)
                UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        // ── Drain Methods ─────────────────────────────────────────────────

        /// <summary>Try to spend stamina for a dash. Returns false if insufficient.</summary>
        public bool TrySpendDash()   => TrySpend(_stats.dashStaminaCost);

        /// <summary>Try to spend stamina for an attack. Returns false if insufficient.</summary>
        public bool TrySpendAttack() => TrySpend(_stats.attackStaminaCost);

        /// <summary>Try to spend stamina for a block hit. Returns false if insufficient.</summary>
        public bool TrySpendBlock()  => TrySpend(_stats.blockStaminaCost);

        /// <summary>
        /// Drain stamina per second for sprint. Call every Update while sprinting.
        /// Returns false when stamina runs out — caller should exit sprint.
        /// </summary>
        public bool DrainSprint()
        {
            if (_isExhausted) return false;
            Drain(_stats.sprintStaminaDrain * Time.deltaTime);
            return _current > 0f;
        }

        // ── Restore Methods ───────────────────────────────────────────────

        /// <summary>Restore stamina to full (Grace Point, potion, etc.).</summary>
        public void RestoreFull()
        {
            _current         = _stats.maxStamina;
            _regenDelayTimer = 0f;
            _isExhausted     = false;
        }

        /// <summary>Restore a specific amount.</summary>
        public void Restore(float amount)
        {
            _current = Mathf.Min(_current + amount, _stats.maxStamina);
        }

        // ── Private ───────────────────────────────────────────────────────

        private bool TrySpend(float cost)
        {
            if (_isExhausted || _current < cost) return false;
            Drain(cost);
            return true;
        }

        private void Drain(float amount)
        {
            _current         = Mathf.Max(0f, _current - amount);
            _regenDelayTimer = _stats.staminaRegenDelay;
        }

        private void TickRegenDelay()
        {
            if (_regenDelayTimer > 0f)
                _regenDelayTimer -= Time.deltaTime;
        }

        private void TickRegen()
        {
            if (_regenDelayTimer > 0f)          return;
            if (_current >= _stats.maxStamina)  return;

            _current = Mathf.Min(
                _current + _stats.staminaRegenRate * Time.deltaTime,
                _stats.maxStamina);
        }

        private void UpdateExhaustedState()
        {
            if (_isExhausted)
            {
                // Recover from exhaustion at 25% threshold to prevent flicker
                if (_current >= _stats.maxStamina * 0.25f)
                    _isExhausted = false;
            }
            else
            {
                if (_current <= 0f)
                    _isExhausted = true;
            }
        }

        // ── Odin color helper ─────────────────────────────────────────────

        private Color StaminaBarColor =>
            _isExhausted       ? Color.red :
            Normalized < 0.25f ? new Color(1f, 0.5f, 0f) :
            Normalized < 0.5f  ? Color.yellow :
                                 new Color(0.4f, 0.9f, 0.4f);

        // ── Debug Buttons ─────────────────────────────────────────────────

#if UNITY_EDITOR
        [TitleGroup("Debug")]
        [HorizontalGroup("Debug/Buttons")]
        [Button("Drain 25", ButtonSizes.Medium), GUIColor(1f, 0.5f, 0.3f)]
        private void DebugDrain25()    => Drain(25f);

        [HorizontalGroup("Debug/Buttons")]
        [Button("Restore Full", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 0.4f)]
        private void DebugRestore()    => RestoreFull();

        [HorizontalGroup("Debug/Buttons")]
        [Button("Empty", ButtonSizes.Medium), GUIColor(0.8f, 0.2f, 0.2f)]
        private void DebugEmpty()      => Drain(_stats ? _stats.maxStamina : 100f);
#endif
    }
}