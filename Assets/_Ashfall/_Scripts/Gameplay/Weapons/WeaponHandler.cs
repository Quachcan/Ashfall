using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Weapons
{
    /// <summary>
    /// Manages the player's equipped weapon.
    ///
    /// Separation of concerns:
    ///   WeaponData    — combat numbers (always in memory, zero visual assets)
    ///   WeaponVisuals — prefab + animator + animations (resolved on equip via registry)
    ///
    /// Flow:
    ///   1. PlayerController calls Initialize() in Awake.
    ///   2. WeaponHandler equips defaultWeapon, resolves its WeaponVisuals from the registry.
    ///   3. Callers invoke Equip(WeaponData) at runtime — visuals resolve automatically.
    ///   4. OnWeaponChanged fires so other systems (HUD, audio) can react.
    /// </summary>
    public class WeaponHandler : MonoBehaviour
    {
        // ── Inspector ─────────────────────────────────────────────────────

        [Header("Config")]
        [Tooltip("Weapon data equipped at game start")]
        [SerializeField] private WeaponData defaultWeapon;

        [Tooltip("Maps WeaponType → WeaponVisuals. Assign the WeaponVisualRegistry SO here.")]
        [SerializeField] private WeaponVisualRegistry visualRegistry;

        [Tooltip("The base Animator Controller (AC_Player_Base). " +
                 "Used as-is when WeaponVisuals.animatorOverride is null (e.g. Unarmed). " +
                 "Its default clips should be the unarmed punch/kick animations.")]
        [SerializeField] private RuntimeAnimatorController baseAnimatorController;

        [Header("Sockets")]
        [Tooltip("Transform where the weapon model is parented (e.g. right hand bone)")]
        [SerializeField] private Transform weaponSocket;

        // ── Runtime ───────────────────────────────────────────────────────

        /// <summary>Currently equipped weapon data (combat stats, combo, block).</summary>
        public WeaponData Current { get; private set; }

        /// <summary>Visual assets for the currently equipped weapon.</summary>
        public WeaponVisuals CurrentVisuals { get; private set; }

        /// <summary>Runtime copy of the pre-built AnimatorOverrideController from CurrentVisuals. Safe to mutate (e.g. SwapParryClip).</summary>
        public AnimatorOverrideController OverrideController { get; private set; }

        /// <summary>Fired after a new weapon is fully equipped. Arg = new WeaponData.</summary>
        public event Action<WeaponData> OnWeaponChanged;

        private Animator   _animator;
        private GameObject _weaponModelInstance;

        // ── Initialization ────────────────────────────────────────────────

        /// <summary>
        /// Called by PlayerController.Awake() once the Animator is resolved.
        /// Equips the default weapon silently (no OnWeaponChanged fired).
        /// </summary>
        public void Initialize(Animator animator)
        {
            _animator = animator;

            if (defaultWeapon != null)
                EquipInternal(defaultWeapon, fireEvent: false);
            else
                Debug.LogWarning("[WeaponHandler] No defaultWeapon assigned.", this);
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// Equip a weapon at runtime.
        /// Resolves the matching WeaponVisuals from the registry automatically.
        /// No-op if the same weapon is already equipped.
        /// </summary>
        public void Equip(WeaponData weapon)
        {
            if (weapon == null || weapon == Current) return;
            EquipInternal(weapon, fireEvent: true);
        }

        // ── Private ───────────────────────────────────────────────────────

        private void EquipInternal(WeaponData weapon, bool fireEvent)
        {
            Current        = weapon;
            CurrentVisuals = visualRegistry != null
                ? visualRegistry.Get(weapon.weaponType)
                : null;

            if (CurrentVisuals == null)
                Debug.LogWarning($"[WeaponHandler] No WeaponVisuals found for {weapon.weaponType}. Model and animator will not update.", this);

            SwapAnimator();
            SwapModel();

            if (fireEvent) OnWeaponChanged?.Invoke(weapon);
        }

        private void SwapAnimator()
        {
            if (!_animator) return;

            var source = CurrentVisuals?.animatorOverride;

            if (source != null)
            {
                // Weapon has a pre-built override controller — create a runtime copy so that
                // runtime mutations (e.g. SwapParryClip) never affect the shared project asset.
                OverrideController = new AnimatorOverrideController(source.runtimeAnimatorController);

                var overrides = new List<System.Collections.Generic.KeyValuePair<AnimationClip, AnimationClip>>(source.overridesCount);
                source.GetOverrides(overrides);
                OverrideController.ApplyOverrides(overrides);
            }
            else
            {
                // No override (e.g. Unarmed) — wrap the base controller directly.
                // AC_Player_Base default clips serve as the unarmed animations.
                if (baseAnimatorController == null)
                {
                    Debug.LogWarning("[WeaponHandler] baseAnimatorController is not assigned — animator will not update.", this);
                    return;
                }
                OverrideController = new AnimatorOverrideController(baseAnimatorController);
            }

            _animator.runtimeAnimatorController = OverrideController;
        }

        private void OnDestroy()
        {
            if (_weaponModelInstance)
                Destroy(_weaponModelInstance);
        }

        private void SwapModel()
        {
            if (_weaponModelInstance)
                Destroy(_weaponModelInstance);

            if (CurrentVisuals?.weaponPrefab != null && weaponSocket != null)
            {
                _weaponModelInstance = Instantiate(
                    CurrentVisuals.weaponPrefab,
                    weaponSocket.position,
                    weaponSocket.rotation,
                    weaponSocket);
            }
        }
    }
}
