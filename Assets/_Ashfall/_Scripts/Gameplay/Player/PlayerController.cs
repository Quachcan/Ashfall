using System.Collections.Generic;
using UnityEngine;
using _Ashfall._Scripts.Gameplay.Combat;
using _Ashfall._Scripts.Gameplay.Player.States;
using _Ashfall._Scripts.Gameplay.Weapons;
using _Ashfall._Scripts.Core.StateMachineCore;
using _Ashfall._Scripts.Gameplay.Stats;

namespace _Ashfall._Scripts.Gameplay.Player
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CapsuleCollider))]
    public class PlayerController : MonoBehaviour, IDebugStateProvider, ICombatStats
    {
        [Header("Config")]
        [SerializeField] private PlayerStats stats;
        [SerializeField] private Transform arrowSpawnPoint;

        private Rigidbody          _rb;
        private CapsuleCollider    _collider;
        private Animator           _animator;
        private WeaponHandler      _weaponHandler;

        private HealthSystem       _health;
        private PostureSystem      _posture;

        private StateMachine<PlayerState>       _fsm;
        private PlayerContext                   _ctx;
        private Dictionary<PlayerState, IState> _states;

        private void Awake()
        {
            _rb       = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();
            _animator = GetComponentInChildren<Animator>();

            if (!stats)
            {
                Debug.LogError("[PlayerController] PlayerStats SO is missing!");
                return;
            }

            _rb.isKinematic = true; 

            _weaponHandler = GetComponent<WeaponHandler>();

            _health  = new HealthSystem(stats.maxHp);
            _posture = new PostureSystem(stats.maxPosture, stats.postureRecoverDelay, stats.postureRecoverRate, stats.finishingBlowWindow);
            _health.OnDeath += OnPlayerDeath;

            _ctx = new PlayerContext(_rb, transform, _animator, stats, _collider, _health);
            _ctx.ArrowSpawnPoint = arrowSpawnPoint;

            _weaponHandler.Initialize(_animator);
            _ctx.Weapon = _weaponHandler;
            _ctx.CanBlock = _weaponHandler.Current?.canBlock ?? false;
            _weaponHandler.OnWeaponChanged += w => _ctx.CanBlock = w.canBlock;

            _fsm = BuildFSM();
            _fsm.Initialize();
        }

        private void Update()
        {
            if (_fsm == null) return;
            _posture.Tick(Time.deltaTime);
            _fsm.Tick();
            UpdateAnimator();
        }

        private StateMachine<PlayerState> BuildFSM()
        {
            _states = new Dictionary<PlayerState, IState>
            {
                { PlayerState.Idle,        new PlayerIdleState(this, _ctx)       },
                { PlayerState.Dash,        new PlayerDashState(this, _ctx)       },
                { PlayerState.Attack,      new PlayerAttackState(this, _ctx)     },
                { PlayerState.Block,       new PlayerBlockState(this, _ctx)      },
                { PlayerState.GuardBreak,  new PlayerGuardBreakState(this, _ctx) },
                { PlayerState.Dead,        new PlayerDeadState(this, _ctx)       },
            };

            return new StateMachine<PlayerState>(_states, PlayerState.Idle);
        }

        public void ChangeState(PlayerState next) => _fsm.ChangeState(next);
        public PlayerState CurrentState => _fsm.CurrentState;

        public float ATK => _weaponHandler?.Current?.atk ?? 0f;
        public float MAG => _weaponHandler?.Current?.mag ?? 0f;
        public float DEF => _weaponHandler?.Current?.def ?? 0f;

        private void UpdateAnimator()
        {
            if (!_animator || !_animator.isActiveAndEnabled || _animator.runtimeAnimatorController == null)
                return;

            _animator.SetBool(AnimHash.IsGrounded, true);
        }

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

        public void OnAttackHit()
        {
            if (_fsm.CurrentState == PlayerState.Attack && _states.TryGetValue(PlayerState.Attack, out var state))
                ((PlayerAttackState)state).OnAttackHit();
        }

        public void OnAttackEnd()
        {
            if (_fsm.CurrentState == PlayerState.Attack && _states.TryGetValue(PlayerState.Attack, out var state))
                ((PlayerAttackState)state).OnAttackEnd();
        }
        
        /// <summary>
        /// Exposes a specific state from the FSM for direct modification.
        /// </summary>
        public T GetState<T>(PlayerState stateEnum) where T : class, IState
        {
            return _fsm?.GetState<T>(stateEnum);
        }

        public void OnDeathSettled() { }

        private void OnPlayerDeath() => _fsm.ChangeState(PlayerState.Dead);

        public string GetDebugStateName() => _fsm?.CurrentState.ToString() ?? "Uninitialized";
    }
}