// Шлях: Assets/_MythHunter/Code/Resources/PreloadManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Resources.Core;
using MythHunter.Utils.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MythHunter.Resources
{
    /// <summary>
    /// Менеджер для прееміптивного завантаження ресурсів
    /// </summary>
    public class PreloadManager : IPreloadManager, IEventSubscriber
    {
        private readonly Dictionary<GamePhase, List<PreloadConfig>> _phasePreloadConfigs = new Dictionary<GamePhase, List<PreloadConfig>>();
        private readonly Dictionary<string, List<PreloadConfig>> _scenePreloadConfigs = new Dictionary<string, List<PreloadConfig>>();

        private readonly IResourceManager _resourceManager;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        private bool _isSubscribed = false;
        private GamePhase _currentPhase = GamePhase.None;
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
        public PreloadManager(IResourceManager resourceManager, IEventBus eventBus, IMythLogger logger)
        {
            _resourceManager = resourceManager;
            _eventBus = eventBus;
            _logger = logger;

            SubscribeToEvents();
            InitializeDefaultConfigs();
        }

        public void SubscribeToEvents()
        {
            if (_isSubscribed)
                return;

            _eventBus.Subscribe<PhaseChangedEvent>(OnPhaseChanged);
            SceneManager.sceneLoaded += OnSceneLoaded;

            _isSubscribed = true;
        }

        public void UnsubscribeFromEvents()
        {
            if (!_isSubscribed)
                return;

            _eventBus.Unsubscribe<PhaseChangedEvent>(OnPhaseChanged);
            SceneManager.sceneLoaded -= OnSceneLoaded;

            _isSubscribed = false;
        }

        private void OnPhaseChanged(PhaseChangedEvent evt)
        {
            _currentPhase = evt.CurrentPhase;
            PreloadForPhaseAsync(evt.CurrentPhase).Forget();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _currentScene = scene.name;
            PreloadForSceneAsync(scene.name).Forget();
        }

        /// <summary>
        /// Ініціалізує стандартні конфігурації прееміптивного завантаження
        /// </summary>
        private void InitializeDefaultConfigs()
        {
            // Додавання конфігурацій завантаження для різних фаз гри
            // Приклад:
            RegisterPhasePreload(GamePhase.Rune, "UI/RunePhaseUI", typeof(GameObject), 100, true, 1);
            RegisterPhasePreload(GamePhase.Combat, "Effects/CombatEffects", typeof(GameObject), 100, true, 5);
            RegisterPhasePreload(GamePhase.Combat, "Audio/CombatSounds", typeof(AudioClip), 50);

            // Додавання конфігурацій завантаження для різних сцен
            RegisterScenePreload("MainMenu", "UI/MainMenuUI", typeof(GameObject), 100);
            RegisterScenePreload("Gameplay", "Characters/PlayerModels", typeof(GameObject), 100, true, 3);
        }

        /// <summary>
        /// Реєструє ресурс для прееміптивного завантаження для вказаної фази
        /// </summary>
        public void RegisterPhasePreload<T>(GamePhase phase, string resourceKey, int priority = 0, bool createPool = false, int poolSize = 10) where T : UnityEngine.Object
        {
            RegisterPhasePreload(phase, resourceKey, typeof(T), priority, createPool, poolSize);
        }

        /// <summary>
        /// Реєструє ресурс для прееміптивного завантаження для вказаної фази
        /// </summary>
        public void RegisterPhasePreload(GamePhase phase, string resourceKey, Type resourceType, int priority = 0, bool createPool = false, int poolSize = 10)
        {
            if (!_phasePreloadConfigs.TryGetValue(phase, out var configs))
            {
                configs = new List<PreloadConfig>();
                _phasePreloadConfigs[phase] = configs;
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
            if (phase == _currentPhase)
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
        /// Асинхронно завантажує ресурси для вказаної фази
        /// </summary>
        private async UniTask PreloadForPhaseAsync(GamePhase phase)
        {
            if (!_phasePreloadConfigs.TryGetValue(phase, out var configs) || configs.Count == 0)
                return;

            _logger.LogInfo($"Starting preloading for phase: {phase}", "Preload");

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

            _logger.LogInfo($"Completed preloading for phase: {phase}", "Preload");
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
    }
}
