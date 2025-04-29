// PoolManager.cs
using System.Collections.Generic;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
using UnityEngine;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Менеджер об'єктних пулів
    /// </summary>
    public class PoolManager : IPoolManager
    {
        private readonly Dictionary<string, ObjectPool> _pools = new Dictionary<string, ObjectPool>();
        private readonly IMythLogger _logger;

        [Inject]
        public PoolManager(IMythLogger logger)
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

        public void ReturnToPool(string key, UnityEngine.Object instance)
        {
            if (instance == null)
                return;

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

            Transform poolParent = null;
            if (prefab is GameObject)
            {
                var poolContainer = new GameObject($"Pool_{key}");
                UnityEngine.Object.DontDestroyOnLoad(poolContainer);
                poolParent = poolContainer.transform;
            }

            var pool = new ObjectPool(prefab, initialSize, poolParent);
            _pools[key] = pool;
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
