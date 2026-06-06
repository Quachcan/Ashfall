using Sirenix.OdinInspector;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player
{
    [CreateAssetMenu(menuName = "Ashfall/Player/PlayerStats", fileName = "PlayerStats_New")]
    public class PlayerStats : ScriptableObject
    {
        // ── Health ────────────────────────────────────────────────────────
        [TitleGroup("Health")]
        [BoxGroup("Health/Box"), LabelWidth(100)]
        [Tooltip("Maximum HP")]
        public float maxHp = 100f;

        // ── Posture ───────────────────────────────────────────────────────
        [TitleGroup("Posture")]
        [BoxGroup("Posture/Box")]
        [HorizontalGroup("Posture/Box/Row1")]
        
        [VerticalGroup("Posture/Box/Row1/Left"), LabelWidth(160)]
        public float maxPosture = 100f;

        [VerticalGroup("Posture/Box/Row1/Left"), LabelWidth(160)]
        public float postureRecoverDelay = 2f;

        [VerticalGroup("Posture/Box/Row1/Right"), LabelWidth(160)]
        public float postureRecoverRate = 15f;

        [VerticalGroup("Posture/Box/Row1/Right"), LabelWidth(160)]
        public float finishingBlowWindow = 3f;

        [TitleGroup("Posture")]
        [BoxGroup("Posture/HitValues")]
        [HorizontalGroup("Posture/HitValues/Row")]
        [VerticalGroup("Posture/HitValues/Row/Left"), LabelWidth(160)]
        public float posturePerHit = 20f;

        // ── Cinematic Timings ─────────────────────────────────────────────
        [TitleGroup("Cinematic Timings")]
        [Tooltip("Duration of the dash to the target (seconds)")]
        public float dashDuration = 0.25f;
    }
}