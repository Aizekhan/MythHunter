// Шлях: Assets/_MythHunter/Code/Systems/Core/SystemRegistryExtensions.cs

using System;
using System.Collections.Generic;
using System.Linq;
using MythHunter.Core.ECS;
using MythHunter.Systems.Core;
using MythHunter.Systems.Groups;
using MythHunter.Systems.Phase;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.Extensions
{
    /// <summary>
    /// Розширення для SystemRegistry для роботи з фазами
    /// </summary>
    public static class SystemRegistryExtensions
    {
        /// <summary>
        /// Реєструє систему, яка активна лише у вказаних фазах
        /// </summary>
        public static void RegisterPhaseSystem(this ISystemRegistry registry, ISystem system, params string[] activePhaseIds)
        {
            if (system is IPhaseFilteredSystem phaseSystem)
            {
                phaseSystem.SetActivePhaseIds(activePhaseIds);
            }

            registry.RegisterSystem(system);
        }

        /// <summary>
        /// Реєструє систему для роботи зі старим типом GamePhase (для сумісності)
        /// </summary>
        public static void RegisterPhaseSystem(this ISystemRegistry registry, ISystem system, params Events.Domain.GamePhase[] activePhases)
        {
            // Конвертуємо GamePhase в string IDs
            string[] phaseIds = activePhases.Select(p => p.ToString()).ToArray();
            RegisterPhaseSystem(registry, system, phaseIds);
        }

        /// <summary>
        /// Реєструє систему з пріоритетом оновлення
        /// </summary>
        public static void RegisterSystemWithPriority(this ISystemRegistry registry, ISystem system, int priority)
        {
            if (registry is SystemRegistry systemRegistry)
            {
                systemRegistry.RegisterSystemWithPriority(system, priority);
            }
            else
            {
                registry.RegisterSystem(system);
            }
        }

        /// <summary>
        /// Реєструє групу систем для певних фаз, використовуючи строкові ідентифікатори
        /// </summary>
        public static SystemGroup RegisterPhaseSystemGroup(
            this ISystemRegistry registry,
            string groupName,
            int groupPriority,
            IMythLogger logger,
            IPhaseProvider phaseProvider,
            params string[] activePhaseIds)
        {
            // Перевіряємо, чи реєстр підтримує перевірку на існуючі групи
            if (registry is SystemRegistry systemRegistry)
            {
                // Перевіряємо, чи група з такою назвою вже існує
                var existingGroup = systemRegistry.GetAllSystems()
                    .FirstOrDefault(s => s is SystemGroup group && group.GroupName == groupName) as SystemGroup;

                if (existingGroup != null)
                {
                    systemRegistry.LogInfo($"System group '{groupName}' is already registered, skipping");
                    return existingGroup;
                }
            }

            // Створення групи систем з підтримкою фаз
            var systemGroup = new PhaseSystemGroup(groupName, groupPriority, logger, activePhaseIds, phaseProvider);

            // Реєстрація групи з пріоритетом
            registry.RegisterSystemWithPriority(systemGroup, groupPriority);

            // Логування
            if (registry is SystemRegistry sr)
            {
                // Перетворюємо масив фаз в рядок для логування
                string phasesString = string.Join(", ", activePhaseIds);
                sr.LogInfo($"Registered system group '{groupName}' with priority {groupPriority} for phases: {phasesString}");
            }

            return systemGroup;
        }

        /// <summary>
        /// Реєструє групу систем для певних фаз, використовуючи старий тип GamePhase (для сумісності)
        /// </summary>
        public static SystemGroup RegisterPhaseSystemGroup(
            this ISystemRegistry registry,
            string groupName,
            int groupPriority,
            IMythLogger logger,
            IPhaseProvider phaseProvider,
            params Events.Domain.GamePhase[] activePhases)
        {
            // Конвертуємо GamePhase в string IDs
            string[] phaseIds = activePhases.Select(p => p.ToString()).ToArray();
            return RegisterPhaseSystemGroup(registry, groupName, groupPriority, logger, phaseProvider, phaseIds);
        }

        /// <summary>
        /// Метод для сумісності зі старим кодом
        /// </summary>
        public static SystemGroup RegisterPhaseSystemGroup(
            this ISystemRegistry registry,
            string groupName,
            int groupPriority,
            IMythLogger logger,
            params Events.Domain.GamePhase[] activePhases)
        {
            // Отримуємо IPhaseProvider через reflection (це не найкращий підхід, але для сумісності)
            IPhaseProvider phaseProvider = null;

            // Спроба знайти IPhaseProvider через реєстр систем (якщо він там зареєстрований)
            if (registry is SystemRegistry systemRegistry)
            {
                var systems = systemRegistry.GetAllSystems();
                foreach (var system in systems)
                {
                    if (system is IPhaseProvider provider)
                    {
                        phaseProvider = provider;
                        break;
                    }
                }
            }

            // Якщо не знайдено, використовуємо заглушку
            if (phaseProvider == null)
            {
                logger.LogWarning($"IPhaseProvider not found when registering group '{groupName}'. Using fallback implementation.", "System");
                phaseProvider = new FallbackPhaseProvider(logger);
            }

            // Конвертуємо GamePhase в string IDs
            string[] phaseIds = activePhases.Select(p => p.ToString()).ToArray();
            return RegisterPhaseSystemGroup(registry, groupName, groupPriority, logger, phaseProvider, phaseIds);
        }
    }

    /// <summary>
    /// Заглушка для IPhaseProvider, яка використовується, коли справжній провайдер недоступний
    /// </summary>
    internal class FallbackPhaseProvider : IPhaseProvider
    {
        private readonly IMythLogger _logger;
        private string _currentPhaseId = "None";
        private readonly List<Action<string, string>> _callbacks = new List<Action<string, string>>();

        public FallbackPhaseProvider(IMythLogger logger)
        {
            _logger = logger;
            _logger.LogWarning("Using FallbackPhaseProvider - this is not intended for production use", "Phase");
        }

        public string GetCurrentPhaseId() => _currentPhaseId;

        public bool IsCurrentPhase(string phaseId) => _currentPhaseId == phaseId;

        public void SubscribeToPhaseChange(Action<string, string> onPhaseChanged)
        {
            if (!_callbacks.Contains(onPhaseChanged))
                _callbacks.Add(onPhaseChanged);
        }

        public void UnsubscribeFromPhaseChange(Action<string, string> onPhaseChanged)
        {
            _callbacks.Remove(onPhaseChanged);
        }

        public string[] GetAllPhaseIds()
        {
            // Повертаємо базові фази
            return new[] { "None", "Rune", "Planning", "Movement", "Combat", "Freeze" };
        }
    }
}
