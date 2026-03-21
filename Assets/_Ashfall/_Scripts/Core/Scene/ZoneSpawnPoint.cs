using UnityEngine;

namespace _Ashfall._Scripts.Core.Scene
{
    /// <summary>
    /// Marks a spawn position inside a zone.
    /// fromZoneId should match the scene name of the zone the player is coming from.
    /// isDefault = true means this point is used when no matching entry is found.
    /// </summary>
    public class ZoneSpawnPoint : MonoBehaviour
    {
        [Tooltip("Scene name of the zone player is coming from")]
        public string fromZoneId;

        [Tooltip("Use this point if no matching fromZoneId is found")]
        public bool isDefault;

        private void OnDrawGizmos()
        {
            Gizmos.color = isDefault ? Color.green : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.8f);
        }
    }
}