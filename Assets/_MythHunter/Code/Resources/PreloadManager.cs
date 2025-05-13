// Шлях: Assets/_MythHunter/Code/Resources/PreloadManager.cs

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Resources.Core;
using MythHunter.Systems.Phase;
using MythHunter.Utils.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MythHunter.Resources
{
    /// <summary>
    /// Менеджер для прееміптивного завантаження ресурсів
    /// </summary>
    public class PreloadManager : IPreloadManager, IDisposable
    {
        private readonly Dictionary<string, List<PreloadConfig>> _phasePreloadConfigs = new Dictionary<string, List<PreloadConfig>>();
        private readonly Dictionary<string, List<PreloadConfig>> _scenePreloadConfigs = new Dictionary<string, List<PreloadConfig>>();

        private readonly IResourceManager _resourceManager;
        private readonly IPhaseProvider _phaseProvider;
        private readonly IMythLogger _logger;

        private string _currentPhaseId = string.Empty;
        private string _currentScene = string.Empty;

        private struct PreloadConfig
        {
            public string ResourceKey;
            public Type ResourceType;
            public int Priority;
            public bool CreatePool;
            public int PoolSize;
        }

        [Inject]
        public PreloadManager(IResourceManager resourceManager, IPhaseProvider phaseProvider, IMythLogger logger)
        {
            _resourceManager = resourceManager;
            _phaseProvider = phaseProvider;
            _logger = logger;

            // Підписка на зміни фаз і сцен
            _phaseProvider.SubscribeToPhaseChange(OnPhaseChanged);
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Зберігаємо поточну фазу
            _currentPhaseId = _phaseProvider.GetCurrentPhaseId();

            // Ініціалізуємо конфігурації
            InitializeDefaultConfigs();
        }

        /// <summary>
        /// Ініціалізує стандартні конфігурації прееміптивного завантаження
        /// </summary>
        private void InitializeDefaultConfigs()
        {
            // Додавання конфігурацій завантаження для різних фаз гри
            // Приклад:
            RegisterPhasePreload("Rune", "UI/RunePhaseUI", typeof(GameObject), 100, true, 1);
            RegisterPhasePreload("Combat", "Effects/CombatEffects", typeof(GameObject), 100, true, 5);
            RegisterPhasePreload("Combat", "Audio/CombatSounds", typeof(AudioClip), 50);

            // Додавання конфігурацій завантаження для різних сцен
            RegisterScenePreload("MainMenu", "UI/MainMenuUI", typeof(GameObject), 100);
            RegisterScenePreload("Gameplay", "Characters/PlayerModels", typeof(GameObject), 100, true, 3);
        }

        /// <summary>
        /// Реєструє ресурс для прееміптивного завантаження для вказаної фази
        /// </summary>
        public void RegisterPhasePreload<T>(string phaseId, string resourceKey, int priority = 0, bool createPool = false, int poolSize = 10) where T : UnityEngine.Object
        {
            RegisterPhasePreload(phaseId, resourceKey, typeof(T), priority, createPool, poolSize);
        }

        /// <summary>
        /// Реєструє ресурс для прееміптивного завантаження для вказаної фази
        /// </summary>
        public void RegisterPhasePreload(string phaseId, string resourceKey, Type resourceType, int priority = 0, bool createPool = false, int poolSize = 10)
        {
            if (!_phasePreloadConfigs.TryGetValue(phaseId, out var configs))
            {
                configs = new List<PreloadConfig>();
                _phasePreloadConfigs[phaseId] = configs;
            }

            configs.Add(new PreloadConfig
            {
                ResourceKey = resourceKey,
                ResourceType = resourceType,
                Priority = priority,
                CreatePool = createPool,
                PoolSize = poolSize
            });

            // Якщо це поточна фаза, зразу почати завантаження
            if (phaseId == _currentPhaseId)
            {
                PreloadResourceAsync(resourceKey, resourceType, createPool, poolSize).Forget();
            }
        }

        /// <summary>
        /// Реєструє ресурс для прееміптивного завантаження для вказаної сцени
        /// </summary>
        public void RegisterScenePreload<T>(string sceneName, string resourceKey, int priority = 0, bool createPool = false, int poolSize = 10) where T : UnityEngine.Object
        {
            RegisterScenePreload(sceneName, resourceKey, typeof(T), priority, createPool, poolSize);
        }

        /// <summary>
        /// Реєструє ресурс для прееміптивного завантаження для вказаної сцени
        /// </summary>
        public void RegisterScenePreload(string sceneName, string resourceKey, Type resourceType, int priority = 0, bool createPool = false, int poolSize = 10)
        {
            if (!_scenePreloadConfigs.TryGetValue(sceneName, out var configs))
            {
                configs = new List<PreloadConfig>();
                _scenePreloadConfigs[sceneName] = configs;
            }

            configs.Add(new PreloadConfig
            {
                ResourceKey = resourceKey,
                ResourceType = resourceType,
                Priority = priority,
                CreatePool = createPool,
                PoolSize = poolSize
            });

            // Якщо це поточна сцена, зразу почати завантаження
            if (sceneName == _currentScene)
            {
                PreloadResourceAsync(resourceKey, resourceType, createPool, poolSize).Forget();
            }
        }

        /// <summary>
        /// Обробник зміни фази
        /// </summary>
        private void OnPhaseChanged(string previousPhaseId, string currentPhaseId)
        {
            _currentPhaseId = currentPhaseId;
            PreloadForPhaseAsync(currentPhaseId).Forget();
        }

        /// <summary>
        /// Обробник завантаження сцени
        /// </summary>
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _currentScene = scene.name;
            PreloadForSceneAsync(scene.name).Forget();
        }

        /// <summary>
        /// Асинхронно завантажує ресурси для вказаної фази
        /// </summary>
        private async UniTask PreloadForPhaseAsync(string phaseId)
        {
            if (!_phasePreloadConfigs.TryGetValue(phaseId, out var configs) || configs.Count == 0)
                return;

            _logger.LogInfo($"Starting preloading for phase: {phaseId}", "Preload");

            // Сортуємо за пріоритетом
            var sortedConfigs = configs.OrderByDescending(c => c.Priority).ToList();

            // Послідовно завантажуємо ресурси з високим пріоритетом (> 50)
            foreach (var config in sortedConfigs.Where(c => c.Priority > 50))
            {
                await PreloadResourceAsync(config.ResourceKey, config.ResourceType, config.CreatePool, config.PoolSize);
            }

            // Паралельно завантажуємо ресурси з низьким пріоритетом
            var lowPriorityTasks = sortedConfigs
                .Where(c => c.Priority <= 50)
                .Select(c => PreloadResourceAsync(c.ResourceKey, c.ResourceType, c.CreatePool, c.PoolSize))
                .ToArray();

            await UniTask.WhenAll(lowPriorityTasks);

            _logger.LogInfo($"Completed preloading for phase: {phaseId}", "Preload");
        }

        /// <summary>
        /// Асинхронно завантажує ресурси для вказаної сцени
        /// </summary>
        private async UniTask PreloadForSceneAsync(string sceneName)
        {
            if (!_scenePreloadConfigs.TryGetValue(sceneName, out var configs) || configs.Count == 0)
                return;

            _logger.LogInfo($"Starting preloading for scene: {sceneName}", "Preload");

            // Сортуємо за пріоритетом
            var sortedConfigs = configs.OrderByDescending(c => c.Priority).ToList();

            // Послідовно завантажуємо ресурси з високим пріоритетом (> 50)
            foreach (var config in sortedConfigs.Where(c => c.Priority > 50))
            {
                await PreloadResourceAsync(config.ResourceKey, config.ResourceType, config.CreatePool, config.PoolSize);
            }

            // Паралельно завантажуємо ресурси з низьким пріоритетом
            var lowPriorityTasks = sortedConfigs
                .Where(c => c.Priority <= 50)
                .Select(c => PreloadResourceAsync(c.ResourceKey, c.ResourceType, c.CreatePool, c.PoolSize))
                .ToArray();

            await UniTask.WhenAll(lowPriorityTasks);

            _logger.LogInfo($"Completed preloading for scene: {sceneName}", "Preload");
        }

        /// <summary>
        /// Асинхронно завантажує вказаний ресурс
        /// </summary>
        private async UniTask PreloadResourceAsync(string resourceKey, Type resourceType, bool createPool, int poolSize)
        {
            try
            {
                // Використовуємо рефлексію для виклику типізованого методу LoadAsync
                var method = typeof(IResourceManager).GetMethod("LoadAsync").MakeGenericMethod(resourceType);
                var task = (UniTask<UnityEngine.Object>)method.Invoke(_resourceManager, new object[] { resourceKey });

                var result = await task;

                if (result == null)
                {
                    _logger.LogWarning($"Failed to preload resource: {resourceKey}", "Preload");
                    return;
                }

                _logger.LogDebug($"Preloaded resource: {resourceKey}", "Preload");

                // Створюємо пул, якщо потрібно
                if (createPool)
                {
                    // Використовуємо рефлексію для виклику типізованого методу InitializePoolAsync
                    var poolMethod = typeof(IResourceManager).GetMethod("InitializePoolAsync").MakeGenericMethod(resourceType);
                    var poolTask = (UniTask)poolMethod.Invoke(_resourceManager, new object[] { resourceKey, poolSize });

                    await poolTask;

                    _logger.LogDebug($"Created pool for resource: {resourceKey} with size {poolSize}", "Preload");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error preloading resource {resourceKey}: {ex.Message}", "Preload", ex);
            }
        }

        /// <summary>
        /// Звільнення ресурсів
        /// </summary>
        public void Dispose()
        {
            _phaseProvider.UnsubscribeFromPhaseChange(OnPhaseChanged);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
