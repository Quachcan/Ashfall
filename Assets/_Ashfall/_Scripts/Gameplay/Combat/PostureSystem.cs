using System;
using UnityEngine;

namespace _Ashfall._Scripts.Gameplay.Combat
{
    /// <summary>
    /// Sekiro-style posture system — pure C#, ticked by owner each frame.
    ///
    /// Posture increases when taking hits, being parried, or backstabbed.
    /// Posture decreases automatically after a recovery delay.
    /// When posture reaches max → Stagger + open finishing blow window.
    /// If finishing blow not performed within window → exit stagger, reset posture.
    ///
    /// Displayed on Enemy as posture bar. Player posture is internal only.
    /// </summary>
    public class PostureSystem
    {
        // ── Config ────────────────────────────────────────────────────────

        private readonly float _maxPosture;
        private readonly float _recoverDelay;   // seconds before posture starts recovering
        private readonly float _recoverRate;    // posture recovered per second
        private readonly float _finishingBlowWindow; // seconds to land finishing blow

        // ── Runtime ───────────────────────────────────────────────────────

        public float Current       { get; private set; }
        public float Normalized    => _maxPosture > 0f ? Current / _maxPosture : 0f;
        public bool  IsStaggered   { get; private set; }

        private float _recoverDelayTimer;
        private float _finishingBlowTimer;

        // ── Events ────────────────────────────────────────────────────────

        /// <summary>Raised when posture fills up — start stagger animation.</summary>
        public event Action OnStagger;

        /// <summary>Raised when finishing blow window expires — exit stagger.</summary>
        public event Action OnStaggerEnd;

        /// <summary>Raised when finishing blow lands successfully.</summary>
        public event Action OnFinishingBlow;

        // ── Constructor ───────────────────────────────────────────────────

        public PostureSystem(float maxPosture, float recoverDelay,
                             float recoverRate, float finishingBlowWindow)
        {
            _maxPosture          = maxPosture;
            _recoverDelay        = recoverDelay;
            _recoverRate         = recoverRate;
            _finishingBlowWindow = finishingBlowWindow;
        }

        // ── Tick ──────────────────────────────────────────────────────────

        /// <summary>Called every frame by owner (EnemyBase/PlayerController).</summary>
        public void Tick(float dt)
        {
            if (IsStaggered)
            {
                TickFinishingBlowWindow(dt);
                return; // no recovery while staggered
            }

            TickRecoverDelay(dt);
            TickRecover(dt);
        }

        // ── Add Posture ───────────────────────────────────────────────────

        /// <summary>Add posture from a normal hit.</summary>
        public void AddFromHit(float amount)      => Add(amount);

        /// <summary>Add posture from a parry — typically larger amount.</summary>
        public void AddFromParry(float amount)    => Add(amount);

        /// <summary>Add posture from a backstab — instant fill or large amount.</summary>
        public void AddFromBackstab(float amount) => Add(amount);

        /// <summary>Add posture from a skill effect.</summary>
        public void AddFromSkill(float amount)    => Add(amount);

        // ── Finishing Blow ────────────────────────────────────────────────

        /// <summary>
        /// Called when player performs a finishing blow during stagger window.
        /// </summary>
        public void TriggerFinishingBlow()
        {
            if (!IsStaggered) return;

            IsStaggered         = false;
            _finishingBlowTimer = 0f;
            Current             = 0f;

            OnFinishingBlow?.Invoke();
        }

        /// <summary>Reset posture to 0 — called on death or Grace Point.</summary>
        public void Reset()
        {
            Current             = 0f;
            IsStaggered         = false;
            _recoverDelayTimer  = 0f;
            _finishingBlowTimer = 0f;
        }

        // ── Private ───────────────────────────────────────────────────────

        private void Add(float amount)
        {
            if (IsStaggered || amount <= 0f) return;

            Current             = Mathf.Min(Current + amount, _maxPosture);
            _recoverDelayTimer  = _recoverDelay; // reset recover delay on each hit

            if (Current >= _maxPosture)
                TriggerStagger();
        }

        private void TriggerStagger()
        {
            IsStaggered         = true;
            _finishingBlowTimer = _finishingBlowWindow;
            OnStagger?.Invoke();
        }

        private void TickFinishingBlowWindow(float dt)
        {
            _finishingBlowTimer -= dt;
            if (_finishingBlowTimer <= 0f)
                ExitStagger();
        }

        private void ExitStagger()
        {
            IsStaggered = false;
            Current     = 0f; // reset posture after stagger ends
            OnStaggerEnd?.Invoke();
        }

        private void TickRecoverDelay(float dt)
        {
            if (_recoverDelayTimer > 0f)
                _recoverDelayTimer -= dt;
        }

        private void TickRecover(float dt)
        {
            if (_recoverDelayTimer > 0f) return;
            if (Current <= 0f)           return;

            Current = Mathf.Max(0f, Current - _recoverRate * dt);
        }
    }
}