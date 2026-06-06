using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Weapons
{
    [CreateAssetMenu(menuName = "Ashfall/Weapons/WeaponVisualRegistry", fileName = "WeaponVisualRegistry")]
    public class WeaponVisualRegistry : ScriptableObject
    {
        [TitleGroup("Registry")]
        [SerializeField] private WeaponVisuals[] entries;

        private Dictionary<WeaponType, WeaponVisuals> _map;

        private void OnEnable() => BuildMap();

        public WeaponVisuals Get(WeaponType type)
        {
            if (_map == null) BuildMap();
            _map.TryGetValue(type, out var result);

            if (result == null)
                Debug.LogWarning($"[WeaponVisualRegistry] No visuals registered for WeaponType.{type}");

            return result;
        }

        private void BuildMap()
        {
            _map = new Dictionary<WeaponType, WeaponVisuals>();
            if (entries == null) return;

            foreach (var entry in entries)
            {
                if (entry == null) continue;
                if (_map.ContainsKey(entry.weaponType)) continue;
                _map[entry.weaponType] = entry;
            }
        }

#if UNITY_EDITOR
        private void OnValidate() => BuildMap();
#endif
    }
}