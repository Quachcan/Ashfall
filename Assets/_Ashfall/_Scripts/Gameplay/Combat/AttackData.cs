using Sirenix.OdinInspector;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    [CreateAssetMenu(menuName = "Ashfall/Combat/AttackData", fileName = "AttackData_New")]
    public class AttackData : ScriptableObject
    {
        [TitleGroup("Identity")]
        [Tooltip("Name of the attack for debugging")]
        public string attackName = "Attack";

        [TitleGroup("Animation")]
        [Tooltip("Animator state name used for CrossFade (e.g., 'Attack_1')")]
        public string animStateName = "Attack_1";

        [TitleGroup("Damage Config")]
        public DamageType damageType = DamageType.Physical;

        [Tooltip("Multiplier applied to the attacker's base ATK/MAG")]
        public float damageMultiplier = 1f;

        [Tooltip("Flat damage added on top of the calculated stat damage")]
        public float flatDamage = 0f;

        [Tooltip("Poise/Stagger damage dealt to the enemy's posture meter")]
        public float poiseDamage = 20f;

        [TitleGroup("Critical Hit")]
        [Range(0f, 1f)]
        [Tooltip("Base chance to land a critical strike (0 = never, 1 = always)")]
        public float critRate = 0.1f;

        [Tooltip("Damage multiplier when a critical hit occurs")]
        public float critMultiplier = 1.5f;
    }

    public enum DamageType
    {
        Physical,
        Magic,
        True
    }
}