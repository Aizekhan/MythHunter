// PoolManager.cs
namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Менеджер об'єктних пулів
    /// </summary>
    public class PoolManager : IPoolManager
    {
        private readonly System.Collections.Generic.Dictionary<string, IObjectPool<UnityEngine.Object>> _pools =
            new System.Collections.Generic.Dictionary<string, IObjectPool<UnityEngine.Object>>();
        private readonly MythHunter.Utils.Logging.IMythLogger _logger;

        [MythHunter.Core.DI.Inject]
        public PoolManager(MythHunter.Utils.Logging.IMythLogger logger)
        {
            _logger = logger;
        }

        public T GetFromPool<T>(string key) where T : UnityEngine.Object
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                _logger.LogWarning($"Pool with key '{key}' does not exist", "Pool");
                return null;
            }

            return pool.Get() as T;
        }

        public void ReturnToPool<T>(string key, T instance) where T : UnityEngine.Object
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                _logger.LogWarning($"Pool with key '{key}' does not exist", "Pool");
                return;
            }

            pool.Return(instance);
        }

        public void CreatePool<T>(string key, T prefab, int initialSize) where T : UnityEngine.Object
        {
            if (_pools.ContainsKey(key))
            {
                _logger.LogWarning($"Pool with key '{key}' already exists", "Pool");
                return;
            }

            var pool = new ObjectPool<T>(prefab, initialSize);
            _pools[key] = pool as IObjectPool<UnityEngine.Object>;
            _logger.LogInfo($"Created pool with key '{key}' of type {typeof(T).Name}", "Pool");
        }

        public void ClearPool(string key)
        {
            if (!_pools.TryGetValue(key, out var pool))
            {
                _logger.LogWarning($"Pool with key '{key}' does not exist", "Pool");
                return;
            }

            pool.Clear();
            _pools.Remove(key);
            _logger.LogInfo($"Cleared and removed pool with key '{key}'", "Pool");
        }

        public void ClearAllPools()
        {
            foreach (var pool in _pools.Values)
            {
                pool.Clear();
            }

            _pools.Clear();
            _logger.LogInfo("Cleared all pools", "Pool");
        }
    }
}
