using System;
using System.Collections.Generic;
using _Ashfall._Scripts.Core.StateMachineCore;
using _Ashfall._Scripts.Gameplay.Combat;
using _Ashfall._Scripts.Gameplay.Enemy.EnemyFSM;
using _Ashfall._Scripts.Gameplay.Enemy.EnemyFSM.States;
using _Ashfall._Scripts.Gameplay.Stats;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Enemy
{
    public class EnemyController : MonoBehaviour, ICombatStats, IHittable
    {
        [Header("Config")]
        [SerializeField] private EnemyStats stats;
        
        private Animator _animator;
        private HealthSystem _health;
        private StateMachine<EnemyState> _fsm;
        private EnemyContext _ctx;

        public float ATK => stats.atk;
        public float MAG => stats.mag;
        public float DEF => stats.def;
        
        public bool IsInvincible { get; }

        private void Awake()
        {
            _animator = GetComponentInChildren<Animator>();

            if (!stats)
            {
                Debug.LogError("[EnemyController] EnemyStats is missing", this);
                return;
            }

            _health = new HealthSystem(stats.maxHp);
            // _health.OnDeath += OnDeath;

            _ctx = new EnemyContext(_animator, stats, _health);
            _fsm = BuildFSM();
            _fsm.Initialize();
        }

        private void Update()
        {
            _fsm?.Tick();
        }

        private StateMachine<EnemyState> BuildFSM()
        {
            var states = new Dictionary<EnemyState, IState>
            {
                { EnemyState.Idle,    new EnemyIdleState(this, _ctx) },
                { EnemyState.TakeHit, new EnemyTakeHitState(this, _ctx) },
                { EnemyState.Dead,    new EnemyDeadState(this, _ctx) }
                // EnemyAttackState will be added later when integrating with Match-3 turns
            };

            return new StateMachine<EnemyState>(states, EnemyState.Idle);
        }
        
        public void ChangeState(EnemyState next) => _fsm.ChangeState(next);

        public void TakeHit(AttackData data, ICombatStats attackerStats, GameObject attacker)
        {
            if (IsInvincible) return;

            // Calculate damage using the shared math utility
            float damage = DamageCalculator.Calculate(attackerStats.ATK, DEF, data, out bool isCrit);
            
            _health.TakeDamage(damage);
            
            //TODO: pop up damage numbers UI here using 'damage' and 'isCrit'

            if (!_health.IsDead)
            {
                _fsm.ChangeState(EnemyState.TakeHit);
            }
        }
        
        private void OnDeath()
        {
            _fsm.ChangeState(EnemyState.Dead);
            // TODO: Notify EventHub that this enemy is defeated to progress the wave
        }
    }
}