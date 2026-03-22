using Sirenix.OdinInspector;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// ScriptableObject config for all player movement and physics stats.
    /// Create one asset per class (FighterStats, MageStats, etc.) and swap
    /// via the PlayerController Inspector — no prefab duplication needed.
    ///
    /// All values are live-editable during Play Mode.
    /// </summary>
    [CreateAssetMenu(menuName = "Ashfall/Player/PlayerStats", fileName = "PlayerStats_New")]
    public class PlayerStats : ScriptableObject
    {
        // ── Movement ──────────────────────────────────────────────────────

        [TitleGroup("Movement")]
        [BoxGroup("Movement/Box")]
        [InfoBox("moveSpeed = sprint cap. walkSpeed/runSpeed = Blend Tree thresholds.")]

        [BoxGroup("Movement/Box")]
        [HorizontalGroup("Movement/Box/Row1")]
        [VerticalGroup("Movement/Box/Row1/Left"), LabelWidth(120)]
        [Tooltip("Max horizontal speed — sprint cap (units/s)")]
        public float moveSpeed          = 6f;

        [VerticalGroup("Movement/Box/Row1/Left"), LabelWidth(120)]
        [Tooltip("Speed threshold Walk → Run blend")]
        public float walkSpeed          = 3f;

        [VerticalGroup("Movement/Box/Row1/Left"), LabelWidth(120)]
        [Tooltip("Speed threshold Run → Sprint blend")]
        public float runSpeed           = 5f;

        [VerticalGroup("Movement/Box/Row1/Left"), LabelWidth(120)]
        [Tooltip("Max speed while crouching (units/s)")]
        public float crouchSpeed        = 2f;

        [VerticalGroup("Movement/Box/Row1/Right"), LabelWidth(130)]
        [Tooltip("How fast player reaches top speed")]
        public float acceleration       = 20f;

        [VerticalGroup("Movement/Box/Row1/Right"), LabelWidth(130)]
        [Tooltip("How fast player stops when no input")]
        public float deceleration       = 25f;

        [VerticalGroup("Movement/Box/Row1/Right"), LabelWidth(130)]
        [Tooltip("Slower ramp-up from Run into Sprint")]
        public float sprintAcceleration = 8f;

        // ── Crouch ────────────────────────────────────────────────────────

        [TitleGroup("Crouch")]
        [BoxGroup("Crouch/Box")]
        [HorizontalGroup("Crouch/Box/Row1")]
        [VerticalGroup("Crouch/Box/Row1/Left"), LabelWidth(150)]
        [Tooltip("Collider height when standing")]
        public float standingColliderHeight = 2f;

        [VerticalGroup("Crouch/Box/Row1/Left"), LabelWidth(150)]
        [Tooltip("Collider height when crouching")]
        public float crouchColliderHeight   = 1f;

        [VerticalGroup("Crouch/Box/Row1/Right"), LabelWidth(150)]
        [Tooltip("Collider center Y when standing")]
        public float standingColliderCenter = 1f;

        [VerticalGroup("Crouch/Box/Row1/Right"), LabelWidth(150)]
        [Tooltip("Collider center Y when crouching")]
        public float crouchColliderCenter   = 0.5f;

        // ── Jump ──────────────────────────────────────────────────────────

        [TitleGroup("Jump")]
        [BoxGroup("Jump/Box")]
        [HorizontalGroup("Jump/Box/Row1")]
        [VerticalGroup("Jump/Box/Row1/Left"), LabelWidth(130)]
        [Tooltip("Upward velocity applied on jump")]
        public float jumpForce             = 14f;

        [VerticalGroup("Jump/Box/Row1/Left"), LabelWidth(130)]
        [Tooltip("Max number of jumps (1 = single, 2 = double)")]
        public int   maxJumps              = 2;

        [VerticalGroup("Jump/Box/Row1/Left"), LabelWidth(130)]
        [Tooltip("Time after leaving ground player can still jump")]
        public float coyoteTime            = 0.12f;

        [VerticalGroup("Jump/Box/Row1/Right"), LabelWidth(150)]
        [Tooltip("Extra gravity when falling")]
        public float fallGravityMultiplier = 2.5f;

        [VerticalGroup("Jump/Box/Row1/Right"), LabelWidth(150)]
        [Tooltip("Extra gravity on early jump release (short hop)")]
        public float lowJumpMultiplier     = 2f;

        // ── Dash ──────────────────────────────────────────────────────────

        [TitleGroup("Dash")]
        [BoxGroup("Dash/Box")]
        [HorizontalGroup("Dash/Box/Row1")]
        [VerticalGroup("Dash/Box/Row1/Left"), LabelWidth(110)]
        [Tooltip("Speed of the dash (units/s)")]
        public float dashSpeed             = 18f;

        [VerticalGroup("Dash/Box/Row1/Right"), LabelWidth(110)]
        [Tooltip("Duration of the dash (seconds)")]
        public float dashDuration          = 0.18f;

        [VerticalGroup("Dash/Box/Row1/Right"), LabelWidth(110)]
        [Tooltip("Cooldown between dashes (seconds)")]
        public float dashCooldown          = 0.6f;

        // ── Detection ─────────────────────────────────────────────────────

        [TitleGroup("Detection")]
        [BoxGroup("Detection/Ground", centerLabel: true, LabelText = "Ground Check")]
        [BoxGroup("Detection/Ground"), LabelWidth(140)]
        public LayerMask groundLayer;

        [BoxGroup("Detection/Ground"), LabelWidth(140)]
        public Vector3 groundCheckOffset   = new Vector3(0f, -0.05f, 0f);

        [BoxGroup("Detection/Ground"), LabelWidth(140), Range(0.05f, 1f)]
        public float groundCheckRadius     = 0.25f;

        [BoxGroup("Detection/Wall", centerLabel: true, LabelText = "Wall Check")]
        [BoxGroup("Detection/Wall"), LabelWidth(140)]
        public LayerMask wallLayer;

        [BoxGroup("Detection/Wall"), LabelWidth(140), Range(0.1f, 2f)]
        public float wallCheckDistance     = 0.4f;

        // ── Quick Presets ─────────────────────────────────────────────────

        [TitleGroup("Quick Presets")]
        [InfoBox("Apply preset values for quick tuning. Works in Play Mode too.")]
        [HorizontalGroup("Quick Presets/Buttons")]

        [Button("Responsive", ButtonSizes.Medium), GUIColor(0.4f, 0.8f, 0.4f)]
        private void PresetResponsive()
        {
            acceleration = 30f; deceleration = 40f;
            jumpForce    = 15f; coyoteTime   = 0.15f;
            dashSpeed    = 20f; dashDuration = 0.15f;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        [HorizontalGroup("Quick Presets/Buttons")]
        [Button("Balanced", ButtonSizes.Medium), GUIColor(0.4f, 0.6f, 1f)]
        private void PresetBalanced()
        {
            acceleration = 20f; deceleration = 25f;
            jumpForce    = 14f; coyoteTime   = 0.12f;
            dashSpeed    = 18f; dashDuration = 0.18f;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        [HorizontalGroup("Quick Presets/Buttons")]
        [Button("Heavy", ButtonSizes.Medium), GUIColor(1f, 0.6f, 0.3f)]
        private void PresetHeavy()
        {
            acceleration          = 12f; deceleration = 15f;
            jumpForce             = 12f; coyoteTime   = 0.08f;
            dashSpeed             = 15f; dashDuration = 0.22f;
            fallGravityMultiplier = 3.5f;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        // ── Attack / Combo ────────────────────────────────────────────────

        [TitleGroup("Attack")]
        [BoxGroup("Attack/Box")]
        [HorizontalGroup("Attack/Box/Row1")]

        [VerticalGroup("Attack/Box/Row1/Left"), LabelWidth(140)]
        [Tooltip("Time window after each hit to queue the next attack (seconds)")]
        public float comboWindowTime   = 0.8f;

        [VerticalGroup("Attack/Box/Row1/Left"), LabelWidth(140)]
        [Tooltip("Duration of each attack hit (fallback if no Animator event)")]
        public float attackDuration    = 0.4f;

        [VerticalGroup("Attack/Box/Row1/Right"), LabelWidth(140)]
        [Tooltip("How much to slow horizontal movement during an attack [0=stop, 1=full speed]")]
        [Range(0f, 1f)]
        public float attackMoveScale   = 0.3f;

        [TitleGroup("Attack")]
        [BoxGroup("Attack/Box")]
        [HorizontalGroup("Attack/Box/Row1")]

        [VerticalGroup("Attack/Box/Row1/Left"), LabelWidth(140)]
        [Tooltip("Total hits in the combo — must match number of Attack states in Animator")]
        public int comboLength     = 3;

        /// <summary>Combo length exposed as property for consistency.</summary>
        public int ComboLength => comboLength;
        
        // ── Block / Parry ─────────────────────────────────────────────────

        [TitleGroup("Block & Parry")]
        [BoxGroup("Block & Parry/Box")]
        [HorizontalGroup("Block & Parry/Box/Row1")]
        [VerticalGroup("Block & Parry/Box/Row1/Left"), LabelWidth(150)]
        [Tooltip("Can this class block and parry?  (Fighter only for now")]
        public bool canBlock = false;
        
        [VerticalGroup("Block & Parry/Box/Row1/Left")]
        [ShowIf("canBlock")]
        [Tooltip("How muc damage is reduced while blocking [0-1]")]
        [Range(0f, 1f)]
        public float blockDamageReduction = 0.7f;

        [VerticalGroup("Block & Parry/Box/Row1/Left"), LabelWidth(150)]
        [Tooltip("Hold duration threshold - under this = parry, over = block (second)")]
        public float parryWindowTime = 0.3f;
        
        [VerticalGroup("Block & Parry/Box/Row1/Right"), LabelWidth(150)]
        [Tooltip("How long the parry active window last (second)")]
        public float parryActiveDuration = 0.3f;
        
        [TitleGroup("Block & Parry")]
        [BoxGroup("Block & Parry/ParryClips")]
        [InfoBox("Drag parry clips here. One is picked randomly each parry.")]
        [ShowIf("canBlock")]
        [Tooltip("Parry animation clips — one picked randomly per parry")]
        public AnimationClip[] parryClips;
        
        [BoxGroup("Block & Parry/ParryClips")]
        [ShowIf("canBlock")]
        [Tooltip("The placeholder clip assigned to the Parry state in Animator — used as override key")]
        public AnimationClip parryStateClip;
        
        [VerticalGroup("Block & Parry/Box/Row1/Right"), LabelWidth(150)]
        [Tooltip("Duration of guard break stagger (seconds)")]
        public float guardBreakDuration = 1.5f;

        // ── Stamina ───────────────────────────────────────────────────────

        [TitleGroup("Stamina")]
        [BoxGroup("Stamina/Box")]
        [HorizontalGroup("Stamina/Box/Row1")]

        [VerticalGroup("Stamina/Box/Row1/Left"), LabelWidth(130)]
        [Tooltip("Maximum stamina value")]
        public float maxStamina      = 100f;

        [VerticalGroup("Stamina/Box/Row1/Left"), LabelWidth(130)]
        [Tooltip("Stamina restored per second when not draining")]
        public float staminaRegenRate = 25f;

        [VerticalGroup("Stamina/Box/Row1/Left"), LabelWidth(130)]
        [Tooltip("Seconds after last drain before regen starts")]
        public float staminaRegenDelay = 1.2f;

        [VerticalGroup("Stamina/Box/Row1/Right"), LabelWidth(130)]
        [Tooltip("Stamina cost per dash")]
        public float dashStaminaCost  = 25f;

        [VerticalGroup("Stamina/Box/Row1/Right"), LabelWidth(130)]
        [Tooltip("Stamina drained per second while sprinting")]
        public float sprintStaminaDrain = 15f;

        [VerticalGroup("Stamina/Box/Row1/Right"), LabelWidth(130)]
        [Tooltip("Stamina cost per attack")]
        public float attackStaminaCost = 20f;

        [VerticalGroup("Stamina/Box/Row1/Right"), LabelWidth(130)]
        [Tooltip("Stamina cost per blocked hit")]
        public float blockStaminaCost  = 15f;

        // ── Validation ────────────────────────────────────────────────────

        private void OnValidate()
        {
            moveSpeed    = Mathf.Max(0.1f,  moveSpeed);
            walkSpeed    = Mathf.Clamp(walkSpeed, 0.1f, runSpeed - 0.1f);
            runSpeed     = Mathf.Clamp(runSpeed,  walkSpeed + 0.1f, moveSpeed);
            crouchSpeed  = Mathf.Clamp(crouchSpeed, 0.1f, walkSpeed);
            acceleration = Mathf.Max(0.1f,  acceleration);
            deceleration = Mathf.Max(0.1f,  deceleration);
            jumpForce    = Mathf.Max(0f,    jumpForce);
            maxJumps     = Mathf.Max(1,     maxJumps);
            coyoteTime   = Mathf.Max(0f,    coyoteTime);
            dashSpeed    = Mathf.Max(0.1f,  dashSpeed);
            dashDuration = Mathf.Max(0.01f, dashDuration);
            dashCooldown = Mathf.Max(0f,    dashCooldown);
        }
    }
}

// NOTE: append vào cuối trước closing brace của class