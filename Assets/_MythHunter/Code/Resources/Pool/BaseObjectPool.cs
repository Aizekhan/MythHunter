// Шлях: Assets/_MythHunter/Code/Resources/Pool/BaseObjectPool.cs
using System;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Базовий абстрактний клас для всіх пулів об'єктів
    /// </summary>
    public abstract class BaseObjectPool : IObjectPool
    {
        protected readonly IMythLogger _logger;
        protected readonly string _poolName;
        protected int _activeCount;

        /// <summary>
        /// Кількість активних об'єктів у пулі
        /// </summary>
        public int CountActive => _activeCount;

        /// <summary>
        /// Кількість неактивних об'єктів у пулі
        /// </summary>
        public abstract int CountInactive
        {
            get;
        }

        /// <summary>
        /// Створює новий базовий пул об'єктів
        /// </summary>
        protected BaseObjectPool(IMythLogger logger, string poolName)
        {
            _logger = logger;
            _poolName = poolName ?? "UnnamedPool";
        }

        /// <summary>
        /// Очищає пул, видаляючи всі об'єкти
        /// </summary>
        public abstract void Clear();

        /// <summary>
        /// Реалізація IObjectPool.ReturnObject
        /// </summary>
        public virtual void ReturnObject(UnityEngine.Object obj)
        {
            if (obj == null)
            {
                LogWarning("Trying to return null instance to pool");
                return;
            }

            ReturnObjectInternal(obj);
        }

        /// <summary>
        /// Внутрішня реалізація повернення об'єкта
        /// </summary>
        protected abstract void ReturnObjectInternal(UnityEngine.Object obj);

        /// <summary>
        /// Журналювання інформаційного повідомлення
        /// </summary>
        protected void LogInfo(string message)
        {
            _logger?.LogInfo($"{message} [Pool: {_poolName}]", "Pool");
        }

        /// <summary>
        /// Журналювання повідомлення попередження
        /// </summary>
        protected void LogWarning(string message)
        {
            _logger?.LogWarning($"{message} [Pool: {_poolName}]", "Pool");
        }

        /// <summary>
        /// Журналювання повідомлення про трасування
        /// </summary>
        protected void LogTrace(string message)
        {
            _logger?.LogTrace($"{message} [Pool: {_poolName}]", "Pool");
        }

        /// <summary>
        /// Журналювання повідомлення про помилку
        /// </summary>
        protected void LogError(string message)
        {
            _logger?.LogError($"{message} [Pool: {_poolName}]", "Pool");
        }
    }
}
