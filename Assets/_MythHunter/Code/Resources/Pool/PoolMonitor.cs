// Шлях: Assets/_MythHunter/Code/Resources/Pool/PoolMonitor.cs
using System;
using System.Collections.Generic;
using System.Linq;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Система моніторингу пулів об'єктів для виявлення витоків та управління життєвим циклом
    /// </summary>
    public class PoolMonitor : MonoBehaviour
    {
        private IPoolManager _poolManager;
        private IMythLogger _logger;
        private PooledObjectLifetimeTracker _lifetimeTracker;

        // Налаштування
        [SerializeField] private float _checkInterval = 30f; // Перевірка кожні 30 секунд
        [SerializeField] private int _maxInactivePerPool = 20; // Максимальна кількість неактивних об'єктів у пулі
        [SerializeField] private float _leakThresholdTime = 300f; // 5 хвилин активності = потенційний витік
        [SerializeField] private bool _cleanOnSceneChange = true; // Очищення при зміні сцени
        [SerializeField] private bool _enableBackgroundCleaning = true; // Фонове очищення
        [SerializeField] private float _fpsTriggerThreshold = 50f; // FPS для запуску очищення

        // Стан
        private float _lastCheckTime;
        private float _lastCleanupTime;
        private float _lastFrameTime;
        private Dictionary<string, int> _lastActiveCount = new Dictionary<string, int>();
        private List<string> _sceneDependentPools = new List<string>();

        // Кеш інформації про сцени
        private HashSet<string> _previousScenes = new HashSet<string>();
        private Dictionary<string, HashSet<string>> _poolsPerScene = new Dictionary<string, HashSet<string>>();
        private float _cleanupTimer;
        private float _fpsCheckInterval = 0.5f; // Перевірка FPS кожні 0.5 секунди
        private float _lastFpsCheckTime;
        private float _currentFps;
        private int _frameCount;
        private float _backgroundCleanupInterval = 10f; // Секунди між фоновими очищеннями

        [Inject]
        public void Initialize(IPoolManager poolManager, IMythLogger logger)
        {
            _poolManager = poolManager;
            _logger = logger;
            _lifetimeTracker = new PooledObjectLifetimeTracker(logger);
            _lastCheckTime = Time.realtimeSinceStartup;
            _lastCleanupTime = Time.realtimeSinceStartup;
            _lastFpsCheckTime = Time.realtimeSinceStartup;

            // Підписка на події зміни сцени
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SceneManager.activeSceneChanged += OnActiveSceneChanged;

            // Зберігаємо початковий стан сцен
            UpdateCurrentScenes();

            _logger.LogInfo("PoolMonitor initialized", "Pool");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        }

        private void Update()
        {
            // Оновлення FPS
            CalculateFPS();

            // Періодична перевірка витоків
            if (Time.realtimeSinceStartup - _lastCheckTime > _checkInterval)
            {
                CheckForLeaks();
                _lastCheckTime = Time.realtimeSinceStartup;
            }

            // Фонове очищення за умови високого FPS
            if (_enableBackgroundCleaning &&
                Time.realtimeSinceStartup - _lastCleanupTime > _backgroundCleanupInterval)
            {
                if (_currentFps > _fpsTriggerThreshold)
                {
                    PerformBackgroundCleaning();
                    _lastCleanupTime = Time.realtimeSinceStartup;
                }
            }
        }

        /// <summary>
        /// Розрахунок поточного FPS
        /// </summary>
        private void CalculateFPS()
        {
            _frameCount++;

            if (Time.realtimeSinceStartup - _lastFpsCheckTime >= _fpsCheckInterval)
            {
                _currentFps = _frameCount / (Time.realtimeSinceStartup - _lastFpsCheckTime);
                _frameCount = 0;
                _lastFpsCheckTime = Time.realtimeSinceStartup;
            }
        }

        /// <summary>
        /// Перевіряє наявність потенційних витоків пам'яті
        /// </summary>
        public void CheckForLeaks()
        {
            _poolManager.CheckForLeaks();

            // Аналіз даних трекера часу життя
            var potentialLeaks = _lifetimeTracker.DetectPotentialLeaks(_leakThresholdTime);
            if (potentialLeaks.Count > 0)
            {
                _logger.LogWarning($"Detected {potentialLeaks.Count} potential pool object leaks", "Pool");
                foreach (var leak in potentialLeaks.Take(5)) // Показуємо до 5 витоків
                {
                    if (leak.ObjectType == typeof(GameObject))
                    {
                        _logger.LogWarning($"Potential leak: GameObject '{leak.LastComponentPath}' in pool '{leak.PoolKey}', " +
                                          $"active for {(DateTime.Now - leak.ActivationTime).TotalSeconds:F1}s, " +
                                          $"scene: {leak.LastSceneName}", "Pool");
                    }
                    else
                    {
                        _logger.LogWarning($"Potential leak: Object of type {leak.ObjectType.Name} in pool '{leak.PoolKey}', " +
                                          $"active for {(DateTime.Now - leak.ActivationTime).TotalSeconds:F1}s", "Pool");
                    }
                }
            }

            // Статистика пулів
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

            // Очищення інформації про знищені об'єкти
            _lifetimeTracker.CleanupDestroyed();
        }

        /// <summary>
        /// Фонове очищення пулів при високому FPS
        /// </summary>
        private void PerformBackgroundCleaning()
        {
            _logger.LogInfo($"Performing background pool cleaning (FPS: {_currentFps:F1})", "Pool");

            // Отримання статистики пулів
            var stats = _poolManager.GetAllPoolsStatistics();
            int totalCleaned = 0;

            // Знаходимо найбільші пули та очищаємо їх
            var largePools = stats
                .Where(s => s.Value.InactiveCount > _maxInactivePerPool * 2) // Пули з великою кількістю неактивних об'єктів
                .OrderByDescending(s => s.Value.InactiveCount)
                .Take(3); // Очищаємо до 3 пулів за раз

            foreach (var pool in largePools)
            {
                // Обчислюємо нове цільове значення - зберігаємо активні + максимум неактивних
                int targetInactiveCount = Math.Min(pool.Value.InactiveCount / 2, _maxInactivePerPool);

                // Додаткова логіка - якщо пул використовується рідко (мало активних об'єктів),
                // можемо бути більш агресивними в очищенні
                if (pool.Value.ActiveCount < _maxInactivePerPool / 4)
                {
                    targetInactiveCount = Math.Min(targetInactiveCount, _maxInactivePerPool / 2);
                }

                int trimCount = pool.Value.InactiveCount - targetInactiveCount;
                if (trimCount > 0)
                {
                    // Спеціальний метод для GameObjectPool
                    if (typeof(GameObjectPool).Name == pool.Value.PoolType)
                    {
                        // Використання IPoolManager для очищення
                        _poolManager.TrimExcessObjects(targetInactiveCount);
                        totalCleaned += trimCount;

                        _logger.LogInfo($"Background cleaning: trimmed pool '{pool.Key}' from {pool.Value.InactiveCount} to {targetInactiveCount} inactive objects", "Pool");
                    }
                }
            }

            _logger.LogInfo($"Background cleaning completed: trimmed {totalCleaned} excess objects", "Pool");
        }

        /// <summary>
        /// Оновлення списку поточних сцен
        /// </summary>
        private void UpdateCurrentScenes()
        {
            _previousScenes.Clear();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                _previousScenes.Add(scene.name);

                // Ініціалізуємо колекцію для сцени, якщо потрібно
                if (!_poolsPerScene.ContainsKey(scene.name))
                {
                    _poolsPerScene[scene.name] = new HashSet<string>();
                }
            }
        }

        /// <summary>
        /// Обробляє подію завантаження сцени
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _logger.LogInfo($"Scene loaded: {scene.name}", "Pool");

            // Додаємо сцену до активних
            _previousScenes.Add(scene.name);

            // Ініціалізуємо колекцію для сцени
            if (!_poolsPerScene.ContainsKey(scene.name))
            {
                _poolsPerScene[scene.name] = new HashSet<string>();
            }
        }

        /// <summary>
        /// Обробляє подію вивантаження сцени
        /// </summary>
        private void OnSceneUnloaded(Scene scene)
        {
            _logger.LogInfo($"Scene unloaded: {scene.name}", "Pool");

            // Видаляємо сцену з активних
            _previousScenes.Remove(scene.name);

            if (_cleanOnSceneChange)
            {
                // Очищаємо пули, пов'язані з цією сценою
                if (_poolsPerScene.TryGetValue(scene.name, out var scenePools))
                {
                    foreach (var poolKey in scenePools)
                    {
                        // Перевіряємо, чи пул використовується іншими сценами
                        bool usedByOtherScenes = false;
                        foreach (var otherScenePools in _poolsPerScene.Where(kv => kv.Key != scene.name))
                        {
                            if (otherScenePools.Value.Contains(poolKey))
                            {
                                usedByOtherScenes = true;
                                break;
                            }
                        }

                        // Якщо пул використовується тільки цією сценою, можемо очистити його повністю
                        if (!usedByOtherScenes)
                        {
                            _logger.LogInfo($"Clearing pool '{poolKey}' associated only with unloaded scene: {scene.name}", "Pool");
                            _poolManager.ClearPool(poolKey);
                        }
                        else
                        {
                            // Інакше обмежуємо його розмір
                            _logger.LogInfo($"Trimming pool '{poolKey}' due to scene unload: {scene.name}", "Pool");
                            _poolManager.TrimExcessObjects(_maxInactivePerPool);
                        }
                    }

                    // Видаляємо записи про пули цієї сцени
                    _poolsPerScene.Remove(scene.name);
                }
                else
                {
                    // Загальне очищення пулів
                    _logger.LogInfo($"Scene unloaded: {scene.name}, performing general pool trimming", "Pool");
                    _poolManager.TrimExcessObjects(_maxInactivePerPool);
                }
            }
        }

        /// <summary>
        /// Обробляє подію зміни активної сцени
        /// </summary>
        private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            _logger.LogInfo($"Active scene changed from {oldScene.name} to {newScene.name}", "Pool");

            if (_cleanOnSceneChange)
            {
                // Якщо попередня сцена все ще завантажена (не вивантажена), але більше не активна
                if (oldScene.isLoaded && !_sceneDependentPools.Contains(oldScene.name))
                {
                    // Очищаємо тільки пули, які не використовуються в основній сцені
                    _logger.LogInfo($"Active scene changed, trimming pools", "Pool");
                    _poolManager.TrimExcessObjects(_maxInactivePerPool);
                }
            }
        }

        /// <summary>
        /// Реєструє пул як залежний від конкретної сцени
        /// </summary>
        public void RegisterSceneDependentPool(string poolKey, string sceneName)
        {
            if (!_poolsPerScene.TryGetValue(sceneName, out var scenePools))
            {
                scenePools = new HashSet<string>();
                _poolsPerScene[sceneName] = scenePools;
            }

            scenePools.Add(poolKey);
            _logger.LogInfo($"Registered pool '{poolKey}' as dependent on scene '{sceneName}'", "Pool");
        }

        /// <summary>
        /// Отримує статистику про час життя об'єктів пулу
        /// </summary>
        public Dictionary<string, List<PooledObjectLifetimeTracker.ObjectLifetimeInfo>> GetLifetimeStatistics()
        {
            return _lifetimeTracker.GetLifetimeStatistics();
        }

        /// <summary>
        /// Доступ до трекера часу життя об'єктів
        /// </summary>
        public PooledObjectLifetimeTracker GetLifetimeTracker()
        {
            return _lifetimeTracker;
        }
    }
}
