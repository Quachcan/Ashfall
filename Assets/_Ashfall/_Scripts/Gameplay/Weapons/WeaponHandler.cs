using System;
using UnityEngine;
using _Ashfall._Scripts.Gameplay.Combat;

namespace _Ashfall._Scripts.Gameplay.Weapons
{
    /// <summary>
    /// Manages the player's equipped weapon.
    /// Handles equip/unequip logic: swaps AnimatorController, spawns the weapon model,
    /// and exposes the current WeaponData to all other systems.
    ///
    /// Owned by PlayerController — call Initialize() in Awake after the Animator is ready.
    /// Equip a weapon via Equip(WeaponData). Subscribe to OnWeaponChanged for reactions.
    /// </summary>
    public class WeaponHandler : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────

        [Header("Config")]
        [Tooltip("Weapon equipped at game start")]
        [SerializeField] private WeaponData defaultWeapon;

        [Header("Sockets")]
        [Tooltip("Transform where the weapon model is parented (e.g. right hand bone)")]
        [SerializeField] private Transform weaponSocket;

        // ── Runtime ───────────────────────────────────────────────────────

        /// <summary>Currently equipped weapon. Null until Initialize() is called.</summary>
        public WeaponData Current { get; private set; }

        /// <summary>The AnimatorOverrideController created for the current weapon's animator.</summary>
        public AnimatorOverrideController OverrideController { get; private set; }

        /// <summary>Fired when a new weapon is equipped. Arg = new weapon.</summary>
        public event Action<WeaponData> OnWeaponChanged;

        private Animator       _animator;
        private HitboxWeapon   _hitbox;
        private GameObject     _weaponModelInstance;

        // ── Initialization ────────────────────────────────────────────────

        /// <summary>
        /// Must be called by PlayerController.Awake() after the Animator is resolved.
        /// Equips the default weapon without firing OnWeaponChanged.
        /// </summary>
        public void Initialize(Animator animator, HitboxWeapon hitbox)
        {
            _animator = animator;
            _hitbox   = hitbox;

            if (defaultWeapon != null)
                EquipInternal(defaultWeapon, fireEvent: false);
            else
                Debug.LogWarning("[WeaponHandler] No defaultWeapon assigned.", this);
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// Equip a new weapon at runtime.
        /// Swaps the Animator controller, spawns new weapon model, fires OnWeaponChanged.
        /// No-op if the weapon is already equipped.
        /// </summary>
        public void Equip(WeaponData weapon)
        {
            if (weapon == null || weapon == Current) return;
            EquipInternal(weapon, fireEvent: true);
        }

        // ── Private ───────────────────────────────────────────────────────

        private void EquipInternal(WeaponData weapon, bool fireEvent)
        {
            Current = weapon;
            SwapAnimator(weapon);
            SwapModel(weapon);
            if (fireEvent) OnWeaponChanged?.Invoke(weapon);
        }

        private void SwapAnimator(WeaponData weapon)
        {
            if (!_animator) return;

            // Use weapon's controller if provided; fall back to current runtime controller
            var baseController = weapon.animatorController
                              ?? _animator.runtimeAnimatorController;

            if (baseController == null) return;

            OverrideController = new AnimatorOverrideController(baseController);
            _animator.runtimeAnimatorController = OverrideController;
        }

        private void SwapModel(WeaponData weapon)
        {
            if (_weaponModelInstance)
                Destroy(_weaponModelInstance);

            if (weapon.weaponPrefab && weaponSocket)
            {
                _weaponModelInstance = Instantiate(
                    weapon.weaponPrefab,
                    weaponSocket.position,
                    weaponSocket.rotation,
                    weaponSocket);
            }
        }
    }
}
