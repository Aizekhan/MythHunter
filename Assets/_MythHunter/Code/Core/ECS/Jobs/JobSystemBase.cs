// Шлях: Assets/_MythHunter/Code/Core/ECS/Jobs/JobSystemBase.cs
using System;
using System.Collections.Generic;
using Unity.Jobs;
using Cysharp.Threading.Tasks;
using MythHunter.Utils.Logging;
using MythHunter.Events;

namespace MythHunter.Core.ECS.Jobs
{
    /// <summary>
    /// Базовий клас для систем, що підтримують паралельне виконання
    /// </summary>
    public abstract class JobSystemBase : SystemBase, IJobSystem, IJobAwareSystem
    {
        protected ISystemJobScheduler _scheduler;
        private readonly List<JobHandle> _activeJobs = new List<JobHandle>();
        private bool _jobsScheduled = false;

        protected JobSystemBase(IMythLogger logger, IEventBus eventBus)
            : base(logger, eventBus)
        {
        }

        /// <summary>
        /// Встановлює планувальник задач для системи
        /// </summary>
        public void SetJobScheduler(ISystemJobScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        /// <summary>
        /// Підготовлює системні задачі для паралельного виконання
        /// </summary>
        public abstract List<JobHandle> PrepareJobs(ISystemJobScheduler scheduler);

        /// <summary>
        /// Обробляє результати виконання задач, викликається після їх завершення
        /// </summary>
        public abstract void ProcessJobResults();

        /// <summary>
        /// Оновлює систему
        /// </summary>
        public override void Update(float deltaTime)
        {
            // Якщо планувальник задач не встановлений, виконуємо звичайне оновлення
            if (_scheduler == null)
            {
                base.Update(deltaTime);
                return;
            }

            // Якщо задачі ще не заплановані, плануємо їх
            if (!_jobsScheduled)
            {
                try
                {
                    _activeJobs.Clear();
                    _activeJobs.AddRange(PrepareJobs(_scheduler));
                    _jobsScheduled = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error preparing jobs: {ex.Message}", GetType().Name, ex);
                }
            }
        }

        /// <summary>
        /// Завершує виконання задач і обробляє результати
        /// </summary>
        public void CompleteJobs()
        {
            if (!_jobsScheduled || _activeJobs.Count == 0)
                return;

            try
            {
                // Завершуємо всі задачі
                foreach (var job in _activeJobs)
                {
                    if (job.IsValid())
                        _scheduler.Complete(job);
                }

                // Обробляємо результати
                ProcessJobResults();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error completing jobs: {ex.Message}", GetType().Name, ex);
            }
            finally
            {
                _activeJobs.Clear();
                _jobsScheduled = false;
            }
        }

        /// <summary>
        /// Асинхронно завершує виконання задач і обробляє результати
        /// </summary>
        public async UniTask CompleteJobsAsync()
        {
            if (!_jobsScheduled || _activeJobs.Count == 0)
                return;

            try
            {
                // Створюємо комбіновану задачу для очікування всіх задач
                JobHandle combinedHandle = JobHandle.CombineDependencies(_activeJobs.ToArray());

                // Асинхронно очікуємо завершення
                await _scheduler.CompleteAsync(combinedHandle);

                // Обробляємо результати
                ProcessJobResults();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error completing jobs asynchronously: {ex.Message}", GetType().Name, ex);
            }
            finally
            {
                _activeJobs.Clear();
                _jobsScheduled = false;
            }
        }
    }
}
