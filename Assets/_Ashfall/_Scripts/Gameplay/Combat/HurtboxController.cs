using Sirenix.OdinInspector;
using UnityEngine;
using _Ashfall._Scripts.Gameplay.Player;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    /// <summary>
    /// Manages the hurtbox on a character (Player or Enemy).
    /// Implements IHittable — HitboxWeapon calls TakeHit() on this.
    ///
    /// Responsibilities:
    ///   - Receive hits and forward to HealthSystem for HP reduction
    ///   - Apply crit multiplier from AttackData
    ///   - Forward to PostureSystem for stagger
    ///   - Forward knockback to Rigidbody
    ///   - Manage invincibility frames
    ///   - Route hits to block logic when blocking
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class HurtboxController : MonoBehaviour, IHittable
    {
        // ── Inspector ─────────────────────────────────────────────────────

        [TitleGroup("Zone References")]
        [InfoBox("Drag the BlockZone here, or use Auto-Find button below.")]
        [SerializeField] private HurtboxZone blockZone;

        // ── Runtime ───────────────────────────────────────────────────────

        [TitleGroup("Runtime")]
        [ShowInInspector, ReadOnly] private bool  _isInvincible;
        [ShowInInspector, ReadOnly] private bool  _isBlocking;
        [ShowInInspector, ReadOnly] private float _invincibleTimer;

        private Rigidbody     _rb;
        private HealthSystem  _health;
        private ICombatStats  _defenderStats;
        private PostureSystem _posture;

        /// <summary>
        /// Inject systems — called by owner (PlayerController/EnemyBase) after creation.
        /// </summary>
        public void Initialize(Rigidbody rb, HealthSystem health,
                               ICombatStats defenderStats, PostureSystem posture)
        {
            _rb            = rb;
            _health        = health;
            _defenderStats = defenderStats;
            _posture       = posture;

            if (_health != null)
                _health.OnDeath += OnDeath;

            if (_posture != null)
            {
                _posture.OnStagger       += OnStagger;
                _posture.OnStaggerEnd    += OnStaggerEnd;
                _posture.OnFinishingBlow += OnFinishingBlow;
            }
        }

        // ── IHittable ─────────────────────────────────────────────────────

        public bool IsInvincible => _isInvincible;

        // ── Unity Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            if (blockZone != null)
                blockZone.Initialize(this);
        }

        private void Update()
        {
            if (_invincibleTimer > 0f)
            {
                _invincibleTimer -= Time.deltaTime;
                if (_invincibleTimer <= 0f)
                    _isInvincible = false;
            }
        }

        // ── IHittable Implementation ──────────────────────────────────────

        public void TakeHit(AttackData data, Vector3 hitPoint, Vector3 hitDirection, GameObject attacker)
        {
            if (_isInvincible) return;

            if (_isBlocking)
            {
                OnBlockHit(data, attacker);
                return;
            }

            var   attackerStats = attacker?.GetComponent<ICombatStats>();
            float atkValue      = attackerStats?.ATK ?? 0f;
            float magValue      = attackerStats?.MAG ?? 0f;
            float defValue      = _defenderStats?.DEF ?? 0f;

            float offensiveStat = data.damageType == DamageType.Magic ? magValue : atkValue;

            float baseDamage = data.useAttackerStats
                ? DamageCalculator.Calculate(offensiveStat, defValue, data.damageType)
                : DamageCalculator.CalculateFlat(data.flatDamage, data.damageType, defValue);

            // Crit roll
            bool  isCrit      = data.critRate > 0f && Random.value < data.critRate;
            float finalDamage = isCrit ? baseDamage * data.critMultiplier : baseDamage;

            if (_health != null)
                _health.TakeDamage(finalDamage);
            else
                Debug.LogWarning($"[HurtboxController] No HealthSystem on {gameObject.name}!");

            Debug.Log($"[HurtboxController] {gameObject.name} took {finalDamage:0.0} dmg" +
                      $"{(isCrit ? " (CRIT)" : "")} from {attacker?.name} [{data.attackName}]");

            ApplyKnockback(data, hitDirection);
            _posture?.AddFromHit(data.poiseDamage);
            PlayHitReaction();

            OnHitReceived?.Invoke(data, hitPoint, hitDirection, attacker);
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// Raised when a hit lands — owner can subscribe to react visually/logically.
        /// Raised AFTER damage is applied to HealthSystem.
        /// </summary>
        public event System.Action<AttackData, Vector3, Vector3, GameObject> OnHitReceived;

        /// <summary>Set invincibility for a duration (dash i-frame, respawn, etc.).</summary>
        public void SetInvincible(bool invincible, float duration = 0f)
        {
            _isInvincible    = invincible;
            _invincibleTimer = invincible ? duration : 0f;
        }

        /// <summary>Add posture from parry — called by ParryState on success.</summary>
        public void AddPostureFromParry(float amount) => _posture?.AddFromParry(amount);

        /// <summary>Add posture from backstab.</summary>
        public void AddPostureFromBackstab(float amount) => _posture?.AddFromBackstab(amount);

        /// <summary>Set blocking state — routes hits to block logic instead of damage.</summary>
        public void SetBlocking(bool blocking)
        {
            _isBlocking = blocking;
            blockZone?.SetActive(blocking);
        }

        // ── Private ───────────────────────────────────────────────────────

        private void OnDeath()
        {
            SetInvincible(true);
        }

        private void ApplyKnockback(AttackData data, Vector3 direction)
        {
            if (!_rb || data.knockbackForce <= 0f) return;

            Vector3 force = direction * data.knockbackForce;
            force.y      += data.knockbackForce * data.knockbackUpward;
            force.z       = 0f;

            _rb.AddForce(force, ForceMode.Impulse);
        }

        private void PlayHitReaction()
        {
            var animator = GetComponentInChildren<Animator>();
            if (!animator) return;

            animator.SetTrigger(AnimHash.HitReact);
        }

        private void OnBlockHit(AttackData data, GameObject attacker)
        {
            // TODO: GetComponent<PlayerController>()?.OnBlockHit(data, attacker);
            Debug.Log($"[HurtboxController] {gameObject.name} blocked [{data.attackName}] from {attacker?.name}");
        }

        private void OnStagger()
        {
            // TODO: GetComponent<PlayerController>()?.ChangeState(PlayerState.Stagger);
            Debug.Log($"[HurtboxController] {gameObject.name} staggered!");
        }

        private void OnStaggerEnd()
        {
            Debug.Log($"[HurtboxController] {gameObject.name} recovered from stagger.");
        }

        private void OnFinishingBlow()
        {
            Debug.Log($"[HurtboxController] {gameObject.name} finishing blow!");
        }

#if UNITY_EDITOR
        [TitleGroup("Debug")]
        [Button("Auto-Find Block Zone"), GUIColor(0.4f, 0.8f, 1f)]
        private void AutoFindZones()
        {
            var zones = GetComponentsInChildren<HurtboxZone>(true);
            foreach (var zone in zones)
            {
                if (zone.ZoneType == HurtboxZoneType.BlockZone)
                    blockZone = zone;
            }
            Debug.Log($"[HurtboxController] Found {zones.Length} zone(s).");
        }
#endif
    }
}
