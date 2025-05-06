// Файл: Assets/_MythHunter/Code/Systems/Core/SystemRegistryExtensions.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.ECS;
using MythHunter.Events.Domain;
using MythHunter.Systems.Core;

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
    }
}
