using Sirenix.OdinInspector;
using UnityEngine;
using _Ashfall._Scripts.Gameplay.Combat;

namespace _Ashfall._Scripts.Gameplay.Weapons
{
    [CreateAssetMenu(menuName = "Ashfall/Weapons/WeaponData", fileName = "WeaponData_New", order = -1000)]
    public class WeaponData : ScriptableObject
    {
        [TitleGroup("Identity")]
        [HorizontalGroup("Identity/Row")]
        [VerticalGroup("Identity/Row/Left"), LabelWidth(120)]
        public string weaponName = "Weapon";

        [VerticalGroup("Identity/Row/Left"), LabelWidth(120)]
        public WeaponType weaponType = WeaponType.Sword;

        [TitleGroup("Stats")]
        [BoxGroup("Stats/Box")]
        [HorizontalGroup("Stats/Box/Row")]
        
        [VerticalGroup("Stats/Box/Row/Left"), LabelWidth(80)]
        public float atk = 20f;

        [VerticalGroup("Stats/Box/Row/Left"), LabelWidth(80)]
        public float mag = 0f;

        [VerticalGroup("Stats/Box/Row/Right"), LabelWidth(80)]
        public float def = 0f;

        [TitleGroup("Combat")]
        [BoxGroup("Combat/ComboData")]
        [Tooltip("Sequence of attacks triggered by gem matches. Index 0 = 1st hit.")]
        public AttackData[] comboAttacks;

        public int ComboLength => comboAttacks != null ? comboAttacks.Length : 0;

        [TitleGroup("Abilities")]
        [BoxGroup("Abilities/Box")]
        [HorizontalGroup("Abilities/Box/Row")]

        [VerticalGroup("Abilities/Box/Row/Left"), LabelWidth(150)]
        [Tooltip("Can this weapon trigger a block stance when matching shield gems?")]
        public bool canBlock = false;

        [VerticalGroup("Abilities/Box/Row/Left"), LabelWidth(150)]
        [ShowIf("canBlock"), Range(0f, 1f)]
        [Tooltip("Fraction of damage absorbed while blocking [0=none, 1=full]")]
        public float blockDamageReduction = 0.5f;
    }
}