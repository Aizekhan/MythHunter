// Файл: Assets/_MythHunter/Code/Entities/Archetypes/ArchetypeTemplateRegistry.cs

using System;
using System.Collections.Generic;
using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;

namespace MythHunter.Entities.Archetypes
{
    /// <summary>
    /// Оптимізований реєстр шаблонів архетипів
    /// </summary>
    public class ArchetypeTemplateRegistry : IArchetypeTemplateRegistry
    {
        private readonly Dictionary<string, ArchetypeTemplate> _templates = new Dictionary<string, ArchetypeTemplate>();
        private readonly IEntityManager _entityManager;
        private readonly IMythLogger _logger;

        // Кеш делегатів для операцій з компонентами (замість рефлексії)
        private readonly Dictionary<Type, Func<int, object>> _getComponentDelegates = new Dictionary<Type, Func<int, object>>();
        private readonly Dictionary<Type, Action<int, object>> _addComponentDelegates = new Dictionary<Type, Action<int, object>>();
        private readonly Dictionary<Type, Func<int, bool>> _hasComponentDelegates = new Dictionary<Type, Func<int, bool>>();

        /// <summary>
        /// Шаблон архетипу з оптимізованим зберіганням компонентів
        /// </summary>
        public class ArchetypeTemplate
        {
            public string ArchetypeId
            {
                get; set;
            }
            public Dictionary<Type, object> DefaultComponents { get; } = new Dictionary<Type, object>();

            // Делегати для швидкої перевірки компонентів
            public List<Func<int, IEntityManager, bool>> ComponentCheckers { get; } = new List<Func<int, IEntityManager, bool>>();
        }

        [Inject]
        public ArchetypeTemplateRegistry(IEntityManager entityManager, IMythLogger logger)
        {
            _entityManager = entityManager;
            _logger = logger;

            // Ініціалізуємо кеші делегатів
            InitializeDelegateCache();

            // Реєструємо стандартні архетипи
            RegisterDefaultArchetypes();
        }

        /// <summary>
        /// Ініціалізує кеші делегатів для роботи з компонентами
        /// </summary>
        private void InitializeDelegateCache()
        {
            // Тут ми можемо статично зареєструвати відомі типи компонентів,
            // для яких ми знаємо, що вони будуть використовуватися
            CacheComponentDelegates<Components.Core.NameComponent>();
            CacheComponentDelegates<Components.Core.DescriptionComponent>();
            CacheComponentDelegates<Components.Core.ValueComponent>();

            _logger.LogInfo("Component delegate cache initialized", "Entity");
        }

        /// <summary>
        /// Кешує делегати для роботи з конкретним типом компонента
        /// </summary>
        private void CacheComponentDelegates<T>() where T : struct, IComponent
        {
            var type = typeof(T);

            // Створюємо делегат для GetComponent
            _getComponentDelegates[type] = (entityId) => _entityManager.GetComponent<T>(entityId);

            // Створюємо делегат для AddComponent
            _addComponentDelegates[type] = (entityId, component) => _entityManager.AddComponent(entityId, (T)component);

            // Створюємо делегат для HasComponent
            _hasComponentDelegates[type] = (entityId) => _entityManager.HasComponent<T>(entityId);

            _logger.LogDebug($"Cached delegates for component type {type.Name}", "Entity");
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

            // Тут можна додати інші стандартні архетипи

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
        /// Створює сутність за шаблоном архетипу (оптимізована версія)
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

                // Використовуємо кешований делегат для додавання компонента
                if (_addComponentDelegates.TryGetValue(componentType, out var addDelegate))
                {
                    addDelegate(entityId, component);
                }
                else
                {
                    // Якщо делегат не знайдено, створюємо його на льоту та кешуємо
                    var methodInfo = typeof(IEntityManager).GetMethod("AddComponent").MakeGenericMethod(componentType);
                    var newDelegate = (Action<int, object>)((id, comp) =>
                        methodInfo.Invoke(_entityManager, new[] { id, comp }));

                    _addComponentDelegates[componentType] = newDelegate;
                    newDelegate(entityId, component);

                    _logger.LogTrace($"Created and cached AddComponent delegate for {componentType.Name}", "Entity");
                }
            }

