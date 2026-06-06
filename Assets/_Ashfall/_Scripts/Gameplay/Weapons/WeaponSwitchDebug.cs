using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Weapons
{
#if UNITY_EDITOR
    [AddComponentMenu("Ashfall/Debug/Weapon Switch Debug")]
#endif
    public class WeaponSwitchDebug : MonoBehaviour
    {
        [Header("Weapon Slots (Debug)")]
        [SerializeField] private WeaponData slot0;
        [SerializeField] private WeaponData slot1;
        [SerializeField] private WeaponData slot2;
        [SerializeField] private WeaponData slot3;

        private WeaponHandler _handler;

        private void Awake()
        {
            _handler = GetComponent<WeaponHandler>();
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
            if (weapon == null) return;
            _handler.Equip(weapon);
            Debug.Log($"[WeaponSwitchDebug] Equipped: {weapon.weaponName} (slot {slot})");
        }
    }
}