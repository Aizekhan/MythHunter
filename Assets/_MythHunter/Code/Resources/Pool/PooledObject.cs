// Шлях: Assets/_MythHunter/Code/Resources/Pool/PooledObject.cs
using UnityEngine;
using MythHunter.Resources.Pool;
using MythHunter.Core.DI;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Компонент для автоматичного повернення GameObject до пулу
    /// </summary>
    public class PooledObject : MonoBehaviour
    {
        [SerializeField] private bool _autoReturn = true;

        private string _poolKey;
        private IPoolManager _poolManager;
        private bool _isReturning;

        /// <summary>
        /// Ініціалізує PooledObject для автоматичного повернення
        /// </summary>
        public void Initialize(string poolKey, IPoolManager poolManager)
        {
            _poolKey = poolKey;
            _poolManager = poolManager;
            _isReturning = false;
        }

        /// <summary>
        /// Обробляє деактивацію об'єкта
        /// </summary>
        private void OnDisable()
        {
            if (_autoReturn && !_isReturning && _poolManager != null)
            {
                ReturnToPool();
            }
        }

        /// <summary>
        /// Повертає об'єкт у пул
        /// </summary>
        public void ReturnToPool()
        {
            if (_isReturning || string.IsNullOrEmpty(_poolKey) || _poolManager == null)
                return;

            _isReturning = true;
            _poolManager.ReturnToPool(_poolKey, gameObject);
            _isReturning = false;
        }

        /// <summary>
        /// Викликається при знищенні об'єкта
        /// </summary>
        private void OnDestroy()
        {
            _poolManager = null;
        }
    }
}
