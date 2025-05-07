// Шлях: Assets/_MythHunter/Code/Resources/Pool/PoolMonitor.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Система моніторингу пулів об'єктів для виявлення витоків
    /// </summary>
    public class PoolMonitor : MonoBehaviour
    {
        private IPoolManager _poolManager;
        private IMythLogger _logger;

        private float _checkInterval = 30f; // Перевірка кожні 30 секунд
        private float _lastCheckTime;
        private Dictionary<string, int> _lastActiveCount = new Dictionary<string, int>();

        [Inject]
        public void Initialize(IPoolManager poolManager, IMythLogger logger)
        {
            _poolManager = poolManager;
            _logger = logger;
            _lastCheckTime = Time.realtimeSinceStartup;

            // Підписка на події зміни сцени для очищення пулів
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded += OnSceneUnloaded;

            _logger.LogInfo("PoolMonitor initialized", "Pool");
        }

        private void Update()
        {
            // Періодична перевірка витоків
            if (Time.realtimeSinceStartup - _lastCheckTime > _checkInterval)
            {
                CheckForLeaks();
                _lastCheckTime = Time.realtimeSinceStartup;
            }
        }

        /// <summary>
        /// Перевіряє наявність потенційних витоків пам'яті
        /// </summary>
        public void CheckForLeaks()
        {
            _poolManager.CheckForLeaks();

            var stats = _poolManager.GetAllPoolsStatistics();
            foreach (var stat in stats)
            {
                string poolKey = stat.Key;
                var poolStat = stat.Value;

                // Перевірка на збільшення активних об'єктів без повернення
                if (_lastActiveCount.TryGetValue(poolKey, out int lastCount))
                {
                    if (poolStat.ActiveCount > lastCount + 10)
                    {
                        _logger.LogWarning($"Potential leak detected in pool '{poolKey}': " +
                            $"Active count increased from {lastCount} to {poolStat.ActiveCount}", "Pool");
                    }
                }

                _lastActiveCount[poolKey] = poolStat.ActiveCount;
            }
        }

        /// <summary>
        /// Обробляє подію вивантаження сцени
        /// </summary>
        private void OnSceneUnloaded(UnityEngine.SceneManagement.Scene scene)
        {
            _logger.LogInfo($"Scene unloaded: {scene.name}, trimming pools", "Pool");
            _poolManager.TrimExcessObjects();
        }

        private void OnDestroy()
        {
            UnityEngine.SceneManagement.SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
    }
}
