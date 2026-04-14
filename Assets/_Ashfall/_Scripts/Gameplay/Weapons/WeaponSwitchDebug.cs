using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Weapons
{
    /// <summary>
    /// Debug-only helper — press keys 1-4 at runtime to hot-swap the weapon.
    ///
    /// Add this component to the Player root alongside WeaponHandler.
    /// Assign up to 4 WeaponData assets in the Inspector.
    ///
    /// Keybindings:
    ///   1 → slot0  (e.g. Sword)
    ///   2 → slot1  (e.g. DualSword)
    ///   3 → slot2  (e.g. Staff)
    ///   4 → slot3  (e.g. Bow)
    /// </summary>
#if UNITY_EDITOR
    [AddComponentMenu("Ashfall/Debug/Weapon Switch Debug")]
#endif
    public class WeaponSwitchDebug : MonoBehaviour
    {
        [Header("Weapon Slots (Debug)")]
        [Tooltip("Slot 0 — press key 1 to equip")]
        [SerializeField] private WeaponData slot0;

        [Tooltip("Slot 1 — press key 2 to equip")]
        [SerializeField] private WeaponData slot1;

        [Tooltip("Slot 2 — press key 3 to equip")]
        [SerializeField] private WeaponData slot2;

        [Tooltip("Slot 3 — press key 4 to equip")]
        [SerializeField] private WeaponData slot3;

        private WeaponHandler _handler;

        private void Awake()
        {
            _handler = GetComponent<WeaponHandler>();
            if (_handler == null)
                Debug.LogError("[WeaponSwitchDebug] No WeaponHandler found on this GameObject.", this);
        }

        private void Update()
        {
            if (_handler == null) return;

            if (Input.GetKeyDown(KeyCode.Alpha1)) TryEquip(slot0, 1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) TryEquip(slot1, 2);
            if (Input.GetKeyDown(KeyCode.Alpha3)) TryEquip(slot2, 3);
            if (Input.GetKeyDown(KeyCode.Alpha4)) TryEquip(slot3, 4);
        }

        private void TryEquip(WeaponData weapon, int slot)
        {
            if (weapon == null)
            {
                Debug.LogWarning($"[WeaponSwitchDebug] Slot {slot} is empty.", this);
                return;
            }

            _handler.Equip(weapon);
            Debug.Log($"[WeaponSwitchDebug] Equipped: {weapon.weaponName} (slot {slot})");
        }
    }
}
