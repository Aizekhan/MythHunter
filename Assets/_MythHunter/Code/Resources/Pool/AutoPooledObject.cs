// Assets/_MythHunter/Code/Resources/Pool/AutoPooledObject.cs
using UnityEngine;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Компонент для автоматичного повернення об'єкта до пулу при деактивації
    /// з підтримкою DI через LazyMonoBehaviour
    /// </summary>
    public class AutoPooledObject : LazyMonoBehaviour
    {
        [Inject] private IPoolManager _poolManager;
        [Inject] private IMythLogger _logger;

        [Tooltip("Ключ пулу, з якого взято об'єкт")]
        [SerializeField] private string _poolKey;

        [Tooltip("Автоматично повертати в пул при деактивації")]
        [SerializeField] private bool _autoReturn = true;

        [Tooltip("Максимальний час життя об'єкта (в секундах), 0 = необмежено")]
        [SerializeField] private float _maxLifetime = 0f;

        private bool _isReturning = false;
        private float _activationTime;

        protected override void OnInitialized()
        {
            _activationTime = Time.time;
        }

        private void Update()
        {
            if (_maxLifetime > 0 && Time.time - _activationTime > _maxLifetime)
                ReturnToPool();
        }

        private void OnDisable()
        {
            if (_autoReturn && !_isReturning && _poolManager != null)
                ReturnToPool();
        }

        public void Initialize(string poolKey)
        {
            _poolKey = poolKey;
            _isReturning = false;
            _activationTime = Time.time;
        }

        public void SetMaxLifetime(float seconds)
        {
            _maxLifetime = seconds;
        }

        public void ReturnToPool()
        {
            if (_isReturning || _poolManager == null || string.IsNullOrEmpty(_poolKey))
            {
                _logger?.LogWarning("Cannot return to pool — key or manager missing", "Pool");
                return;
            }

            _isReturning = true;
            _poolManager.ReturnToPool(_poolKey, gameObject);
            _isReturning = false;
        }
    }
}
