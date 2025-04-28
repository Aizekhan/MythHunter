using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.AI
{
    /// <summary>
    /// Система штучного інтелекту
    /// </summary>
    public class AISystem : SystemBase, IAISystem, IEventSubscriber
    {
        private readonly IEntityManager _entityManager;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        [MythHunter.Core.DI.Inject]
        public AISystem(IEntityManager entityManager, IEventBus eventBus, IMythLogger logger)
        {
            _entityManager = entityManager;
            _eventBus = eventBus;
            _logger = logger;
        }

        public override void Initialize()
        {
            SubscribeToEvents();
            _logger.LogInfo("AISystem initialized", "AI");
        }

        public void SubscribeToEvents()
        {
            // Підписка на події
        }

        public void UnsubscribeFromEvents()
        {
            // Відписка від подій
        }

        public void AddBehavior(int entityId, AIBehaviorType behaviorType)
        {
            _logger.LogInfo($"Adding behavior {behaviorType} to entity {entityId}", "AI");
            // Логіка додавання поведінки
        }

        public void RemoveBehavior(int entityId, AIBehaviorType behaviorType)
        {
            _logger.LogInfo($"Removing behavior {behaviorType} from entity {entityId}", "AI");
            // Логіка видалення поведінки
        }

        public void UpdateAI(int entityId)
        {
            // Заглушка для оновлення AI конкретної сутності
        }

        public override void Update(float deltaTime)
        {
            // Логіка оновлення всіх AI сутностей
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            _logger.LogInfo("AISystem disposed", "AI");
        }
    }
}
