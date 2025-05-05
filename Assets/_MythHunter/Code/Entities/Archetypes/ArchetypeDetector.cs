// Шлях: Assets/_MythHunter/Code/Entities/Archetypes/ArchetypeDetector.cs
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;

namespace MythHunter.Entities.Archetypes
{
    /// <summary>
    /// Детектор архетипів для визначення архетипів сутностей на основі їх компонентів
    /// </summary>
    public class ArchetypeDetector
    {
        private readonly IEntityManager _entityManager;
        private readonly ArchetypeTemplateRegistry _templateRegistry;
        private readonly ArchetypeRegistry _archetypeRegistry;
        private readonly IMythLogger _logger;

        [Inject]
        public ArchetypeDetector(
            IEntityManager entityManager,
            ArchetypeTemplateRegistry templateRegistry,
            ArchetypeRegistry archetypeRegistry,
            IMythLogger logger)
        {
            _entityManager = entityManager;
            _templateRegistry = templateRegistry;
            _archetypeRegistry = archetypeRegistry;
            _logger = logger;
        }

        /// <summary>
        /// Визначає архетип сутності на основі її компонентів
        /// </summary>
        public bool DetectAndRegisterArchetype(int entityId)
        {
            foreach (string archetypeId in _templateRegistry.GetAllTemplateIds())
            {
                if (_templateRegistry.MatchesTemplate(entityId, archetypeId))
                {
                    _archetypeRegistry.RegisterEntityArchetype(entityId, archetypeId);
                    _logger.LogDebug($"Auto-detected archetype '{archetypeId}' for entity {entityId}", "Entity");
                    return true;
                }
            }

            return false;
        }
    }
}
