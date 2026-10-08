// Assets/_MythHunter/Code/Systems/Movement/MovementUISystem.cs
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events.Domain;
using MythHunter.Events;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.Movement
{
    /// <summary>
    /// Система для візуалізації руху в UI
    /// </summary>
    public class MovementUISystem : SystemBase
    {
        private readonly IEventThrottler _eventThrottler;
        private readonly IMovementSystem _movementSystem;
        private readonly IPathfindingSystem _pathfindingSystem;

        [Inject]
        public MovementUISystem(
            IEventBus eventBus,
            IMythLogger logger,
            IEventThrottler eventThrottler,
            IMovementSystem movementSystem,
            IPathfindingSystem pathfindingSystem)
            : base(logger, eventBus)
        {
            _eventThrottler = eventThrottler;
            _movementSystem = movementSystem;
            _pathfindingSystem = pathfindingSystem;
        }

        public override void Initialize()
        {
            base.Initialize();

            // Реєструємо обмеження для подій руху, щоб не перевантажувати систему
            _eventThrottler.RegisterThrottle<PositionUpdatedEvent>(0.05f); // Оновлення 20 разів на секунду

            _logger.LogInfo("MovementUISystem initialized", "MovementUI");
        }

        protected override void OnSubscribeToEvents()
        {
            Subscribe<PathPlannedEvent>(OnPathPlanned);
            Subscribe<MovementStartedEvent>(OnMovementStarted);
            Subscribe<MovementStoppedEvent>(OnMovementStopped);
        }

        protected override void OnUnsubscribeFromEvents()
        {
            Unsubscribe<PathPlannedEvent>(OnPathPlanned);
            Unsubscribe<MovementStartedEvent>(OnMovementStarted);
            Unsubscribe<MovementStoppedEvent>(OnMovementStopped);
        }

        private void OnPathPlanned(PathPlannedEvent evt)
        {
            // Тут можна додати логіку для відображення запланованого шляху в UI
            // Наприклад, намалювати лінію шляху або відобразити точки
            _logger.LogDebug($"UI: Path planned for entity {evt.EntityId} with {evt.Waypoints.Count} waypoints", "MovementUI");
        }

        private void OnMovementStarted(MovementStartedEvent evt)
        {
            // Тут можна додати логіку для відображення початку руху в UI
            // Наприклад, показати анімацію, індикатор руху тощо
            _logger.LogDebug($"UI: Movement started for entity {evt.EntityId}", "MovementUI");
        }

        private void OnMovementStopped(MovementStoppedEvent evt)
        {
            // Тут можна додати логіку для відображення зупинки руху в UI
            // Наприклад, відобразити статус зупинки, прибрати індикатори руху тощо
            _logger.LogDebug($"UI: Movement stopped for entity {evt.EntityId}, path completed: {evt.PathCompleted}", "MovementUI");
        }
    }
}
