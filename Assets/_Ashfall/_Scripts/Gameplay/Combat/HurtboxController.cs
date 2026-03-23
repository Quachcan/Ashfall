using Sirenix.OdinInspector;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    /// <summary>
    /// Manages all hurtbox zones on a character (Player or Enemy).
    /// Implements IHittable — HitboxWeapon calls TakeHit() on this.
    ///
    /// Responsibilities:
    ///   - Receive hits and apply damage multiplier per zone
    ///   - Forward to HealthSystem for HP reduction
    ///   - Forward to PoisSystem for stagger
    ///   - Forward knockback to Rigidbody
    ///   - Manage invincibility frames
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class HurtboxController : MonoBehaviour, IHittable
    {
        // ── Inspector ─────────────────────────────────────────────────────

        [TitleGroup("Zone References")]
        [InfoBox("Drag each HurtboxZone here, or use Auto-Find button below.")]
        [SerializeField] private HurtboxZone headZone;
        [SerializeField] private HurtboxZone torsoZone;
        [SerializeField] private HurtboxZone legsZone;
        [SerializeField] private HurtboxZone blockZone;

        [TitleGroup("Damage Multipliers")]
        [HorizontalGroup("Damage Multipliers/Row")]
        [VerticalGroup("Damage Multipliers/Row/Left"), LabelWidth(120)]
        [Range(0.1f, 5f)] public float headMultiplier  = 1.5f;

        [VerticalGroup("Damage Multipliers/Row/Left"), LabelWidth(120)]
        [Range(0.1f, 5f)] public float torsoMultiplier = 1.0f;

        [VerticalGroup("Damage Multipliers/Row/Right"), LabelWidth(120)]
        [Range(0.1f, 5f)] public float legsMultiplier  = 0.8f;

        // ── Runtime ───────────────────────────────────────────────────────

        [TitleGroup("Runtime")]
        [ShowInInspector, ReadOnly] private bool _isInvincible;
        [ShowInInspector, ReadOnly] private bool _isBlocking;
        [ShowInInspector, ReadOnly] private float _invincibleTimer;

        private Rigidbody    _rb;
        private HealthSystem _health;

        /// <summary>
        /// Inject HealthSystem — called by owner (PlayerController/EnemyBase) after creation.
        /// Pure C# systems cannot be found via GetComponent.
        /// </summary>
        public void Initialize(Rigidbody rb, HealthSystem health)
        {
            _rb     = rb;
            _health = health;

            if (_health != null)
                _health.OnDeath += OnDeath;
        }

        // ── IHittable ─────────────────────────────────────────────────────

        public bool IsInvincible => _isInvincible;

        // ── Unity Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            // _rb and _health injected via Initialize() called by owner
            InitZone(headZone);
            InitZone(torsoZone);
            InitZone(legsZone);
            InitZone(blockZone);
        }

