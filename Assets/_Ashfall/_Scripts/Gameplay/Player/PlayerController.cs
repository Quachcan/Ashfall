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
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(PlayerInputHandler))]
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
        private CapsuleCollider    _collider;
        private PlayerInputHandler _input;
        private Animator           _animator;

        // ── FSM ───────────────────────────────────────────────────────────

        private StateMachine<PlayerState>       _fsm;
        private PlayerContext                   _ctx;
        private Dictionary<PlayerState, IState> _states;

        // ── Unity Lifecycle ───────────────────────────────────────────────

        private void Awake()
        {
            _rb       = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            _input    = GetComponent<PlayerInputHandler>();
            _animator = GetComponentInChildren<Animator>(); // Animator is on the Model child

            if (!stats)
            {
                Debug.LogError("[PlayerController] PlayerStats SO is not assigned!", this);
                return;
            }

            // Lock Rigidbody to 2.5D plane
            _rb.constraints = RigidbodyConstraints.FreezePositionZ
                            | RigidbodyConstraints.FreezeRotationX
                            | RigidbodyConstraints.FreezeRotationY
                            | RigidbodyConstraints.FreezeRotationZ;

            // Build shared context
            _ctx = new PlayerContext(_rb, transform, _animator, _input, stats, _collider);
            _ctx.CoyoteTimeDuration = stats.coyoteTime;

            // Run checks before FSM init to prevent false Fall on frame 0
            CheckGrounded();
            CheckWall();

            _fsm = BuildFSM();
            _fsm.StateChanged += OnStateChanged;
            _fsm.Initialize();
        }

        private void Update()
        {
            if (_fsm == null) return;
            UpdateDashCooldown();
            _fsm.Tick();
            UpdateAnimator();
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
            _states = new Dictionary<PlayerState, IState>
            {
                { PlayerState.Idle,        new PlayerIdleState(this, _ctx)        },
                { PlayerState.Run,         new PlayerRunState(this, _ctx)         },
                { PlayerState.Jump,        new PlayerJumpState(this, _ctx)        },
                { PlayerState.Fall,        new PlayerFallState(this, _ctx)        },
                { PlayerState.Dash,        new PlayerDashState(this, _ctx)        },
                { PlayerState.Attack,      new PlayerAttackState(this, _ctx)      },
                { PlayerState.CrouchIdle,  new PlayerCrouchIdleState(this, _ctx)  },
                { PlayerState.CrouchWalk,  new PlayerCrouchWalkState(this, _ctx)  },
                { PlayerState.Dead,        new PlayerDeadState(this, _ctx)        },
            };

            return new StateMachine<PlayerState>(_states, PlayerState.Idle);
        }

        // ── Public API ────────────────────────────────────────────────────

        public void ChangeState(PlayerState next) => _fsm.ChangeState(next);

        public void ForceJump()
        {
            if (_fsm.CurrentState != PlayerState.Jump)
            {
                _fsm.ChangeState(PlayerState.Jump);
                return;
            }

            if (_states.TryGetValue(PlayerState.Jump, out var jumpState))
            {
                jumpState.Exit();
                jumpState.Enter();
            }
        }

        /// <summary>
        /// Resize CapsuleCollider height and center for crouch/stand.
        /// Called by crouch states on Enter/Exit.
        /// </summary>
        public void SetColliderCrouch(bool crouching)
        {
            if (!_collider) return;
            _collider.height = crouching ? stats.crouchColliderHeight  : stats.standingColliderHeight;
            var c = _collider.center;
            c.y = crouching ? stats.crouchColliderCenter : stats.standingColliderCenter;
            _collider.center = c;
            _ctx.IsCrouching = crouching;
        }

        public PlayerState CurrentState => _fsm.CurrentState;

        // ── Ground / Wall Detection ───────────────────────────────────────

        private void CheckGrounded()
        {
            bool wasGrounded = _ctx.IsGrounded;

            Vector3 origin = transform.position + stats.groundCheckOffset;
            _ctx.IsGrounded = Physics.CheckSphere(
                origin, stats.groundCheckRadius, stats.groundLayer,
                QueryTriggerInteraction.Ignore);

            _ctx.JustLanded = !wasGrounded && _ctx.IsGrounded;

            if (_ctx.IsGrounded)
                _ctx.JumpsUsed = 0;
        }

        private void CheckWall()
        {
            Vector3 origin    = transform.position;
            Vector3 direction = new Vector3(_ctx.FacingDirection, 0f, 0f);

            _ctx.IsTouchingWall = Physics.Raycast(
                origin, direction, stats.wallCheckDistance, stats.wallLayer,
                QueryTriggerInteraction.Ignore);
        }

        // ── Coyote Time ───────────────────────────────────────────────────

        private void UpdateCoyoteTime()
        {
            if (_ctx.IsGrounded)
                _ctx.CoyoteTimeCounter = _ctx.CoyoteTimeDuration;
            else
            {
                _ctx.CoyoteTimeCounter -= Time.fixedDeltaTime;
                if (_ctx.CoyoteTimeCounter < 0f) _ctx.CoyoteTimeCounter = 0f;
            }
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

        // ── Animator Driver ───────────────────────────────────────────────

        private void UpdateAnimator()
        {
            if (!_animator || !_animator.isActiveAndEnabled || _animator.runtimeAnimatorController == null)
                return;

            float targetSpeed = Mathf.Abs(_ctx.AnimMoveSpeed);
            float dampTime    = targetSpeed < 0.01f ? 0f : 0.1f;

            _animator.SetFloat(AnimHash.MoveSpeed,   targetSpeed, dampTime, Time.deltaTime);
            _animator.SetBool(AnimHash.IsGrounded,   _ctx.IsGrounded);
            // Read IsCrouching from Input directly — _ctx.IsCrouching is set by SetColliderCrouch()
            // which only fires on state Enter/Exit, causing 1-frame delay.
            // Input.IsCrouching updates immediately on button press.
            _animator.SetBool(AnimHash.IsCrouching,  _ctx.Input.IsCrouching);
        }

        // ── Flip ──────────────────────────────────────────────────────────

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
            // TODO: eventHub.playerEvents.onStateChanged.Raise(to);
        }

        // ── IDebugStateProvider ───────────────────────────────────────────

        public string GetDebugStateName() => _fsm?.CurrentState.ToString() ?? "Uninitialized";

        // ── Gizmos ────────────────────────────────────────────────────────

        private void OnDrawGizmos()
        {
            if (!stats) return;

            if (showGroundGizmo)
            {
                Gizmos.color = _ctx is { IsGrounded: true } ? Color.green : Color.red;
                Gizmos.DrawWireSphere(transform.position + stats.groundCheckOffset, stats.groundCheckRadius);
            }

            if (showWallGizmo)
            {
                Gizmos.color = Color.blue;
                float dir = _ctx?.FacingDirection ?? 1f;
                Gizmos.DrawRay(transform.position, new Vector3(dir * stats.wallCheckDistance, 0f, 0f));
            }
        }
    }
}