// Assets/_MythHunter/Code/Game/Systems/Phase/PhaseSystem.cs
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
    public class PhaseSystem : IPhaseSystem
    {
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly IEventThrottler _eventThrottler;

        private GamePhase _currentPhase;
        private float _phaseTimer;
        private float _phaseDuration;
        private readonly Dictionary<GamePhase, float> _phaseDurations = new Dictionary<GamePhase, float>();
        private bool _isPaused;

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

            // Реєструємо обмеження для PhaseUpdateEvent - максимум 4 рази на секунду
            _eventThrottler.RegisterThrottle<PhaseUpdateEvent>(0.25f);

            // Ініціалізуємо стандартні тривалості фаз
            InitDefaultPhaseDurations();

            // Підписуємося на події
            SubscribeToEvents();
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
            _logger.LogInfo("PhaseSystem initialized", "PhaseSystem");
        }

        public void Update(float deltaTime)
        {
            if (_currentPhase == GamePhase.None || _isPaused)
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
            _currentPhase = phase;
            _phaseTimer = 0;

            // Встановлення тривалості фази
            _phaseDuration = _phaseDurations.TryGetValue(phase, out float duration)
                ? duration
                : 10f; // Стандартне значення, якщо не задано інше

            _logger.LogInfo($"Starting phase: {phase}, duration: {_phaseDuration}s", "PhaseSystem");

            // Публікація події початку фази
            var evt = new PhaseStartedEvent
            {
                Phase = phase,
                Duration = _phaseDuration,
                Timestamp = DateTime.UtcNow
            };

            _eventBus.Publish(evt);
        }

        public void EndPhase(GamePhase phase)
        {
            if (_currentPhase != phase)
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
            _phaseDurations[phase] = duration;

            // Якщо це поточна фаза, оновлюємо тривалість
            if (_currentPhase == phase)
            {
                _phaseDuration = duration;
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
            _isPaused = true;
            _logger.LogInfo("Game phases paused", "PhaseSystem");
        }

        public void Resume()
        {
            _isPaused = false;
            _logger.LogInfo("Game phases resumed", "PhaseSystem");
        }

        private void SubscribeToEvents()
        {
            _eventBus.Subscribe<PhaseChangeRequestEvent>(OnPhaseChangeRequest);
            _eventBus.Subscribe<GameStartedEvent>(OnGameStarted);
            _eventBus.Subscribe<GamePausedEvent>(OnGamePaused);
            _eventBus.SubscribeAsync<GameEndedEvent>(OnGameEndedAsync);
        }

        private void OnPhaseChangeRequest(PhaseChangeRequestEvent evt)
        {
            if (_isPaused)
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
            _currentPhase = GamePhase.None;
            _phaseTimer = 0;
            _phaseDuration = 0;

            // Асинхронне очищення даних фаз якщо потрібно
            await UniTask.CompletedTask;
        }

        public void Dispose()
        {
            // Відписуємося від подій
            _eventBus.Unsubscribe<PhaseChangeRequestEvent>(OnPhaseChangeRequest);
            _eventBus.Unsubscribe<GameStartedEvent>(OnGameStarted);
            _eventBus.Unsubscribe<GamePausedEvent>(OnGamePaused);
            _eventBus.UnsubscribeAsync<GameEndedEvent>(OnGameEndedAsync);

            _logger.LogInfo("PhaseSystem disposed", "PhaseSystem");
        }
    }
}
