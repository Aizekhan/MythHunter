// Шлях: Assets/_MythHunter/Code/Entities/Archetypes/ArchetypeRegistry.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.Entities.Archetypes
{
    /// <summary>
    /// Реєстр для відстеження архетипів сутностей
    /// </summary>
    public class ArchetypeRegistry
    {
        private readonly Dictionary<int, string> _entityToArchetype = new Dictionary<int, string>();
        private readonly Dictionary<string, List<int>> _archetypeToEntities = new Dictionary<string, List<int>>();
        private readonly IMythLogger _logger;

        [Inject]
        public ArchetypeRegistry(IMythLogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Реєструє архетип сутності у кеші
        /// </summary>
        public void RegisterEntityArchetype(int entityId, string archetypeId)
        {
            if (string.IsNullOrEmpty(archetypeId))
            {
                _logger.LogWarning($"Cannot register entity {entityId} with empty archetype ID", "Entity");
                return;
            }

            // Видаляємо сутність з попереднього архетипу, якщо вона була зареєстрована
            if (_entityToArchetype.TryGetValue(entityId, out var oldArchetypeId))
            {
                if (_archetypeToEntities.TryGetValue(oldArchetypeId, out var entities))
                {
                    entities.Remove(entityId);
                }
            }

            // Реєструємо сутність у новому архетипі
            _entityToArchetype[entityId] = archetypeId;

            if (!_archetypeToEntities.TryGetValue(archetypeId, out var archetypeEntities))
            {
                archetypeEntities = new List<int>();
                _archetypeToEntities[archetypeId] = archetypeEntities;
            }

            archetypeEntities.Add(entityId);
            _logger.LogDebug($"Registered entity {entityId} with archetype '{archetypeId}'", "Entity");
        }

        /// <summary>
        /// Видаляє реєстрацію архетипу сутності
        /// </summary>
        public void UnregisterEntityArchetype(int entityId)
        {
            if (_entityToArchetype.TryGetValue(entityId, out var archetypeId))
            {
                if (_archetypeToEntities.TryGetValue(archetypeId, out var entities))
                {
                    entities.Remove(entityId);
                }

                _entityToArchetype.Remove(entityId);
                _logger.LogDebug($"Unregistered entity {entityId} from archetype '{archetypeId}'", "Entity");
            }
        }

        /// <summary>
        /// Отримує архетип сутності
        /// </summary>
        public string GetEntityArchetype(int entityId)
        {
            return _entityToArchetype.TryGetValue(entityId, out var archetypeId)
                ? archetypeId
                : null;
        }

        /// <summary>
        /// Отримує всі сутності заданого архетипу
        /// </summary>
        public List<int> GetEntitiesByArchetype(string archetypeId)
        {
            if (_archetypeToEntities.TryGetValue(archetypeId, out var entities))
            {
                return new List<int>(entities);
            }

            return new List<int>();
        }

        /// <summary>
        /// Перевіряє, чи відповідає сутність заданому архетипу
        /// </summary>
        public bool IsEntityOfArchetype(int entityId, string archetypeId)
        {
            return _entityToArchetype.TryGetValue(entityId, out var currentArchetypeId) &&
                   currentArchetypeId == archetypeId;
        }

        /// <summary>
        /// Очищає реєстр
        /// </summary>
        public void Clear()
        {
            _entityToArchetype.Clear();
            _archetypeToEntities.Clear();
        }
    }
}
