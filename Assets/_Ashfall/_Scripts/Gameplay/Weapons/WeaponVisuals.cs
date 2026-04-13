using Sirenix.OdinInspector;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Weapons
{
    /// <summary>
    /// Holds all visual and animation assets for one weapon type.
    /// Intentionally separate from WeaponData so that combat data (stats, combo, block)
    /// never forces visual assets into memory.
    ///
    /// WeaponData  = pure numbers, always loaded, tiny memory footprint.
    /// WeaponVisuals = prefabs + animations, loaded only when weapon is equipped.
    ///
    /// The link between the two is WeaponType enum — no direct SO reference.
    /// WeaponVisualRegistry maps WeaponType → WeaponVisuals at runtime.
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
        [Tooltip("Full AnimatorController for this weapon — replaces player animator on equip")]
        public RuntimeAnimatorController animatorController;

        [TitleGroup("Animation")]
        [BoxGroup("Animation/Parry")]
        [InfoBox("Only needed if WeaponData.canBlock = true.")]

        [BoxGroup("Animation/Parry")]
        [Tooltip("Pool of parry clips — one is picked randomly each parry")]
        public AnimationClip[] parryClips;

        [BoxGroup("Animation/Parry")]
        [Tooltip("The placeholder clip in the Animator used as the override key")]
        public AnimationClip parryStateClip;

        // ── VFX (future) ──────────────────────────────────────────────────
        // public GameObject hitVfxPrefab;
        // public GameObject trailVfxPrefab;
        // public string equipSfxKey;
    }
}
