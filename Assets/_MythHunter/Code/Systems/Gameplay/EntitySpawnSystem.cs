using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Entities;
using MythHunter.Entities.Archetypes;
using MythHunter.Events;
using MythHunter.Systems.Core;
using MythHunter.Utils.Logging;
using MythHunter.Events.Domain.Gameplay;
namespace MythHunter.Systems.Gameplay
{
    /// <summary>
    /// Система для створення сутностей на основі архетипів
    /// </summary>
    public class EntitySpawnSystem : SystemBase, IEntitySpawnSystem
    {
        private readonly EntityFactory _entityFactory;
        private readonly ArchetypeSystem _archetypeSystem;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        [Inject]
        public EntitySpawnSystem(
            EntityFactory entityFactory,
            ArchetypeSystem archetypeRegistry,
            IEventBus eventBus,
            IMythLogger logger)
        {
            _entityFactory = entityFactory;
            _archetypeSystem = archetypeRegistry;
            _eventBus = eventBus;
            _logger = logger;
        }

        public override void Initialize()
        {
            

            // Підписка на події
            _eventBus.Subscribe<Events.Domain.Gameplay.SpawnCharacterEvent>(OnSpawnCharacter);
            _eventBus.Subscribe<Events.Domain.Gameplay.SpawnEnemyEvent>(OnSpawnEnemy);

            _logger.LogInfo("EntitySpawnSystem initialized");
        }

        private void OnSpawnCharacter(Events.Domain.Gameplay.SpawnCharacterEvent evt)
        {
            int entityId = _entityFactory.CreatePlayerCharacter(evt.CharacterName);

            if (entityId >= 0)
            {
                _logger.LogInfo($"Spawned character '{evt.CharacterName}' with ID {entityId}");

                // Публікуємо подію про створення персонажа
                _eventBus.Publish(new Events.Domain.Gameplay.CharacterSpawnedEvent
                {
                    EntityId = entityId,
                    CharacterName = evt.CharacterName,
                    Timestamp = System.DateTime.UtcNow
                });
            }
            else
            {
                _logger.LogError($"Failed to spawn character '{evt.CharacterName}'");
            }
        }

        private void OnSpawnEnemy(Events.Domain.Gameplay.SpawnEnemyEvent evt)
        {
            int entityId = _entityFactory.CreateEnemy(evt.EnemyName, evt.Health, evt.AttackPower);

            if (entityId >= 0)
            {
                _logger.LogInfo($"Spawned enemy '{evt.EnemyName}' with ID {entityId}");

                // Публікуємо подію про створення ворога
                _eventBus.Publish(new Events.Domain.Gameplay.EnemySpawnedEvent
                {
                    EntityId = entityId,
                    EnemyName = evt.EnemyName,
                    Timestamp = System.DateTime.UtcNow
                });
            }
            else
            {
                _logger.LogError($"Failed to spawn enemy '{evt.EnemyName}'");
            }
        }

        public override void Dispose()
        {
            // Відписка від подій
            _eventBus.Unsubscribe<Events.Domain.Gameplay.SpawnCharacterEvent>(OnSpawnCharacter);
            _eventBus.Unsubscribe<Events.Domain.Gameplay.SpawnEnemyEvent>(OnSpawnEnemy);

            _logger.LogInfo("EntitySpawnSystem disposed");
        }
    }
}
