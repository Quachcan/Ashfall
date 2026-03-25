using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    /// <summary>
    /// Test dummy — stands still, receives hits, plays reactions.
    /// No FSM, no AI. Delete when real Enemy is implemented.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(HurtboxController))]
    public class DummyEnemy : MonoBehaviour, ICombatStats
    {
        // ── Inspector ─────────────────────────────────────────────────────

        [TitleGroup("References")]
        [SerializeField] private Animator animator;

        [TitleGroup("Stats")]
        [HorizontalGroup("Stats/Row")]
        [VerticalGroup("Stats/Row/Left"), LabelWidth(120)]
        public float maxHp               = 200f;
        [VerticalGroup("Stats/Row/Left"), LabelWidth(120)]
        public float def                 = 10f;
        [VerticalGroup("Stats/Row/Right"), LabelWidth(120)]
        public float maxPosture          = 100f;
        [VerticalGroup("Stats/Row/Right"), LabelWidth(120)]
        public float postureRecoverDelay = 2f;
        [VerticalGroup("Stats/Row/Right"), LabelWidth(120)]
        public float postureRecoverRate  = 15f;
        [VerticalGroup("Stats/Row/Right"), LabelWidth(120)]
        public float finishingBlowWindow = 3f;

        // ── ICombatStats ──────────────────────────────────────────────────

        public float ATK => 0f;
        public float MAG => 0f;
        public float DEF => def;

        // ── Runtime ───────────────────────────────────────────────────────

        [TitleGroup("Runtime")]
        [ShowInInspector, ReadOnly] private float  _currentHp;
        [ShowInInspector, ReadOnly] private float  _postureBar;
        [ShowInInspector, ReadOnly] private string _status = "Idle";

        private Rigidbody         _rb;
        private HurtboxController _hurtbox;
        private HealthSystem      _health;
        private PostureSystem     _posture;

        // ── Unity Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            _rb      = GetComponent<Rigidbody>();
            _hurtbox = GetComponent<HurtboxController>();

            if (!animator) animator = GetComponentInChildren<Animator>();

            _health  = new HealthSystem(maxHp);
            _posture = new PostureSystem(
                maxPosture, postureRecoverDelay,
                postureRecoverRate, finishingBlowWindow);

            _health.OnDamageTaken += amount =>
                Debug.Log($"[Dummy] -{amount:0.0} | HP {_health.CurrentHp:0}/{maxHp}");
            _health.OnDeath += () =>
            {
                _status = "Dead";
                _hurtbox.SetInvincible(true);
                animator?.SetTrigger("Dead");
                Debug.Log("[Dummy] Dead.");
            };

            _posture.OnStagger += () =>
            {
                _status = "Staggered";
                _rb.linearVelocity = Vector3.zero;
                animator?.SetTrigger("Stagger");
                Debug.Log("[Dummy] Staggered — finishing blow window open!");
            };

            _posture.OnStaggerEnd += () =>
            {
                if (_status == "Staggered") _status = "Idle";
                Debug.Log("[Dummy] Recovered from stagger.");
            };

            _posture.OnFinishingBlow += () =>
                Debug.Log("[Dummy] Finishing blow!");

            _hurtbox.Initialize(_rb, _health, this, _posture);
            _hurtbox.OnHitReceived += OnHitReceived;
        }

        private void Update()
        {
            _posture.Tick(Time.deltaTime);
            _currentHp  = _health.CurrentHp;
            _postureBar = _posture.Normalized;
        }

        // ── Hit Response ──────────────────────────────────────────────────

        private void OnHitReceived(HitData data, Vector3 hitPoint,
                                   Vector3 direction, GameObject attacker)
        {
            if (_health.IsDead) return;
            if (_status == "Staggered") return;

            if (data.knockbackForce > 0.5f)
            {
                StartCoroutine(KnockbackRoutine(direction, data.knockbackForce,
                                                data.knockbackDuration));
            }
            else
            {
                PlayHitReact(hitPoint);
            }
        }

        private void PlayHitReact(Vector3 hitPoint)
        {
            float relY = hitPoint.y - transform.position.y;
            string trigger = relY > 1.4f ? "HitReact_Head" :
                             relY > 0.6f ? "HitReact_Torso" : "HitReact_Legs";
            animator?.SetTrigger(trigger);
            _status = "HitReact";
            StartCoroutine(ResetStatusAfter(0.5f));
        }

        private IEnumerator KnockbackRoutine(Vector3 direction, float force, float duration)
        {
            _status = "Knockback";
            animator?.SetTrigger("HitReact_Torso");

            Vector3 knockForce = direction * force;
            knockForce.z       = 0f;
            _rb.linearVelocity = Vector3.zero;
            _rb.AddForce(knockForce, ForceMode.Impulse);

            yield return new WaitForSeconds(duration);

            _rb.linearVelocity = Vector3.zero;
            if (_status == "Knockback") _status = "Idle";
        }

        private IEnumerator ResetStatusAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (_status == "HitReact") _status = "Idle";
        }

        // ── Debug ─────────────────────────────────────────────────────────

#if UNITY_EDITOR
        [TitleGroup("Debug")]
        [Button("Reset Dummy"), GUIColor(0.4f, 0.8f, 1f)]
        private void ResetDummy()
        {
            StopAllCoroutines();
            _health.Revive(maxHp);
            _posture.Reset();
            _hurtbox.SetInvincible(false);
            _rb.linearVelocity  = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _status = "Idle";
            Debug.Log("[Dummy] Reset.");
        }
#endif
    }
}