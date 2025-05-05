using System.Collections.Generic;
using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Реєстр систем з підтримкою всіх типів систем
    /// </summary>
    public class SystemRegistry : ISystemRegistry
    {
        private readonly List<ISystem> _allSystems = new List<ISystem>();
        private readonly List<IUpdateSystem> _updateSystems = new List<IUpdateSystem>();
        private readonly List<IFixedUpdateSystem> _fixedUpdateSystems = new List<IFixedUpdateSystem>();
        private readonly List<ILateUpdateSystem> _lateUpdateSystems = new List<ILateUpdateSystem>();
        private readonly List<IEventSystem> _eventSystems = new List<IEventSystem>();
        private readonly IMythLogger _logger;

        public SystemRegistry(IMythLogger logger)
        {
            _logger = logger;
        }
        public SystemRegistry()
        {
            // Пустий конструктор для випадків, коли логер ще недоступний
        }
        public void RegisterSystem(ISystem system)
        {
            _allSystems.Add(system);

            if (system is IUpdateSystem updateSystem)
                _updateSystems.Add(updateSystem);

            if (system is IFixedUpdateSystem fixedUpdateSystem)
                _fixedUpdateSystems.Add(fixedUpdateSystem);

            if (system is ILateUpdateSystem lateUpdateSystem)
                _lateUpdateSystems.Add(lateUpdateSystem);

            if (system is IEventSystem eventSystem)
                _eventSystems.Add(eventSystem);

            _logger.LogInfo($"Registered system: {system.GetType().Name}", "Systems");
        }

        public void InitializeAll()
        {
            foreach (var system in _allSystems)
            {
                system.Initialize();
            }
        }

        public void UpdateAll(float deltaTime)
        {
            foreach (var system in _updateSystems)
            {
                system.Update(deltaTime);
            }
        }

        public void FixedUpdateAll(float fixedDeltaTime)
        {
            foreach (var system in _fixedUpdateSystems)
            {
                system.FixedUpdate(fixedDeltaTime);
            }
        }

        public void LateUpdateAll(float deltaTime)
        {
            foreach (var system in _lateUpdateSystems)
            {
                system.LateUpdate(deltaTime);
            }
        }

        public void DisposeAll()
        {
            foreach (var system in _allSystems)
            {
                system.Dispose();
            }

            _allSystems.Clear();
            _updateSystems.Clear();
            _fixedUpdateSystems.Clear();
            _lateUpdateSystems.Clear();
            _eventSystems.Clear();
        }
    }
}
