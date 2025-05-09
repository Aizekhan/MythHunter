using System;
using System.Collections.Generic;

namespace MythHunter.Core.StateMachine
{
    /// <summary>
    /// Реалізація машини станів з підтримкою enum
    /// </summary>
    public class StateMachine<TStateEnum> : IStateMachine<TStateEnum> where TStateEnum : Enum
    {
        private readonly Dictionary<TStateEnum, IState<TStateEnum>> _states = new Dictionary<TStateEnum, IState<TStateEnum>>();
        private readonly HashSet<(TStateEnum from, TStateEnum to)> _allowedTransitions = new HashSet<(TStateEnum from, TStateEnum to)>();
        
        private IState<TStateEnum> _currentState;
        
        public TStateEnum CurrentState => _currentState != null ? _currentState.StateId : default;
        
        public void RegisterState(TStateEnum stateId, IState<TStateEnum> state)
        {
            _states[stateId] = state;
        }
        
        public void UnregisterState(TStateEnum stateId)
        {
            if (_states.ContainsKey(stateId))
            {
                if (_currentState != null && EqualityComparer<TStateEnum>.Default.Equals(_currentState.StateId, stateId))
                {
                    _currentState.Exit();
                    _currentState = null;
                }
                
                _states.Remove(stateId);
            }
        }
        
        public bool SetState(TStateEnum stateId)
        {
            if (!_states.ContainsKey(stateId))
                return false;
                
            if (_currentState != null)
            {
                if (EqualityComparer<TStateEnum>.Default.Equals(_currentState.StateId, stateId))
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
        
        public void AddTransition(TStateEnum fromStateId, TStateEnum toStateId)
        {
            _allowedTransitions.Add((fromStateId, toStateId));
        }
        
        public bool CanTransition(TStateEnum fromStateId, TStateEnum toStateId)
        {
            return _allowedTransitions.Contains((fromStateId, toStateId));
        }
    }
}