// Файл: Assets/_MythHunter/Code/Systems/Phase/PhaseSystem.cs
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Events.Extensions;
using MythHunter.Systems.Core;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.Phase
{
    /// <summary>
    /// Система для керування фазами гри з асинхронною обробкою подій
    /// </summary>
    public class PhaseSystem : SystemBase, IPhaseSystem
    {
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        private Events.Domain.GamePhase _currentPhase;
        private float _phaseTimer;
        private float _phaseDuration;

        public Events.Domain.GamePhase CurrentPhase => _currentPhase;

        [Inject]
        public PhaseSystem(
            IEventBus eventBus,
            IMythLogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;
            _currentPhase = Events.Domain.GamePhase.None;
            _phaseTimer = 0;
            _phaseDuration = 0;
        }

        public override void Initialize()
        {
            // Підписка на синхронні події з явним вказуванням аргументів типу
            this.Subscribe<Events.Domain.PhaseChangeRequestEvent>(_eventBus, OnPhaseChangeRequest);

            // Підписка на асинхронні події
            this.SubscribeAsync<Events.Domain.GameStartedEvent>(_eventBus, OnGameStartedAsync);
            this.SubscribeAsync<Events.Domain.GameEndedEvent>(_eventBus, OnGameEndedAsync);

            _logger.LogInfo("PhaseSystem initialized");
        }

        public override void Update(float deltaTime)
        {
            if (_currentPhase == Events.Domain.GamePhase.None)
                return;

            // Оновлення таймера фази
            _phaseTimer += deltaTime;

            // Перевірка завершення фази
            if (_phaseTimer >= _phaseDuration)
            {
                // Завершення поточної фази
                Events.Domain.GamePhase previousPhase = _currentPhase;

                // Визначення наступної фази
                Events.Domain.GamePhase nextPhase = GetNextPhase(_currentPhase);

                // Запуск нової фази
                StartPhase(nextPhase);

                // Публікація події завершення фази
                var evt = new Events.Domain.PhaseEndedEvent
                {
                    Phase = previousPhase,
                    Timestamp = System.DateTime.UtcNow
                };

                _eventBus.Publish(evt);
            }
        }

        private void OnPhaseChangeRequest(Events.Domain.PhaseChangeRequestEvent evt)
        {
            _logger.LogInfo($"Phase change requested from {_currentPhase} to {evt.RequestedPhase}");

            // Завершення поточної фази
            Events.Domain.GamePhase previousPhase = _currentPhase;

            // Запуск нової фази
            StartPhase(evt.RequestedPhase);

            // Публікація події зміни фази
            var phaseChangedEvent = new Events.Domain.PhaseChangedEvent
            {
                PreviousPhase = previousPhase,
                CurrentPhase = evt.RequestedPhase,
                Timestamp = System.DateTime.UtcNow
            };

            _eventBus.Publish(phaseChangedEvent);
        }

        private async UniTask OnGameStartedAsync(Events.Domain.GameStartedEvent evt)
        {
            _logger.LogInfo("Game started, initializing phases");

            // Асинхронна ініціалізація даних для фаз
            await PreparePhaseDataAsync();

            // Запуск першої фази
            StartPhase(Events.Domain.GamePhase.Rune);
        }

        private async UniTask OnGameEndedAsync(Events.Domain.GameEndedEvent evt)
        {
            _logger.LogInfo("Game ended, cleaning up phases");

            // Асинхронне очищення даних фаз
            await CleanupPhaseDataAsync();

            // Скидання фази
            _currentPhase = Events.Domain.GamePhase.None;
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

        public void StartPhase(Events.Domain.GamePhase phase)
        {
            _currentPhase = phase;
            _phaseTimer = 0;

            // Встановлення тривалості фази
            _phaseDuration = GetPhaseDuration(phase);

            _logger.LogInfo($"Started phase {phase} with duration {_phaseDuration}s");

            // Публікація події початку фази
            var evt = new Events.Domain.PhaseStartedEvent
            {
                Phase = phase,
                Duration = _phaseDuration,
                Timestamp = System.DateTime.UtcNow
            };

            _eventBus.Publish(evt);
        }

        public void EndPhase(Events.Domain.GamePhase phase)
        {
            _logger.LogInfo($"Phase {phase} ended manually");

            var evt = new Events.Domain.PhaseEndedEvent
            {
                Phase = phase,
                Timestamp = System.DateTime.UtcNow
            };

            _eventBus.Publish(evt);
        }

        private Events.Domain.GamePhase GetNextPhase(Events.Domain.GamePhase currentPhase)
        {
            return currentPhase switch
            {
                Events.Domain.GamePhase.Rune => Events.Domain.GamePhase.Planning,
                Events.Domain.GamePhase.Planning => Events.Domain.GamePhase.Movement,
                Events.Domain.GamePhase.Movement => Events.Domain.GamePhase.Combat,
                Events.Domain.GamePhase.Combat => Events.Domain.GamePhase.Freeze,
                Events.Domain.GamePhase.Freeze => Events.Domain.GamePhase.Rune,
                _ => Events.Domain.GamePhase.Rune
            };
        }

        private float GetPhaseDuration(Events.Domain.GamePhase phase)
        {
            return phase switch
            {
                Events.Domain.GamePhase.Rune => 5f,
                Events.Domain.GamePhase.Planning => 15f,
                Events.Domain.GamePhase.Movement => 10f,
                Events.Domain.GamePhase.Combat => 8f,
                Events.Domain.GamePhase.Freeze => 3f,
                _ => 0f
            };
        }

        public float GetPhaseTimeRemaining()
        {
            return _phaseDuration - _phaseTimer;
        }

        public override void Dispose()
        {
            // Відписка від подій з явним вказуванням аргументів типу
            this.Unsubscribe<Events.Domain.PhaseChangeRequestEvent>(_eventBus, OnPhaseChangeRequest);
            this.UnsubscribeAsync<Events.Domain.GameStartedEvent>(_eventBus, OnGameStartedAsync);
            this.UnsubscribeAsync<Events.Domain.GameEndedEvent>(_eventBus, OnGameEndedAsync);

            _logger.LogInfo("PhaseSystem disposed");
        }
    }
}
