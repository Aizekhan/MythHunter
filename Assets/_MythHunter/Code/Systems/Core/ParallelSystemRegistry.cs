// Шлях: Assets/_MythHunter/Code/Systems/Core/ParallelSystemRegistry.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Core.ECS.Jobs;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Реєстр систем з підтримкою паралельного виконання
    /// </summary>
    public class ParallelSystemRegistry : SystemRegistry
    {
        private readonly ISystemJobScheduler _jobScheduler;
        private readonly Dictionary<Type, SystemMetadata> _systemMetadata = new Dictionary<Type, SystemMetadata>();

        // Групи паралельних систем
        private readonly Dictionary<string, List<IJobSystem>> _parallelSystemGroups = new Dictionary<string, List<IJobSystem>>();

        // Послідовні системи
        private readonly List<ISystem> _sequentialSystems = new List<ISystem>();

        // Граф залежностей систем
        private readonly Dictionary<Type, HashSet<Type>> _systemDependencies = new Dictionary<Type, HashSet<Type>>();

        // Додатковий стан виконання
        private bool _isUpdating = false;

        /// <summary>
        /// Метадані системи
        /// </summary>
        private class SystemMetadata
        {
            public bool IsParallelizable
            {
                get; set;
            }
            public bool IsSequential
            {
                get; set;
            }
            public string ParallelGroupId
            {
                get; set;
            }
            public int Priority
            {
                get; set;
            }
        }

        [Inject]
        public ParallelSystemRegistry(IMythLogger logger, IEventBus eventBus, ISystemJobScheduler jobScheduler)
            : base(logger, eventBus)
        {
            _jobScheduler = jobScheduler;
        }

        /// <summary>
        /// Реєструє систему з виявленням метаданих
        /// </summary>
        public override void RegisterSystem(ISystem system)
        {
            base.RegisterSystem(system);

            ProcessSystemMetadata(system);

            // Встановлюємо планувальник задач для систем, які цього потребують
            if (system is IJobAwareSystem jobAwareSystem)
            {
                jobAwareSystem.SetJobScheduler(_jobScheduler);
            }
        }

        /// <summary>
        /// Аналізує метадані системи для визначення паралелізму
        /// </summary>
        private void ProcessSystemMetadata(ISystem system)
        {
            Type systemType = system.GetType();

            var metadata = new SystemMetadata
            {
                IsParallelizable = false,
                IsSequential = false,
                ParallelGroupId = "",
                Priority = 0
            };

            // Перевіряємо атрибути
            var parallelAttr = systemType.GetCustomAttribute<ParallelizableSystemAttribute>();
            var sequentialAttr = systemType.GetCustomAttribute<SequentialSystemAttribute>();

            if (parallelAttr != null)
            {
                metadata.IsParallelizable = true;
                metadata.ParallelGroupId = parallelAttr.GroupId ?? systemType.Name;
                metadata.Priority = parallelAttr.Priority;

                // Додаємо до групи паралельних систем, якщо підтримується
                if (system is IJobSystem jobSystem)
                {
                    if (!_parallelSystemGroups.TryGetValue(metadata.ParallelGroupId, out var group))
                    {
                        group = new List<IJobSystem>();
                        _parallelSystemGroups[metadata.ParallelGroupId] = group;
                    }

                    group.Add(jobSystem);
                }
            }
            else if (sequentialAttr != null)
            {
                metadata.IsSequential = true;
                metadata.Priority = sequentialAttr.Priority;

                // Додаємо до списку послідовних систем
                _sequentialSystems.Add(system);
            }
            else
            {
                // За замовчуванням вважаємо систему послідовною
                metadata.IsSequential = true;
                _sequentialSystems.Add(system);
            }

            // Аналізуємо залежності між системами
            var dependencyAttrs = systemType.GetCustomAttributes<SystemDependencyAttribute>(true);
            if (dependencyAttrs != null && dependencyAttrs.Any())
            {
                if (!_systemDependencies.TryGetValue(systemType, out var dependencies))
                {
                    dependencies = new HashSet<Type>();
                    _systemDependencies[systemType] = dependencies;
                }

                foreach (var depAttr in dependencyAttrs)
                {
                    dependencies.Add(depAttr.DependsOnSystemType);
                }
            }

            // Зберігаємо метадані
            _systemMetadata[systemType] = metadata;
        }

        /// <summary>
        /// Оновлює всі системи з урахуванням паралелізму
        /// </summary>
        public override void UpdateAll(float deltaTime)
        {
            if (_isUpdating)
                return;

            _isUpdating = true;

            try
            {
                // Відсортуємо послідовні системи за пріоритетом
                _sequentialSystems.Sort((a, b) =>
                {
                    var metaA = _systemMetadata.TryGetValue(a.GetType(), out var ma) ? ma : null;
                    var metaB = _systemMetadata.TryGetValue(b.GetType(), out var mb) ? mb : null;

                    int priorityA = metaA?.Priority ?? 0;
                    int priorityB = metaB?.Priority ?? 0;

                    return priorityB.CompareTo(priorityA); // Вищий пріоритет - раніше виконання
                });

                // Виконуємо паралельне оновлення для систем, що підтримують це
                ExecuteParallelSystemGroups(deltaTime);

                // Виконуємо послідовне оновлення для решти систем
                foreach (var system in _sequentialSystems)
                {
                    if (IsSystemActiveInCurrentPhase(system))
                    {
                        UpdateSystem(system, deltaTime);
                    }
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        /// <summary>
        /// Виконує паралельні групи систем
        /// </summary>
        private void ExecuteParallelSystemGroups(float deltaTime)
        {
            // Перевіряємо наявність паралельних систем
            if (_parallelSystemGroups.Count == 0)
                return;

            // Виконуємо планування задач для всіх паралельних груп
            List<IJobSystem> scheduledSystems = new List<IJobSystem>();

            foreach (var group in _parallelSystemGroups.Values)
            {
                foreach (var system in group)
                {
                    if (IsSystemActiveInCurrentPhase(system))
                    {
                        try
                        {
                            // Викликаємо базове оновлення для можливої підготовки даних
                            system.Update(deltaTime);

                            // Плануємо задачі
                            system.PrepareJobs(_jobScheduler);
                            scheduledSystems.Add(system);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error scheduling jobs for system {system.GetType().Name}: {ex.Message}", "Systems", ex);
                        }
                    }
                }
            }

            // Очікуємо завершення всіх задач і обробляємо результати
            foreach (var system in scheduledSystems)
            {
                try
                {
                    system.CompleteJobs();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Error completing jobs for system {system.GetType().Name}: {ex.Message}", "Systems", ex);
                }
            }
        }

        /// <summary>
        /// Асинхронно оновлює всі системи з урахуванням паралелізму
        /// </summary>
        public async UniTask UpdateAllAsync(float deltaTime)
        {
            if (_isUpdating)
                return;

            _isUpdating = true;

            try
            {
                // Спочатку запускаємо паралельні системи
                var parallelTask = ExecuteParallelSystemGroupsAsync(deltaTime);

                // Одночасно запускаємо асинхронні системи
                var asyncTasks = new List<UniTask>();
                foreach (var system in _sequentialSystems.OfType<IAsyncSystem>())
                {
                    if (IsSystemActiveInCurrentPhase(system))
                    {
                        asyncTasks.Add(system.UpdateAsync(deltaTime));
                    }
                }

                // Очікуємо завершення паралельних і асинхронних систем
                await UniTask.WhenAll(new[] { parallelTask }.Concat(asyncTasks));

                // Потім оновлюємо звичайні послідовні системи
                foreach (var system in _sequentialSystems.Where(s => !(s is IAsyncSystem)))
                {
                    if (IsSystemActiveInCurrentPhase(system))
                    {
                        UpdateSystem(system, deltaTime);
                    }
                }
            }
            finally
            {
                _isUpdating = false;
            }
        }

        /// <summary>
        /// Асинхронно виконує паралельні групи систем
        /// </summary>
        private async UniTask ExecuteParallelSystemGroupsAsync(float deltaTime)
        {
            // Перевіряємо наявність паралельних систем
            if (_parallelSystemGroups.Count == 0)
                return;

            // Виконуємо планування задач для всіх паралельних груп
            List<IJobSystem> scheduledSystems = new List<IJobSystem>();

            foreach (var group in _parallelSystemGroups.Values)
            {
                foreach (var system in group)
                {
                    if (IsSystemActiveInCurrentPhase(system))
                    {
                        try
                        {
                            // Викликаємо базове оновлення для можливої підготовки даних
                            system.Update(deltaTime);

                            // Плануємо задачі
                            system.PrepareJobs(_jobScheduler);
                            scheduledSystems.Add(system);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError($"Error scheduling jobs for system {system.GetType().Name}: {ex.Message}", "Systems", ex);
                        }
                    }
                }
            }

            // Очікуємо завершення всіх задач асинхронно
            var completeJobs = scheduledSystems.Select(system => system.CompleteJobsAsync()).ToArray();
            await UniTask.WhenAll(completeJobs);
        }

        /// <summary>
        /// Перевіряє, чи система активна в поточній фазі
        /// </summary>
        private bool IsSystemActiveInCurrentPhase(ISystem system)
        {
            if (system is IPhaseFilteredSystem phaseSystem)
            {
                return phaseSystem.IsActiveInPhase(CurrentPhase);
            }

            return true;
        }

        /// <summary>
        /// Оновлює систему з обробкою помилок
        /// </summary>
        private void UpdateSystem(ISystem system, float deltaTime)
        {
            try
            {
                system.Update(deltaTime);
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error updating system {system.GetType().Name}: {ex.Message}", "Systems", ex);
            }
        }
    }
}
