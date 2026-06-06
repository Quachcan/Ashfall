using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Weapons
{
    public class WeaponHandler : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private WeaponData defaultWeapon;
        [SerializeField] private WeaponVisualRegistry visualRegistry;
        [SerializeField] private RuntimeAnimatorController baseAnimatorController;

        [Header("Sockets")]
        [SerializeField] private Transform weaponSocket;

        public WeaponData Current { get; private set; }
        public WeaponVisuals CurrentVisuals { get; private set; }
        public AnimatorOverrideController OverrideController { get; private set; }

        public event Action<WeaponData> OnWeaponChanged;

        private Animator   _animator;
        private GameObject _weaponModelInstance;

        public void Initialize(Animator animator)
        {
            _animator = animator;
            if (defaultWeapon != null)
                EquipInternal(defaultWeapon, fireEvent: false);
        }

        public void Equip(WeaponData weapon)
        {
            if (weapon == null || weapon == Current) return;
            EquipInternal(weapon, fireEvent: true);
        }

        private void EquipInternal(WeaponData weapon, bool fireEvent)
        {
            Current        = weapon;
            CurrentVisuals = visualRegistry != null ? visualRegistry.Get(weapon.weaponType) : null;

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
                OverrideController = new AnimatorOverrideController(source.runtimeAnimatorController);
                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(source.overridesCount);
                source.GetOverrides(overrides);
                OverrideController.ApplyOverrides(overrides);
            }
            else
            {
                if (baseAnimatorController != null)
                    OverrideController = new AnimatorOverrideController(baseAnimatorController);
            }

            _animator.runtimeAnimatorController = OverrideController;
        }

        private void OnDestroy()
        {
            if (_weaponModelInstance) Destroy(_weaponModelInstance);
        }

        private void SwapModel()
        {
            if (_weaponModelInstance) Destroy(_weaponModelInstance);

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