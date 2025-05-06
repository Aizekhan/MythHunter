// У новому файлі Assets/_MythHunter/Code/Utils/Extensions/UniTaskExtensions.cs
using System;
using Cysharp.Threading.Tasks;

namespace MythHunter.Utils.Extensions
{
    public static class UniTaskExtensions
    {
        /// <summary>
        /// Додає обробник помилок до UniTask без порушення async flow
        /// </summary>
        public static async UniTask AttachExceptionHandler(this UniTask task, Action<Exception> exceptionHandler)
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
        /// Додає обробник помилок до UniTask<T> без порушення async flow
        /// </summary>
        public static async UniTask<T> AttachExceptionHandler<T>(this UniTask<T> task, Action<Exception> exceptionHandler)
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {
                exceptionHandler?.Invoke(ex);
                return default;
            }
        }
    }
}
