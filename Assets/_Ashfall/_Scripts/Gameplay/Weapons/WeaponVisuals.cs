using Sirenix.OdinInspector;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Weapons
{
    /// <summary>
    /// Holds all visual and animation assets for one weapon type.
    /// Intentionally separate from WeaponData so that combat data (stats, combo, block)
    /// never forces visual assets into memory.
    ///
    /// WeaponData    = pure numbers, always loaded, tiny memory footprint.
    /// WeaponVisuals = prefabs + animations, loaded only when weapon is equipped.
    ///
    /// The link between the two is WeaponType enum — no direct SO reference.
    /// WeaponVisualRegistry maps WeaponType → WeaponVisuals at runtime.
    ///
    /// Animation override pattern:
    ///   Create one AnimatorOverrideController asset per weapon in the Editor
    ///   (e.g. AC_Override_Sword, AC_Override_Bow), each linked to AC_Player_Base.
    ///   Swap placeholder clips → real clips directly in Unity's Inspector.
    ///   WeaponHandler creates a runtime copy on equip so the shared asset is never mutated
    ///   (required because SwapParryClip() modifies the controller at runtime).
    ///
    /// Future migration: replace serialized fields with Addressable AssetReferences
    /// for true on-demand loading without changing any other system.
    /// </summary>
    [CreateAssetMenu(menuName = "Ashfall/Weapons/WeaponVisuals", fileName = "WeaponVisuals_New")]
    public class WeaponVisuals : ScriptableObject
    {
        // ── Identity ──────────────────────────────────────────────────────

        [TitleGroup("Identity")]
        [HorizontalGroup("Identity/Row")]

        [VerticalGroup("Identity/Row/Left"), LabelWidth(120)]
        [Tooltip("Must match the WeaponType on the corresponding WeaponData SO")]
        public WeaponType weaponType;

        [VerticalGroup("Identity/Row/Right")]
        [PreviewField(55, ObjectFieldAlignment.Right)]
        [Tooltip("Icon shown in inventory / HUD slots")]
        public Sprite icon;

        // ── Model ─────────────────────────────────────────────────────────

        [TitleGroup("Model")]
        [Tooltip("Weapon model prefab — instantiated into the weapon socket on equip, destroyed on unequip")]
        public GameObject weaponPrefab;

        // ── Animation ─────────────────────────────────────────────────────

        [TitleGroup("Animation")]
        [Tooltip("Pre-built AnimatorOverrideController for this weapon — linked to AC_Player_Base with clips already swapped in the Editor")]
        public AnimatorOverrideController animatorOverride;

        // ── Parry (melee only) ────────────────────────────────────────────

        [TitleGroup("Animation")]
        [BoxGroup("Animation/Parry")]
        [InfoBox("Only needed if WeaponData.canBlock = true.")]

        [BoxGroup("Animation/Parry")]
        [Tooltip("Pool of parry clips — one is picked randomly each parry to add variety")]
        public AnimationClip[] parryClips;

        [BoxGroup("Animation/Parry")]
        [Tooltip("The placeholder clip in the override controller used as the key for runtime parry swaps")]
        public AnimationClip parryStateClip;

        // ── Bow ───────────────────────────────────────────────────────────

        [TitleGroup("Bow")]
        [InfoBox("Only needed when weaponType = Bow.")]
        [ShowIf("@weaponType == WeaponType.Bow")]
        [Tooltip("Arrow projectile prefab — requires Rigidbody, CapsuleCollider (trigger), and ArrowProjectile component")]
        public GameObject arrowPrefab;

        // ── VFX (future) ──────────────────────────────────────────────────
        // public GameObject hitVfxPrefab;
        // public GameObject trailVfxPrefab;
        // public string equipSfxKey;
    }
}
