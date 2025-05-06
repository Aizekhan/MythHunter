// Файл: Assets/_MythHunter/Code/Systems/Phase/PhaseSystem.cs (оновлений)
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Systems.Phase
{
    /// <summary>
    /// Система для керування фазами гри
    /// </summary>
    public class PhaseSystem : SystemBase, IPhaseSystem
    {
        private GamePhase _currentPhase;
        private float _phaseTimer;
        private float _phaseDuration;
        private readonly Dictionary<GamePhase, float> _phaseDurations = new Dictionary<GamePhase, float>();

        public GamePhase CurrentPhase => _currentPhase;

        [Inject]
        public PhaseSystem(IEventBus eventBus, IMythLogger logger)
            : base(logger, eventBus)
        {
            _currentPhase = GamePhase.None;
            _phaseTimer = 0;
            _phaseDuration = 0;

            // Ініціалізація стандартних тривалостей фаз
            InitDefaultPhaseDurations();
        }

        private void InitDefaultPhaseDurations()
        {
            // Стандартні тривалості фаз у секундах
            _phaseDurations[GamePhase.Rune] = 5f;
            _phaseDurations[GamePhase.Planning] = 15f;
            _phaseDurations[GamePhase.Movement] = 10f;
            _phaseDurations[GamePhase.Combat] = 8f;
            _phaseDurations[GamePhase.Freeze] = 3f;
        }

        /// <summary>
        /// Змінює тривалість фази
        /// </summary>
        public void SetPhaseDuration(GamePhase phase, float duration)
        {
            _phaseDurations[phase] = duration;
            _logger.LogInfo($"Phase {phase} duration set to {duration}s", "Phase");

            // Якщо це поточна фаза, оновлюємо тривалість
            if (_currentPhase == phase)
            {
                _phaseDuration = duration;
            }
        }

        protected override void OnSubscribeToEvents()
        {
            // Підписуємося на синхронні події
            Subscribe<PhaseChangeRequestEvent>(OnPhaseChangeRequest);

            // Підписуємося на асинхронні події
            _eventBus.SubscribeAsync<GameStartedEvent>(OnGameStartedAsync);
            _eventBus.SubscribeAsync<GameEndedEvent>(OnGameEndedAsync);
        }

        protected override void OnUnsubscribeFromEvents()
        {
            // Відписуємося від синхронних подій
            Unsubscribe<PhaseChangeRequestEvent>(OnPhaseChangeRequest);

            // Відписуємося від асинхронних подій
            _eventBus.UnsubscribeAsync<GameStartedEvent>(OnGameStartedAsync);
            _eventBus.UnsubscribeAsync<GameEndedEvent>(OnGameEndedAsync);
        }

        public override void Update(float deltaTime)
        {
            if (_currentPhase == GamePhase.None)
                return;

            // Оновлення таймера фази
            _phaseTimer += deltaTime;

            // Публікуємо подію оновлення фази для інформування інших систем
            PublishPhaseUpdateEvent();

            // Перевірка завершення фази
            if (_phaseTimer >= _phaseDuration)
            {
                GamePhase previousPhase = _currentPhase;
                GamePhase nextPhase = GetNextPhase(_currentPhase);

                StartPhase(nextPhase);

                Publish(new PhaseEndedEvent
                {
                    Phase = previousPhase,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Публікує подію оновлення фази
        /// </summary>
        private void PublishPhaseUpdateEvent()
        {
            // Публікуємо подію оновлення фази з частотою 4 рази на секунду
            if (_phaseTimer % 0.25f < Time.deltaTime)
            {
                Publish(new PhaseUpdateEvent
                {
                    Phase = _currentPhase,
                    ElapsedTime = _phaseTimer,
                    RemainingTime = _phaseDuration - _phaseTimer,
                    TotalDuration = _phaseDuration,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        private void OnPhaseChangeRequest(PhaseChangeRequestEvent evt)
        {
            _logger.LogInfo($"Phase change requested from {_currentPhase} to {evt.RequestedPhase}");

            // Завершення поточної фази
            GamePhase previousPhase = _currentPhase;

            // Запуск нової фази
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

        private async UniTask OnGameStartedAsync(GameStartedEvent evt)
        {
            _logger.LogInfo("Game started, initializing phases");

            // Асинхронна ініціалізація даних для фаз
            await PreparePhaseDataAsync();

            // Запуск першої фази
            StartPhase(GamePhase.Rune);
        }

        private async UniTask OnGameEndedAsync(GameEndedEvent evt)
        {
            _logger.LogInfo("Game ended, cleaning up phases");

            // Асинхронне очищення даних фаз
            await CleanupPhaseDataAsync();

            // Скидання фази
            _currentPhase = GamePhase.None;
            _phaseTimer = 0;
            _phaseDuration = 0;
        }

        private async UniTask PreparePhaseDataAsync()
        {
            // Симуляція асинхронного завантаження даних
            await UniTask.Delay(100);

            _logger.LogDebug("Phase data prepared");
        }

        private async UniTask CleanupPhaseDataAsync()
        {
            // Симуляція асинхронного очищення даних
            await UniTask.Delay(100);

            _logger.LogDebug("Phase data cleaned up");
        }

        public void StartPhase(GamePhase phase)
        {
            _currentPhase = phase;
            _phaseTimer = 0;

            // Встановлення тривалості фази
            _phaseDuration = _phaseDurations.TryGetValue(phase, out float duration)
                ? duration
                : 0f;

            _logger.LogInfo($"Started phase {phase} with duration {_phaseDuration}s");

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
            {
                _logger.LogWarning($"Cannot end phase {phase} - current phase is {_currentPhase}");
                return;
            }

            _logger.LogInfo($"Phase {phase} ended manually");

            var evt = new PhaseEndedEvent
            {
                Phase = phase,
                Timestamp = DateTime.UtcNow
            };

            _eventBus.Publish(evt);

            // Запускаємо наступну фазу
            StartPhase(GetNextPhase(phase));
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

        public float GetPhaseTimeRemaining()
        {
            return _phaseDuration - _phaseTimer;
        }

        public float GetPhaseDuration(GamePhase phase)
        {
            return _phaseDurations.TryGetValue(phase, out float duration) ? duration : 0f;
        }

        public float GetPhaseProgress()
        {
            return _phaseDuration > 0 ? _phaseTimer / _phaseDuration : 0f;
        }
    }
}
