using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    /// <summary>
    /// Projectile spawned by PlayerBowAttackState when an arrow is fired.
    ///
    /// Physics model:
    ///   - Rigidbody with useGravity = true — arrow follows a natural arc
    ///   - FreezePositionZ — keeps the projectile in the 2.5D plane
    ///   - Initial velocity set via Init() — avoids AddForce mass dependency
    ///
    /// Damage pipeline:
    ///   OnTriggerEnter → find HurtboxController → call TakeHit (reuses full damage pipeline)
    ///
    /// Lifetime:
    ///   Self-destructs after <lifetime> seconds or immediately on first valid hit.
    ///   Prefab should be on a dedicated "Projectile" layer that only collides with Enemy/Hurtbox layers.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class ArrowProjectile : MonoBehaviour
    {
        [Tooltip("Maximum travel time in seconds before the arrow is destroyed if it misses")]
        [SerializeField] private float lifetime = 6f;

        // ── Runtime ───────────────────────────────────────────────────────

        private AttackData _attackData;
        private GameObject _attacker;
        private float      _facingDir;
        private bool       _hasHit;

        private Rigidbody _rb;

        // ── Initialization ────────────────────────────────────────────────

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
        }

        /// <summary>
        /// Initializes the arrow immediately after Instantiate.
        /// Called by PlayerBowAttackState.SpawnArrow().
        /// </summary>
        /// <param name="data">AttackData for this shot (quick shot or charged shot).</param>
        /// <param name="attacker">Shooter's GameObject — passed to HurtboxController so it can read ICombatStats.</param>
        /// <param name="speed">Travel speed in units per second.</param>
        /// <param name="facingDir">Shooter's facing direction: +1 = right, -1 = left.</param>
        public void Init(AttackData data, GameObject attacker, float speed, float facingDir)
        {
            _attackData = data;
            _attacker   = attacker;
            _facingDir  = facingDir;
            _hasHit     = false;

            if (_rb != null)
                _rb.linearVelocity = new Vector3(speed * facingDir, 0f, 0f);

            Destroy(gameObject, lifetime);
        }

        // ── Unity Lifecycle ───────────────────────────────────────────────

        private void Update()
        {
            // Rotate arrow tip to match velocity direction for a natural arc visual
            if (_rb != null && _rb.linearVelocity.sqrMagnitude > 0.5f)
            {
                // In 2.5D: tilt the arrow along Z so the tip angles up/down with the arc
                float angle = Mathf.Atan2(_rb.linearVelocity.y, Mathf.Abs(_rb.linearVelocity.x)) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle * _facingDir);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasHit) return;
            if (_attackData == null) return;

            // Never hit the shooter or any of their child colliders
            if (_attacker != null && other.gameObject == _attacker) return;
            if (_attacker != null && other.transform.IsChildOf(_attacker.transform)) return;

            var hurtbox = other.GetComponentInParent<HurtboxController>();
            if (hurtbox == null) return;

            _hasHit = true;

            Vector3 hitDirection = new Vector3(_facingDir, 0f, 0f);
            hurtbox.TakeHit(_attackData, transform.position, hitDirection, _attacker);

            Destroy(gameObject);
        }
    }
}
