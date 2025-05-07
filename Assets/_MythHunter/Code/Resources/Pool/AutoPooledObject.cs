// Шлях: Assets/_MythHunter/Code/Resources/Pool/AutoPooledObject.cs
using UnityEngine;

using MythHunter.Core.Game;
using MythHunter.Utils.Logging;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Компонент для автоматичного повернення об'єкта до пулу при деактивації
    /// та відстеження його життєвого циклу
    /// </summary>
    public class AutoPooledObject : MonoBehaviour
    {
        [Tooltip("Ключ пулу, з якого взято об'єкт")]
        [SerializeField] private string _poolKey;

        [Tooltip("Автоматично повертати в пул при деактивації")]
        [SerializeField] private bool _autoReturn = true;

        [Tooltip("Відстежувати час життя об'єкта")]
        [SerializeField] private bool _trackLifetime = true;

        [Tooltip("Максимальний час життя об'єкта (в секундах), 0 = необмежено")]
        [SerializeField] private float _maxLifetime = 0f;

        private IPoolManager _poolManager;
        private bool _isReturning = false;
        private float _activationTime;
        private IMythLogger _logger;

        private void Awake()
        {
            // Отримуємо менеджер пулів через GameBootstrapper
            if (GameBootstrapper.Instance != null)
            {
                _poolManager = GameBootstrapper.Instance.GetPoolManager();
                // Також отримуємо логер, якщо він доступний
                _logger = MythLoggerFactory.GetDefaultLogger();
            }
        }

        private void OnEnable()
        {
            _activationTime = Time.time;
        }

        private void Update()
        {
            // Якщо встановлено максимальний час життя, перевіряємо його
            if (_maxLifetime > 0 && Time.time - _activationTime > _maxLifetime)
            {
                ReturnToPool();
            }
        }

        private void OnDisable()
        {
            if (_autoReturn && !_isReturning && _poolManager != null)
            {
                ReturnToPool();
            }
        }

        /// <summary>
        /// Ініціалізує об'єкт з вказаним ключем пулу
        /// </summary>
        public void Initialize(string poolKey, IPoolManager poolManager)
        {
            _poolKey = poolKey;
            _poolManager = poolManager;
            _isReturning = false;
            _activationTime = Time.time;
        }

        /// <summary>
        /// Повертає об'єкт у пул
        /// </summary>
        public void ReturnToPool()
        {
            if (_isReturning || _poolManager == null)
                return;

            if (string.IsNullOrEmpty(_poolKey))
            {
                // Використовуємо власний логер замість Debug.LogWarning
                if (_logger != null)
                {
                    _logger.LogWarning($"Cannot return object to pool: pool key is empty", "Pool");
                }
                else
                {
                    // Запасний варіант, якщо логер недоступний
                    UnityEngine.Debug.LogWarning($"Cannot return object to pool: pool key is empty", gameObject);
                }
                return;
            }

            _isReturning = true;
            _poolManager.ReturnToPool(_poolKey, gameObject);
            _isReturning = false;
        }

        /// <summary>
        /// Встановлює максимальний час життя об'єкта
        /// </summary>
        public void SetMaxLifetime(float seconds)
        {
            _maxLifetime = seconds;
        }
    }
}
