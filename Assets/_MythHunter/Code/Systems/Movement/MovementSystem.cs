using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Systems.Movement
{
    /// <summary>
    /// Система переміщення об'єктів
    /// </summary>
    public class MovementSystem : SystemBase, IMovementSystem, IEventSubscriber
    {
        private readonly IEntityManager _entityManager;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        [MythHunter.Core.DI.Inject]
        public MovementSystem(IEntityManager entityManager, IEventBus eventBus, IMythLogger logger)
        {
            _entityManager = entityManager;
            _eventBus = eventBus;
            _logger = logger;
        }

        public override void Initialize()
        {
            SubscribeToEvents();
            _logger.LogInfo("MovementSystem initialized", "Movement");
        }

        public void SubscribeToEvents()
        {
            // Підписка на події
        }

        public void UnsubscribeFromEvents()
        {
            // Відписка від подій
        }

        public void MoveEntity(int entityId, Vector3 destination)
        {
            _logger.LogInfo($"Moving entity {entityId} to {destination}", "Movement");
            // Базова логіка переміщення
        }

        public void StopEntity(int entityId)
        {
            _logger.LogInfo($"Stopping entity {entityId}", "Movement");
            // Логіка зупинки руху
        }

        public float GetEntitySpeed(int entityId)
        {
            // Заглушка для швидкості
            return 5.0f;
        }

        public bool IsEntityMoving(int entityId)
        {
            // Заглушка для перевірки руху
            return false;
        }

        public override void Update(float deltaTime)
        {
            // Логіка оновлення системи руху
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            _logger.LogInfo("MovementSystem disposed", "Movement");
        }
    }
}
