// Шлях: Assets/_MythHunter/Code/Entities/ArchetypeFactory.cs

using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;

namespace MythHunter.Entities
{
    /// <summary>
    /// Фабрика для створення сутностей на основі архетипів з додатковими параметрами
    /// </summary>
    public class ArchetypeFactory
    {
        private readonly EntityArchetypeRegistry _archetypeRegistry;
        private readonly IEntityManager _entityManager;
        private readonly IMythLogger _logger;

        [Inject]
        public ArchetypeFactory(
            EntityArchetypeRegistry archetypeRegistry,
            IEntityManager entityManager,
            IMythLogger logger)
        {
            _archetypeRegistry = archetypeRegistry;
            _entityManager = entityManager;
            _logger = logger;
        }

        /// <summary>
        /// Створює сутність за архетипом з додатковими параметрами
        /// </summary>
        public int CreateEntityFromArchetype(string archetypeId, Dictionary<string, object> parameters = null)
        {
            var archetype = _archetypeRegistry.GetArchetype(archetypeId);

            if (archetype == null)
            {
                _logger.LogWarning($"Archetype '{archetypeId}' not found", "Entity");
                return -1;
            }

            int entityId = archetype.CreateEntity(_entityManager);

            if (entityId >= 0 && parameters != null)
            {
                ApplyParameters(entityId, parameters);
            }

            _logger.LogInfo($"Created entity of archetype '{archetypeId}' with ID {entityId}", "Entity");

            return entityId;
        }

        /// <summary>
        /// Застосовує додаткові параметри до сутності
        /// </summary>
        private void ApplyParameters(int entityId, Dictionary<string, object> parameters)
        {
            foreach (var parameter in parameters)
            {
                _logger.LogDebug($"Applying parameter {parameter.Key} to entity {entityId}", "Entity");

                // Реалізація застосування параметрів буде залежати від конкретних компонентів
                // Наприклад, можна застосовувати параметри через рефлексію або спеціальні методи
            }
        }

        /// <summary>
        /// Клонує існуючу сутність з усіма її компонентами
        /// </summary>
        public int CloneEntity(int sourceEntityId)
        {
            if (!_entityManager.HasComponent<Components.Core.IdComponent>(sourceEntityId))
            {
                _logger.LogWarning($"Cannot clone entity {sourceEntityId} - entity not found", "Entity");
                return -1;
            }

            int newEntityId = _entityManager.CreateEntity();

            // Отримуємо всі компоненти вихідної сутності та копіюємо їх
            // Реалізація залежить від конкретних типів компонентів

            _logger.LogInfo($"Cloned entity {sourceEntityId} to new entity {newEntityId}", "Entity");

            return newEntityId;
        }
    }
}
