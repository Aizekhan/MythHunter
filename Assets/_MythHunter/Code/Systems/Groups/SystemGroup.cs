// Шлях: Assets/_MythHunter/Code/Systems/Groups/SystemGroup.cs

using System.Collections.Generic;
using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;
using MythHunter.Events.Domain;
using MythHunter.Systems.Core;

namespace MythHunter.Systems.Groups
{
    /// <summary>
    /// Група систем для послідовного виконання з підтримкою нової ієрархії
    /// </summary>
    public class SystemGroup : ISystem, IUpdateSystem, IFixedUpdateSystem, ILateUpdateSystem, IEventSystem, IPhaseFilteredSystem
    {
        private readonly List<ISystem> _systems = new List<ISystem>();
        private readonly List<IUpdateSystem> _updateSystems = new List<IUpdateSystem>();
        private readonly List<IFixedUpdateSystem> _fixedUpdateSystems = new List<IFixedUpdateSystem>();
        private readonly List<ILateUpdateSystem> _lateUpdateSystems = new List<ILateUpdateSystem>();
        private readonly List<IEventSystem> _eventSystems = new List<IEventSystem>();
        private readonly IMythLogger _logger;

        // Додаємо поле для активних фаз
        private HashSet<GamePhase> _activePhases = new HashSet<GamePhase>();

        public SystemGroup(IMythLogger logger)
        {
            _logger = logger;
        }

        public void AddSystem(ISystem system)
        {
            _systems.Add(system);

            if (system is IUpdateSystem updateSystem)
                _updateSystems.Add(updateSystem);

            if (system is IFixedUpdateSystem fixedUpdateSystem)
                _fixedUpdateSystems.Add(fixedUpdateSystem);

            if (system is ILateUpdateSystem lateUpdateSystem)
                _lateUpdateSystems.Add(lateUpdateSystem);

            if (system is IEventSystem eventSystem)
                _eventSystems.Add(eventSystem);

            _logger.LogInfo($"System {system.GetType().Name} added to group", "Systems");
        }

        public void Initialize()
        {
            foreach (var system in _systems)
            {
                system.Initialize();
            }
        }

        public void Update(float deltaTime)
        {
            foreach (var system in _updateSystems)
            {
                system.Update(deltaTime);
            }
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            foreach (var system in _fixedUpdateSystems)
            {
                system.FixedUpdate(fixedDeltaTime);
            }
        }

        public void LateUpdate(float deltaTime)
        {
            foreach (var system in _lateUpdateSystems)
            {
                system.LateUpdate(deltaTime);
            }
        }

        public void SubscribeToEvents()
        {
            foreach (var system in _eventSystems)
            {
                system.SubscribeToEvents();
            }
        }

        public void UnsubscribeFromEvents()
        {
            foreach (var system in _eventSystems)
            {
                system.UnsubscribeFromEvents();
            }
        }

        public void Dispose()
        {
            foreach (var system in _systems)
            {
                system.Dispose();
            }

            _systems.Clear();
            _updateSystems.Clear();
            _fixedUpdateSystems.Clear();
            _lateUpdateSystems.Clear();
            _eventSystems.Clear();
        }

        /// <summary>
        /// Реалізація IPhaseFilteredSystem.SetActivePhases
        /// </summary>
        public void SetActivePhases(GamePhase[] phases)
        {
            _activePhases.Clear();
            foreach (var phase in phases)
            {
                _activePhases.Add(phase);
            }

            _logger.LogInfo($"Set active phases for group: {string.Join(", ", phases)}", "Systems");
        }

        /// <summary>
        /// Реалізація IPhaseFilteredSystem.IsActiveInPhase
        /// </summary>
        public bool IsActiveInPhase(GamePhase currentPhase)
        {
            // Якщо активних фаз не вказано, система активна завжди
            if (_activePhases.Count == 0)
                return true;

            return _activePhases.Contains(currentPhase);
        }
    }
}
