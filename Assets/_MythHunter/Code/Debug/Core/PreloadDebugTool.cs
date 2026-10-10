// Assets/_MythHunter/Code/Debug/Tools/PreloadDebugTool.cs
using System.Collections.Generic;
using System.Linq;
using MythHunter.Core.DI;
using MythHunter.Debug.Core;
using MythHunter.Resources;
using MythHunter.Utils.Logging;
using UnityEditor;
using UnityEngine;

namespace MythHunter.Debug.Tools
{
    /// <summary>
    /// Debug tool для моніторингу та валідації preload конфігурацій
    /// </summary>
    public class PreloadDebugTool : IDebugTool
    {
        private readonly IPreloadManager _preloadManager;
        private readonly IMythLogger _logger;
        private readonly Dictionary<string, List<PreloadConfigInfo>> _sceneConfigs = new();
        private readonly Dictionary<string, List<PreloadConfigInfo>> _phaseConfigs = new();
        public string ToolCategory => "Resources";
        public string ToolName => "Preload Monitor";
        public bool IsEnabled { get; set; } = true;

        private struct PreloadConfigInfo
        {
            public string ResourceKey;
            public string ResourceType;
            public int Priority;
            public bool CreatePool;
            public int PoolSize;
            public bool IsLoaded;
            public string Status;
        }

        [Inject]
        public PreloadDebugTool(IPreloadManager preloadManager, IMythLogger logger)
        {
            _preloadManager = preloadManager;
            _logger = logger;
        }

        /// <summary>
        /// Реєструє preload конфігурацію для відстеження
        /// </summary>
        public void TrackScenePreload(string sceneName, string resourceKey, System.Type resourceType, int priority, bool createPool, int poolSize)
        {
            if (!_sceneConfigs.ContainsKey(sceneName))
                _sceneConfigs[sceneName] = new List<PreloadConfigInfo>();

            _sceneConfigs[sceneName].Add(new PreloadConfigInfo
            {
                ResourceKey = resourceKey,
                ResourceType = resourceType.Name,
                Priority = priority,
                CreatePool = createPool,
                PoolSize = poolSize,
                IsLoaded = false,
                Status = "Зареєстровано"
            });
        }

        /// <summary>
        /// Виводить детальну інформацію про preload для сцени
        /// </summary>
        public void LogScenePreloadInfo(string sceneName)
        {
            if (!_sceneConfigs.TryGetValue(sceneName, out var configs))
            {
                _logger.LogWarning($"📋 Немає preload конфігурацій для сцени: {sceneName}", "PreloadDebug");
                return;
            }

            _logger.LogInfo($"📋 Preload конфігурація для сцени '{sceneName}':", "PreloadDebug");
            _logger.LogInfo($"   Загальна кількість ресурсів: {configs.Count}", "PreloadDebug");

            // Групуємо за пріоритетом
            var groupedByPriority = configs
                .GroupBy(c => c.Priority)
                .OrderByDescending(g => g.Key);

            foreach (var group in groupedByPriority)
            {
                _logger.LogInfo($"   📌 Пріоритет {group.Key}:", "PreloadDebug");
                foreach (var config in group)
                {
                    string poolInfo = config.CreatePool ? $"[Пул: {config.PoolSize}]" : "[Без пулу]";
                    _logger.LogInfo($"     • {config.ResourceKey} ({config.ResourceType}) {poolInfo}", "PreloadDebug");
                }
            }

            // Валідація потенційних проблем
            ValidateSceneConfig(sceneName, configs);
        }

        /// <summary>
        /// Валідує конфігурацію на потенційні проблеми
        /// </summary>
        private void ValidateSceneConfig(string sceneName, List<PreloadConfigInfo> configs)
        {
            var issues = new List<string>();

            // Перевірка на дублікати
            var duplicates = configs
                .GroupBy(c => c.ResourceKey)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var duplicate in duplicates)
            {
                issues.Add($"Дублікат ресурсу: {duplicate}");
            }

            // Перевірка на занадто великі пули
            var largePools = configs
                .Where(c => c.CreatePool && c.PoolSize > 50)
                .Select(c => $"{c.ResourceKey} (пул: {c.PoolSize})");

            foreach (var largePool in largePools)
            {
                issues.Add($"Великий пул: {largePool}");
            }

            // Перевірка пріоритетів
            var highPriorityCount = configs.Count(c => c.Priority > 80);
            if (highPriorityCount > 5)
            {
                issues.Add($"Забагато високопріоритетних ресурсів: {highPriorityCount}");
            }

            // Виводимо попередження якщо є проблеми
            if (issues.Any())
            {
                _logger.LogWarning($"⚠️ Потенційні проблеми preload для '{sceneName}':", "PreloadDebug");
                foreach (var issue in issues)
                {
                    _logger.LogWarning($"   • {issue}", "PreloadDebug");
                }
            }
            else
            {
                _logger.LogInfo($"✅ Конфігурація preload для '{sceneName}' виглядає добре", "PreloadDebug");
            }
        }

        /// <summary>
        /// Отримує статистику всіх preload конфігурацій
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
    {
        { "TotalScenes", _sceneConfigs.Count },
        { "TotalResources", _sceneConfigs.Values.Sum(configs => configs.Count) },
        { "TotalPools", _sceneConfigs.Values.SelectMany(configs => configs).Count(c => c.CreatePool) },
        { "ScenesWithConfigs", _sceneConfigs.Keys.ToArray() }
    };
        }
        public string[] GetLogEntries(int maxCount = 100)
        {
            return new[] { "PreloadDebugTool активний", $"Загальна кількість сцен: {_sceneConfigs.Count}" };
        }

        public struct PreloadStatistics
        {
            public int TotalScenes;
            public int TotalResources;
            public int TotalPools;
            public string[] ScenesWithConfigs;
        }

        public void RenderGUI(Rect area)
        {
            GUILayout.BeginArea(area);
            GUILayout.Label("=== Preload Monitor ===", EditorStyles.boldLabel);

            var stats = GetStatistics();

            GUILayout.Label($"Сцен з preload: {(int)stats["TotalScenes"]}");
            GUILayout.Label($"Загальна кількість ресурсів: {(int)stats["TotalResources"]}");
            GUILayout.Label($"Загальна кількість пулів: {(int)stats["TotalPools"]}");

            GUILayout.Space(10);

            var sceneNames = stats["ScenesWithConfigs"] as string[];
            if (sceneNames != null)
            {
                foreach (var sceneName in sceneNames)
                {
                    if (GUILayout.Button($"Показати конфігурацію для '{sceneName}'"))
                    {
                        LogScenePreloadInfo(sceneName);
                    }
                }
            }

            GUILayout.EndArea();
        }



        public void Initialize()
        {
            // Можна залишити порожнім або додати ініціалізацію логіки
        }

        public void Update()
        {
            // Можна залишити порожнім або додати оновлення даних
        }

        public void Dispose()
        {
            // Очистити словники, якщо потрібно
            _sceneConfigs.Clear();
            _phaseConfigs.Clear();
        }
    }
}
