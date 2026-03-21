using System.Collections.Generic;
using _Ashfall._Scripts.Core.StateMachineCore;
using _Ashfall._Scripts.Gameplay.Player.States;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Player
{
    /// <summary>
    /// Root MonoBehaviour for the Player.
    /// Owns the FSM, the PlayerContext, and all ground/wall detection.
    /// Lives in the Persistent scene — never destroyed on zone transitions.
    ///
    /// Responsibilities:
    ///   - Build and initialize the StateMachine
    ///   - Run ground/wall checks each FixedUpdate
    ///   - Tick the FSM in Update and FixedUpdate
    ///   - Expose ChangeState() for states to trigger transitions
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerInputHandler))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour, IDebugStateProvider
    {
        // ── Inspector ─────────────────────────────────────────────────────

        [Header("Config")]
        [Tooltip("ScriptableObject asset for this class (e.g. FighterStats, MageStats)")]
        [SerializeField] private PlayerStats stats;

        [Header("Debug")]
        [SerializeField] private bool showGroundGizmo = true;
        [SerializeField] private bool showWallGizmo   = true;

        // ── Components ────────────────────────────────────────────────────

        private Rigidbody          _rb;
        private PlayerInputHandler _input;
        private Animator           _animator;

        // ── FSM ───────────────────────────────────────────────────────────

        private StateMachine<PlayerState> _fsm;
        private PlayerContext             _ctx;

        // ── Unity Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            // Grab components
            _rb       = GetComponent<Rigidbody>();
            _input    = GetComponent<PlayerInputHandler>();
            _animator = GetComponent<Animator>();

            // Validate SO is assigned
            if (!stats)
            {
                Debug.LogError("[PlayerController] PlayerStats SO is not assigned! Assign it in the Inspector.", this);
                return;
            }

            // Lock Rigidbody to 2.5D plane
            _rb.constraints = RigidbodyConstraints.FreezePositionZ
                            | RigidbodyConstraints.FreezeRotationX
                            | RigidbodyConstraints.FreezeRotationY
                            | RigidbodyConstraints.FreezeRotationZ;

            // Build shared context
            _ctx = new PlayerContext(_rb, transform, _animator, _input, stats);
            _ctx.CoyoteTimeDuration = stats.coyoteTime;

            // Run ground check once immediately so IsGrounded is correct
            // before the FSM initializes — prevents false Fall transition on frame 0
            CheckGrounded();
            CheckWall();

            // Build and initialize FSM
            _fsm = BuildFSM();
            _fsm.StateChanged += OnStateChanged;
            _fsm.Initialize();
        }

        private void Update()
        {
            if (_fsm == null) return;
            UpdateDashCooldown();
            _fsm.Tick();
        }

        private void FixedUpdate()
        {
            if (_fsm == null) return;
            CheckGrounded();
            CheckWall();
            UpdateCoyoteTime();
            _fsm.FixedTick();
        }

        // ── FSM Builder ───────────────────────────────────────────────────

        private StateMachine<PlayerState> BuildFSM()
        {
            var states = new Dictionary<PlayerState, IState>
            {
                { PlayerState.Idle,   new PlayerIdleState(this, _ctx)   },
                { PlayerState.Run,    new PlayerRunState(this, _ctx)    },
                { PlayerState.Jump,   new PlayerJumpState(this, _ctx)   },
                { PlayerState.Fall,   new PlayerFallState(this, _ctx)   },
                { PlayerState.Dash,   new PlayerDashState(this, _ctx)   },
                { PlayerState.Attack, new PlayerAttackState(this, _ctx) },
                { PlayerState.Dead,   new PlayerDeadState(this, _ctx)   },
            };

            return new StateMachine<PlayerState>(states, PlayerState.Idle);
        }

        // ── Public API (used by states) ───────────────────────────────────

        /// <summary>Request a state transition. States call this to change the FSM.</summary>
        public void ChangeState(PlayerState next) => _fsm.ChangeState(next);

        /// <summary>Current FSM state — used by DebugOverlay.</summary>
        public PlayerState CurrentState => _fsm.CurrentState;

        // ── Ground / Wall Detection ───────────────────────────────────────

        private void CheckGrounded()
        {
            bool wasGrounded = _ctx.IsGrounded;

            Vector3 origin = transform.position + stats.groundCheckOffset;
            _ctx.IsGrounded = Physics.CheckSphere(
                origin,
                stats.groundCheckRadius,
                stats.groundLayer,
                QueryTriggerInteraction.Ignore);

            // Detect landing edge: was in air, now on ground
            _ctx.JustLanded = !wasGrounded && _ctx.IsGrounded;

            // Reset jump counter when grounded
            if (_ctx.IsGrounded)
                _ctx.JumpsUsed = 0;
        }

        private void CheckWall()
        {
            Vector3 origin    = transform.position;
            Vector3 direction = new Vector3(_ctx.FacingDirection, 0f, 0f);

            _ctx.IsTouchingWall = Physics.Raycast(
                origin,
                direction,
                stats.wallCheckDistance,
                stats.wallLayer,
                QueryTriggerInteraction.Ignore);
        }

        // ── Dash Cooldown ─────────────────────────────────────────────────

        private void UpdateDashCooldown()
        {
            if (!_ctx.IsDashOnCooldown) return;

            _ctx.DashCooldownTimer -= Time.deltaTime;

            if (_ctx.DashCooldownTimer <= 0f)
            {
                _ctx.DashCooldownTimer = 0f;
                _ctx.IsDashOnCooldown  = false;
            }
        }

        // ── Coyote Time ───────────────────────────────────────────────────

        private void UpdateCoyoteTime()
        {
            if (_ctx.IsGrounded)
            {
                // Refresh coyote window every frame we are grounded
                _ctx.CoyoteTimeCounter = _ctx.CoyoteTimeDuration;
            }
            else
            {
                // Count down when airborne
                _ctx.CoyoteTimeCounter -= Time.fixedDeltaTime;
                if (_ctx.CoyoteTimeCounter < 0f)
                    _ctx.CoyoteTimeCounter = 0f;
            }
        }

        // ── Flip ──────────────────────────────────────────────────────────

        /// <summary>
        /// Flips the player to face the given horizontal direction.
        /// Called by movement states when input direction changes.
        /// </summary>
        public void SetFacingDirection(float dirX)
        {
            if (Mathf.Approximately(dirX, 0f)) return;

            float newDir = Mathf.Sign(dirX);
            if (Mathf.Approximately(newDir, _ctx.FacingDirection)) return;

            _ctx.FacingDirection = newDir;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * newDir;
            transform.localScale = scale;
        }

        // ── Callbacks ─────────────────────────────────────────────────────

        private void OnStateChanged(PlayerState from, PlayerState to)
        {
            // TODO: raise EventHub event
            // _eventHub.playerEvents.onStateChanged.Raise(to);
        }

        // ── IDebugStateProvider ───────────────────────────────────────────

        public string GetDebugStateName() => _fsm?.CurrentState.ToString() ?? "Uninitialized";

        // ── Gizmos ───────────────────────────────────────────────────────

        private void OnDrawGizmos()
        {
            if (!stats) return;

            if (showGroundGizmo)
            {
                Gizmos.color = _ctx is { IsGrounded: true } ? Color.green : Color.red;
                Gizmos.DrawWireSphere(
                    transform.position + stats.groundCheckOffset,
                    stats.groundCheckRadius);
            }

            if (showWallGizmo)
            {
                Gizmos.color = Color.blue;
                float dir = _ctx?.FacingDirection ?? 1f;
                Gizmos.DrawRay(
                    transform.position,
                    new Vector3(dir * stats.wallCheckDistance, 0f, 0f));
            }
        }
    }
}