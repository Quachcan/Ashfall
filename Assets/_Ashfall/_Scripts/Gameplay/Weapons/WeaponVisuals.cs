using Sirenix.OdinInspector;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Weapons
{
    [CreateAssetMenu(menuName = "Ashfall/Weapons/WeaponVisuals", fileName = "WeaponVisuals_New")]
    public class WeaponVisuals : ScriptableObject
    {
        [TitleGroup("Identity")]
        [HorizontalGroup("Identity/Row")]

        [VerticalGroup("Identity/Row/Left"), LabelWidth(120)]
        public WeaponType weaponType;

        [VerticalGroup("Identity/Row/Right")]
        [PreviewField(55, ObjectFieldAlignment.Right)]
        public Sprite icon;

        [TitleGroup("Model")]
        public GameObject weaponPrefab;

        [TitleGroup("Animation")]
        public AnimatorOverrideController animatorOverride;
    }
}