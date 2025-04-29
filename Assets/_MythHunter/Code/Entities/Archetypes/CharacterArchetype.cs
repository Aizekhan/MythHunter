using MythHunter.Components.Character;
using MythHunter.Components.Combat;
using MythHunter.Components.Core;
using MythHunter.Components.Movement;
using MythHunter.Core.ECS;

namespace MythHunter.Entities.Archetypes
{
    /// <summary>
    /// Архетип для персонажа гравця
    /// </summary>
    public class CharacterArchetype : EntityArchetypeBase
    {
        public override string ArchetypeId => "Character";

        protected override void DefineRequiredComponents()
        {
            // Визначаємо необхідні компоненти для персонажа
            AddRequiredComponent<NameComponent>();
            AddRequiredComponent<HealthComponent>();
            AddRequiredComponent<MovementComponent>();
            AddRequiredComponent<CombatStatsComponent>();
            AddRequiredComponent<InventoryComponent>();
        }

        protected override void ApplyBaseComponents(int entityId, IEntityManager entityManager)
        {
            // Створюємо базові компоненти для персонажа
            entityManager.AddComponent(entityId, new NameComponent { Name = "Character" });
            entityManager.AddComponent(entityId, new HealthComponent { CurrentHealth = 100, MaxHealth = 100 });
            entityManager.AddComponent(entityId, new MovementComponent { Speed = 5f });
            entityManager.AddComponent(entityId, new CombatStatsComponent { AttackPower = 10, Defense = 5 });
            entityManager.AddComponent(entityId, new InventoryComponent { Capacity = 20 });
        }
    }
}
