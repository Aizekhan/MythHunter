// Файл: Assets/_MythHunter/Code/Systems/Phase/PhaseSystem.cs

using UnityEngine;
using Cysharp.Threading.Tasks;
using MythHunter.Core.ECS;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Utils.Logging;
using MythHunter.Data.ScriptableObjects;
using System;
using System.Threading;

namespace MythHunter.Systems.Phase
{
    /// <summary>
    /// Система керування фазами гри
    /// </summary>
    public class PhaseSystem : SystemBase, IEventSubscriber
    {
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly PhaseConfig _config;

        private GamePhase _currentPhase = GamePhase.None;
        private float _phaseTimer;
        private bool _isPaused;
        private CancellationTokenSource _timerCts;

        [Inject]
        public PhaseSystem(IEventBus eventBus, IMythLogger logger, PhaseConfig config)
        {
            _eventBus = eventBus;
            _logger = logger;
            _config = config;
        }

        public override void Initialize()
        {
            SubscribeToEvents();
            _logger.LogInfo("PhaseSystem initialized");

            // Запуск першої фази
            ChangePhase(GamePhase.Rune);
        }

        public void SubscribeToEvents()
        {
            _eventBus.Subscribe<GameStartedEvent>(OnGameStarted);
            _eventBus.Subscribe<GamePausedEvent>(OnGamePaused);
            _eventBus.Subscribe<RequestNextPhaseEvent>(OnRequestNextPhase);
        }

        public void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<GameStartedEvent>(OnGameStarted);
            _eventBus.Unsubscribe<GamePausedEvent>(OnGamePaused);
            _eventBus.Unsubscribe<RequestNextPhaseEvent>(OnRequestNextPhase);
        }

        private void OnGameStarted(GameStartedEvent evt)
        {
            _logger.LogInfo("Game started, initializing first phase");
            ChangePhase(GamePhase.Rune);
        }

        private void OnGamePaused(GamePausedEvent evt)
        {
            _isPaused = evt.IsPaused;
            _logger.LogInfo($"Game {(_isPaused ? "paused" : "resumed")}, current phase: {_currentPhase}");
        }

        private void OnRequestNextPhase(RequestNextPhaseEvent evt)
        {
            if (evt.CurrentPhase == _currentPhase)
            {
                _logger.LogInfo($"Manual phase change requested from {_currentPhase}");
                ChangePhase(_config.GetNextPhase(_currentPhase));
            }
        }

        public override void Update(float deltaTime)
        {
            if (_isPaused || _currentPhase == GamePhase.None)
                return;

            _phaseTimer -= deltaTime;

            if (_phaseTimer <= 0f)
            {
                // Перехід до наступної фази
                GamePhase nextPhase = _config.GetNextPhase(_currentPhase);
                ChangePhase(nextPhase);
            }
        }

        private void ChangePhase(GamePhase newPhase)
        {
            // Завершення поточної фази
            if (_currentPhase != GamePhase.None)
            {
                _timerCts?.Cancel();
                _timerCts?.Dispose();
                _timerCts = null;

                _eventBus.Publish(new PhaseEndedEvent
                {
                    Phase = _currentPhase,
                    Timestamp = DateTime.UtcNow
                });
            }

            // Запуск нової фази
            GamePhase previousPhase = _currentPhase;
            _currentPhase = newPhase;
            _phaseTimer = _config.GetPhaseDuration(_currentPhase);

            _logger.LogInfo($"Phase changed from {previousPhase} to {_currentPhase}, duration: {_phaseTimer} seconds");

            // Публікація події про зміну фази
            _eventBus.Publish(new PhaseChangedEvent
            {
                PreviousPhase = previousPhase,
                CurrentPhase = _currentPhase,
                Timestamp = DateTime.UtcNow
            });

            // Публікація події про початок фази
            _eventBus.Publish(new PhaseStartedEvent
            {
                Phase = _currentPhase,
                Duration = _phaseTimer,
                Timestamp = DateTime.UtcNow
            });

            // Запуск таймера для оновлення UI
            if (_config.EnableTimerUpdates)
            {
                _timerCts = new CancellationTokenSource();
                StartTimerUpdates(_timerCts.Token).Forget();
            }
        }

        private async UniTaskVoid StartTimerUpdates(CancellationToken cancellationToken)
        {
            float interval = _config.TimerUpdateInterval;
            float lastTime = _phaseTimer;

            while (!cancellationToken.IsCancellationRequested && _phaseTimer > 0)
            {
                // Публікація події оновлення таймера, тільки якщо час суттєво змінився
                if (Mathf.Abs(lastTime - _phaseTimer) >= interval * 0.5f)
                {
                    _eventBus.Publish(new PhaseTimerUpdatedEvent
                    {
                        Phase = _currentPhase,
                        RemainingTime = _phaseTimer,
                        TotalTime = _config.GetPhaseDuration(_currentPhase)
                    });
                    lastTime = _phaseTimer;
                }

                await UniTask.Delay(TimeSpan.FromSeconds(interval), cancellationToken: cancellationToken);
            }
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            _timerCts?.Cancel();
            _timerCts?.Dispose();
            _logger.LogInfo("PhaseSystem disposed");
        }
    }
}
