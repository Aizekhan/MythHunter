using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using System.Collections.Generic;

namespace MythHunter.Entities
{
    /// <summary>
    /// Реєстр доступних архетипів сутностей
    /// </summary>
    public class EntityArchetypeRegistry
    {
        private readonly Dictionary<string, IEntityArchetype> _archetypes = new Dictionary<string, IEntityArchetype>();
        private readonly IEntityManager _entityManager;

        [Inject]
        public EntityArchetypeRegistry(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        /// <summary>
        /// Реєструє архетип у реєстрі
        /// </summary>
        public void RegisterArchetype(IEntityArchetype archetype)
        {
            _archetypes[archetype.ArchetypeId] = archetype;
        }

        /// <summary>
        /// Отримує архетип за ідентифікатором
        /// </summary>
        public IEntityArchetype GetArchetype(string archetypeId)
        {
            return _archetypes.TryGetValue(archetypeId, out var archetype) ? archetype : null;
        }

        /// <summary>
        /// Створює сутність за архетипом
        /// </summary>
        public int CreateEntityFromArchetype(string archetypeId)
        {
            if (_archetypes.TryGetValue(archetypeId, out var archetype))
            {
                return archetype.CreateEntity(_entityManager);
            }

            return -1; // Повертаємо невалідний ID, якщо архетип не знайдено
        }

        /// <summary>
        /// Визначає архетип для існуючої сутності
        /// </summary>
        public string GetArchetypeForEntity(int entityId)
        {
            foreach (var pair in _archetypes)
            {
                if (pair.Value.MatchesArchetype(entityId, _entityManager))
                {
                    return pair.Key;
                }
            }

            return null; // Повертаємо null, якщо архетип не знайдено
        }
    }
}
