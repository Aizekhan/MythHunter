// Шлях: Assets/_MythHunter/Code/Core/ECS/ComponentFactory.cs

using System;
using System.Collections.Generic;
using MythHunter.Core.ECS;
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

            // Реєструємо стандартні значення для відомих типів компонентів
            RegisterDefaults();
        }

        /// <summary>
        /// Реєструє стандартні значення для відомих типів компонентів
        /// </summary>
        private void RegisterDefaults()
        {
            // Тут можна додати значення за замовчуванням для всіх компонентів
            // Наприклад:
           

            // Решту типів компонентів можна додати за необхідності
        }

        /// <summary>
        /// Реєструє значення за замовчуванням для типу компонента
        /// </summary>
        public void RegisterDefaultValue<T>(T defaultValue) where T : struct, IComponent
        {
            _defaultValues[typeof(T)] = defaultValue;
            _logger.LogDebug($"Registered default value for component type {typeof(T).Name}", "Component");
        }

        /// <summary>
        /// Отримує значення за замовчуванням для типу компонента
        /// </summary>
        public T GetDefaultValue<T>() where T : struct, IComponent
        {
            if (_defaultValues.TryGetValue(typeof(T), out var defaultValue))
            {
                return (T)defaultValue;
            }

            return default;
        }

        /// <summary>
        /// Створює компонент за замовчуванням
        /// </summary>
        public T CreateComponent<T>() where T : struct, IComponent
        {
            return GetDefaultValue<T>();
        }

        /// <summary>
        /// Створює компонент і додає його до сутності
        /// </summary>
        public void AddComponentToEntity<T>(int entityId) where T : struct, IComponent
        {
            var component = CreateComponent<T>();
            _entityManager.AddComponent(entityId, component);
            _logger.LogDebug($"Added component {typeof(T).Name} to entity {entityId}", "Component");
        }

        /// <summary>
        /// Створює компонент з вказаними параметрами і додає його до сутності
        /// </summary>
        public void AddComponentToEntity<T>(int entityId, Action<T> initializer) where T : struct, IComponent
        {
            var component = CreateComponent<T>();

            // Ініціалізуємо компонент за допомогою ініціалізатора
            initializer(component);

            _entityManager.AddComponent(entityId, component);
            _logger.LogDebug($"Added initialized component {typeof(T).Name} to entity {entityId}", "Component");
        }
    }
}
