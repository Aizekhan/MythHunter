using Cysharp.Threading.Tasks;
using System;

namespace MythHunter.Utils.Extensions
{
    /// <summary>
    /// Розширення для UniTask спеціально для проекту MythHunter
    /// </summary>
    public static class MythTaskExtensions
    {

        /// <summary>
        /// Додає обробник винятків до UniTask
        /// </summary>
        public static async UniTask WithExceptionHandler(this UniTask task, Action<Exception> exceptionHandler)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                exceptionHandler?.Invoke(ex);
            }
        }

        /// <summary>
        /// Додає обробник винятків без передачі самого винятку
        /// </summary>
        public static async UniTask WithExceptionHandler(this UniTask task, Action exceptionHandler)
        {
            try
            {
                await task;
            }
            catch
            {
                exceptionHandler?.Invoke();
            }
        }

        /// <summary>
        /// Безпечно запускає асинхронну задачу з обробкою винятків
        /// </summary>
        public static async UniTask FireAndForgetSafely(this UniTask task, Action<Exception> errorHandler = null)
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                errorHandler?.Invoke(ex);
            }
        }

        /// <summary>
        /// Безпечно запускає асинхронну задачу без передачі винятку
        /// </summary>
        public static async UniTask FireAndForgetSafely(this UniTask task, Action errorHandler = null)
        {
            try
            {
                await task;
            }
            catch (Exception)
            {
                errorHandler?.Invoke();
            }
        }

        /// <summary>
        /// Виконує задачу з таймаутом
        /// </summary>
        public static async UniTask<bool> WithTimeout(this UniTask task, float timeoutSeconds)
        {
            var timeoutTask = UniTask.Delay(TimeSpan.FromSeconds(timeoutSeconds));
            var resultTask = await UniTask.WhenAny(task, timeoutTask);
            return resultTask == 0; // Повертає true, якщо задача завершилася до таймауту
        }
    }
}
