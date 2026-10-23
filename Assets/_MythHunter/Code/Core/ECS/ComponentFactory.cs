// Шлях: Assets/_MythHunter/Code/Core/ECS/ComponentFactory.cs
using System;
using System.Collections.Generic;
using RPGFramework.ECS;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
using MythHunter.Entities;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Фабрика для створення та реєстрації компонентів
    /// </summary>
    public class ComponentFactory : IComponentFactory
    {
        private readonly IEntityManager _entityManager;
        private readonly IMythLogger _logger;
        private readonly Dictionary<Type, object> _defaultValues = new Dictionary<Type, object>();

        [Inject]
        public ComponentFactory(IEntityManager entityManager, IMythLogger logger)
        {
            _entityManager = entityManager;
            _logger = logger;
            RegisterDefaults();
        }

        private void RegisterDefaults()
        {
            // Стандартні значення реєструються за потреби.
        }

        public void RegisterDefaultValue<T>(T defaultValue) where T : struct, IComponent
        {
            _defaultValues[typeof(T)] = defaultValue;
            _logger.LogDebug($"Registered default value for component type {typeof(T).Name}", "Component");
        }

        public T GetDefaultValue<T>() where T : struct, IComponent
        {
            if (_defaultValues.TryGetValue(typeof(T), out var defaultValue))
                return (T)defaultValue;

            return default;
        }

        public T CreateComponent<T>() where T : struct, IComponent => GetDefaultValue<T>();

        public void AddComponentToEntity<T>(int entityId) where T : struct, IComponent
        {
            var component = CreateComponent<T>();
            _entityManager.AddComponent(entityId, component);
            _logger.LogDebug($"Added component {typeof(T).Name} to entity {entityId}", "Component");
        }

        public void AddComponentToEntity<T>(int entityId, Action<T> initializer) where T : struct, IComponent
        {
            var component = CreateComponent<T>();
            initializer(component);
            _entityManager.AddComponent(entityId, component);
            _logger.LogDebug($"Added initialized component {typeof(T).Name} to entity {entityId}", "Component");
        }
    }
}
