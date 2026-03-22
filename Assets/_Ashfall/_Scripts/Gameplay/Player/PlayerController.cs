using System.Collections.Generic;
using UnityEngine;
using _Ashfall._Scripts.Gameplay.Player.States;
using _Ashfall._Scripts.Core.StateMachineCore;

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
    [RequireComponent(typeof(StaminaSystem))]
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

        private Rigidbody                  _rb;
        private CapsuleCollider            _collider;
        private PlayerInputHandler         _input;
        private Animator                   _animator;
        private StaminaSystem              _stamina;
        private AnimatorOverrideController _overrideController;

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
            _stamina  = GetComponent<StaminaSystem>();

            // Animator lives on the Model child — never on the root
            _animator = GetComponentInChildren<Animator>();

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

            // Setup AnimatorOverrideController for random parry clips
            if (_animator && _animator.runtimeAnimatorController)
            {
                _overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);
                _animator.runtimeAnimatorController = _overrideController;
            }

            // Initialize stamina with stats config
            _stamina.Initialize(stats);

            // Build shared context — must be created before anything reads _ctx
            _ctx = new PlayerContext(_rb, transform, _animator, _input, stats, _collider, _stamina);
            _ctx.CoyoteTimeDuration = stats.coyoteTime;

            // Set class capability flags — after _ctx is created
            _ctx.CanBlock = stats.canBlock;

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
                { PlayerState.Idle,       new PlayerIdleState(this, _ctx)       },
                { PlayerState.Run,        new PlayerRunState(this, _ctx)        },
                { PlayerState.Jump,       new PlayerJumpState(this, _ctx)       },
                { PlayerState.Fall,       new PlayerFallState(this, _ctx)       },
                { PlayerState.Dash,       new PlayerDashState(this, _ctx)       },
                { PlayerState.Attack,     new PlayerAttackState(this, _ctx)     },
                { PlayerState.CrouchIdle, new PlayerCrouchIdleState(this, _ctx) },
                { PlayerState.CrouchWalk,  new PlayerCrouchWalkState(this, _ctx)  },
                { PlayerState.Block,       new PlayerBlockState(this, _ctx)       },
                { PlayerState.Parry,       new PlayerParryState(this, _ctx)       },
                { PlayerState.GuardBreak,  new PlayerGuardBreakState(this, _ctx)  },
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

            bool isBlocking = _fsm.CurrentState == PlayerState.Block;
            float targetSpeed = isBlocking ? _ctx.AnimMoveSpeed : Mathf.Abs(_ctx.AnimMoveSpeed);
            float dampTime    = isBlocking ? 0.15f :
                                Mathf.Abs(targetSpeed) < 0.01f ? 0f : 0.1f;

            _animator.SetFloat(AnimHash.MoveSpeed,  targetSpeed, dampTime, Time.deltaTime);
            _animator.SetBool(AnimHash.IsGrounded,  _ctx.IsGrounded);
            _animator.SetBool(AnimHash.IsCrouching, _ctx.Input.IsCrouching);
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

        // ── Animator Event Receivers ─────────────────────────────────────
        // These are called by Unity Animator events on the Model child.
        // They forward to the current AttackState if active.

        /// <summary>Called by Animator event at hit impact frame.</summary>
        public void OnAttackHit()
        {
            if (_fsm.CurrentState == PlayerState.Attack &&
                _states.TryGetValue(PlayerState.Attack, out var state))
                ((PlayerAttackState)state).OnAttackHit();
        }

        /// <summary>Called by Animator event at end of each attack clip.</summary>
        public void OnAttackEnd()
        {
            if (_fsm.CurrentState == PlayerState.Attack &&
                _states.TryGetValue(PlayerState.Attack, out var state))
                ((PlayerAttackState)state).OnAttackEnd();
        }

        /// <summary>
        /// Gets the ParryState instance — used by BlockState to pass attacker reference.
        /// </summary>
        public bool TryGetParryState(out PlayerParryState parryState)
        {
            if (_states.TryGetValue(PlayerState.Parry, out var state) && state is PlayerParryState ps)
            {
                parryState = ps;
                return true;
            }
            parryState = null;
            return false;
        }

        /// <summary>
        /// Swaps the Parry state clip with a randomly chosen one from parryClips.
        /// Called by PlayerParryState before CrossFade.
        /// </summary>
        public void SwapParryClip()
        {
            var clips     = stats.parryClips;
            var stateClip = stats.parryStateClip;

            if (clips == null || clips.Length == 0
                || stateClip == null || _overrideController == null) return;

            var randomClip = clips[Random.Range(0, clips.Length)];
            if (randomClip != null)
                _overrideController[stateClip.name] = randomClip;
        }

        /// <summary>
        /// Called by Animator event at the active parry frame.
        /// Notifies ParryState that the deflection window is now open.
        /// </summary>
        public void OnParryWindowActive()
        {
            if (_fsm.CurrentState == PlayerState.Parry &&
                _states.TryGetValue(PlayerState.Parry, out var state))
                ((PlayerParryState)state).OnParryWindowOpen();
        }

        /// <summary>Called by Animator event when parry animation ends.</summary>
        public void OnParryEnd()
        {
            if (_fsm.CurrentState == PlayerState.Parry &&
                _states.TryGetValue(PlayerState.Parry, out var state))
                ((PlayerParryState)state).OnParryEnd();
        }

        /// <summary>Called by Animator event when death animation settles.</summary>
        public void OnDeathSettled()
        {
            // TODO: trigger respawn sequence via Grace Point system
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