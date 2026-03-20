using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace _Ashfall._Scripts.Core.StateMachineCore
{
    /// <summary>
    /// A high-performance Finite State Machine optimized for Unity Update loops.
    /// Caches the active state instance to avoid Dictionary lookups every frame.
    /// </summary>
    public sealed class StateMachine<TState> where TState : Enum
    {
        private readonly Dictionary<TState, IState> _states;
        private TState _currentStateEnum;
        private IState _currentStateImplementation;
        
        private bool _isInitialized;
        private bool _inTransition;
        
        public bool ThrowOnError { get; set; } = false;
        public bool ErrorOnSameState { get; set; } = false;

        public event Action<TState, TState> StateChanged;
        public event Action<string, Exception> ErrorRaised;

        public TState CurrentState => _currentStateEnum;
        public TState PreviousState { get; private set; }

        public StateMachine(Dictionary<TState, IState> states, TState initialState)
        {
            _states = states ?? throw new ArgumentNullException(nameof(states));
            _currentStateEnum = initialState;
            PreviousState = initialState;

            if (_states.TryGetValue(initialState, out var stateImpl))
            {
                _currentStateImplementation = stateImpl;
            }
            else
            {
                RaiseError($"Initial state '{initialState}' is not registered.");
            }
        }

        public void Initialize()
        {
            if (_isInitialized) return;
            _isInitialized = true;
            
            SafeCall(_currentStateImplementation.Enter, "Enter Initial State");
        }

        public void ChangeState(TState nextState)
        {
            bool success = TryChangeState(nextState);
            
            if (!success)
            {
                if (EqualityComparer<TState>.Default.Equals(_currentStateEnum, nextState))
                {
                    if (ErrorOnSameState) RaiseError($"Already in state '{nextState}'");
                }
                else if (!_states.ContainsKey(nextState))
                {
                    RaiseError($"State '{nextState}' is not defined!");
                }
            }
        }
        
        /// <summary>
        /// Attempts to transition to the next state. 
        /// Returns true if successful, false if the state is invalid or same as current.
        /// Does NOT throw errors (Silent Fail).
        /// </summary>
        public bool TryChangeState(TState nextState)
        {
            if (_inTransition) return false;

            // 1. Check if same state
            if (EqualityComparer<TState>.Default.Equals(_currentStateEnum, nextState))
            {
                return false;
            }

            // 2. Check if state exists (Silent check, no error log)
            if (!_states.TryGetValue(nextState, out var nextStateImpl))
            {
                return false;
            }

            // 3. Perform Transition
            _inTransition = true;

            // Exit old
            SafeCall(_currentStateImplementation.Exit, "Exit State");

            // Swap
            PreviousState = _currentStateEnum;
            _currentStateEnum = nextState;
            _currentStateImplementation = nextStateImpl;
            
            SafeCall(_currentStateImplementation.Enter, "Enter State");

            StateChanged?.Invoke(PreviousState, _currentStateEnum);
    
            _inTransition = false;
            return true;
        }

        /// <summary>
        /// Highly optimized Tick. Direct interface call, zero dictionary lookup.
        /// </summary>
        public void Tick()
        {
            if (!_isInitialized) return;
            // No TryGetValue here -> Super Fast
            _currentStateImplementation?.Tick(); 
        }

        public void FixedTick()
        {
            if (!_isInitialized) return;
            _currentStateImplementation?.FixedTick();
        }

        private void SafeCall(Action action, string context)
        {
            try { action?.Invoke(); }
            catch (Exception ex) { RaiseError($"Error during {context}", ex); }
        }

        private void RaiseError(string msg, Exception ex = null, [CallerMemberName] string caller = "")
        {
            string log = $"[FSM Error] {msg} (Caller: {caller})";
            if (ThrowOnError) throw ex ?? new InvalidOperationException(log);
            Debug.LogError(log);
            ErrorRaised?.Invoke(log, ex);
        }
    }
}