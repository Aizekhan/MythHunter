// Шлях: Assets/_MythHunter/Code/Core/ECS/Jobs/SystemJobScheduler.cs
using System;
using System.Collections.Generic;
using System.Threading;
using Unity.Jobs;
using Unity.Collections;
using Cysharp.Threading.Tasks;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.ECS.Jobs
{
    /// <summary>
    /// Реалізація планувальника системних задач з підтримкою паралельного виконання
    /// </summary>
    public class SystemJobScheduler : ISystemJobScheduler
    {
        private readonly IMythLogger _logger;
        private readonly List<JobHandle> _activeJobs = new List<JobHandle>();
        private static int _jobIdCounter = 0;

        public SystemJobScheduler(IMythLogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Планує системну задачу для паралельного виконання
        /// </summary>
        public JobHandle ScheduleJob<T>(T jobData) where T : struct, IJob
        {
            int jobId = Interlocked.Increment(ref _jobIdCounter);
            _logger.LogDebug($"Scheduling job {typeof(T).Name} (ID: {jobId})", "Jobs");

            try
            {
                JobHandle handle = jobData.Schedule();
                RegisterActiveJob(handle);
                return handle;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error scheduling job {typeof(T).Name}: {ex.Message}", "Jobs", ex);
                return default;
            }
        }

        /// <summary>
        /// Планує системну задачу для паралельного виконання з розділенням за сутностями
        /// </summary>
        public JobHandle ScheduleParallelJob<T>(T jobData, int entityCount, int batchSize = 64)
            where T : struct, IJobParallelFor
        {
            int jobId = Interlocked.Increment(ref _jobIdCounter);

            if (entityCount <= 0)
            {
                _logger.LogDebug($"Skipping parallel job {typeof(T).Name} (ID: {jobId}) - no entities to process", "Jobs");
                return default;
            }

            _logger.LogDebug($"Scheduling parallel job {typeof(T).Name} for {entityCount} entities (ID: {jobId})", "Jobs");

            try
            {
                // Адаптація розміру пакету залежно від кількості сутностей
                int effectiveBatchSize = Math.Min(batchSize, Math.Max(1, entityCount / 4));

                JobHandle handle = jobData.Schedule(entityCount, effectiveBatchSize);
                RegisterActiveJob(handle);
                return handle;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error scheduling parallel job {typeof(T).Name}: {ex.Message}", "Jobs", ex);
                return default;
            }
        }

        /// <summary>
        /// Планує групу системних задач для паралельного виконання
        /// </summary>
        public JobHandle ScheduleJobBatch(List<JobHandle> jobs)
        {
            if (jobs == null || jobs.Count == 0)
                return default;

            if (jobs.Count == 1)
                return jobs[0];

            _logger.LogDebug($"Scheduling job batch with {jobs.Count} jobs", "Jobs");

            try
            {
                JobHandle combinedHandle = JobHandle.CombineDependencies(jobs.ToArray());
                RegisterActiveJob(combinedHandle);
                return combinedHandle;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error scheduling job batch: {ex.Message}", "Jobs", ex);
                return default;
            }
        }

        /// <summary>
        /// Асинхронно очікує завершення виконання задачі
        /// </summary>
        public async UniTask CompleteAsync(JobHandle handle)
        {
            if (!handle.IsValid())
                return;

            try
            {
                // Перевіряємо стан задачі з інтервалом
                while (!handle.IsCompleted)
                {
                    await UniTask.Yield();
                }

                // Явне очікування завершення
                handle.Complete();
                UnregisterActiveJob(handle);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error completing job asynchronously: {ex.Message}", "Jobs", ex);
            }
        }

        /// <summary>
        /// Синхронно очікує завершення виконання задачі
        /// </summary>
        public void Complete(JobHandle handle)
        {
            if (!handle.IsValid())
                return;

            try
            {
                handle.Complete();
                UnregisterActiveJob(handle);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error completing job: {ex.Message}", "Jobs", ex);
            }
        }

        /// <summary>
        /// Очікує завершення всіх активних задач
        /// </summary>
        public void CompleteAll()
        {
            if (_activeJobs.Count == 0)
                return;

            _logger.LogDebug($"Completing all {_activeJobs.Count} active jobs", "Jobs");

            try
            {
                JobHandle.CompleteAll(_activeJobs.ToArray());
                _activeJobs.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error completing all jobs: {ex.Message}", "Jobs", ex);
            }
        }

        // Додає задачу до списку активних
        private void RegisterActiveJob(JobHandle handle)
        {
            if (handle.IsValid())
                _activeJobs.Add(handle);
        }

        // Видаляє задачу зі списку активних
        private void UnregisterActiveJob(JobHandle handle)
        {
            _activeJobs.Remove(handle);
        }
    }
}
