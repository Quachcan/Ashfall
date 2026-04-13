using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Weapons
{
    /// <summary>
    /// Maps WeaponType → WeaponVisuals.
    /// Single source of truth for resolving visual assets from a weapon type.
    ///
    /// Assign this SO to WeaponHandler in the Inspector.
    /// Add one WeaponVisuals entry per weapon type.
    ///
    /// Design note:
    ///   WeaponData has NO reference to WeaponVisuals.
    ///   This registry is the only bridge between data and visuals,
    ///   keeping the two SO types fully decoupled.
    /// </summary>
    [CreateAssetMenu(menuName = "Ashfall/Weapons/WeaponVisualRegistry", fileName = "WeaponVisualRegistry")]
    public class WeaponVisualRegistry : ScriptableObject
    {
        [TitleGroup("Registry")]
        [InfoBox("Add one WeaponVisuals SO per WeaponType. Duplicate types will log a warning.")]
        [SerializeField] private WeaponVisuals[] entries;

        private Dictionary<WeaponType, WeaponVisuals> _map;

        private void OnEnable() => BuildMap();

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// Returns the WeaponVisuals for the given type, or null if not registered.
        /// </summary>
        public WeaponVisuals Get(WeaponType type)
        {
            if (_map == null) BuildMap();
            _map.TryGetValue(type, out var result);

            if (result == null)
                Debug.LogWarning($"[WeaponVisualRegistry] No visuals registered for WeaponType.{type}");

            return result;
        }

        // ── Private ───────────────────────────────────────────────────────

        private void BuildMap()
        {
            _map = new Dictionary<WeaponType, WeaponVisuals>();

            if (entries == null) return;

            foreach (var entry in entries)
            {
                if (entry == null) continue;

                if (_map.ContainsKey(entry.weaponType))
                {
                    Debug.LogWarning($"[WeaponVisualRegistry] Duplicate entry for WeaponType.{entry.weaponType} — keeping first.", this);
                    continue;
                }

                _map[entry.weaponType] = entry;
            }
        }

#if UNITY_EDITOR
        private void OnValidate() => BuildMap();
#endif
    }
}
