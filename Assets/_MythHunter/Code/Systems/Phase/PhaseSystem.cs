// Шлях: Assets/_MythHunter/Code/Systems/Phase/PhaseSystem.cs
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Systems.Phase;
using MythHunter.Utils.Logging;

namespace MythHunter.Game.Systems.Phase
{
    /// <summary>
    /// Система для керування фазами гри
    /// </summary>
    public class PhaseSystem : IPhaseSystem, IDisposable
    {
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly IEventThrottler _eventThrottler;

        private GamePhase _currentPhase;
        private float _phaseTimer;
        private float _phaseDuration;
        private readonly Dictionary<GamePhase, float> _phaseDurations = new Dictionary<GamePhase, float>();
        private bool _isPaused;
        private bool _isInitialized;
        private bool _isDisposed;

        // Геттер для поточної фази
        public GamePhase CurrentPhase => _currentPhase;

        // Конструктор
        [Inject]
        public PhaseSystem(IEventBus eventBus, IMythLogger logger, IEventThrottler eventThrottler)
        {
            _eventBus = eventBus;
            _logger = logger;
            _eventThrottler = eventThrottler;

            _currentPhase = GamePhase.None;
            _phaseTimer = 0;
            _phaseDuration = 0;
            _isPaused = false;
            _isInitialized = false;
            _isDisposed = false;

            // Реєструємо обмеження для PhaseUpdateEvent - максимум 4 рази на секунду
            _eventThrottler.RegisterThrottle<PhaseUpdateEvent>(0.25f);

            // Ініціалізуємо стандартні тривалості фаз
            InitDefaultPhaseDurations();
        }

        private void InitDefaultPhaseDurations()
        {
            _phaseDurations[GamePhase.Rune] = 15f;       // Фаза вибору руни: 15 секунд
            _phaseDurations[GamePhase.Planning] = 30f;   // Фаза планування руху: 30 секунд
            _phaseDurations[GamePhase.Movement] = 20f;   // Фаза руху: 20 секунд
            _phaseDurations[GamePhase.Combat] = 10f;     // Фаза бою: 10 секунд (частина активної фази)
            _phaseDurations[GamePhase.Freeze] = 5f;      // Фаза завмирання: 5 секунд
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            // Підписуємося на події
            SubscribeToEvents();
            _isInitialized = true;

            _logger.LogInfo("PhaseSystem initialized", "PhaseSystem");
        }

        public void Update(float deltaTime)
        {
            if (_currentPhase == GamePhase.None || _isPaused || _isDisposed)
                return;

            // Оновлення таймера фази
            _phaseTimer += deltaTime;

            // Публікуємо подію оновлення фази через обмежувач
            _eventThrottler.PublishThrottled(new PhaseUpdateEvent
            {
                Phase = _currentPhase,
                ElapsedTime = _phaseTimer,
                RemainingTime = _phaseDuration - _phaseTimer,
                TotalDuration = _phaseDuration,
                Timestamp = DateTime.UtcNow
            });

            // Перевірка завершення фази
            if (_phaseTimer >= _phaseDuration)
            {
                GamePhase previousPhase = _currentPhase;
                GamePhase nextPhase = GetNextPhase(_currentPhase);

                EndPhase(previousPhase);
                StartPhase(nextPhase);
            }
        }

        public void StartPhase(GamePhase phase)
        {
            if (_isDisposed)
                return;

            // Зберігаємо попередню фазу перед оновленням
            GamePhase previousPhase = _currentPhase;

            _currentPhase = phase;
            _phaseTimer = 0;

            // Встановлення тривалості фази
            _phaseDuration = _phaseDurations.TryGetValue(phase, out float duration)
                ? duration
                : 10f; // Стандартне значення, якщо не задано інше

            _logger.LogInfo($"Starting phase: {phase}, duration: {_phaseDuration}s", "PhaseSystem");

            // Публікація події початку фази
            var startEvent = new PhaseStartedEvent
            {
                Phase = phase,
                Duration = _phaseDuration,
                Timestamp = DateTime.UtcNow
            };

            _eventBus.Publish(startEvent);

            // Публікуємо подію зміни фази, якщо це не перша фаза або фаза змінилася
            if (previousPhase != GamePhase.None || phase != GamePhase.None)
            {
                var phaseChangedEvent = new PhaseChangedEvent
                {
                    PreviousPhase = previousPhase,
                    CurrentPhase = phase,
                    Timestamp = DateTime.UtcNow
                };

                _eventBus.Publish(phaseChangedEvent);
            }
        }

        public void EndPhase(GamePhase phase)
        {
            if (_isDisposed || _currentPhase != phase)
                return;

            _logger.LogInfo($"Ending phase: {phase}", "PhaseSystem");

            var evt = new PhaseEndedEvent
            {
                Phase = phase,
                Timestamp = DateTime.UtcNow
            };

            _eventBus.Publish(evt);
        }

