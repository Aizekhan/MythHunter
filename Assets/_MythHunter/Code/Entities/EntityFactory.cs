// Шлях: Assets/_MythHunter/Code/Entities/EntityFactory.cs

using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Entities.Archetypes;
using MythHunter.Utils.Logging;

namespace MythHunter.Entities
{
    /// <summary>
    /// Фабрика для створення сутностей
    /// </summary>
    public class EntityFactory
    {
        private readonly IEntityManager _entityManager;
        private readonly ArchetypeSystem _archetypeSystem;
        private readonly IMythLogger _logger;

        [Inject]
        public EntityFactory(
            IEntityManager entityManager,
            ArchetypeSystem archetypeSystem,
            IMythLogger logger)
        {
            _entityManager = entityManager;
            _archetypeSystem = archetypeSystem;
            _logger = logger;
        }

        /// <summary>
        /// Створює персонажа гравця
        /// </summary>
        public int CreatePlayerCharacter(string name)
        {
            var overrides = new Dictionary<Type, object>
            {
                { typeof(Components.Core.NameComponent), new Components.Core.NameComponent { Name = name } }
            };

            int entityId = _archetypeSystem.CreateEntityFromArchetype("Character", overrides);

            if (entityId >= 0)
            {
                // Додаємо тег гравця
                _entityManager.AddComponent(entityId, new Components.Tags.PlayerControlledTag());

                _logger.LogInfo($"Created player character '{name}' with ID {entityId}", "Entity");
            }

            return entityId;
        }

        /// <summary>
        /// Створює ворога з заданими параметрами
        /// </summary>
        public int CreateEnemy(string name, float health, float attackPower)
        {
            var overrides = new Dictionary<Type, object>
            {
                { typeof(Components.Core.NameComponent), new Components.Core.NameComponent { Name = name } },
                { typeof(Components.Character.HealthComponent), new Components.Character.HealthComponent { CurrentHealth = health, MaxHealth = health } },
                { typeof(Components.Combat.CombatStatsComponent), new Components.Combat.CombatStatsComponent { AttackPower = attackPower } }
            };

            int entityId = _archetypeSystem.CreateEntityFromArchetype("Enemy", overrides);

            if (entityId >= 0)
            {
                _logger.LogInfo($"Created enemy '{name}' with ID {entityId}", "Entity");
            }

            return entityId;
        }

        /// <summary>
        /// Створює предмет
        /// </summary>
        public int CreateItem(string name, string description, int value)
        {
            var overrides = new Dictionary<Type, object>
            {
                { typeof(Components.Core.NameComponent), new Components.Core.NameComponent { Name = name } },
                { typeof(Components.Core.DescriptionComponent), new Components.Core.DescriptionComponent { Description = description } },
                { typeof(Components.Core.ValueComponent), new Components.Core.ValueComponent { Value = value } }
            };

            int entityId = _archetypeSystem.CreateEntityFromArchetype("Item", overrides);

            if (entityId >= 0)
            {
                _logger.LogInfo($"Created item '{name}' with ID {entityId}", "Entity");
            }

            return entityId;
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

            // Отримуємо архетип вихідної сутності
            string archetypeId = _archetypeSystem.GetEntityArchetype(sourceEntityId);

            // Копіюємо всі компоненти
            foreach (var type in GetAllComponentTypes(sourceEntityId))
            {
                var getComponentMethod = typeof(IEntityManager).GetMethod("GetComponent").MakeGenericMethod(type);
                var addComponentMethod = typeof(IEntityManager).GetMethod("AddComponent").MakeGenericMethod(type);

                var component = getComponentMethod.Invoke(_entityManager, new object[] { sourceEntityId });
                addComponentMethod.Invoke(_entityManager, new object[] { newEntityId, component });
            }

            // Реєструємо архетип
            if (!string.IsNullOrEmpty(archetypeId))
            {
                _archetypeSystem.RegisterEntityArchetype(newEntityId, archetypeId);
            }

            _logger.LogInfo($"Cloned entity {sourceEntityId} to new entity {newEntityId}", "Entity");

            return newEntityId;
        }

        /// <summary>
        /// Отримує всі типи компонентів сутності
        /// </summary>
        private IEnumerable<Type> GetAllComponentTypes(int entityId)
        {
            var result = new List<Type>();

            // Це спрощена реалізація - потрібно адаптувати до реальної системи
            // В ідеалі EntityManager повинен мати метод для отримання всіх типів компонентів сутності

            // Поки що повертаємо базові типи, які точно є
            result.Add(typeof(Components.Core.IdComponent));

            if (_entityManager.HasComponent<Components.Core.NameComponent>(entityId))
                result.Add(typeof(Components.Core.NameComponent));

            if (_entityManager.HasComponent<Components.Character.HealthComponent>(entityId))
                result.Add(typeof(Components.Character.HealthComponent));

            if (_entityManager.HasComponent<Components.Movement.MovementComponent>(entityId))
                result.Add(typeof(Components.Movement.MovementComponent));

            if (_entityManager.HasComponent<Components.Combat.CombatStatsComponent>(entityId))
                result.Add(typeof(Components.Combat.CombatStatsComponent));

            if (_entityManager.HasComponent<Components.Character.InventoryComponent>(entityId))
                result.Add(typeof(Components.Character.InventoryComponent));

            if (_entityManager.HasComponent<Components.Core.DescriptionComponent>(entityId))
                result.Add(typeof(Components.Core.DescriptionComponent));

            if (_entityManager.HasComponent<Components.Core.ValueComponent>(entityId))
                result.Add(typeof(Components.Core.ValueComponent));

            return result;
        }
    }
}
