using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    /// <summary>
    /// Attached to the weapon/attack bone.
    /// When active, detects overlapping Hurtboxes and calls TakeHit on them.
    ///
    /// Lifecycle:
    ///   AnimatorEventBridge.OnAttackHit() → SetActive(true)  — start of hit window
    ///   AnimatorEventBridge.OnAttackEnd() → SetActive(false) — end of hit window
    ///
    /// Prevents hitting same target multiple times per swing via hit list.
    ///
    /// Note: OnTriggerEnter alone is unreliable for combos — if the target is already
    /// inside the collider when it re-enables (same-frame disable/enable), Unity may
    /// skip the event. CheckOverlap() on SetActive(true) covers this case.
    /// </summary>
    public class HitboxWeapon : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────

        [TitleGroup("Config")]
        [Tooltip("Attack data for this attack — drag AttackData SO here")]
        [SerializeField] private AttackData attackData;

        [Tooltip("Layer mask of valid targets (EnemyHurtbox or PlayerHurtbox)")]
        [SerializeField] private LayerMask targetLayer;

        [TitleGroup("Runtime")]
        [ShowInInspector, ReadOnly]
        private bool _isActive;

        // ── Internal ──────────────────────────────────────────────────────

        private Collider                     _collider;
        private readonly HashSet<GameObject> _hitThisSwing = new();
        private GameObject                   _owner;

        private static readonly Collider[] OverlapBuffer = new Collider[16];

        // ── Unity Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            _collider           = GetComponent<Collider>();
            _collider.isTrigger = true;
            _owner              = transform.root.gameObject;

            SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive || attackData == null) return;
            ProcessHit(other);
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// Enable or disable the hitbox.
        /// Called by PlayerAttackState on OnAttackHit / OnAttackEnd events.
        /// </summary>
        public void SetActive(bool active)
        {
            _isActive         = active;
            _collider.enabled = active;

            if (!active)
            {
                _hitThisSwing.Clear();
            }
            else
            {
                // Immediately scan for targets already inside the collider.
                // OnTriggerEnter won't fire if the collider was disabled/re-enabled
                // in the same physics frame (common during fast combos).
                CheckOverlap();
            }
        }

        /// <summary>Swap attack data at runtime (different combo hits, skill types).</summary>
        public void SetAttackData(AttackData data) => attackData = data;

        // ── Private ───────────────────────────────────────────────────────

        private void ProcessHit(Collider other)
        {
            var root = GetHittableRoot(other.gameObject);
            if (root == null || root == _owner)          return;
            if (_hitThisSwing.Contains(root))            return;

            var hittable = root.GetComponent<IHittable>();
            if (hittable == null || hittable.IsInvincible) return;

            _hitThisSwing.Add(root);

            Vector3 dir = (root.transform.position - _owner.transform.position).normalized;
            dir.z = 0f;

            hittable.TakeHit(attackData, other.ClosestPoint(transform.position), dir, _owner);
        }

        private void CheckOverlap()
        {
            if (attackData == null) return;

            int count = 0;

            if (_collider is SphereCollider sc)
            {
                Vector3 center = sc.transform.TransformPoint(sc.center);
                float   radius = sc.radius * sc.transform.lossyScale.x;
                count = Physics.OverlapSphereNonAlloc(center, radius, OverlapBuffer, targetLayer, QueryTriggerInteraction.Collide);
            }
            else if (_collider is BoxCollider bc)
            {
                Vector3 center  = bc.transform.TransformPoint(bc.center);
                Vector3 halfExt = Vector3.Scale(bc.size * 0.5f, bc.transform.lossyScale);
                count = Physics.OverlapBoxNonAlloc(center, halfExt, OverlapBuffer, bc.transform.rotation, targetLayer, QueryTriggerInteraction.Collide);
            }

            for (int i = 0; i < count; i++)
                ProcessHit(OverlapBuffer[i]);
        }

        private GameObject GetHittableRoot(GameObject obj)
        {
            var t = obj.transform;
            while (t != null)
            {
                if (t.GetComponent<IHittable>() != null)
                    return t.gameObject;
                t = t.parent;
            }
            return null;
        }

        // ── Gizmos ────────────────────────────────────────────────────────

#if UNITY_EDITOR
        [TitleGroup("Debug")]
        [Button("Force Activate (3s)"), GUIColor(1f, 0.5f, 0.3f)]
        private void DebugActivate()
        {
            SetActive(true);
            Invoke(nameof(DebugDeactivate), 3f);
            Debug.Log("[HitboxWeapon] Force activated for 3s");
        }

        private void DebugDeactivate() => SetActive(false);
#endif

        private void OnDrawGizmos()
        {
            if (!_isActive) return;
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);

            var col = GetComponent<Collider>();
            if (col is SphereCollider sc)
            {
                // Sphere has no rotation — world-space center + scaled radius is enough
                Vector3 center = sc.transform.TransformPoint(sc.center);
                float   radius = sc.radius * sc.transform.lossyScale.x;
                Gizmos.DrawWireSphere(center, radius);
            }
            else if (col is BoxCollider bc)
            {
                // Must apply the transform matrix so the cube rotates with the object.
                // DrawWireCube is always axis-aligned in world space without this.
                var prev = Gizmos.matrix;
                Gizmos.matrix = bc.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(bc.center, bc.size);
                Gizmos.matrix = prev;
            }
        }
    }
}
