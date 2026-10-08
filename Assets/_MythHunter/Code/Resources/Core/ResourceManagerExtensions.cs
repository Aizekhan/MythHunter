// Шлях: Assets/_MythHunter/Code/Resources/Core/ResourceManagerExtensions.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Resources.Core;
using UnityEngine;

namespace MythHunter.Resources
{
    /// <summary>
    /// Розширення для ResourceManager з підтримкою асинхронного вивантаження
    /// </summary>
    public static class ResourceManagerExtensions
    {
        /// <summary>
        /// Асинхронно вивантажує ресурс
        /// </summary>
        public static async UniTask UnloadAsync(this IResourceManager resourceManager, string key)
        {
            // Вивантажуємо ресурс
            resourceManager.Unload(key);

            // Очікуємо один кадр для обробки вивантаження
            await UniTask.Yield();
        }

        /// <summary>
        /// Асинхронно вивантажує всі ресурси
        /// </summary>
        public static async UniTask UnloadAllAsync(this IResourceManager resourceManager)
        {
            // Вивантажуємо всі ресурси
            resourceManager.UnloadAll();

            // Запускаємо очищення невикористовуваних ассетів
            var operation = UnityEngine.Resources.UnloadUnusedAssets();
            await operation;

            // Запускаємо Garbage Collector
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Асинхронно очищує ресурси пам'яті
        /// </summary>
        public static async UniTask CleanupMemoryAsync(this IResourceManager resourceManager)
        {
            // Запускаємо очищення невикористовуваних ассетів
            var operation = UnityEngine.Resources.UnloadUnusedAssets();
            await operation;

            // Запускаємо Garbage Collector
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Асинхронно вивантажує невикористовувані пули
        /// </summary>
        public static async UniTask TrimUnusedPoolsAsync(this IResourceManager resourceManager, int maxInactivePerPool = 20)
        {
            // Отримуємо доступ до пул-менеджера через рефлексію для сумісності
            var fieldInfo = typeof(ResourceManager).GetField("_poolManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (fieldInfo == null)
                return;

            var poolManager = fieldInfo.GetValue(resourceManager) as MythHunter.Resources.Pool.IPoolManager;
            if (poolManager == null)
                return;

            // Викликаємо TrimExcessObjects
            poolManager.TrimExcessObjects(maxInactivePerPool);

            // Чекаємо один кадр для обробки
            await UniTask.Yield();
        }
    }
}
