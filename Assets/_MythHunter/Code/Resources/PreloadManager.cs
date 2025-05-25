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
using MythHunter.Resources.Config;

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
            public PreloadSceneConfig.LoadingMode LoadingMode;
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
        /// Реєструє ресурс для прееміптивного завантаження для вказаної фази (generic)
        /// </summary>
        public void RegisterPhasePreload<T>(string phaseId, string resourceKey, int priority = 0, bool createPool = false, int poolSize = 10) where T : UnityEngine.Object
        {
            RegisterPhasePreload(phaseId, resourceKey, typeof(T), priority, createPool, poolSize, PreloadSceneConfig.LoadingMode.SingleResource);
        }

        /// <summary>
        /// Реєструє ресурс для прееміптивного завантаження для вказаної фази
        /// </summary>
        public void RegisterPhasePreload(string phaseId, string resourceKey, Type resourceType, int priority = 0, bool createPool = false, int poolSize = 10, PreloadSceneConfig.LoadingMode loadingMode = PreloadSceneConfig.LoadingMode.SingleResource)
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
                LoadingMode = loadingMode,
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
                    LoadingMode = loadingMode,
                    Priority = priority,
                    CreatePool = createPool,
                    PoolSize = poolSize
                }).Forget();
            }
        }

        /// <summary>
        /// Реєструє ресурс для прееміптивного завантаження для вказаної сцени (generic)
        /// </summary>
        public void RegisterScenePreload<T>(string sceneName, string resourceKey, int priority = 0, bool createPool = false, int poolSize = 10, PreloadSceneConfig.LoadingMode loadingMode = PreloadSceneConfig.LoadingMode.SingleResource) where T : UnityEngine.Object
        {
            RegisterScenePreload(sceneName, resourceKey, typeof(T), priority, createPool, poolSize, loadingMode);
        }

        /// <summary>
        /// Реєструє ресурс для прееміптивного завантаження для вказаної сцени
        /// </summary>
        public void RegisterScenePreload(string sceneName, string resourceKey, Type resourceType, int priority = 0, bool createPool = false, int poolSize = 10, PreloadSceneConfig.LoadingMode loadingMode = PreloadSceneConfig.LoadingMode.SingleResource)
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
                LoadingMode = loadingMode,
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
                    LoadingMode = loadingMode,
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

                // Завантажуємо ресурси за конфігурацією
                var resources = await LoadResourcesByPathAsync(config.ResourceKey, config.ResourceType, config.LoadingMode);

                if (resources.Length == 0)
                {
                    // Публікуємо подію помилки
                    if (!contextName.StartsWith("Phase_"))
                    {
                        _eventBus.Publish(new PreloadResourceFailedEvent
                        {
                            SceneName = contextName,
                            ResourceKey = config.ResourceKey,
                            ErrorMessage = "Ресурси не знайдено",
                            Timestamp = DateTime.UtcNow
                        });
                    }

                    if (_sceneProgress.TryGetValue(contextName, out var progressData))
                    {
                        progressData.FailedResources.Add(config.ResourceKey);
                        _sceneProgress[contextName] = progressData;
                    }

                    _logger.LogWarning($"⚠️ Помилка preload ресурсів: {config.ResourceKey}", "Preload");
                }
                else
                {
                    _logger.LogInfo($"✅ Preload ресурсів: {config.ResourceKey} ({resources.Length} штук)", "Preload");

                    // Створюємо пули для GameObject
                    if (config.CreatePool && config.ResourceType == typeof(GameObject))
                    {
                        await CreatePoolsForGameObjectsAsync(config.ResourceKey, resources.OfType<GameObject>().ToArray(), config.PoolSize);
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

                _logger.LogError($"❌ Критична помилка preload ресурсів {config.ResourceKey}: {ex.Message}", "Preload", ex);
            }
        }

        /// <summary>
        /// Завантажує ресурси за шляхом залежно від режиму завантаження
        /// </summary>
        private async UniTask<UnityEngine.Object[]> LoadResourcesByPathAsync(string resourcePath, Type resourceType, PreloadSceneConfig.LoadingMode loadingMode)
        {
            try
            {
                switch (loadingMode)
                {
                    case PreloadSceneConfig.LoadingMode.SingleResource:
                        var singleResource = await LoadResourceByTypeAsync(resourcePath, resourceType);
                        return singleResource != null ? new[] { singleResource } : new UnityEngine.Object[0];

                    case PreloadSceneConfig.LoadingMode.AllFromFolder:
                        return await LoadAllFromFolderAsync(resourcePath);

                    case PreloadSceneConfig.LoadingMode.AllOfTypeFromFolder:
                        return await LoadAllOfTypeFromFolderAsync(resourcePath, resourceType);

                    default:
                        _logger.LogWarning($"⚠️ Невідомий режим завантаження: {loadingMode}", "Preload");
                        return new UnityEngine.Object[0];
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка завантаження ресурсів з {resourcePath}: {ex.Message}", "Preload", ex);
                return new UnityEngine.Object[0];
            }
        }

        /// <summary>
        /// Завантажує всі ресурси з папки
        /// </summary>
        private async UniTask<UnityEngine.Object[]> LoadAllFromFolderAsync(string folderPath)
        {
            _logger.LogInfo($"📁 Завантаження всіх ресурсів з папки: {folderPath}", "Preload");

            // ✅ ВИКОРИСТОВУЄМО UnityEngine.Resources замість Resources
            var resources = UnityEngine.Resources.LoadAll(folderPath);

            if (resources.Length == 0)
            {
                _logger.LogWarning($"⚠️ Папка {folderPath} порожня або не існує", "Preload");
            }
            else
            {
                _logger.LogInfo($"✅ Завантажено {resources.Length} ресурсів з папки {folderPath}", "Preload");
            }

            await UniTask.Yield(); // Даємо Unity обробити
            return resources;
        }

        /// <summary>
        /// Завантажує всі ресурси певного типу з папки
        /// </summary>
        private async UniTask<UnityEngine.Object[]> LoadAllOfTypeFromFolderAsync(string folderPath, Type resourceType)
        {
            _logger.LogInfo($"📁 Завантаження ресурсів типу {resourceType.Name} з папки: {folderPath}", "Preload");

            UnityEngine.Object[] resources;

            // ✅ ВИКОРИСТОВУЄМО UnityEngine.Resources
            if (resourceType == typeof(GameObject))
            {
                resources = UnityEngine.Resources.LoadAll<GameObject>(folderPath);
            }
            else if (resourceType == typeof(Sprite))
            {
                resources = UnityEngine.Resources.LoadAll<Sprite>(folderPath);
            }
            else if (resourceType == typeof(AudioClip))
            {
                resources = UnityEngine.Resources.LoadAll<AudioClip>(folderPath);
            }
            else if (resourceType == typeof(ScriptableObject) || resourceType.IsSubclassOf(typeof(ScriptableObject)))
            {
                // Для ScriptableObject і його нащадків
                if (resourceType == typeof(MythHunter.Entities.Archetypes.HeroArchetypeSO))
                {
                    resources = UnityEngine.Resources.LoadAll<MythHunter.Entities.Archetypes.HeroArchetypeSO>(folderPath);
                }
                else
                {
                    resources = UnityEngine.Resources.LoadAll<ScriptableObject>(folderPath);
                }
            }
            else if (resourceType == typeof(Material))
            {
                resources = UnityEngine.Resources.LoadAll<Material>(folderPath);
            }
            else if (resourceType == typeof(Texture2D))
            {
                resources = UnityEngine.Resources.LoadAll<Texture2D>(folderPath);
            }
            else
            {
                // Запасний варіант - загальне завантаження
                resources = UnityEngine.Resources.LoadAll(folderPath);

                // Фільтруємо за типом
                resources = resources.Where(r => resourceType.IsAssignableFrom(r.GetType())).ToArray();
            }

            if (resources.Length == 0)
            {
                _logger.LogWarning($"⚠️ Не знайдено ресурсів типу {resourceType.Name} в папці {folderPath}", "Preload");
            }
            else
            {
                _logger.LogInfo($"✅ Завантажено {resources.Length} ресурсів типу {resourceType.Name} з папки {folderPath}", "Preload");
            }

            await UniTask.Yield();
            return resources;
        }

        /// <summary>
        /// Створює пули для масиву GameObject
        /// </summary>
        private async UniTask CreatePoolsForGameObjectsAsync(string baseKey, GameObject[] gameObjects, int poolSize)
        {
            foreach (var gameObject in gameObjects)
            {
                if (gameObject == null)
                    continue;

                try
                {
                    string poolKey;

                    if (gameObjects.Length == 1 && gameObject.name == GetFileNameFromPath(baseKey))
                    {
                        // 🟢 Якщо одиночний префаб, і його ім’я збігається з назвою файлу → не додаємо нічого
                        poolKey = baseKey;
                    }
                    else
                    {
                        // 🔵 Якщо багато об’єктів — додаємо ім’я об’єкта до шляху
                        poolKey = $"{baseKey}/{gameObject.name}";
                    }

                    await _resourceManager.InitializePoolAsync<GameObject>(poolKey, poolSize);
                    _logger.LogDebug($"🏊 Створено пул для {poolKey} (розмір: {poolSize})", "Preload");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"❌ Помилка створення пулу для {gameObject.name}: {ex.Message}", "Preload", ex);
                }
            }
        }
        private string GetFileNameFromPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";

            var parts = path.Split('/');
            return parts.Length > 0 ? parts[^1] : path;
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