            _logger.LogInfo($"Created entity {entityId} from template '{archetypeId}'", "Entity");
            return entityId;
        }

        /// <summary>
        /// Перевіряє, чи відповідає сутність шаблону архетипу (оптимізована версія)
        /// </summary>
        public bool MatchesTemplate(int entityId, string archetypeId)
        {
            if (!_templates.TryGetValue(archetypeId, out var template))
            {
                return false;
            }

            // Якщо у шаблоні є кешовані функції перевірки, використовуємо їх
            if (template.ComponentCheckers.Count > 0)
            {
                foreach (var checker in template.ComponentCheckers)
                {
                    if (!checker(entityId, _entityManager))
                    {
                        return false;
                    }
                }
                return true;
            }

            // Інакше перевіряємо за типами компонентів
            foreach (var componentType in template.DefaultComponents.Keys)
            {
                // Використовуємо кешований делегат для перевірки наявності компонента
                if (_hasComponentDelegates.TryGetValue(componentType, out var hasDelegate))
                {
                    if (!hasDelegate(entityId))
                    {
                        return false;
                    }
                }
                else
                {
                    // Якщо делегат не знайдено, створюємо його на льоту та кешуємо
                    var methodInfo = typeof(IEntityManager).GetMethod("HasComponent").MakeGenericMethod(componentType);
                    var newDelegate = (Func<int, bool>)((id) =>
                        (bool)methodInfo.Invoke(_entityManager, new object[] { id }));

                    _hasComponentDelegates[componentType] = newDelegate;

                    if (!newDelegate(entityId))
                    {
                        return false;
                    }

                    _logger.LogTrace($"Created and cached HasComponent delegate for {componentType.Name}", "Entity");
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
        /// Додає функцію перевірки компонента до шаблону
        /// </summary>
        public void AddComponentChecker<T>(string archetypeId, Func<T, bool> predicate) where T : struct, IComponent
        {
            if (!_templates.TryGetValue(archetypeId, out var template))
            {
                _logger.LogWarning($"Cannot add component checker: template '{archetypeId}' not found", "Entity");
                return;
            }

            // Створюємо функцію перевірки, яка використовує предикат
            Func<int, IEntityManager, bool> checker = (entityId, entityManager) =>
            {
                if (!entityManager.HasComponent<T>(entityId))
                    return false;

                var component = entityManager.GetComponent<T>(entityId);
                return predicate(component);
            };

            template.ComponentCheckers.Add(checker);
            _logger.LogDebug($"Added component checker for {typeof(T).Name} to template '{archetypeId}'", "Entity");
        }

        /// <summary>
        /// Добавляє компонент у шаблон
        /// </summary>
        public void AddComponentToTemplate<T>(string archetypeId, T component) where T : struct, IComponent
        {
            if (!_templates.TryGetValue(archetypeId, out var template))
            {
                _logger.LogWarning($"Cannot add component: template '{archetypeId}' not found", "Entity");
                return;
            }

            template.DefaultComponents[typeof(T)] = component;

            // Додаємо функцію перевірки для цього компонента
            Func<int, IEntityManager, bool> checker = (entityId, entityManager) =>
                entityManager.HasComponent<T>(entityId);

            template.ComponentCheckers.Add(checker);

            _logger.LogDebug($"Added component {typeof(T).Name} to template '{archetypeId}'", "Entity");
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

            // Додаємо функцію перевірки для цього компонента
            _registry.AddComponentToTemplate(_template.ArchetypeId, component);

            return this;
        }

        /// <summary>
        /// Додає функцію перевірки компонента
        /// </summary>
        public ArchetypeTemplateBuilder WithComponentCheck<T>(Func<T, bool> predicate) where T : struct, IComponent
        {
            _registry.AddComponentChecker(_template.ArchetypeId, predicate);
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
