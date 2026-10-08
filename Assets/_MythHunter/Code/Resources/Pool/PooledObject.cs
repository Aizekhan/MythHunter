using UnityEngine;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.Resources.Pool
{
    public class PooledObject : LazyMonoBehaviour
    {
        [Inject] private IPoolManager _poolManager;
        [Inject] private IMythLogger _logger;

        [SerializeField] private bool _autoReturn = true;

        private string _poolKey;
        private bool _isReturning;

        public void Initialize(string poolKey)
        {
            _poolKey = poolKey;
            _isReturning = false;
        }

        private void OnDisable()
        {
            if (_autoReturn && !_isReturning && _poolManager != null)
                ReturnToPool();
        }

        public void ReturnToPool()
        {
            if (_isReturning || string.IsNullOrEmpty(_poolKey) || _poolManager == null)
            {
                _logger?.LogWarning($"Неможливо повернути об'єкт в пул: ключ порожній або менеджер відсутній", "Pool");
                return;
            }

            _isReturning = true;
            _poolManager.ReturnToPool(_poolKey, gameObject);
            _isReturning = false;
        }
    }
}
