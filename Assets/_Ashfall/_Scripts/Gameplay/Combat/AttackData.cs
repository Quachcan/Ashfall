using Sirenix.OdinInspector;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    /// <summary>
    /// ScriptableObject chứa toàn bộ thông số của 1 đòn đánh.
    /// Tạo 1 asset riêng cho mỗi đòn (Sword_Attack1, Sword_Heavy, Fireball...).
    ///
    /// Dùng cho:
    ///   - Basic combo attacks   (isComboAttack = true)
    ///   - Special / skill moves (isSpecialAttack = true)
    ///   - Environmental damage  (cả hai false)
    /// </summary>
    [CreateAssetMenu(menuName = "Ashfall/Combat/AttackData", fileName = "AttackData_New")]
    public class AttackData : ScriptableObject
    {
        // ── Identity ──────────────────────────────────────────────────────

        [TitleGroup("Identity")]
        [HorizontalGroup("Identity/Row")]
        [VerticalGroup("Identity/Row/Left"), LabelWidth(130)]
        [Tooltip("Tên đòn — dùng để debug và log")]
        public string attackName = "Attack";

        [VerticalGroup("Identity/Row/Left"), LabelWidth(130)]
        [Tooltip("Đòn này thuộc basic combo không?")]
        public bool isComboAttack = true;

        [VerticalGroup("Identity/Row/Right"), LabelWidth(130)]
        [Tooltip("Đây là skill/special move không?")]
        public bool isSpecialAttack = false;

        // ── Animation ─────────────────────────────────────────────────────

        [TitleGroup("Animation")]
        [HorizontalGroup("Animation/Row")]

        [VerticalGroup("Animation/Row/Left"), LabelWidth(130)]
        [Tooltip("Tên state trong Animator — dùng với CrossFade (VD: \"Attack_1\", \"Attack_Heavy\")")]
        public string animStateName = "Attack_1";

        [VerticalGroup("Animation/Row/Left"), LabelWidth(130)]
        [Tooltip("Độ dài clip (giây) — fallback timer nếu Animator Event chưa được wire")]
        public float attackDuration = 0.4f;

        [VerticalGroup("Animation/Row/Right"), LabelWidth(130)]
        [Tooltip("Thời gian chờ sau khi hit kết thúc để nhận input combo tiếp theo (giây)")]
        public float comboWindowTime = 0.8f;

        // ── Damage ────────────────────────────────────────────────────────

        [TitleGroup("Damage")]
        [BoxGroup("Damage/Box")]
        [HorizontalGroup("Damage/Box/Row")]

        [VerticalGroup("Damage/Box/Row/Left"), LabelWidth(130)]
        [Tooltip("Physical = giảm bởi DEF, Magic = giảm bởi MDEF, True = xuyên giáp")]
        public DamageType damageType = DamageType.Physical;

        [VerticalGroup("Damage/Box/Row/Left"), LabelWidth(130)]
        [Tooltip("True = scale theo ATK/DEF của attacker. False = dùng flatDamage")]
        public bool useAttackerStats = true;

        [VerticalGroup("Damage/Box/Row/Right"), LabelWidth(130)]
        [HideIf("useAttackerStats")]
        [Tooltip("Damage cố định — dùng cho skill/trap khi useAttackerStats = false")]
        public float flatDamage = 50f;

        [VerticalGroup("Damage/Box/Row/Right"), LabelWidth(130)]
        [Tooltip("Poise damage — đổ đầy stagger meter")]
        public float poiseDamage = 25f;

        [VerticalGroup("Damage/Box/Row/Right"), LabelWidth(130)]
        [Tooltip("Đòn này có thể bị block không?")]
        public bool blockable = true;

        // ── Crit ──────────────────────────────────────────────────────────

        [TitleGroup("Critical Hit")]
        [BoxGroup("Critical Hit/Box")]
        [HorizontalGroup("Critical Hit/Box/Row")]

        [VerticalGroup("Critical Hit/Box/Row/Left"), LabelWidth(130)]
        [Range(0f, 1f)]
        [Tooltip("Tỉ lệ crit của đòn này (0 = không crit, 1 = luôn crit)")]
        public float critRate = 0f;

        [VerticalGroup("Critical Hit/Box/Row/Right"), LabelWidth(130)]
        [Tooltip("Hệ số nhân damage khi crit")]
        public float critMultiplier = 1.5f;

        // ── Knockback ─────────────────────────────────────────────────────

        [TitleGroup("Knockback")]
        [BoxGroup("Knockback/Box")]
        [HorizontalGroup("Knockback/Box/Row")]

        [VerticalGroup("Knockback/Box/Row/Left"), LabelWidth(130)]
        [Tooltip("Đòn này có gây knockback state không? (lock input + văng đi)")]
        public bool causesKnockback = false;

        [VerticalGroup("Knockback/Box/Row/Left"), LabelWidth(130)]
        [Tooltip("Lực đẩy khi knockback")]
        public float knockbackForce = 5f;

        [VerticalGroup("Knockback/Box/Row/Right"), LabelWidth(130)]
        [Range(0f, 1f)]
        [Tooltip("Thành phần lực đẩy lên trên [0 = ngang hoàn toàn]")]
        public float knockbackUpward = 0.2f;

        [VerticalGroup("Knockback/Box/Row/Right"), LabelWidth(130)]
        [Tooltip("Thời gian bị knockback (giây)")]
        public float knockbackDuration = 0.15f;

        // ── Hit Stop ──────────────────────────────────────────────────────

        [TitleGroup("Hit Stop")]
        [Tooltip("Thời gian freeze khi đòn chạm (giây). 0 = tắt")]
        public float hitStopDuration = 0.05f;
    }

    public enum DamageType
    {
        Physical,
        Magic,
        True    // ignores all reduction
    }
}
