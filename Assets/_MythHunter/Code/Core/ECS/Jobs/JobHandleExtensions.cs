// Шлях: Assets/_MythHunter/Code/Core/ECS/Jobs/JobHandleExtensions.cs
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Jobs;

namespace MythHunter.Core.ECS.Jobs
{
    /// <summary>
    /// Розширення для JobHandle для спрощення асинхронної роботи
    /// </summary>
    public static class JobHandleExtensions
    {
        /// <summary>
        /// Перевіряє валідність дескриптора задачі
        /// </summary>
        /// <summary>
        /// Перевіряє валідність дескриптора задачі
        /// </summary>
        public static bool IsValid(this JobHandle handle)
        {
            // Порівнюємо з default замість доступу до приватного поля
            return handle != default;
        }

        /// <summary>
        /// Асинхронно очікує завершення задачі
        /// </summary>
        public static async UniTask AwaitCompletion(this JobHandle handle, CancellationToken cancellationToken = default)
        {
            if (!handle.IsValid())
                return;

            // Чекаємо доки задача завершиться
            while (!handle.IsCompleted)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    handle.Complete(); // Якщо запит скасований, завершуємо задачу
                    return;
                }
            }

            // Явно завершуємо задачу для звільнення ресурсів
            handle.Complete();
        }

        /// <summary>
        /// Перетворює JobHandle в UniTask
        /// </summary>
        public static UniTask AsUniTask(this JobHandle handle, CancellationToken cancellationToken = default)
        {
            return handle.AwaitCompletion(cancellationToken);
        }
    }
}
