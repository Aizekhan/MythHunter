// Шлях: Assets/_MythHunter/Code/Systems/Core/SystemRegistryExtensions.cs

using System;
using System.Collections.Generic;
using System.Linq;
using MythHunter.Core.ECS;
using MythHunter.Events;

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
            var phaseGroup = new PhaseSystemGroup(groupName, groupPriority, logger, activePhaseIds, phaseProvider);

            // Логування
            if (registry is SystemRegistry sr)
            {
                string phasesString = string.Join(", ", activePhaseIds);
                sr.LogInfo($"Created phase system group '{groupName}' for phases: {phasesString}");
            }

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
        /// Метод для сумісності зі старим кодом - знайде IPhaseProvider в реєстрі
        /// </summary>
        public static SystemGroup RegisterPhaseSystemGroup(
            this ISystemRegistry registry,
            string groupName,
            int groupPriority,
            IMythLogger logger,
            params Events.Domain.GamePhase[] activePhases)
        {
            // Спроба знайти IPhaseProvider через реєстр систем
            IPhaseProvider phaseProvider = FindPhaseProvider(registry, logger, groupName);

            // Конвертуємо GamePhase в string IDs
            string[] phaseIds = activePhases.Select(p => p.ToString()).ToArray();
            return RegisterPhaseSystemGroup(registry, groupName, groupPriority, logger, phaseProvider, phaseIds);
        }

        /// <summary>
        /// Знаходить IPhaseProvider в реєстрі систем або створює новий
        /// </summary>
        private static IPhaseProvider FindPhaseProvider(ISystemRegistry registry, IMythLogger logger, string groupName)
        {
            IPhaseProvider phaseProvider = null;

            // Спроба знайти IPhaseProvider через реєстр систем
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

            // Якщо не знайдено, створюємо новий GamePhaseProvider
            if (phaseProvider == null)
            {
                logger.LogWarning($"IPhaseProvider not found when registering group '{groupName}'. Creating new one.", "System");

                // Спроба отримати EventBus
                IEventBus eventBus = null;
                if (registry is SystemRegistry sr)
                {
                    foreach (var system in sr.GetAllSystems())
                    {
                        if (system is IEventSystem eventSystem)
                        {
                            // У даному випадку нам треба отримати EventBus через рефлексію
                            var field = eventSystem.GetType().GetField("_eventBus", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            if (field != null)
                            {
                                eventBus = field.GetValue(eventSystem) as IEventBus;
                                if (eventBus != null)
                                    break;
                            }
                        }
                    }
                }

                // Створюємо новий провайдер
                phaseProvider = eventBus != null
                    ? new GamePhaseProvider(eventBus, logger)
                    : new EmergencyPhaseProvider(logger);
            }

            return phaseProvider;
        }

        /// <summary>
        /// Реєструє групу систем з вказаною категорією ініціалізації
        /// </summary>
        /// <typeparam name="TGroup">Тип групи систем</typeparam>
        /// <param name="registry">Реєстр систем</param>
        /// <param name="groupName">Назва групи</param>
        /// <param name="priority">Пріоритет виконання</param>
        /// <param name="category">Категорія ініціалізації</param>
        /// <param name="logger">Логер</param>
        /// <param name="systems">Типи систем для додавання в групу</param>
        /// <returns>Створена група систем</returns>
        public static TGroup RegisterGroupWithCategory<TGroup>(
            this ISystemRegistry registry,
            string groupName,
            int priority,
            SystemInitializationCategory category,
            IMythLogger logger,
            Type[] systems = null)
            where TGroup : SystemGroup
        {
            // Створюємо екземпляр групи через активатор
            var group = (TGroup)Activator.CreateInstance(
                typeof(TGroup),
                new object[] { groupName, logger });

            // Застосовуємо категорію через атрибут
            Type groupType = typeof(TGroup);
            if (groupType.GetCustomAttributes(typeof(SystemCategoryAttribute), true)
                .FirstOrDefault() is SystemCategoryAttribute existingAttr)
            {
                // Якщо атрибут вже існує, перевіряємо чи потрібно його замінити
                if (existingAttr.Category != category)
                {
                    logger.LogWarning($"Group type {groupType.Name} already has category {existingAttr.Category}, overriding with {category}", "SystemRegistry");
                }
            }

            // Реєструємо групу з пріоритетом та категорією
            registry.RegisterSystemWithPriority(group, priority);

            // Додаємо системи, якщо вони вказані
            if (systems != null && systems.Length > 0)
            {
                var container = registry.GetContainer();
                foreach (var systemType in systems)
                {
                    try
                    {
                        // Отримуємо екземпляр через DI контейнер
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
            IMythLogger logger)
            where TGroup : SystemGroup
            where TSystem1 : ISystem
        {
            return registry.RegisterGroupWithCategory<TGroup>(
                groupName,
                priority,
                category,
                logger,
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
            IMythLogger logger)
            where TGroup : SystemGroup
            where TSystem1 : ISystem
            where TSystem2 : ISystem
        {
            return registry.RegisterGroupWithCategory<TGroup>(
                groupName,
                priority,
                category,
                logger,
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
            IMythLogger logger)
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
                new Type[] { typeof(TSystem1), typeof(TSystem2), typeof(TSystem3) });
        }
    }

    
}
