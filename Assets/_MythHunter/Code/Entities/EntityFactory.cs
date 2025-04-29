using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;

namespace MythHunter.Entities
{
    /// <summary>
    /// Фабрика для створення типових сутностей
    /// </summary>
    public class EntityFactory
    {
        private readonly IEntityManager _entityManager;
        private readonly EntityArchetypeRegistry _archetypeRegistry;
        private readonly IMythLogger _logger;

        [Inject]
        public EntityFactory(
            IEntityManager entityManager,
            EntityArchetypeRegistry archetypeRegistry,
            IMythLogger logger)
        {
            _entityManager = entityManager;
            _archetypeRegistry = archetypeRegistry;
            _logger = logger;
        }

        /// <summary>
        /// Створює персонажа гравця
        /// </summary>
        public int CreatePlayerCharacter(string name)
        {
            int entityId = _archetypeRegistry.CreateEntityFromArchetype("Character");

            if (entityId >= 0)
            {
                // Модифікуємо компоненти створеної сутності
                var nameComponent = _entityManager.GetComponent<Components.Core.NameComponent>(entityId);
                nameComponent.Name = name;
                _entityManager.AddComponent(entityId, nameComponent);

                // Додаємо тег гравця
                _entityManager.AddComponent(entityId, new Components.Tags.PlayerControlledTag());

                _logger.LogInfo($"Created player character '{name}' with ID {entityId}");
            }

            return entityId;
        }

        /// <summary>
        /// Створює ворога з заданими параметрами
        /// </summary>
        public int CreateEnemy(string name, float health, float attackPower)
        {
            int entityId = _archetypeRegistry.CreateEntityFromArchetype("Enemy");

            if (entityId >= 0)
            {
                // Модифікуємо компоненти створеної сутності
                var nameComponent = _entityManager.GetComponent<Components.Core.NameComponent>(entityId);
                nameComponent.Name = name;
                _entityManager.AddComponent(entityId, nameComponent);

                var healthComponent = _entityManager.GetComponent<Components.Character.HealthComponent>(entityId);
                healthComponent.CurrentHealth = health;
                healthComponent.MaxHealth = health;
                _entityManager.AddComponent(entityId, healthComponent);

                var combatStatsComponent = _entityManager.GetComponent<Components.Combat.CombatStatsComponent>(entityId);
                combatStatsComponent.AttackPower = attackPower;
                _entityManager.AddComponent(entityId, combatStatsComponent);

                _logger.LogInfo($"Created enemy '{name}' with ID {entityId}");
            }

            return entityId;
        }

        /// <summary>
        /// Створює предмет
        /// </summary>
        public int CreateItem(string name, string description, int value)
        {
            int entityId = _entityManager.CreateEntity();

            _entityManager.AddComponent(entityId, new Components.Core.NameComponent { Name = name });
            _entityManager.AddComponent(entityId, new Components.Core.DescriptionComponent { Description = description });
            _entityManager.AddComponent(entityId, new Components.Core.ValueComponent { Value = value });

            _logger.LogInfo($"Created item '{name}' with ID {entityId}");

            return entityId;
        }
    }
}