private void OnDeath()
        {
            // Set invincible on death — no more hits after dying
            SetInvincible(true);

            // TODO: trigger dead state via PlayerController or EnemyBase
            // GetComponent<PlayerController>()?.ChangeState(PlayerState.Dead);
            // GetComponent<EnemyBase>()?.ChangeState(EnemyState.Dead);
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

        public void TakeHit(HitData data, Vector3 hitPoint, Vector3 hitDirection, GameObject attacker)
        {
            if (_isInvincible) return;

            // Determine which zone was hit
            HurtboxZoneType zone     = GetHitZone(hitPoint);
            float           multiplier = GetMultiplier(zone);

            // Handle block zone
            if (zone == HurtboxZoneType.BlockZone && _isBlocking)
            {
                OnBlockHit(data, attacker);
                return;
            }

            // Calculate damage:
            // useAttackerStats = true  → damage from attacker ATK vs defender DEF (basic attack)
            // useAttackerStats = false → flat damage from HitData (skill, trap, environmental)
            float rawDamage;
            if (data.useAttackerStats)
            {
                // TODO: DamageCalculation.Calculate(attackerStats.ATK, defenderStats.DEF)
                // Placeholder until StatSystem is implemented
                float attackerATK  = GetATK(attacker);
                float defenderDEF  = GetDEF();
                rawDamage          = Mathf.Max(1f, attackerATK - defenderDEF);
            }
            else
            {
                rawDamage = data.flatDamage;
            }

            float finalDamage = rawDamage * multiplier;

            if (_health != null)
                _health.TakeDamage(finalDamage);
            else
                Debug.LogWarning($"[HurtboxController] No HealthSystem on {gameObject.name}!");

            Debug.Log($"[HurtboxController] {gameObject.name} took {finalDamage:0.0} dmg " +
                      $"({zone} x{multiplier}) from {attacker?.name}");

            ApplyKnockback(data, hitDirection);

            // TODO: GetComponent<PoiseSystem>()?.TakePoiseDamage(data.poiseDamage);
            PlayHitReaction(zone);

            // TODO: _eventHub.onHit.Raise(...)
        }

        // ── Stat Helpers (placeholder until StatSystem) ───────────────────

        private float GetATK(GameObject attacker)
        {
            // TODO: attacker.GetComponent<StatSystem>()?.GetFinalStat(StatType.ATK)
            // Placeholder — read from PlayerStats SO if available
            var playerCtrl = attacker?.GetComponent<_Ashfall._Scripts.Gameplay.Player.PlayerController>();
            // Will be replaced by proper StatSystem
            return 20f;
        }

        private float GetDEF()
        {
            // TODO: GetComponent<StatSystem>()?.GetFinalStat(StatType.DEF)
            return 5f;
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>Set invincibility for a duration (dash i-frame, respawn, etc.).</summary>
        public void SetInvincible(bool invincible, float duration = 0f)
        {
            _isInvincible    = invincible;
            _invincibleTimer = invincible ? duration : 0f;
        }

        /// <summary>Set blocking state — routes hits to block logic instead of damage.</summary>
        public void SetBlocking(bool blocking)
        {
            _isBlocking = blocking;
            blockZone?.SetActive(blocking);
        }

        public void SetZoneActive(HurtboxZoneType zone, bool active)
            => GetZone(zone)?.SetActive(active);

        // ── Private ───────────────────────────────────────────────────────

        private void InitZone(HurtboxZone zone)
        {
            if (zone == null) return;
            zone.Initialize(this);
        }

        private HurtboxZoneType GetHitZone(Vector3 hitPoint)
        {
            // Find closest zone to hit point
            HurtboxZoneType closest    = HurtboxZoneType.Torso;
            float           minDist    = float.MaxValue;

            CheckZoneDistance(headZone,  hitPoint, HurtboxZoneType.Head,      ref closest, ref minDist);
            CheckZoneDistance(torsoZone, hitPoint, HurtboxZoneType.Torso,     ref closest, ref minDist);
            CheckZoneDistance(legsZone,  hitPoint, HurtboxZoneType.Legs,      ref closest, ref minDist);
            CheckZoneDistance(blockZone, hitPoint, HurtboxZoneType.BlockZone, ref closest, ref minDist);

            return closest;
        }

        private void CheckZoneDistance(HurtboxZone zone, Vector3 hitPoint,
            HurtboxZoneType type, ref HurtboxZoneType closest, ref float minDist)
        {
            if (zone == null) return;
            float dist = Vector3.Distance(zone.transform.position, hitPoint);
            if (dist < minDist) { minDist = dist; closest = type; }
        }

        private float GetMultiplier(HurtboxZoneType zone) => zone switch
        {
            HurtboxZoneType.Head      => headMultiplier,
            HurtboxZoneType.Torso     => torsoMultiplier,
            HurtboxZoneType.Legs      => legsMultiplier,
            HurtboxZoneType.BlockZone => 0f,
            _                         => torsoMultiplier
        };

        private void ApplyKnockback(HitData data, Vector3 direction)
        {
            if (!_rb || data.knockbackForce <= 0f) return;

            Vector3 force = direction * data.knockbackForce;
            force.y      += data.knockbackForce * data.knockbackUpward;
            force.z       = 0f; // lock to 2.5D plane

            _rb.AddForce(force, ForceMode.Impulse);
        }

        private void PlayHitReaction(HurtboxZoneType zone)
        {
            var animator = GetComponentInChildren<Animator>();
            if (!animator) return;

            // TODO: import AnimHash from player namespace or make shared
            switch (zone)
            {
                case HurtboxZoneType.Head:  animator.SetTrigger("HitReact_Head");  break;
                case HurtboxZoneType.Torso: animator.SetTrigger("HitReact_Torso"); break;
                case HurtboxZoneType.Legs:  animator.SetTrigger("HitReact_Legs");  break;
            }
        }

        private void OnBlockHit(HitData data, GameObject attacker)
        {
            // Forward to PlayerBlockState or EnemyBlockState
            // TODO: GetComponent<PlayerController>()?.OnBlockHit(data, attacker);
            Debug.Log($"[HurtboxController] {gameObject.name} blocked hit from {attacker?.name}");
        }

        private HurtboxZone GetZone(HurtboxZoneType type) => type switch
        {
            HurtboxZoneType.Head      => headZone,
            HurtboxZoneType.Torso     => torsoZone,
            HurtboxZoneType.Legs      => legsZone,
            HurtboxZoneType.BlockZone => blockZone,
            _                         => null
        };

#if UNITY_EDITOR
        [TitleGroup("Debug")]
        [Button("Auto-Find Zones In Children"), GUIColor(0.4f, 0.8f, 1f)]
        private void AutoFindZones()
        {
            var zones = GetComponentsInChildren<HurtboxZone>(true);
            foreach (var zone in zones)
            {
                switch (zone.ZoneType)
                {
                    case HurtboxZoneType.Head:      headZone  = zone; break;
                    case HurtboxZoneType.Torso:     torsoZone = zone; break;
                    case HurtboxZoneType.Legs:      legsZone  = zone; break;
                    case HurtboxZoneType.BlockZone: blockZone = zone; break;
                }
            }
            Debug.Log($"[HurtboxController] Found {zones.Length} zone(s).");
        }
#endif
    }
}