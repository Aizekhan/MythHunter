// Шлях: Assets/_MythHunter/Code/Systems/Groups/SystemGroup.cs

using System.Collections.Generic;
using MythHunter.Core.ECS;
using MythHunter.Systems.Core;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.Groups
{
    /// <summary>
    /// Група систем - логічний контейнер для групування систем
    /// </summary>
    public class SystemGroup : ISystem
    {
        protected readonly List<ISystem> _systems = new List<ISystem>();
        protected readonly IMythLogger _logger;

        public string GroupName
        {
            get;
        }

        public SystemGroup(string name, IMythLogger logger)
        {
            GroupName = name;
            _logger = logger;
        }

        public virtual void Initialize()
        {
            foreach (var system in _systems)
            {
                system.Initialize();
            }
            _logger.LogDebug($"Initialized system group '{GroupName}'", "System");
        }

        public virtual void Update(float deltaTime)
        {
            foreach (var system in _systems)
            {
                system.Update(deltaTime);
            }
        }

        public virtual void Dispose()
        {
            foreach (var system in _systems)
            {
                system.Dispose();
            }
            _logger.LogDebug($"Disposed system group '{GroupName}'", "System");
        }

        /// <summary>
        /// Додає систему до групи
        /// </summary>
        public void AddSystem(ISystem system)
        {
            if (system != null && !_systems.Contains(system))
            {
                _systems.Add(system);
                _logger.LogDebug($"Added system {system.GetType().Name} to group '{GroupName}'", "System");
            }
        }

        /// <summary>
        /// Видаляє систему з групи
        /// </summary>
        public void RemoveSystem(ISystem system)
        {
            if (system != null && _systems.Contains(system))
            {
                _systems.Remove(system);
                _logger.LogDebug($"Removed system {system.GetType().Name} from group '{GroupName}'", "System");
            }
        }

        /// <summary>
        /// Отримує всі системи в групі
        /// </summary>
        public IReadOnlyList<ISystem> GetSystems()
        {
            return _systems.AsReadOnly();
        }
    }
}
