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
    /// </summary>
    // [RequireComponent(typeof(Collider))]
    public class HitboxWeapon : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────

        [TitleGroup("Config")]
        [Tooltip("Hit data for this attack — drag HitData SO here")]
        [SerializeField] private HitData hitData;

        [Tooltip("Layer mask of valid targets (EnemyHurtbox or PlayerHurtbox)")]
        [SerializeField] private LayerMask targetLayer;

        [TitleGroup("Runtime")]
        [ShowInInspector, ReadOnly]
        private bool _isActive;

        // ── Internal ──────────────────────────────────────────────────────

        private Collider               _collider;
        private readonly HashSet<GameObject> _hitThisSwing = new();

        // Owner — the root GameObject (Player or Enemy)
        private GameObject _owner;

        // ── Unity Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            _collider         = GetComponent<Collider>();
            _collider.isTrigger = true;
            _owner            = GetRootOwner();

            // Always start inactive — activated by AnimatorEventBridge
            SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_isActive)    return;
            if (hitData == null) return;

            // Skip if already hit this target this swing
            var root = GetHittableRoot(other.gameObject);
            if (root == null)                  return;
            if (_hitThisSwing.Contains(root))  return;

            // Skip owner's own hurtbox
            if (root == _owner) return;

            // Check IHittable
            var hittable = root.GetComponent<IHittable>();
            if (hittable == null || hittable.IsInvincible) return;

            // Record hit to prevent multi-hit this swing
            _hitThisSwing.Add(root);

            // Calculate hit direction (attacker → target)
            Vector3 dir = (root.transform.position - _owner.transform.position).normalized;
            dir.z = 0f; // lock to 2.5D plane

            // Deliver hit
            hittable.TakeHit(hitData, other.ClosestPoint(transform.position), dir, _owner);
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// Enable or disable the hitbox.
        /// Called by AnimatorEventBridge on attack hit/end events.
        /// </summary>
        public void SetActive(bool active)
        {
            _isActive         = active;
            _collider.enabled = active;

            if (!active)
                _hitThisSwing.Clear(); // reset per swing
        }

        /// <summary>Swap hit data at runtime (different combo hits, skill types).</summary>
        public void SetHitData(HitData data) => hitData = data;

        // ── Helpers ───────────────────────────────────────────────────────

        /// <summary>
        /// Walk up the hierarchy to find the root GameObject with IHittable.
        /// Hurtboxes are child objects — we need their root owner.
        /// </summary>
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

        /// <summary>Gets the root owner (top of hierarchy) for self-hit prevention.</summary>
        private GameObject GetRootOwner()
        {
            return transform.root.gameObject;
        }

        // ── Gizmos ────────────────────────────────────────────────────────

#if UNITY_EDITOR
        [Sirenix.OdinInspector.TitleGroup("Debug")]
        [Sirenix.OdinInspector.Button("Force Activate (1s)"), Sirenix.OdinInspector.GUIColor(1f, 0.5f, 0.3f)]
        private void DebugActivate()
        {
            SetActive(true);
            Invoke(nameof(DebugDeactivate), 3f);
            UnityEngine.Debug.Log("[HitboxWeapon] Force activated for 1s");
        }

        private void DebugDeactivate() => SetActive(false);
#endif

        private void OnDrawGizmos()
        {
            if (!_isActive) return;
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);

            var col = GetComponent<Collider>();
            if (col is SphereCollider sc)
                Gizmos.DrawWireSphere(sc.transform.TransformPoint(sc.center), sc.radius);
            else if (col is BoxCollider bc)
                Gizmos.DrawWireCube(bc.transform.TransformPoint(bc.center), bc.size);
        }
    }
}