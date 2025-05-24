// Шлях: Assets/_MythHunter/Code/Resources/PreloadManager.cs

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Resources.Core;
using MythHunter.Systems.Phase;
using MythHunter.Utils.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;
using MythHunter.Events.Domain.Preload;
using MythHunter.Core.ECS;

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
        private readonly IEventBus _eventBus;

        private readonly Dictionary<string, PreloadProgress> _sceneProgress = new();

        private string _currentPhaseId = string.Empty;
        private string _currentScene = string.Empty;

        private struct PreloadProgress
        {
            public int TotalResources;
            public int LoadedResources;
            public float StartTime;
            public List<string> FailedResources;
        }

        private struct PreloadConfig
        {
            public string ResourceKey;
            public Type ResourceType;
            public int Priority;
            public bool CreatePool;
            public int PoolSize;
        }

        [Inject]
        public PreloadManager(IResourceManager resourceManager, IPhaseProvider phaseProvider, IMythLogger logger, IEventBus eventBus)
        {
            _resourceManager = resourceManager;
            _phaseProvider = phaseProvider;
            _logger = logger;
            _eventBus = eventBus;

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
            // Базові конфігурації для різних фаз
            RegisterPhasePreload("Rune", "UI/RunePhaseUI", typeof(GameObject), 100, true, 1);
            RegisterPhasePreload("Combat", "Effects/CombatEffects", typeof(GameObject), 100, true, 5);
            RegisterPhasePreload("Combat", "Audio/CombatSounds", typeof(AudioClip), 50);

            // Базові конфігурації для сцен
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
                PreloadSingleResourceAsync($"Phase_{phaseId}", new PreloadConfig
                {
                    ResourceKey = resourceKey,
                    ResourceType = resourceType,
                    Priority = priority,
                    CreatePool = createPool,
                    PoolSize = poolSize
                }).Forget();
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
                PreloadSingleResourceAsync(sceneName, new PreloadConfig
                {
                    ResourceKey = resourceKey,
                    ResourceType = resourceType,
                    Priority = priority,
                    CreatePool = createPool,
                    PoolSize = poolSize
                }).Forget();
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

            _logger.LogInfo($"🔄 Початок preload для фази: {phaseId} ({configs.Count} ресурсів)", "Preload");

            // Сортуємо за пріоритетом
            var sortedConfigs = configs.OrderByDescending(c => c.Priority).ToList();
            string contextName = $"Phase_{phaseId}";

            // Ініціалізуємо прогрес для фази
            InitializeProgress(contextName, configs.Count);

            // Послідовно завантажуємо ресурси з високим пріоритетом (> 50)
            var highPriorityConfigs = sortedConfigs.Where(c => c.Priority > 50).ToList();
            foreach (var config in highPriorityConfigs)
            {
                await PreloadSingleResourceAsync(contextName, config);
            }

            // Паралельно завантажуємо ресурси з низьким пріоритетом
            var lowPriorityConfigs = sortedConfigs.Where(c => c.Priority <= 50).ToList();
            var lowPriorityTasks = lowPriorityConfigs
                .Select(c => PreloadSingleResourceAsync(contextName, c))
                .ToArray();

            await UniTask.WhenAll(lowPriorityTasks);

            // Завершуємо прогрес для фази
            CompleteProgress(contextName);

            _logger.LogInfo($"✅ Preload завершено для фази: {phaseId}", "Preload");
        }

        /// <summary>
        /// Асинхронно завантажує ресурси для вказаної сцени
        /// </summary>
        private async UniTask PreloadForSceneAsync(string sceneName)
        {
            if (!_scenePreloadConfigs.TryGetValue(sceneName, out var configs) || configs.Count == 0)
                return;

            _logger.LogInfo($"🔄 Початок preload для сцени: {sceneName} ({configs.Count} ресурсів)", "Preload");

            // Ініціалізуємо прогрес для сцени
            InitializeProgress(sceneName, configs.Count);

            // Публікуємо подію початку
            _eventBus.Publish(new PreloadStartedEvent
            {
                SceneName = sceneName,
                TotalResources = configs.Count,
                Timestamp = DateTime.UtcNow
            });

            // Сортуємо за пріоритетом
            var sortedConfigs = configs.OrderByDescending(c => c.Priority).ToList();

            // Послідовно завантажуємо ресурси з високим пріоритетом (> 50)
            var highPriorityConfigs = sortedConfigs.Where(c => c.Priority > 50).ToList();
            foreach (var config in highPriorityConfigs)
            {
                await PreloadSingleResourceAsync(sceneName, config);
            }

            // Паралельно завантажуємо ресурси з низьким пріоритетом
            var lowPriorityConfigs = sortedConfigs.Where(c => c.Priority <= 50).ToList();
            var lowPriorityTasks = lowPriorityConfigs
                .Select(c => PreloadSingleResourceAsync(sceneName, c))
                .ToArray();

            await UniTask.WhenAll(lowPriorityTasks);

            // Завершуємо прогрес для сцени
            CompleteProgress(sceneName);

            _logger.LogInfo($"✅ Preload завершено для сцени: {sceneName}", "Preload");
        }

        /// <summary>
        /// Ініціалізує відстеження прогресу
        /// </summary>
        private void InitializeProgress(string contextName, int totalResources)
        {
            _sceneProgress[contextName] = new PreloadProgress
            {
                TotalResources = totalResources,
                LoadedResources = 0,
                StartTime = Time.realtimeSinceStartup,
                FailedResources = new List<string>()
            };
        }

        /// <summary>
        /// Завершує відстеження прогресу та публікує подію
        /// </summary>
        private void CompleteProgress(string contextName)
        {
            if (!_sceneProgress.TryGetValue(contextName, out var progress))
                return;

            var totalTime = Time.realtimeSinceStartup - progress.StartTime;

            // Публікуємо подію завершення тільки для сцен
            if (!contextName.StartsWith("Phase_"))
            {
                _eventBus.Publish(new PreloadCompletedEvent
                {
                    SceneName = contextName,
                    TotalLoadedResources = progress.LoadedResources,
                    TotalTime = totalTime,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        /// <summary>
        /// Завантажує один ресурс з відстеженням прогресу
        /// </summary>
        private async UniTask PreloadSingleResourceAsync(string contextName, PreloadConfig config)
        {
            try
            {
                // Публікуємо оновлення прогресу (початок завантаження ресурсу)
                PublishProgressUpdate(contextName, config.ResourceKey);

                // Завантажуємо ресурс без рефлексії
                UnityEngine.Object result = await LoadResourceByTypeAsync(config.ResourceKey, config.ResourceType);

                if (result == null)
                {
                    // Публікуємо подію помилки
                    if (!contextName.StartsWith("Phase_"))
                    {
                        _eventBus.Publish(new PreloadResourceFailedEvent
                        {
                            SceneName = contextName,
                            ResourceKey = config.ResourceKey,
                            ErrorMessage = "Ресурс не знайдено",
                            Timestamp = DateTime.UtcNow
                        });
                    }

                    if (_sceneProgress.TryGetValue(contextName, out var progressData))
                    {
                        progressData.FailedResources.Add(config.ResourceKey);
                        _sceneProgress[contextName] = progressData;
                    }

                    _logger.LogWarning($"⚠️ Помилка preload ресурсу: {config.ResourceKey}", "Preload");
                }
                else
                {
                    _logger.LogDebug($"✅ Preload ресурсу: {config.ResourceKey}", "Preload");

                    // Створюємо пул, якщо потрібно (тільки для GameObject)
                    if (config.CreatePool && config.ResourceType == typeof(GameObject))
                    {
                        try
                        {
                            await _resourceManager.InitializePoolAsync<GameObject>(config.ResourceKey, config.PoolSize);
                            _logger.LogDebug($"🏊 Створено пул для ресурсу: {config.ResourceKey} (розмір: {config.PoolSize})", "Preload");
                        }
                        catch (Exception poolEx)
                        {
                            _logger.LogError($"❌ Помилка створення пулу для {config.ResourceKey}: {poolEx.Message}", "Preload", poolEx);
                        }
                    }
                    else if (config.CreatePool && config.ResourceType != typeof(GameObject))
                    {
                        _logger.LogWarning($"⚠️ Створення пулу підтримується тільки для GameObject. Ресурс: {config.ResourceKey} ({config.ResourceType.Name})", "Preload");
                    }
                }

                // Оновлюємо прогрес
                if (_sceneProgress.TryGetValue(contextName, out var finalProgress))
                {
                    finalProgress.LoadedResources++;
                    _sceneProgress[contextName] = finalProgress;
                }

                // Публікуємо фінальне оновлення прогресу для цього ресурсу
                PublishProgressUpdate(contextName, config.ResourceKey, true);

            }
            catch (Exception ex)
            {
                // Публікуємо подію помилки
                if (!contextName.StartsWith("Phase_"))
                {
                    _eventBus.Publish(new PreloadResourceFailedEvent
                    {
                        SceneName = contextName,
                        ResourceKey = config.ResourceKey,
                        ErrorMessage = ex.Message,
                        Timestamp = DateTime.UtcNow
                    });
                }

                _logger.LogError($"❌ Критична помилка preload ресурсу {config.ResourceKey}: {ex.Message}", "Preload", ex);
            }
        }

        /// <summary>
        /// Завантажує ресурс за типом без рефлексії
        /// </summary>
        private async UniTask<UnityEngine.Object> LoadResourceByTypeAsync(string resourceKey, Type resourceType)
        {
            if (resourceType == typeof(GameObject))
            {
                return await _resourceManager.LoadAsync<GameObject>(resourceKey);
            }
            else if (resourceType == typeof(Sprite))
            {
                return await _resourceManager.LoadAsync<Sprite>(resourceKey);
            }
            else if (resourceType == typeof(AudioClip))
            {
                return await _resourceManager.LoadAsync<AudioClip>(resourceKey);
            }
            else if (resourceType == typeof(ScriptableObject) || resourceType.IsSubclassOf(typeof(ScriptableObject)))
            {
                return await _resourceManager.LoadAsync<ScriptableObject>(resourceKey);
            }
            else if (resourceType == typeof(Material))
            {
                return await _resourceManager.LoadAsync<Material>(resourceKey);
            }
            else if (resourceType == typeof(Texture2D))
            {
                return await _resourceManager.LoadAsync<Texture2D>(resourceKey);
            }
            else
            {
                // Запасний варіант для інших типів
                return await _resourceManager.LoadAsync<UnityEngine.Object>(resourceKey);
            }
        }

        /// <summary>
        /// Публікує оновлення прогресу (тільки для сцен)
        /// </summary>
        private void PublishProgressUpdate(string contextName, string currentResource, bool isCompleted = false)
        {
            // Публікуємо прогрес тільки для сцен, не для фаз
            if (contextName.StartsWith("Phase_") || !_sceneProgress.TryGetValue(contextName, out var progress))
                return;

            float progressValue = progress.TotalResources > 0 ?
                (float)progress.LoadedResources / progress.TotalResources : 0f;

            _eventBus.Publish(new PreloadProgressUpdatedEvent
            {
                SceneName = contextName,
                Progress = progressValue,
                LoadedResources = progress.LoadedResources,
                TotalResources = progress.TotalResources,
                CurrentResource = isCompleted ? $"✅ {currentResource}" : $"🔄 {currentResource}",
                Timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Звільнення ресурсів
        /// </summary>
        public void Dispose()
        {
            _phaseProvider.UnsubscribeFromPhaseChange(OnPhaseChanged);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _sceneProgress.Clear();
        }
    }
}
