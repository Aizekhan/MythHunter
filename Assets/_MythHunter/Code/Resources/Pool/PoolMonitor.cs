// Шлях: Assets/_MythHunter/Code/Resources/Pool/PoolMonitor.cs

using UnityEngine;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;
using System.Collections.Generic;
using System.Linq;
using System;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Розширений монітор пулів об'єктів для відстеження, діагностики та оптимізації
    /// </summary>
    public class PoolMonitor : MonoBehaviour
    {
        private readonly Dictionary<string, string> _sceneDependentPools = new Dictionary<string, string>();
        private readonly Dictionary<string, PoolPerformanceMetrics> _performanceMetrics = new Dictionary<string, PoolPerformanceMetrics>();
        private readonly Dictionary<string, List<GameObject>> _trackedObjects = new Dictionary<string, List<GameObject>>();
        private readonly Dictionary<string, float> _lastCleanupTimes = new Dictionary<string, float>();

        private IMythLogger _logger;
        private IPoolManager _poolManager;

        // Налаштування моніторингу
        [SerializeField] private float _metricsUpdateInterval = 5f;
        [SerializeField] private float _leakDetectionInterval = 30f;
        [SerializeField] private float _autoCleanupInterval = 60f;
        [SerializeField] private int _maxObjectsPerPool = 100;
        [SerializeField] private bool _enableDebugLogging = false;

        private float _lastMetricsUpdate;
        private float _lastLeakDetection;
        private bool _isInitialized = false;

        /// <summary>
        /// Метрики продуктивності для пулу
        /// </summary>
        public class PoolPerformanceMetrics
        {
            public int TotalGets
            {
                get; set;
            }
            public int TotalReturns
            {
                get; set;
            }
            public int PeakActiveCount
            {
                get; set;
            }
            public float AverageActiveCount
            {
                get; set;
            }
            public float LastUpdateTime
            {
                get; set;
            }
            public int LeakWarnings
            {
                get; set;
            }
            public float MemoryUsageMB
            {
                get; set;
            }
            public DateTime CreationTime { get; set; } = DateTime.Now;
        }

        private void Awake()
        {
            InitializeMonitor();
        }

        private void InitializeMonitor()
        {
            if (_isInitialized)
                return;

            _logger = MythHunter.Utils.Logging.MythLoggerFactory.GetDefaultLogger();
            _lastMetricsUpdate = Time.realtimeSinceStartup;
            _lastLeakDetection = Time.realtimeSinceStartup;
            _isInitialized = true;

            _logger.LogInfo("PoolMonitor ініціалізовано", "PoolMonitor");
        }

        /// <summary>
        /// Встановлює посилання на PoolManager для отримання статистики
        /// </summary>
        public void SetPoolManager(IPoolManager poolManager)
        {
            _poolManager = poolManager;
            _logger?.LogInfo("PoolManager підключено до PoolMonitor", "PoolMonitor");
        }

        private void Update()
        {
            if (!_isInitialized)
                return;

            float currentTime = Time.realtimeSinceStartup;

            // Оновлення метрик
            if (currentTime - _lastMetricsUpdate >= _metricsUpdateInterval)
            {
                UpdateMetrics();
                _lastMetricsUpdate = currentTime;
            }

            // Детекція витоків
            if (currentTime - _lastLeakDetection >= _leakDetectionInterval)
            {
                DetectPotentialLeaks();
                _lastLeakDetection = currentTime;
            }

            // Автоматичне очищення
            PerformAutoCleanup();
        }

        /// <summary>
        /// Реєструє пул, що залежить від сцени
        /// </summary>
        public void RegisterSceneDependentPool(string poolKey, string sceneName)
        {
            _sceneDependentPools[poolKey] = sceneName;

            // Ініціалізуємо метрики для нового пулу
            if (!_performanceMetrics.ContainsKey(poolKey))
            {
                _performanceMetrics[poolKey] = new PoolPerformanceMetrics();
                _trackedObjects[poolKey] = new List<GameObject>();
            }

            if (_enableDebugLogging)
                _logger?.LogInfo($"Pool '{poolKey}' зареєстровано для сцени '{sceneName}'", "PoolMonitor");
        }

        /// <summary>
        /// Відстежує активацію об'єкта з пулу
        /// </summary>
        public void TrackObjectActivation(string poolKey, GameObject obj)
        {
            if (obj == null)
                return;

            if (_performanceMetrics.TryGetValue(poolKey, out var metrics))
            {
                metrics.TotalGets++;
                metrics.LastUpdateTime = Time.realtimeSinceStartup;
            }

            if (_trackedObjects.TryGetValue(poolKey, out var objects))
            {
                if (!objects.Contains(obj))
                {
                    objects.Add(obj);
                }
            }

            if (_enableDebugLogging)
                _logger?.LogDebug($"Object activated from pool '{poolKey}': {obj.name}", "PoolMonitor");
        }

        /// <summary>
        /// Відстежує деактивацію об'єкта до пулу
        /// </summary>
        public void TrackObjectDeactivation(string poolKey, GameObject obj)
        {
            if (obj == null)
                return;

            if (_performanceMetrics.TryGetValue(poolKey, out var metrics))
            {
                metrics.TotalReturns++;
                metrics.LastUpdateTime = Time.realtimeSinceStartup;
            }

            if (_trackedObjects.TryGetValue(poolKey, out var objects))
            {
                objects.Remove(obj);
            }

            if (_enableDebugLogging)
                _logger?.LogDebug($"Object returned to pool '{poolKey}': {obj.name}", "PoolMonitor");
        }

        /// <summary>
        /// Отримує назву сцени для пулу
        /// </summary>
        public string GetSceneForPool(string poolKey)
        {
            return _sceneDependentPools.TryGetValue(poolKey, out var sceneName) ? sceneName : null;
        }

        /// <summary>
        /// Очищає інформацію про пули для сцени
        /// </summary>
        public void ClearPoolsForScene(string sceneName)
        {
            var keysToRemove = new List<string>();
            foreach (var kvp in _sceneDependentPools)
            {
                if (kvp.Value == sceneName)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _sceneDependentPools.Remove(key);
                _performanceMetrics.Remove(key);
                _trackedObjects.Remove(key);
                _lastCleanupTimes.Remove(key);
            }

            _logger?.LogInfo($"Очищено пули для сцени '{sceneName}': {keysToRemove.Count} пулів", "PoolMonitor");
        }

        /// <summary>
        /// Оновлює метрики продуктивності
        /// </summary>
        private void UpdateMetrics()
        {
            if (_poolManager == null)
                return;

            var poolStats = _poolManager.GetAllPoolsStatistics();

            foreach (var poolStat in poolStats)
            {
                string poolKey = poolStat.Key;
                var stats = poolStat.Value;

                if (_performanceMetrics.TryGetValue(poolKey, out var metrics))
                {
                    // Оновлюємо метрики
                    int currentActive = stats.ActiveCount;
                    if (currentActive > metrics.PeakActiveCount)
                    {
                        metrics.PeakActiveCount = currentActive;
                    }

                    // Розрахунок середньої кількості активних об'єктів
                    metrics.AverageActiveCount = (metrics.AverageActiveCount + currentActive) / 2f;
                    metrics.LastUpdateTime = Time.realtimeSinceStartup;

                    // Оцінка використання пам'яті (приблизно)
                    metrics.MemoryUsageMB = (currentActive + stats.InactiveCount) * 0.001f; // Приблизна оцінка
                }
            }
        }

        /// <summary>
        /// Детекція потенційних витоків пам'яті
        /// </summary>
        private void DetectPotentialLeaks()
        {
            foreach (var kvp in _trackedObjects)
            {
                string poolKey = kvp.Key;
                var objects = kvp.Value;

                // Видаляємо null об'єкти (знищені Unity)
                objects.RemoveAll(obj => obj == null);

                // Перевіряємо на потенційні витоки
                if (objects.Count > _maxObjectsPerPool)
                {
                    if (_performanceMetrics.TryGetValue(poolKey, out var metrics))
                    {
                        metrics.LeakWarnings++;
                    }

                    _logger?.LogWarning($"Потенційний витік в пулі '{poolKey}': {objects.Count} активних об'єктів", "PoolMonitor");
                }

                // Перевіряємо об'єкти, які активні занадто довго
                var suspiciousObjects = objects.Where(obj =>
                    obj != null &&
                    Time.realtimeSinceStartup - GetObjectActivationTime(obj) > 300f // 5+ хвилин
                ).Take(5).ToList();

                if (suspiciousObjects.Count > 0)
                {
                    _logger?.LogWarning($"Знайдено {suspiciousObjects.Count} об'єктів з підозрілим часом життя в пулі '{poolKey}'", "PoolMonitor");
                }
            }
        }

        /// <summary>
        /// Автоматичне очищення надлишкових об'єктів
        /// </summary>
        private void PerformAutoCleanup()
        {
            float currentTime = Time.realtimeSinceStartup;

            foreach (var poolKey in _sceneDependentPools.Keys.ToList())
            {
                if (!_lastCleanupTimes.TryGetValue(poolKey, out var lastCleanup))
                {
                    lastCleanup = currentTime;
                    _lastCleanupTimes[poolKey] = lastCleanup;
                }

                if (currentTime - lastCleanup >= _autoCleanupInterval)
                {
                    _poolManager?.TrimExcessObjects(20); // Обмежуємо до 20 неактивних об'єктів
                    _lastCleanupTimes[poolKey] = currentTime;

                    if (_enableDebugLogging)
                        _logger?.LogDebug($"Автоматичне очищення пулу '{poolKey}'", "PoolMonitor");
                }
            }
        }

        /// <summary>
        /// Отримує час активації об'єкта (заглушка - можна розширити)
        /// </summary>
        private float GetObjectActivationTime(GameObject obj)
        {
            // Тут може бути логіка відстеження часу активації
            // Поки що повертаємо поточний час як заглушку
            return Time.realtimeSinceStartup - 60f; // Приблизно 1 хвилина назад
        }

        /// <summary>
        /// Отримує детальну статистику всіх пулів
        /// </summary>
        public Dictionary<string, PoolPerformanceMetrics> GetDetailedStatistics()
        {
            return new Dictionary<string, PoolPerformanceMetrics>(_performanceMetrics);
        }

        /// <summary>
        /// Генерує звіт про стан пулів
        /// </summary>
        public string GenerateStatusReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== Pool Monitor Status Report ===");
            report.AppendLine($"Total Pools: {_performanceMetrics.Count}");
            report.AppendLine($"Scene Dependent Pools: {_sceneDependentPools.Count}");

            foreach (var kvp in _performanceMetrics)
            {
                var metrics = kvp.Value;
                report.AppendLine($"\nPool: {kvp.Key}");
                report.AppendLine($"  Gets: {metrics.TotalGets}, Returns: {metrics.TotalReturns}");
                report.AppendLine($"  Peak Active: {metrics.PeakActiveCount}");
                report.AppendLine($"  Avg Active: {metrics.AverageActiveCount:F1}");
                report.AppendLine($"  Leak Warnings: {metrics.LeakWarnings}");
                report.AppendLine($"  Memory Usage: {metrics.MemoryUsageMB:F2} MB");
            }

            return report.ToString();
        }

        /// <summary>
        /// Налаштування параметрів моніторингу
        /// </summary>
        public void ConfigureMonitoring(float metricsInterval, float leakDetectionInterval, bool enableDebug)
        {
            _metricsUpdateInterval = metricsInterval;
            _leakDetectionInterval = leakDetectionInterval;
            _enableDebugLogging = enableDebug;

            _logger?.LogInfo($"PoolMonitor налаштовано: metrics={metricsInterval}s, leak={leakDetectionInterval}s, debug={enableDebug}", "PoolMonitor");
        }

        private void OnDestroy()
        {
            _logger?.LogInfo("PoolMonitor знищено", "PoolMonitor");
        }

        // Методи для Debug UI (можна викликати з Inspector або Debug консолі)
        [ContextMenu("Print Status Report")]
        private void PrintStatusReport()
        {
            _logger.LogInfo(GenerateStatusReport());
        }

        [ContextMenu("Force Leak Detection")]
        private void ForceLeakDetection()
        {
            DetectPotentialLeaks();
        }

        [ContextMenu("Force Cleanup")]
        private void ForceCleanup()
        {
            PerformAutoCleanup();
        }
    }
}
