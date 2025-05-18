// Файл: Assets/_MythHunter/Code/Core/StateMachine/StateMachine.cs
using System;
using System.Collections.Generic;

namespace MythHunter.Core.StateMachine
{
    /// <summary>
    /// Базова реалізація машини станів
    /// </summary>
    public class StateMachine<T> : IStateMachine<T> where T : Enum

    {
        private readonly Dictionary<T, IState<T>> _states = new Dictionary<T, IState<T>>();
        private IState<T> _currentState;
        private readonly HashSet<(T, T)> _transitions = new();
        private T _currentStateId;
        private T _previousStateId;

        public T CurrentState => _currentStateId;
        public T PreviousState => _previousStateId;

        public void RegisterState(T stateId, IState<T> state)
        {
            if (!_states.ContainsKey(stateId))
                _states.Add(stateId, state);
        }

        public bool SetState(T stateId)
        {
            if (!_states.TryGetValue(stateId, out var state))
                return false;

            _currentState?.Exit();

            _previousStateId = _currentStateId;
            _currentStateId = stateId;
            _currentState = state;

            _currentState.Enter(_previousStateId);
            return true;
        }
        public void UnregisterState(T stateId)
        {
            _states.Remove(stateId);
        }
        public void Update()
        {
            _currentState?.Update();
        }
        public void AddTransition(T from, T to)
        {
            _transitions.Add((from, to));
        }



        public bool CanTransition(T from, T to)
        {
            return _transitions.Contains((from, to));
        }
    }
}
