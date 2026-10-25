// Шлях: Assets/_MythHunter/Code/Systems/Core/SystemRegistryExtensions.cs

using System;
using System.Collections.Generic;
using System.Linq;
using MythHunter.Core.ECS;
using RPGFramework.DI;

using MythHunter.Systems.Groups;
using MythHunter.Systems.Phase;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.Core
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
            var existingGroup = registry.GetAllSystems()
                .OfType<SystemGroup>()
                .FirstOrDefault(group => group.GroupName == groupName);

            if (existingGroup != null)
            {
                registry.LogInfo($"System group '{groupName}' is already registered, skipping");
                return existingGroup;
            }

            // Створення групи систем з підтримкою фаз
            var phaseGroup = new PhaseSystemGroup(groupName, groupPriority, logger, activePhaseIds, phaseProvider);

            string phasesString = string.Join(", ", activePhaseIds);
            registry.LogInfo($"Created phase system group '{groupName}' for phases: {phasesString}");

            // Реєстрація групи з пріоритетом
            registry.RegisterSystemWithPriority(phaseGroup, groupPriority);

            return phaseGroup;
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
        /// Compatibility overload for callers that already registered an IPhaseProvider as a system.
        /// Prefer the overload that receives the provider explicitly.
        /// </summary>
        [Obsolete("Pass an IPhaseProvider explicitly when registering a phase system group.")]
        public static SystemGroup RegisterPhaseSystemGroup(
            this ISystemRegistry registry,
            string groupName,
            int groupPriority,
            IMythLogger logger,
            params Events.Domain.GamePhase[] activePhases)
        {
            var phaseProvider = registry.GetAllSystems().OfType<IPhaseProvider>().FirstOrDefault();
            if (phaseProvider == null)
            {
                const string message =
                    "No IPhaseProvider is registered. Pass an IPhaseProvider explicitly when registering a phase system group.";
                logger.LogError(message, "System");
                throw new InvalidOperationException(message);
            }

            return RegisterPhaseSystemGroup(
                registry,
                groupName,
                groupPriority,
                logger,
                phaseProvider,
                activePhases);
        }

        /// <summary>
        /// Registers a system group with the specified lifecycle category.
        /// Use the overload that accepts IDIContainer when child system types must be resolved.
        /// </summary>
        /// <typeparam name="TGroup">System group type.</typeparam>
        /// <param name="registry">System registry.</param>
        /// <param name="groupName">Group name.</param>
        /// <param name="priority">System update/disposal priority.</param>
        /// <param name="category">Requested initialization category.</param>
        /// <param name="logger">Logger.</param>
        /// <returns>The registered system group.</returns>
        public static TGroup RegisterGroupWithCategory<TGroup>(
            this ISystemRegistry registry,
            string groupName,
            int priority,
            SystemInitializationCategory category,
            IMythLogger logger)
            where TGroup : SystemGroup
        {
            return RegisterGroupWithCategory<TGroup>(
                registry,
                groupName,
                priority,
                category,
                logger,
                null,
                null);
        }

        /// <summary>
        /// Registers a system group and optionally resolves children from an explicitly supplied container.
        /// </summary>
        /// <param name="container">The container used to resolve child systems.</param>
        /// <param name="systems">Child system types to resolve and add.</param>
        public static TGroup RegisterGroupWithCategory<TGroup>(
            this ISystemRegistry registry,
            string groupName,
            int priority,
            SystemInitializationCategory category,
            IMythLogger logger,
            IDIContainer container,
            params Type[] systems)
            where TGroup : SystemGroup
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (systems != null && systems.Length > 0 && container == null)
                throw new ArgumentNullException(nameof(container),
                    "Pass the DI container explicitly when resolving child systems.");

            var group = (TGroup)Activator.CreateInstance(
                typeof(TGroup),
                new object[] { groupName, logger });

            registry.RegisterSystemWithPriority(group, priority, category);

            if (systems != null && systems.Length > 0)
            {
                foreach (var systemType in systems)
                {
                    try
                    {
                        if (container.IsRegistered(systemType))
                        {
                            var system = container.Resolve(systemType) as ISystem;
                            if (system != null)
                            {
                                group.AddSystem(system);
                                logger.LogInfo($"Added system {systemType.Name} to group {groupName}", "SystemRegistry");
                            }
                            else
                            {
                                logger.LogWarning($"System {systemType.Name} is not an ISystem", "SystemRegistry");
                            }
                        }
                        else
                        {
                            logger.LogWarning($"System {systemType.Name} is not registered in container", "SystemRegistry");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Error adding system {systemType.Name} to group: {ex.Message}", "SystemRegistry", ex);
                    }
                }
            }

            logger.LogInfo($"Registered system group {groupName} with category {category}", "SystemRegistry");
            return group;
        }

        /// <summary>
        /// Реєструє групу систем з вказаною категорією ініціалізації та списком інтерфейсів систем
        /// </summary>
        public static TGroup RegisterGroupWithCategory<TGroup, TSystem1>(
            this ISystemRegistry registry,
            string groupName,
            int priority,
            SystemInitializationCategory category,
            IMythLogger logger,
            IDIContainer container)
            where TGroup : SystemGroup
            where TSystem1 : ISystem
        {
            return registry.RegisterGroupWithCategory<TGroup>(
                groupName,
                priority,
                category,
                logger,
                container,
                new Type[] { typeof(TSystem1) });
        }

        /// <summary>
        /// Реєструє групу систем з вказаною категорією ініціалізації та списком двох інтерфейсів систем
        /// </summary>
        public static TGroup RegisterGroupWithCategory<TGroup, TSystem1, TSystem2>(
            this ISystemRegistry registry,
            string groupName,
            int priority,
            SystemInitializationCategory category,
            IMythLogger logger,
            IDIContainer container)
            where TGroup : SystemGroup
            where TSystem1 : ISystem
            where TSystem2 : ISystem
        {
            return registry.RegisterGroupWithCategory<TGroup>(
                groupName,
                priority,
                category,
                logger,
                container,
                new Type[] { typeof(TSystem1), typeof(TSystem2) });
        }

        /// <summary>
        /// Реєструє групу систем з вказаною категорією ініціалізації та списком трьох інтерфейсів систем
        /// </summary>
        public static TGroup RegisterGroupWithCategory<TGroup, TSystem1, TSystem2, TSystem3>(
            this ISystemRegistry registry,
            string groupName,
            int priority,
            SystemInitializationCategory category,
            IMythLogger logger,
            IDIContainer container)
            where TGroup : SystemGroup
            where TSystem1 : ISystem
            where TSystem2 : ISystem
            where TSystem3 : ISystem
        {
            return registry.RegisterGroupWithCategory<TGroup>(
                groupName,
                priority,
                category,
                logger,
                container,
                new Type[] { typeof(TSystem1), typeof(TSystem2), typeof(TSystem3) });
        }
    }

    
}
