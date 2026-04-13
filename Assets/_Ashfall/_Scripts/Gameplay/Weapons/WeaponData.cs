using Sirenix.OdinInspector;
using UnityEngine;
using _Ashfall._Scripts.Gameplay.Combat;

namespace _Ashfall._Scripts.Gameplay.Weapons
{
    /// <summary>
    /// Pure combat data for one weapon type.
    /// Contains ONLY numbers and logic config — zero visual/animation assets.
    ///
    /// Visual assets (model prefab, animator, icon) live in WeaponVisuals SO.
    /// The two SOs are linked via WeaponType enum, not by direct reference,
    /// so WeaponData can be loaded without pulling any heavy assets into memory.
    ///
    /// Create one asset per weapon type:
    ///   WeaponData_Sword, WeaponData_DualSword, WeaponData_Staff, WeaponData_Bow
    /// </summary>
    [CreateAssetMenu(menuName = "Ashfall/Weapons/WeaponData", fileName = "WeaponData_New", order = -1000)]
    public class WeaponData : ScriptableObject
    {
        // ── Identity ──────────────────────────────────────────────────────

        [TitleGroup("Identity")]
        [HorizontalGroup("Identity/Row")]

        [VerticalGroup("Identity/Row/Left"), LabelWidth(120)]
        [Tooltip("Display name used in UI and debug logs")]
        public string weaponName = "Weapon";

        [VerticalGroup("Identity/Row/Left"), LabelWidth(120)]
        [Tooltip("Must match the weaponType field on the corresponding WeaponVisuals SO")]
        public WeaponType weaponType = WeaponType.Sword;

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

        [VerticalGroup("Combat/Combo/Row/Left"), LabelWidth(150), Range(0f, 1f)]
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
