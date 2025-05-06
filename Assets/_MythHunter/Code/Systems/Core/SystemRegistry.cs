// Файл: Assets/_MythHunter/Code/Systems/Core/SystemRegistry.cs
using System.Collections.Generic;
using System.Linq;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Реєстр систем з підтримкою фаз та пріоритетів
    /// </summary>
    public class SystemRegistry : ISystemRegistry, IEventSubscriber
    {
        private readonly List<SystemRegistration> _allSystems = new List<SystemRegistration>();
        private readonly IMythLogger _logger;
        private readonly IEventBus _eventBus;

        private GamePhase _currentPhase = GamePhase.None;
        private bool _isSubscribed = false;

        /// <summary>
        /// Клас для реєстрації системи з додатковими даними
        /// </summary>
        private class SystemRegistration
        {
            public ISystem System
            {
                get; set;
            }
            public int Priority
            {
                get; set;
            }
            public bool IsActive { get; set; } = true;
        }

        [Inject]
        public SystemRegistry(IMythLogger logger, IEventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;

            // Підписуємося на події зміни фази
            SubscribeToEvents();
        }

        public void RegisterSystem(ISystem system)
        {
            RegisterSystemWithPriority(system, 0);
        }

        public void RegisterSystemWithPriority(ISystem system, int priority)
        {
            _allSystems.Add(new SystemRegistration
            {
                System = system,
                Priority = priority
            });

            // Сортуємо системи за пріоритетом
            _allSystems.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            _logger.LogInfo($"Registered system: {system.GetType().Name} with priority {priority}", "Systems");
        }

        public void InitializeAll()
        {
            foreach (var reg in _allSystems)
            {
                reg.System.Initialize();
            }
        }

        public void UpdateAll(float deltaTime)
        {
            foreach (var reg in _allSystems.Where(r => r.IsActive))
            {
                // Перевіряємо, чи система активна у поточній фазі
                if (reg.System is IPhaseFilteredSystem phaseSystem && !phaseSystem.IsActiveInPhase(_currentPhase))
                    continue;

                // Оновлюємо систему
                reg.System.Update(deltaTime);
            }
        }

        public void FixedUpdateAll(float fixedDeltaTime)
        {
            foreach (var reg in _allSystems.Where(r => r.IsActive))
            {
                // Перевіряємо, чи система активна у поточній фазі
                if (reg.System is IPhaseFilteredSystem phaseSystem && !phaseSystem.IsActiveInPhase(_currentPhase))
                    continue;

                // Оновлюємо систему, якщо вона підтримує FixedUpdate
                if (reg.System is IFixedUpdateSystem fixedSystem)
                {
                    fixedSystem.FixedUpdate(fixedDeltaTime);
                }
            }
        }

        public void LateUpdateAll(float deltaTime)
        {
            foreach (var reg in _allSystems.Where(r => r.IsActive))
            {
                // Перевіряємо, чи система активна у поточній фазі
                if (reg.System is IPhaseFilteredSystem phaseSystem && !phaseSystem.IsActiveInPhase(_currentPhase))
                    continue;

                // Оновлюємо систему, якщо вона підтримує LateUpdate
                if (reg.System is ILateUpdateSystem lateSystem)
                {
                    lateSystem.LateUpdate(deltaTime);
                }
            }
        }

        public void DisposeAll()
        {
            foreach (var reg in _allSystems)
            {
                reg.System.Dispose();
            }

            _allSystems.Clear();
            UnsubscribeFromEvents();
        }

        public void SubscribeToEvents()
        {
            if (!_isSubscribed)
            {
                _eventBus.Subscribe<PhaseChangedEvent>(OnPhaseChanged);
                _isSubscribed = true;
            }
        }

        public void UnsubscribeFromEvents()
        {
            if (_isSubscribed)
            {
                _eventBus.Unsubscribe<PhaseChangedEvent>(OnPhaseChanged);
                _isSubscribed = false;
            }
        }

        private void OnPhaseChanged(PhaseChangedEvent evt)
        {
            _currentPhase = evt.CurrentPhase;
            _logger.LogDebug($"System Registry phase changed to {_currentPhase}", "Systems");
        }

        /// <summary>
        /// Активує або деактивує систему
        /// </summary>
        public void SetSystemActive(ISystem system, bool isActive)
        {
            var reg = _allSystems.FirstOrDefault(r => r.System == system);
            if (reg != null)
            {
                reg.IsActive = isActive;
                _logger.LogInfo($"System {system.GetType().Name} {(isActive ? "activated" : "deactivated")}", "Systems");
            }
        }

        /// <summary>
        /// Отримує всі зареєстровані системи
        /// </summary>
        public IReadOnlyList<ISystem> GetAllSystems()
        {
            return _allSystems.Select(r => r.System).ToList();
        }
    }
}
