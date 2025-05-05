// Шлях: Assets/_MythHunter/Code/Entities/Archetypes/ArchetypeTemplateRegistry.cs

using System;
using System.Collections.Generic;
using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
namespace MythHunter.Entities.Archetypes
{
    /// <summary>
    /// Реєстр шаблонів архетипів для швидкого створення сутностей
    /// </summary>
    public class ArchetypeTemplateRegistry
    {
        private readonly Dictionary<string, ArchetypeTemplate> _templates = new Dictionary<string, ArchetypeTemplate>();
        private readonly IEntityManager _entityManager;
        private readonly IMythLogger _logger;

        /// <summary>
        /// Шаблон архетипу, який містить типи компонентів і їх значення за замовчуванням
        /// </summary>
        public class ArchetypeTemplate
        {
            public string ArchetypeId
            {
                get; set;
            }
            public Dictionary<Type, object> DefaultComponents { get; } = new Dictionary<Type, object>();
        }

        [Inject]
        public ArchetypeTemplateRegistry(IEntityManager entityManager, IMythLogger logger)
        {
            _entityManager = entityManager;
            _logger = logger;

            // Реєструємо стандартні архетипи
            RegisterDefaultArchetypes();
        }

        /// <summary>
        /// Реєструє стандартні архетипи
        /// </summary>
        private void RegisterDefaultArchetypes()
        {
           
            // Архетип предмета
            RegisterArchetypeTemplate("Item")
                .WithComponent(new Components.Core.NameComponent { Name = "Item" })
                .WithComponent(new Components.Core.DescriptionComponent { Description = "Default item" })
                .WithComponent(new Components.Core.ValueComponent { Value = 10 });

            _logger.LogInfo($"Registered {_templates.Count} default archetype templates", "Entity");
        }

        /// <summary>
        /// Реєструє новий шаблон архетипу
        /// </summary>
        public ArchetypeTemplateBuilder RegisterArchetypeTemplate(string archetypeId)
        {
            if (string.IsNullOrEmpty(archetypeId))
                throw new ArgumentException("Archetype ID cannot be null or empty", nameof(archetypeId));

            var template = new ArchetypeTemplate { ArchetypeId = archetypeId };
            _templates[archetypeId] = template;

            return new ArchetypeTemplateBuilder(template, this);
        }

        /// <summary>
        /// Створює сутність за шаблоном архетипу
        /// </summary>
        public int CreateEntityFromTemplate(string archetypeId, Dictionary<Type, object> overrides = null)
        {
            if (!_templates.TryGetValue(archetypeId, out var template))
            {
                _logger.LogWarning($"Template for archetype '{archetypeId}' not found", "Entity");
                return -1;
            }

            int entityId = _entityManager.CreateEntity();

            foreach (var pair in template.DefaultComponents)
            {
                var componentType = pair.Key;
                var defaultComponent = pair.Value;

                // Перевіряємо, чи є перезапис для цього компоненту
                object component = defaultComponent;
                if (overrides != null && overrides.TryGetValue(componentType, out var overrideComponent))
                {
                    component = overrideComponent;
                }

                // Додаємо компонент до сутності
                var addComponentMethod = typeof(IEntityManager).GetMethod("AddComponent").MakeGenericMethod(componentType);
                addComponentMethod.Invoke(_entityManager, new[] { entityId, component });
            }

            _logger.LogInfo($"Created entity {entityId} from template '{archetypeId}'", "Entity");

            return entityId;
        }

        /// <summary>
        /// Перевіряє, чи відповідає сутність шаблону архетипу
        /// </summary>
        public bool MatchesTemplate(int entityId, string archetypeId)
        {
            if (!_templates.TryGetValue(archetypeId, out var template))
            {
                return false;
            }

            foreach (var componentType in template.DefaultComponents.Keys)
            {
                // Перевіряємо наявність компонента через рефлексію
                var hasComponentMethod = typeof(IEntityManager).GetMethod("HasComponent").MakeGenericMethod(componentType);
                bool hasComponent = (bool)hasComponentMethod.Invoke(_entityManager, new object[] { entityId });

                if (!hasComponent)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Отримує всі зареєстровані шаблони архетипів
        /// </summary>
        public IEnumerable<string> GetAllTemplateIds()
        {
            return _templates.Keys;
        }

        /// <summary>
        /// Перевіряє, чи зареєстрований шаблон архетипу
        /// </summary>
        public bool HasTemplate(string archetypeId)
        {
            return _templates.ContainsKey(archetypeId);
        }

        /// <summary>
        /// Видаляє шаблон архетипу
        /// </summary>
        public void RemoveTemplate(string archetypeId)
        {
            if (_templates.Remove(archetypeId))
            {
                _logger.LogInfo($"Removed template for archetype '{archetypeId}'", "Entity");
            }
        }

        /// <summary>
        /// Отримує всі типи компонентів для архетипу
        /// </summary>
        public IEnumerable<Type> GetArchetypeComponentTypes(string archetypeId)
        {
            if (_templates.TryGetValue(archetypeId, out var template))
            {
                return template.DefaultComponents.Keys;
            }

            return Array.Empty<Type>();
        }
    }

    /// <summary>
    /// Будівельник для шаблону архетипу
    /// </summary>
    public class ArchetypeTemplateBuilder
    {
        private readonly ArchetypeTemplateRegistry.ArchetypeTemplate _template;
        private readonly ArchetypeTemplateRegistry _registry;

        public ArchetypeTemplateBuilder(ArchetypeTemplateRegistry.ArchetypeTemplate template, ArchetypeTemplateRegistry registry)
        {
            _template = template;
            _registry = registry;
        }

        /// <summary>
        /// Додає компонент до шаблону архетипу
        /// </summary>
        public ArchetypeTemplateBuilder WithComponent<T>(T component) where T : struct, IComponent
        {
            _template.DefaultComponents[typeof(T)] = component;
            return this;
        }

        /// <summary>
        /// Завершує створення шаблону архетипу
        /// </summary>
        public ArchetypeTemplateRegistry Build()
        {
            return _registry;
        }
    }
}
