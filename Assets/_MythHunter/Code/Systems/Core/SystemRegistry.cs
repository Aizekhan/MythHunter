// Шлях: Assets/_MythHunter/Code/Systems/Core/SystemRegistry.cs
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
        private readonly IEventBus _eventBus;

        // Єдине поле для зберігання поточної фази
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
            public string SystemType
            {
                get; set;
            } // Тип або назва системи
        }

        [Inject]
        public SystemRegistry(IMythLogger logger, IEventBus eventBus)
        {
            _logger = logger;
            _eventBus = eventBus;

            // Підписуємося на події зміни фази
            SubscribeToEvents();
            _logger.LogInfo("SystemRegistry initialized", "Systems");
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

            // Сортуємо системи за пріоритетом (від високого до низького)
            _allSystems.Sort((a, b) => b.Priority.CompareTo(a.Priority));

            _logger.LogInfo($"Registered system: {(system is SystemGroup sg ? sg.GroupName : systemName)} with priority {priority}", "Systems");
        }

        public void InitializeAll()
        {
            foreach (var reg in _allSystems)
            {
                try
                {
                    reg.System.Initialize();
                    _logger.LogDebug($"Initialized system: {reg.SystemType}", "Systems");
                }
                catch (System.Exception ex)
                {
                    _logger.LogError($"Error initializing system {reg.SystemType}: {ex.Message}", "Systems", ex);
                }
            }
            _logger.LogInfo($"Initialized {_allSystems.Count} systems", "Systems");
        }

        public virtual void UpdateAll(float deltaTime)
        {
            foreach (var reg in _allSystems.Where(r => r.IsActive))
            {
                // Перевіряємо, чи система активна у поточній фазі
                if (reg.System is IPhaseFilteredSystem phaseSystem && !phaseSystem.IsActiveInPhase(_currentPhase))
                    continue;

                // Оновлюємо систему
                try
                {
                    reg.System.Update(deltaTime);
                }
                catch (System.Exception ex)
                {
                    _logger.LogError($"Error updating system {reg.SystemType}: {ex.Message}", "Systems", ex);
                }
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
                    try
                    {
                        fixedSystem.FixedUpdate(fixedDeltaTime);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError($"Error fixed-updating system {reg.SystemType}: {ex.Message}", "Systems", ex);
                    }
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
                    try
                    {
                        lateSystem.LateUpdate(deltaTime);
                    }
                    catch (System.Exception ex)
                    {
                        _logger.LogError($"Error late-updating system {reg.SystemType}: {ex.Message}", "Systems", ex);
                    }
                }
            }
        }

        public void DisposeAll()
        {
            foreach (var reg in _allSystems)
            {
                try
                {
                    reg.System.Dispose();
                    _logger.LogDebug($"Disposed system: {reg.SystemType}", "Systems");
                }
                catch (System.Exception ex)
                {
                    _logger.LogError($"Error disposing system {reg.SystemType}: {ex.Message}", "Systems", ex);
                }
            }

            _allSystems.Clear();
            UnsubscribeFromEvents();
            _logger.LogInfo("Disposed all systems", "Systems");
        }

        public void SubscribeToEvents()
        {
            if (!_isSubscribed)
            {
                _eventBus.Subscribe<PhaseChangedEvent>(OnPhaseChanged);
                _isSubscribed = true;
                _logger.LogDebug("SystemRegistry subscribed to events", "Systems");
            }
        }

        public void UnsubscribeFromEvents()
        {
            if (_isSubscribed)
            {
                _eventBus.Unsubscribe<PhaseChangedEvent>(OnPhaseChanged);
                _isSubscribed = false;
                _logger.LogDebug("SystemRegistry unsubscribed from events", "Systems");
            }
        }

        // Обробка події зміни фази
        private void OnPhaseChanged(PhaseChangedEvent evt)
        {
            _currentPhase = evt.CurrentPhase;
            _logger.LogInfo($"SystemRegistry phase changed to {_currentPhase}", "Systems");
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
                _logger.LogInfo($"System {reg.SystemType} {(isActive ? "activated" : "deactivated")}", "Systems");
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
    }
}
