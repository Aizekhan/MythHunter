using MythHunter.Components.Character;
using MythHunter.Components.Combat;
using MythHunter.Components.Core;
using MythHunter.Components.Movement;
using MythHunter.Components.Tags;
using MythHunter.Core.ECS;

namespace MythHunter.Entities.Archetypes
{
    /// <summary>
    /// Архетип для ворога
    /// </summary>
    public class EnemyArchetype : EntityArchetypeBase
    {
        public override string ArchetypeId => "Enemy";

        protected override void DefineRequiredComponents()
        {
            // Визначаємо необхідні компоненти для ворога
            AddRequiredComponent<NameComponent>();
            AddRequiredComponent<HealthComponent>();
            AddRequiredComponent<MovementComponent>();
            AddRequiredComponent<CombatStatsComponent>();
            AddRequiredComponent<AIControlledTag>();
        }

        protected override void ApplyBaseComponents(int entityId, IEntityManager entityManager)
        {
            // Створюємо базові компоненти для ворога
            entityManager.AddComponent(entityId, new NameComponent { Name = "Enemy" });
            entityManager.AddComponent(entityId, new HealthComponent { CurrentHealth = 50, MaxHealth = 50 });
            entityManager.AddComponent(entityId, new MovementComponent { Speed = 3f });
            entityManager.AddComponent(entityId, new CombatStatsComponent { AttackPower = 8, Defense = 3 });
            entityManager.AddComponent(entityId, new AIControlledTag());
        }
    }
}