        public void SetPhaseDuration(GamePhase phase, float duration)
        {
            if (duration <= 0)
            {
                _logger.LogWarning($"Invalid phase duration: {duration} for phase {phase}. Must be greater than 0.", "PhaseSystem");
                return;
            }

            _phaseDurations[phase] = duration;

            // Якщо це поточна фаза, оновлюємо тривалість
            if (_currentPhase == phase)
            {
                _phaseDuration = duration;
                _logger.LogInfo($"Updated current phase duration to {duration}s", "PhaseSystem");
            }
        }

        public float GetPhaseTimeRemaining()
        {
            return Math.Max(0, _phaseDuration - _phaseTimer);
        }

        public float GetPhaseDuration(GamePhase phase)
        {
            return _phaseDurations.TryGetValue(phase, out float duration) ? duration : 0f;
        }

        public float GetPhaseProgress()
        {
            return _phaseDuration > 0 ? _phaseTimer / _phaseDuration : 0f;
        }

        private GamePhase GetNextPhase(GamePhase currentPhase)
        {
            return currentPhase switch
            {
                GamePhase.Rune => GamePhase.Planning,
                GamePhase.Planning => GamePhase.Movement,
                GamePhase.Movement => GamePhase.Combat,
                GamePhase.Combat => GamePhase.Freeze,
                GamePhase.Freeze => GamePhase.Rune,
                _ => GamePhase.Rune
            };
        }

        public void Pause()
        {
            if (_isPaused)
                return;

            _isPaused = true;
            _logger.LogInfo("Game phases paused", "PhaseSystem");
        }

        public void Resume()
        {
            if (!_isPaused)
                return;

            _isPaused = false;
            _logger.LogInfo("Game phases resumed", "PhaseSystem");
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<PhaseChangeRequestEvent>(OnPhaseChangeRequest);
            _eventBus.Subscribe<GameStartedEvent>(OnGameStarted);
            _eventBus.Subscribe<GamePausedEvent>(OnGamePaused);
            _eventBus.SubscribeAsync<GameEndedEvent>(OnGameEndedAsync);

            _logger.LogDebug("PhaseSystem subscribed to events", "PhaseSystem");
        }

        private void UnsubscribeFromEvents()
        {
            if (_eventBus == null)
                return;

            _eventBus.Unsubscribe<PhaseChangeRequestEvent>(OnPhaseChangeRequest);
            _eventBus.Unsubscribe<GameStartedEvent>(OnGameStarted);
            _eventBus.Unsubscribe<GamePausedEvent>(OnGamePaused);
            _eventBus.UnsubscribeAsync<GameEndedEvent>(OnGameEndedAsync);

            _logger.LogDebug("PhaseSystem unsubscribed from events", "PhaseSystem");
        }

        private void OnPhaseChangeRequest(PhaseChangeRequestEvent evt)
        {
            if (_isPaused || _isDisposed)
                return;

            GamePhase previousPhase = _currentPhase;

            // Завершуємо поточну фазу
            EndPhase(previousPhase);

            // Запускаємо нову фазу
            StartPhase(evt.RequestedPhase);

            // Публікація події зміни фази
            var phaseChangedEvent = new PhaseChangedEvent
            {
                PreviousPhase = previousPhase,
                CurrentPhase = evt.RequestedPhase,
                Timestamp = DateTime.UtcNow
            };

            _eventBus.Publish(phaseChangedEvent);
        }

        private void OnGameStarted(GameStartedEvent evt)
        {
            _isPaused = false;

            // Запуск першої фази при старті гри
            StartPhase(GamePhase.Rune);

            _logger.LogInfo("Game started, phase set to Rune", "PhaseSystem");
        }

        private void OnGamePaused(GamePausedEvent evt)
        {
            if (evt.IsPaused)
                Pause();
            else
                Resume();
        }

        private async UniTask OnGameEndedAsync(GameEndedEvent evt)
        {
            // Зупиняємо поточну фазу
            Pause();

            // Скидаємо фазу
            GamePhase previousPhase = _currentPhase;
            _currentPhase = GamePhase.None;
            _phaseTimer = 0;
            _phaseDuration = 0;

            // Публікуємо подію зміни фази
            var phaseChangedEvent = new PhaseChangedEvent
            {
                PreviousPhase = previousPhase,
                CurrentPhase = GamePhase.None,
                Timestamp = DateTime.UtcNow
            };

            _eventBus.Publish(phaseChangedEvent);

            // Асинхронне очищення даних фаз якщо потрібно
            await UniTask.CompletedTask;

            _logger.LogInfo("Game ended, phase reset to None", "PhaseSystem");
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            // Відписуємося від подій
            UnsubscribeFromEvents();

            _logger.LogInfo("PhaseSystem disposed", "PhaseSystem");
        }
    }
}
