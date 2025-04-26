using System;
using System.Collections.Generic;

namespace MythHunter.Core.StateMachine
{
    /// <summary>
    /// Реалізація машини станів з підтримкою enum
    /// </summary>
    public class StateMachine<TStateId> : IStateMachine<TStateId> where TStateId : Enum
    {
        private readonly Dictionary<TStateId, IState<TStateId>> _states = new Dictionary<TStateId, IState<TStateId>>();
        private readonly HashSet<(TStateId from, TStateId to)> _allowedTransitions = new HashSet<(TStateId from, TStateId to)>();

        private IState<TStateId> _currentState;

        public TStateId CurrentStateId => _currentState != null ? _currentState.StateId : default;

        public void RegisterState(TStateId stateId, IState<TStateId> state)
        {
            _states[stateId] = state;
        }

        public void UnregisterState(TStateId stateId)
        {
            if (_states.ContainsKey(stateId))
            {
                if (_currentState != null && EqualityComparer<TStateId>.Default.Equals(_currentState.StateId, stateId))
                {
                    _currentState.Exit();
                    _currentState = null;
                }

                _states.Remove(stateId);
            }
        }

        public bool SetState(TStateId stateId)
        {
            if (!_states.ContainsKey(stateId))
                return false;

            if (_currentState != null)
            {
                if (EqualityComparer<TStateId>.Default.Equals(_currentState.StateId, stateId))
                    return true;

                if (!CanTransition(_currentState.StateId, stateId))
                    return false;

                _currentState.Exit();
            }

            _currentState = _states[stateId];
            _currentState.Enter();

            return true;
        }

        public void Update()
        {
            _currentState?.Update();
        }

        public void AddTransition(TStateId fromStateId, TStateId toStateId)
        {
            _allowedTransitions.Add((fromStateId, toStateId));
        }

        public bool CanTransition(TStateId fromStateId, TStateId toStateId)
        {
            return _allowedTransitions.Contains((fromStateId, toStateId));
        }
    }

}
