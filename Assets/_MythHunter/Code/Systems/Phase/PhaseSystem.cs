// PhaseSystem.cs
namespace MythHunter.Systems.Phase
{
    /// <summary>
    /// Система керування фазами гри
    /// </summary>
    public class PhaseSystem : MythHunter.Core.ECS.SystemBase, IPhaseSystem, MythHunter.Events.IEventSubscriber
    {
        private readonly MythHunter.Events.IEventBus _eventBus;
        private readonly MythHunter.Utils.Logging.IMythLogger _logger;
        private MythHunter.Events.Domain.GamePhase _currentPhase = MythHunter.Events.Domain.GamePhase.None;
        private float _phaseTimeRemaining = 0f;

        [MythHunter.Core.DI.Inject]
        public PhaseSystem(MythHunter.Events.IEventBus eventBus, MythHunter.Utils.Logging.IMythLogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }

        public override void Initialize()
        {
            SubscribeToEvents();
            _logger.LogInfo("PhaseSystem initialized", "Phase");
        }

        public void SubscribeToEvents()
        {
            // Підписка на події
        }

        public void UnsubscribeFromEvents()
        {
            // Відписка від подій
        }

        public void StartPhase(MythHunter.Events.Domain.GamePhase phase)
        {
            _currentPhase = phase;
            _logger.LogInfo($"Starting phase: {phase}", "Phase");

            // Публікація події початку фази
            _eventBus.Publish(new MythHunter.Events.Domain.PhaseStartedEvent
            {
                Phase = phase,
                Duration = GetPhaseDuration(phase),
                Timestamp = System.DateTime.Now
            });
        }

        public void EndPhase(MythHunter.Events.Domain.GamePhase phase)
        {
            _logger.LogInfo($"Ending phase: {phase}", "Phase");

            // Публікація події завершення фази
            _eventBus.Publish(new MythHunter.Events.Domain.PhaseEndedEvent
            {
                Phase = phase,
                Timestamp = System.DateTime.Now
            });

            // Перехід до наступної фази
            _currentPhase = GetNextPhase(phase);
            StartPhase(_currentPhase);
        }

        public MythHunter.Events.Domain.GamePhase CurrentPhase => _currentPhase;

        public float GetPhaseTimeRemaining() => _phaseTimeRemaining;

        public override void Update(float deltaTime)
        {
            if (_currentPhase != MythHunter.Events.Domain.GamePhase.None)
            {
                _phaseTimeRemaining -= deltaTime;

                if (_phaseTimeRemaining <= 0)
                {
                    EndPhase(_currentPhase);
                }
            }
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            _logger.LogInfo("PhaseSystem disposed", "Phase");
        }

        private float GetPhaseDuration(MythHunter.Events.Domain.GamePhase phase)
        {
            switch (phase)
            {
                case MythHunter.Events.Domain.GamePhase.Rune:
                    return 5f;
                case MythHunter.Events.Domain.GamePhase.Planning:
                    return 15f;
                case MythHunter.Events.Domain.GamePhase.Movement:
                    return 30f;
                case MythHunter.Events.Domain.GamePhase.Combat:
                    return 20f;
                case MythHunter.Events.Domain.GamePhase.Freeze:
                    return 5f;
                default:
                    return 10f;
            }
        }

        private MythHunter.Events.Domain.GamePhase GetNextPhase(MythHunter.Events.Domain.GamePhase currentPhase)
        {
            switch (currentPhase)
            {
                case MythHunter.Events.Domain.GamePhase.Rune:
                    return MythHunter.Events.Domain.GamePhase.Planning;
                case MythHunter.Events.Domain.GamePhase.Planning:
                    return MythHunter.Events.Domain.GamePhase.Movement;
                case MythHunter.Events.Domain.GamePhase.Movement:
                    return MythHunter.Events.Domain.GamePhase.Combat;
                case MythHunter.Events.Domain.GamePhase.Combat:
                    return MythHunter.Events.Domain.GamePhase.Freeze;
                case MythHunter.Events.Domain.GamePhase.Freeze:
                    return MythHunter.Events.Domain.GamePhase.Rune;
                default:
                    return MythHunter.Events.Domain.GamePhase.Rune;
            }
        }
    }
}
