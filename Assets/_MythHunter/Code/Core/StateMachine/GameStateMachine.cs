// Assets/_MythHunter/Code/Core/Game/GameStateMachine.cs
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.States;
using MythHunter.Utils.Logging;
using System;
using System.Collections.Generic;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Машина станів гри
    /// </summary>
    public class GameStateMachine : IGameStateMachine
    {
        private readonly IMythLogger _logger;
        private readonly IDIContainer _container;

        // Додані відсутні поля
        private BaseState<GameStateType> _currentState;
        private readonly Dictionary<GameStateType, BaseState<GameStateType>> _states = new Dictionary<GameStateType, BaseState<GameStateType>>();

        public GameStateMachine(IDIContainer container)
        {
            _container = container;
            _logger = container.Resolve<IMythLogger>();
        }

        public void Initialize()
        {
            // Реєстрація станів
            RegisterState(GameStateType.Boot, new BootState(_container));
            RegisterState(GameStateType.MainMenu, new MainMenuState(_container));
            RegisterState(GameStateType.Profile, new ProfileState(_container));
            RegisterState(GameStateType.Loading, new LoadingState(_container));
            RegisterState(GameStateType.Gameplay, new GameplayState(_container));
            RegisterState(GameStateType.Lobby, new LobbyState(_container));

            // Налаштування переходів (можна залишити, якщо потрібно для логування)
            _logger.LogInfo($"Додано переходи між станами");

            // Перехід до початкового стану
            ChangeState(GameStateType.Boot);

            _logger.LogInfo($"Initialized GameStateMachine with initial state: {GameStateType.Boot}");
        }

        // Додаємо метод для реєстрації станів
        private void RegisterState(GameStateType stateType, BaseState<GameStateType> state)
        {
            _states[stateType] = state;
            _logger.LogInfo($"Зареєстровано стан {stateType}");
        }

        public void Update()
        {
            _currentState?.Update();
        }

        public void ChangeState(GameStateType newState)
        {
            if (_currentState != null && _currentState.StateId == newState)
                return;

            GameStateType previousStateId = _currentState?.StateId ?? GameStateType.None;

            // Запам'ятовуємо попередній стан для виходу
            var previousState = _currentState;

            // Отримуємо новий стан
            if (!_states.TryGetValue(newState, out var nextState))
            {
                _logger.LogError($"Стан {newState} не зареєстровано в StateMachine", "StateMachine");
                return;
            }

            _logger.LogInfo($"Зміна стану з {previousStateId} на {newState}", "StateMachine");

            // Виходимо з попереднього стану
            previousState?.Exit();

            // Встановлюємо новий поточний стан
            _currentState = nextState;

            // Входимо в новий стан асинхронно
            UniTask.Create(async () => {
                try
                {
                    if (_currentState is BaseState<GameStateType> baseState)
                    {
                        await baseState.EnterAsync(previousStateId);
                    }
                    else
                    {
                        _currentState.Enter(previousStateId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Помилка при вході в стан {newState}: {ex.Message}", "StateMachine", ex);
                }
            });
        }
        public void ChangeState(GameStateType newState, object context)
        {
            if (_currentState != null && _currentState.StateId == newState)
                return;

            GameStateType previousStateId = _currentState?.StateId ?? GameStateType.None;
            var previousState = _currentState;

            if (!_states.TryGetValue(newState, out var nextState))
            {
                _logger.LogError($"Стан {newState} не зареєстровано в StateMachine", "StateMachine");
                return;
            }

            _logger.LogInfo($"Зміна стану з {previousStateId} на {newState} (з контекстом)", "StateMachine");

            previousState?.Exit();
            _currentState = nextState;

            UniTask.Create(async () =>
            {
                try
                {
                    if (_currentState is BaseState<GameStateType> baseStateWithContext)
                    {
                        baseStateWithContext.SetStateContext(context);
                        await baseStateWithContext.EnterAsync(previousStateId);
                    }
                    else
                    {
                        _currentState.Enter(previousStateId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Помилка при вході в стан {newState}: {ex.Message}", "StateMachine", ex);
                }
            });
        }
        public GameStateType CurrentState => _currentState?.StateId ?? GameStateType.None;
    }
}
