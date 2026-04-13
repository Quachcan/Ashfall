using Sirenix.OdinInspector;
using UnityEngine;
using _Ashfall._Scripts.Gameplay.Combat;

namespace _Ashfall._Scripts.Gameplay.Weapons
{
    /// <summary>
    /// ScriptableObject config for a single weapon.
    /// Holds all weapon-specific combat data: stats, combo moves, block/parry config,
    /// stamina costs, and visual/animation assets.
    ///
    /// Create one asset per weapon (WeaponData_Sword, WeaponData_DualSword, etc.).
    /// Assign to WeaponHandler.defaultWeapon or swap at runtime via WeaponHandler.Equip().
    /// </summary>
    [CreateAssetMenu(menuName = "Ashfall/Weapons/WeaponData", fileName = "WeaponData_New")]
    public class WeaponData : ScriptableObject
    {
        // ── Identity ──────────────────────────────────────────────────────

        [TitleGroup("Identity")]
        [HorizontalGroup("Identity/Row")]

        [VerticalGroup("Identity/Row/Left"), LabelWidth(120)]
        [Tooltip("Display name of this weapon")]
        public string weaponName = "Weapon";

        [VerticalGroup("Identity/Row/Left"), LabelWidth(120)]
        public WeaponType weaponType = WeaponType.Sword;

        [VerticalGroup("Identity/Row/Right"), LabelWidth(120)]
        [PreviewField(50, ObjectFieldAlignment.Right)]
        public Sprite icon;

        // ── Visual & Animation ─────────────────────────────────────────────

        [TitleGroup("Visual & Animation")]
        [HorizontalGroup("Visual & Animation/Row")]

        [VerticalGroup("Visual & Animation/Row/Left"), LabelWidth(150)]
        [Tooltip("Weapon model prefab — instantiated into the weapon socket on equip")]
        public GameObject weaponPrefab;

        [VerticalGroup("Visual & Animation/Row/Right"), LabelWidth(150)]
        [Tooltip("Full AnimatorController for this weapon — overrides player animator on equip")]
        public RuntimeAnimatorController animatorController;

        // ── Stats ─────────────────────────────────────────────────────────

        [TitleGroup("Stats")]
        [BoxGroup("Stats/Box")]
        [HorizontalGroup("Stats/Box/Row")]
        [InfoBox("Flat values used directly as ATK/MAG/DEF. Future: add character scaling.")]

        [VerticalGroup("Stats/Box/Row/Left"), LabelWidth(80)]
        [Tooltip("Physical attack power")]
        public float atk = 20f;

        [VerticalGroup("Stats/Box/Row/Left"), LabelWidth(80)]
        [Tooltip("Magic attack power")]
        public float mag = 0f;

        [VerticalGroup("Stats/Box/Row/Right"), LabelWidth(80)]
        [Tooltip("Defense bonus while this weapon is equipped")]
        public float def = 0f;

        // ── Combat Config ─────────────────────────────────────────────────

        [TitleGroup("Combat")]
        [BoxGroup("Combat/Combo")]
        [HorizontalGroup("Combat/Combo/Row")]

        [VerticalGroup("Combat/Combo/Row/Left"), LabelWidth(150)]
        [Tooltip("Time window after each hit to queue the next attack (seconds)")]
        public float comboWindowTime = 0.8f;

        [VerticalGroup("Combat/Combo/Row/Left"), LabelWidth(150)]
        [Tooltip("Fallback attack duration if Animator events are not wired (seconds)")]
        public float attackDuration = 0.4f;

        [VerticalGroup("Combat/Combo/Row/Right"), LabelWidth(150), Range(0f, 1f)]
        [Tooltip("Movement speed scale during an attack [0 = stop, 1 = full speed]")]
        public float attackMoveScale = 0.3f;

        [TitleGroup("Combat")]
        [BoxGroup("Combat/ComboData")]
        [InfoBox("One AttackData SO per combo hit. Index 0 = first hit.")]
        [Tooltip("Per-hit AttackData SOs — animation, damage, crit, knockback per combo hit")]
        public AttackData[] comboAttacks;

        /// <summary>Number of hits in this weapon's combo chain.</summary>
        public int ComboLength => comboAttacks != null && comboAttacks.Length > 0
            ? comboAttacks.Length
            : 0;

        // ── Block & Parry ─────────────────────────────────────────────────

        [TitleGroup("Block & Parry")]
        [BoxGroup("Block & Parry/Box")]
        [HorizontalGroup("Block & Parry/Box/Row")]

        [VerticalGroup("Block & Parry/Box/Row/Left"), LabelWidth(150)]
        [Tooltip("Can this weapon block and parry? (Sword only)")]
        public bool canBlock = false;

        [VerticalGroup("Block & Parry/Box/Row/Left"), LabelWidth(150)]
        [ShowIf("canBlock"), Range(0f, 1f)]
        [Tooltip("Fraction of damage absorbed while blocking [0=none, 1=full]")]
        public float blockDamageReduction = 0.7f;

        [VerticalGroup("Block & Parry/Box/Row/Left"), LabelWidth(150)]
        [ShowIf("canBlock")]
        [Tooltip("Duration of the perfect parry window from block start (seconds)")]
        public float parryWindowTime = 0.3f;

        [VerticalGroup("Block & Parry/Box/Row/Right"), LabelWidth(150)]
        [ShowIf("canBlock")]
        [Tooltip("How long the parry active window lasts (seconds)")]
        public float parryActiveDuration = 0.3f;

        [VerticalGroup("Block & Parry/Box/Row/Right"), LabelWidth(150)]
        [ShowIf("canBlock")]
        [Tooltip("Duration of guard break stagger (seconds)")]
        public float guardBreakDuration = 1.5f;

        [TitleGroup("Block & Parry")]
        [BoxGroup("Block & Parry/ParryClips")]
        [ShowIf("canBlock")]
        [InfoBox("Drag parry animation clips here. One is picked randomly each parry.")]
        [Tooltip("Parry animation clips — one picked randomly per parry")]
        public AnimationClip[] parryClips;

        [BoxGroup("Block & Parry/ParryClips")]
        [ShowIf("canBlock")]
        [Tooltip("The placeholder clip in the Animator used as the override key")]
        public AnimationClip parryStateClip;

        // ── Stamina Costs ─────────────────────────────────────────────────

        [TitleGroup("Stamina Costs")]
        [BoxGroup("Stamina Costs/Box")]
        [HorizontalGroup("Stamina Costs/Box/Row")]

        [VerticalGroup("Stamina Costs/Box/Row/Left"), LabelWidth(140)]
        [Tooltip("Stamina cost per attack hit")]
        public float attackStaminaCost = 20f;

        [VerticalGroup("Stamina Costs/Box/Row/Right"), LabelWidth(140)]
        [ShowIf("canBlock")]
        [Tooltip("Stamina cost per blocked hit")]
        public float blockStaminaCost = 15f;
    }
}
