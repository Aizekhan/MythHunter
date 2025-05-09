// Файл: Assets/_MythHunter/Code/Systems/Core/SystemRegistry.cs
using System.Collections.Generic;
using System.Linq;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
using MythHunter.Systems.Groups;

namespace MythHunter.Systems.Core
{
    
    /// <summary>
    /// Реєстр систем з підтримкою фаз та пріоритетів
    /// </summary>
    public class SystemRegistry : ISystemRegistry, IEventSubscriber
    {
        private readonly List<SystemRegistration> _allSystems = new List<SystemRegistration>();
        private readonly IMythLogger _logger;
        protected IMythLogger Logger => _logger;
        private readonly IEventBus _eventBus;

        private GamePhase _currentPhase = GamePhase.None;
        private bool _isSubscribed = false;
        // Додайте в базовий клас SystemRegistry
        protected GamePhase CurrentPhase { get; private set; } = GamePhase.None;
      
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
            public string SystemType
            {
                get; set;
            } // ← Ось ця властивість потрібна
        }

        [Inject]
        public SystemRegistry(IMythLogger logger, IEventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;

            // Підписуємося на події зміни фази
            SubscribeToEvents();
        }

        public virtual void RegisterSystem(ISystem system)
        {
            RegisterSystemWithPriority(system, 0);
        }

        public void RegisterSystemWithPriority(ISystem system, int priority)
        {
            var systemType = system.GetType();
            var systemName = systemType.Name;

            // Перевірка на повторну реєстрацію за типом
            if (_allSystems.Any(r => r.System.GetType() == systemType && !(system is SystemGroup)))
            {
                _logger.LogWarning($"System {systemName} is already registered, skipping", "Systems");
                return;
            }

            // Додаткова перевірка на групи систем за їх назвою
            if (system is SystemGroup systemGroup)
            {
                if (_allSystems.Any(r => r.System is SystemGroup existingGroup &&
                                       existingGroup.GroupName == systemGroup.GroupName))
                {
                    _logger.LogWarning($"System group '{systemGroup.GroupName}' is already registered, skipping", "Systems");
                    return;
                }

                // Додаємо систему до реєстру
                _allSystems.Add(new SystemRegistration
                {
                    System = system,
                    Priority = priority,
                    SystemType = systemGroup.GroupName
                });
            }
            else
            {
                // Додаємо систему до реєстру
                _allSystems.Add(new SystemRegistration
                {
                    System = system,
                    Priority = priority,
                    SystemType = systemName
                });
            }

            // Сортуємо системи за пріоритетом
            _allSystems.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            _logger.LogInfo($"Registered system: {(system is SystemGroup sg ? sg.GroupName : systemName)} with priority {priority}", "Systems");
        }
        public void InitializeAll()
        {
            foreach (var reg in _allSystems)
            {
                reg.System.Initialize();
            }
        }

        public virtual void UpdateAll(float deltaTime)
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

        // І змінити метод обробки події фази
        private void OnPhaseChanged(PhaseChangedEvent evt)
        {
            CurrentPhase = evt.CurrentPhase;
            _logger.LogDebug($"System Registry phase changed to {CurrentPhase}", "Systems");
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

        /// <summary>
        /// Записує інформаційне повідомлення в лог
        /// </summary>
        public void LogInfo(string message)
        {
            _logger?.LogInfo(message, "Systems");
        }
        /// <summary>
        /// Перевіряє, чи система активна в поточній фазі
        /// </summary>
        private bool IsSystemActiveInCurrentPhase(ISystem system)
        {
            if (system is IPhaseFilteredSystem phaseSystem)
            {
                var currentPhase = GetCurrentPhase();
                return phaseSystem.IsActiveInPhase(currentPhase);
            }

            return true;
        }
        /// <summary>
        /// Отримує поточну фазу з батьківського класу
        /// </summary>
        private MythHunter.Events.Domain.GamePhase GetCurrentPhase()
        {
            // Отримуємо поточну фазу через публічний інтерфейс або додаткове поле
            // Це одне з можливих рішень
            var phaseSystem = FindPhaseSystem();
            return phaseSystem?.CurrentPhase ?? MythHunter.Events.Domain.GamePhase.None;
        }

        /// <summary>
        /// Знаходить систему фаз у списку зареєстрованих систем
        /// </summary>
        private MythHunter.Systems.Phase.IPhaseSystem FindPhaseSystem()
        {
            var allSystems = GetAllSystems();
            foreach (var system in allSystems)
            {
                if (system is MythHunter.Systems.Phase.IPhaseSystem phaseSystem)
                {
                    return phaseSystem;
                }
            }
            return null;
        }
    }
}
