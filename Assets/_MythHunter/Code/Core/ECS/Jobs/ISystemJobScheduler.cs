// Шлях: Assets/_MythHunter/Code/Core/ECS/Jobs/ISystemJobScheduler.cs
using System.Collections.Generic;
using Unity.Jobs;
using Cysharp.Threading.Tasks;

namespace MythHunter.Core.ECS.Jobs
{
    /// <summary>
    /// Інтерфейс для планувальника системних задач, що відповідає за розподіл системних операцій
    /// </summary>
    public interface ISystemJobScheduler
    {
        /// <summary>
        /// Планує системну задачу для паралельного виконання
        /// </summary>
        /// <typeparam name="T">Тип задачі, що реалізує IJob</typeparam>
        /// <param name="jobData">Дані задачі</param>
        /// <returns>Дескриптор задачі для відстеження</returns>
        JobHandle ScheduleJob<T>(T jobData) where T : struct, IJob;

        /// <summary>
        /// Планує системну задачу для паралельного виконання з розділенням за сутностями
        /// </summary>
        /// <typeparam name="T">Тип задачі, що реалізує IJobParallelFor</typeparam>
        /// <param name="jobData">Дані задачі</param>
        /// <param name="entityCount">Кількість сутностей для обробки</param>
        /// <param name="batchSize">Розмір порції сутностей для обробки</param>
        /// <returns>Дескриптор задачі для відстеження</returns>
        JobHandle ScheduleParallelJob<T>(T jobData, int entityCount, int batchSize = 64)
            where T : struct, IJobParallelFor;

        /// <summary>
        /// Планує групу системних задач для паралельного виконання
        /// </summary>
        /// <param name="jobs">Список задач для виконання</param>
        /// <returns>Об'єднаний дескриптор для всіх задач</returns>
        JobHandle ScheduleJobBatch(List<JobHandle> jobs);

        /// <summary>
        /// Асинхронно очікує завершення виконання задачі
        /// </summary>
        /// <param name="handle">Дескриптор задачі</param>
        /// <returns>UniTask для очікування</returns>
        UniTask CompleteAsync(JobHandle handle);

        /// <summary>
        /// Синхронно очікує завершення виконання задачі
        /// </summary>
        /// <param name="handle">Дескриптор задачі</param>
        void Complete(JobHandle handle);
    }
}
