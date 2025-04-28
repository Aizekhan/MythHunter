// RuneSystem.cs
namespace MythHunter.Systems.Phase
{
    /// <summary>
    /// Система керування рунами
    /// </summary>
    public class RuneSystem : MythHunter.Core.ECS.SystemBase, IRuneSystem, MythHunter.Events.IEventSubscriber
    {
        private readonly MythHunter.Core.ECS.IEntityManager _entityManager;
        private readonly MythHunter.Events.IEventBus _eventBus;
        private readonly MythHunter.Utils.Logging.IMythLogger _logger;
        private int _currentRuneValue = 0;
        private System.Random _random = new System.Random();

        [MythHunter.Core.DI.Inject]
        public RuneSystem(MythHunter.Core.ECS.IEntityManager entityManager, MythHunter.Events.IEventBus eventBus, MythHunter.Utils.Logging.IMythLogger logger)
        {
            _entityManager = entityManager;
            _eventBus = eventBus;
            _logger = logger;
        }

        public override void Initialize()
        {
            SubscribeToEvents();
            _logger.LogInfo("RuneSystem initialized", "Rune");
        }

        public void SubscribeToEvents()
        {
            // Підписка на події
        }

        public void UnsubscribeFromEvents()
        {
            // Відписка від подій
        }

        public void RollRune()
        {
            // Генеруємо значення від 2 до 12 (як при киданні 2-х шестигранних кубиків)
            _currentRuneValue = _random.Next(1, 7) + _random.Next(1, 7);
            _logger.LogInfo($"Rolled rune value: {_currentRuneValue}", "Rune");

            // TODO: Опублікувати подію про кидання руни
        }

        public int GetRuneValue() => _currentRuneValue;

        public void ApplyRuneEffect(int entityId)
        {
            _logger.LogInfo($"Applying rune effect with value {_currentRuneValue} to entity {entityId}", "Rune");

            // TODO: Застосувати ефект руни до сутності
        }

        public override void Update(float deltaTime)
        {
            // Логіка оновлення системи рун
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            _logger.LogInfo("RuneSystem disposed", "Rune");
        }
    }
}
