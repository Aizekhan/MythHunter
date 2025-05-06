// Шлях: Assets/_MythHunter/Code/Systems/Core/SystemRegistryExtensions.cs

using System;
using System.Collections.Generic;
using MythHunter.Core.ECS;
using MythHunter.Events.Domain;
using MythHunter.Systems.Core;
using MythHunter.Systems.Groups;
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
        public static void RegisterPhaseSystem(this ISystemRegistry registry, ISystem system, params GamePhase[] activePhases)
        {
            if (system is IPhaseFilteredSystem phaseSystem)
            {
                phaseSystem.SetActivePhases(activePhases);
            }

            registry.RegisterSystem(system);
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
        /// Реєструє групу систем для певних фаз
        /// </summary>
        public static SystemGroup RegisterPhaseSystemGroup(this ISystemRegistry registry,
            string groupName, int groupPriority, IMythLogger logger, params GamePhase[] activePhases)
        {
            // Створення групи систем
            var systemGroup = new SystemGroup(logger);

            // Встановлення активних фаз для групи
            if (activePhases != null && activePhases.Length > 0)
            {
                systemGroup.SetActivePhases(activePhases);
            }

            // Реєстрація групи з пріоритетом
            registry.RegisterSystemWithPriority(systemGroup, groupPriority);

            // Логування
            if (registry is SystemRegistry systemRegistry)
            {
                systemRegistry.LogInfo($"Registered system group '{groupName}' with priority {groupPriority} for phases: {string.Join(", ", activePhases)}");
            }

            return systemGroup;
        }
    }
}
