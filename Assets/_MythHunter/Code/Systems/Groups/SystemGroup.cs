// Шлях: Assets/_MythHunter/Code/Systems/Groups/SystemGroup.cs

using System;
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
        private readonly HashSet<ISystem> _initializedSystems = new HashSet<ISystem>();

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
                if (_initializedSystems.Contains(system))
                    continue;

                try
                {
                    system.Initialize();
                    _initializedSystems.Add(system);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        $"Error initializing system {system.GetType().Name} in group '{GroupName}': {ex.Message}",
                        "System",
                        ex);
                }
            }

            _logger.LogDebug($"Initialized system group '{GroupName}'", "System");
        }

        public virtual void Update(float deltaTime)
        {
            foreach (var system in _systems)
            {
                if (!_initializedSystems.Contains(system))
                    continue;

                try
                {
                    system.Update(deltaTime);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        $"Error updating system {system.GetType().Name} in group '{GroupName}': {ex.Message}",
                        "System",
                        ex);
                }
            }
        }

        public virtual void Dispose()
        {
            // Dispose in reverse order and continue even when a child fails to clean up.
            for (var i = _systems.Count - 1; i >= 0; i--)
            {
                var system = _systems[i];
                try
                {
                    system.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        $"Error disposing system {system.GetType().Name} in group '{GroupName}': {ex.Message}",
                        "System",
                        ex);
                }
            }

            _initializedSystems.Clear();
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
                _initializedSystems.Remove(system);
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
