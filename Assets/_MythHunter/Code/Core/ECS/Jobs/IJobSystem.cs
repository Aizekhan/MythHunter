// Шлях: Assets/_MythHunter/Code/Core/ECS/Jobs/IJobSystem.cs
using System.Collections.Generic;
using Unity.Jobs;
using Cysharp.Threading.Tasks;

namespace MythHunter.Core.ECS.Jobs
{
    /// <summary>
    /// Інтерфейс для систем, що підтримують паралельне виконання через Job System
    /// </summary>
    /// <summary>
    /// Інтерфейс для систем, що підтримують паралельне виконання через Job System
    /// </summary>
    public interface IJobSystem : ISystem
    {
        /// <summary>
        /// Підготовлює системні задачі для паралельного виконання
        /// </summary>
        /// <param name="scheduler">Планувальник задач</param>
        /// <returns>Список дескрипторів створених задач</returns>
        List<JobHandle> PrepareJobs(ISystemJobScheduler scheduler);

        /// <summary>
        /// Обробляє результати виконання задач, викликається після їх завершення
        /// </summary>
        void ProcessJobResults();

        /// <summary>
        /// Синхронно завершує виконання задач і обробляє результати
        /// </summary>
        void CompleteJobs();

        /// <summary>
        /// Асинхронно завершує виконання задач і обробляє результати
        /// </summary>
        UniTask CompleteJobsAsync();
    }

    /// <summary>
    /// Інтерфейс для систем, що підтримують асинхронне виконання
    /// </summary>
    public interface IAsyncSystem : ISystem
    {
        /// <summary>
        /// Асинхронно оновлює систему
        /// </summary>
        /// <param name="deltaTime">Час з останнього оновлення</param>
        UniTask UpdateAsync(float deltaTime);
    }

    /// <summary>
    /// Інтерфейс для системи, яка потребує доступу до планувальника задач
    /// </summary>
    public interface IJobAwareSystem : ISystem
    {
        /// <summary>
        /// Встановлює планувальник задач для системи
        /// </summary>
        /// <param name="scheduler">Планувальник задач</param>
        void SetJobScheduler(ISystemJobScheduler scheduler);
    }
}
