using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    /// <summary>
    /// Attached to each individual hurtbox collider child (Head, Torso, Legs, BlockZone).
    /// Acts as a thin receiver — when an attack collider overlaps this trigger,
    /// it reads the ZoneType and forwards the hit to the HurtboxController on the root.
    ///
    /// This component itself has no damage logic — it only identifies the zone
    /// and routes the event upward. All damage calculation stays in HurtboxController.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class HurtboxZone : MonoBehaviour
    {
        [SerializeField] private HurtboxZoneType zoneType = HurtboxZoneType.Torso;

        /// <summary>Which body region this collider represents.</summary>
        public HurtboxZoneType ZoneType => zoneType;

        // Cached reference to the root controller — set by HurtboxController on init
        private HurtboxController _controller;

        private Collider _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider>();

            // Must be trigger — never a physics collider
            _collider.isTrigger = true;
        }

        /// <summary>
        /// Called by HurtboxController during initialization to inject the root reference.
        /// Avoids GetComponentInParent every frame.
        /// </summary>
        public void Initialize(HurtboxController controller)
        {
            _controller = controller;
        }

        /// <summary>Enable or disable this zone's collider (e.g. disable legs while airborne).</summary>
        public void SetActive(bool active)
        {
            if (_collider) _collider.enabled = active;
        }
    }
}