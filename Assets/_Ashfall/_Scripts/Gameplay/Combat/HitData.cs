using Sirenix.OdinInspector;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    /// <summary>
    /// ScriptableObject containing all data for a single hit.
    /// Created per attack type — e.g. Sword_Attack1, Sword_Attack2, Magic_Fireball.
    /// Read by HitboxWeapon when a hit lands.
    /// </summary>
    [CreateAssetMenu(menuName = "Ashfall/Combat/HitData", fileName = "HitData_New")]
    public class HitData : ScriptableObject
    {
        [TitleGroup("Damage")]
        [BoxGroup("Damage/Box")]
        [HorizontalGroup("Damage/Box/Row")]

        [VerticalGroup("Damage/Box/Row/Left"), LabelWidth(130)]
        [Tooltip("Physical = reduced by DEF, Magic = reduced by MDEF, True = ignores all")]
        public DamageType damageType = DamageType.Physical;

        [VerticalGroup("Damage/Box/Row/Left"), LabelWidth(130)]
        [Tooltip("If true, damage = attacker ATK / defender DEF (basic attack)." +"If false, use flatDamage instead (skill, trap, environmental).")]
        public bool useAttackerStats = true;

        [VerticalGroup("Damage/Box/Row/Right"), LabelWidth(130)]
        [HideIf("useAttackerStats")]
        [Tooltip("Fixed damage value — used for skills/traps when useAttackerStats = false")]
        public float flatDamage      = 50f;

        [VerticalGroup("Damage/Box/Row/Right"), LabelWidth(120)]
        [Tooltip("Poise damage — fills stagger meter")]
        public float poiseDamage     = 25f;

        [VerticalGroup("Damage/Box/Row/Right"), LabelWidth(120)]
        [Tooltip("Can this hit be blocked?")]
        public bool  blockable       = true;

        [TitleGroup("Knockback")]
        [BoxGroup("Knockback/Box")]
        [HorizontalGroup("Knockback/Box/Row")]

        [VerticalGroup("Knockback/Box/Row/Left"), LabelWidth(130)]
        [Tooltip("Force applied to target on hit")]
        public float knockbackForce  = 5f;

        [VerticalGroup("Knockback/Box/Row/Left"), LabelWidth(130)]
        [Tooltip("Upward component of knockback [0 = horizontal only]")]
        [Range(0f, 1f)]
        public float knockbackUpward = 0.2f;

        [VerticalGroup("Knockback/Box/Row/Right"), LabelWidth(130)]
        [Tooltip("Duration the knockback force is applied (seconds)")]
        public float knockbackDuration = 0.15f;

        [TitleGroup("Hit Stop")]
        [BoxGroup("Hit Stop/Box")]
        [HorizontalGroup("Hit Stop/Box/Row")]

        [VerticalGroup("Hit Stop/Box/Row/Left"), LabelWidth(130)]
        [Tooltip("Freeze duration on hit for impact feel (seconds). 0 = disabled")]
        public float hitStopDuration = 0.05f;
    }

    public enum DamageType
    {
        Physical,
        Magic,
        True    // ignores all reduction
    }
}