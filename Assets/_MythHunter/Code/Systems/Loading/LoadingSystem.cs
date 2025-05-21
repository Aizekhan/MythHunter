// Assets/_MythHunter/Code/Systems/Loading/LoadingSystem.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Events.Domain.Loading;
using MythHunter.Resources.Core;
using MythHunter.Resources.Pool;
using MythHunter.Systems.Core;
using MythHunter.Utils.Logging;
using MythHunter.Entities;
using MythHunter.Entities.Archetypes;
using UnityEngine;

namespace MythHunter.Systems.Loading
{
    /// <summary>
    /// Система для керування процесом завантаження ресурсів та підготовки ігрової сцени
    /// </summary>
    [SystemCategory(SystemInitializationCategory.OnDemand)]
    public class LoadingSystem : SystemBase, ILoadingSystem
    {
        private readonly IResourceManager _resourceManager;
        private readonly IPoolManager _poolManager;
        private readonly IEntityManager _entityManager;
        private readonly IArchetypeSystem _archetypeSystem;
        private readonly ISystemRegistry _systemRegistry;
        private readonly IEventThrottler _eventThrottler;

        private float _loadingProgress = 0f;
        private string _loadingStatus = "Готуємося до завантаження...";
        private LoadingStage _currentStage = LoadingStage.None;
        private bool _isLoadingComplete = false;
        private bool _isLoadingInProgress = false;
        private UniTaskCompletionSource<bool> _loadingCompletionSource;

        private Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();
        private List<string> _createdEntityIds = new List<string>();
        private string[] _selectedHeroArchetypes;
        private string _mapId;

        [Inject]
        public LoadingSystem(
            IResourceManager resourceManager,
            IPoolManager poolManager,
            IEntityManager entityManager,
            IArchetypeSystem archetypeSystem,
            ISystemRegistry systemRegistry,
            IEventBus eventBus,
            IEventThrottler eventThrottler,
            IMythLogger logger)
            : base(logger, eventBus)
        {
            _resourceManager = resourceManager;
            _poolManager = poolManager;
            _entityManager = entityManager;
            _archetypeSystem = archetypeSystem;
            _systemRegistry = systemRegistry;
            _eventThrottler = eventThrottler;

            // Реєстрація обмеження для подій прогресу (максимум 4 рази на секунду)
            _eventThrottler.RegisterThrottle<LoadingProgressEvent>(0.25f);
        }

        public override void Initialize()
        {
            base.Initialize();
            SubscribeToEvents();
            _logger.LogInfo("LoadingSystem ініціалізовано", "Loading");
        }

        protected override void OnSubscribeToEvents()
        {
            Subscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        protected override void OnUnsubscribeFromEvents()
        {
            Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        }

        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            if (evt.NewState == GameStateType.Loading)
            {
                _logger.LogInfo("Виявлено перехід до стану завантаження", "Loading");
            }
        }

        public override void Update(float deltaTime)
        {
            // Оновлення логіки завантаження, якщо потрібно
            if (_isLoadingInProgress && !_isLoadingComplete)
            {
                // Наприклад, оновлення прогресу для асинхронного завантаження сцен
                UpdateLoadingProgress(deltaTime);
            }
        }

        private void UpdateLoadingProgress(float deltaTime)
        {
            // Логіка оновлення може бути порожньою, якщо весь процес завантаження асинхронний
            // Тут може бути код для моніторингу зовнішніх процесів завантаження, якщо такі є
        }

        public async UniTask<bool> StartLoadingGameAsync(string[] selectedHeroArchetypes, string mapId = "default")
        {
            if (_isLoadingInProgress)
            {
                _logger.LogWarning("Спроба почати завантаження, коли процес вже виконується", "Loading");
                return false;
            }

            _isLoadingInProgress = true;
            _isLoadingComplete = false;
            _loadingProgress = 0f;
            _loadingStatus = "Початок завантаження...";
            _currentStage = LoadingStage.None;
            _loadingCompletionSource = new UniTaskCompletionSource<bool>();
            _selectedHeroArchetypes = selectedHeroArchetypes;
            _mapId = mapId;
            _prefabCache.Clear();
            _createdEntityIds.Clear();

            // Публікуємо подію початку завантаження
            Publish(new LoadingStartedEvent
            {
                SelectedHeroArchetypes = selectedHeroArchetypes,
                MapId = mapId,
                Timestamp = DateTime.UtcNow
            });

            try
            {
                // Запускаємо процес завантаження
                await ExecuteLoadingSequenceAsync();
                return await _loadingCompletionSource.Task;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при завантаженні: {ex.Message}", "Loading", ex);

                // Публікуємо подію помилки завантаження
                Publish(new LoadingErrorEvent
                {
                    ErrorMessage = ex.Message,
                    Stage = _currentStage,
                    Timestamp = DateTime.UtcNow
                });

                _isLoadingInProgress = false;
                _loadingCompletionSource.TrySetResult(false);
                return false;
            }
        }

        private async UniTask ExecuteLoadingSequenceAsync()
        {
            try
            {
                // Етап 1: Підготовка ресурсів
                await ExecuteLoadingStageAsync(LoadingStage.PreparingResources, 0.1f,
                    "Підготовка ресурсів...", PrepareResourcesAsync);

                // Етап 2: Завантаження префабів героїв
                await ExecuteLoadingStageAsync(LoadingStage.LoadingHeroPrefabs, 0.2f,
                    "Завантаження героїв...", LoadHeroPrefabsAsync);

                // Етап 3: Завантаження даних карти
                await ExecuteLoadingStageAsync(LoadingStage.LoadingMapData, 0.2f,
                    "Завантаження карти...", LoadMapDataAsync);

                // Етап 4: Ініціалізація пулів об'єктів
                await ExecuteLoadingStageAsync(LoadingStage.InitializingPools, 0.1f,
                    "Підготовка пулів об'єктів...", InitializePoolsAsync);

                // Етап 5: Створення сутностей
                await ExecuteLoadingStageAsync(LoadingStage.CreatingEntities, 0.2f,
                    "Створення сутностей...", CreateEntitiesAsync);

                // Етап 6: Налаштування систем
                await ExecuteLoadingStageAsync(LoadingStage.SettingUpSystems, 0.1f,
                    "Налаштування ігрових систем...", SetupSystemsAsync);

                // Етап 7: Фінальне налаштування
                await ExecuteLoadingStageAsync(LoadingStage.FinalSetup, 0.1f,
                    "Завершення налаштування...", FinalSetupAsync);

                // Завершення завантаження
                _isLoadingComplete = true;
                _loadingProgress = 1.0f;
                _loadingStatus = "Завантаження завершено";

                // Публікуємо подію завершення завантаження
                Publish(new LoadingCompletedEvent
                {
                    Success = true,
                    CreatedEntityIds = _createdEntityIds.ToArray(),
                    Timestamp = DateTime.UtcNow
                });

                _loadingCompletionSource.TrySetResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка в процесі завантаження: {ex.Message}", "Loading", ex);
                _isLoadingComplete = false;
                _loadingCompletionSource.TrySetResult(false);
                throw;
            }
        }

        private async UniTask ExecuteLoadingStageAsync(
            LoadingStage stage,
            float progressWeight,
            string status,
            Func<UniTask> stageFunction)
        {
            _currentStage = stage;
            _loadingStatus = status;

            // Публікуємо подію оновлення прогресу
            PublishProgressUpdate(_loadingProgress, status, stage);

            // Виконуємо етап завантаження
            await stageFunction();

            // Оновлюємо прогрес
            _loadingProgress += progressWeight;
            _loadingProgress = Mathf.Clamp01(_loadingProgress);

            // Публікуємо оновлений прогрес
            PublishProgressUpdate(_loadingProgress, _loadingStatus, stage);

            // Невелика затримка для візуалізації
            await UniTask.Delay(50);
        }

        private void PublishProgressUpdate(float progress, string status, LoadingStage stage)
        {
            // Публікуємо через обмежувач частоти
            _eventThrottler.PublishThrottled(new LoadingProgressEvent
            {
                Progress = progress,
                Status = status,
                Stage = stage,
                Timestamp = DateTime.UtcNow
            });
        }

        private async UniTask PrepareResourcesAsync()
        {
            _logger.LogInfo("Підготовка ресурсів...", "Loading");

            // Очищаємо невикористані ресурси
            await _resourceManager.UnloadAllAsync();

            // Підготовка інших загальних ресурсів
            await UniTask.Delay(100); // Симуляція роботи
        }

        private async UniTask LoadHeroPrefabsAsync()
        {
            _logger.LogInfo($"Завантаження префабів героїв: {string.Join(", ", _selectedHeroArchetypes)}", "Loading");

            float progressStep = 1.0f / (_selectedHeroArchetypes.Length + 1);
            float baseProgress = _loadingProgress;

            for (int i = 0; i < _selectedHeroArchetypes.Length; i++)
            {
                string archetypeId = _selectedHeroArchetypes[i];
                _loadingStatus = $"Завантаження героя {i + 1}/{_selectedHeroArchetypes.Length}...";

                try
                {
                    // Отримуємо шлях до префаба для архетипу
                    string prefabPath = GetPrefabPathForArchetype(archetypeId);

                    // Завантажуємо префаб
                    var prefab = await _resourceManager.LoadAsync<GameObject>(prefabPath);

                    if (prefab != null)
                    {
                        _prefabCache[archetypeId] = prefab;
                        _logger.LogInfo($"Завантажено префаб {prefabPath} для архетипу {archetypeId}", "Loading");
                    }
                    else
                    {
                        _logger.LogWarning($"Не вдалося завантажити префаб для архетипу {archetypeId}", "Loading");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Помилка при завантаженні префабу для архетипу {archetypeId}: {ex.Message}", "Loading", ex);
                }

                // Оновлюємо прогрес
                float newProgress = baseProgress + progressStep * (i + 1);
                PublishProgressUpdate(newProgress, _loadingStatus, _currentStage);

                // Невелика затримка для візуалізації
                await UniTask.Delay(50);
            }
        }

        private string GetPrefabPathForArchetype(string archetypeId)
        {
            // Логіка визначення шляху до префабу за ідентифікатором архетипу
            // В реальній реалізації це може бути більш складна логіка
            return $"Prefabs/Heroes/{archetypeId}";
        }

        private async UniTask LoadMapDataAsync()
        {
            _logger.LogInfo($"Завантаження даних карти: {_mapId}", "Loading");

            // Завантаження даних карти
            string mapPrefabPath = $"Prefabs/Maps/{_mapId}";

            try
            {
                var mapPrefab = await _resourceManager.LoadAsync<GameObject>(mapPrefabPath);

                if (mapPrefab != null)
                {
                    _prefabCache["map"] = mapPrefab;
                    _logger.LogInfo($"Завантажено префаб карти {mapPrefabPath}", "Loading");
                }
                else
                {
                    _logger.LogWarning($"Не вдалося завантажити префаб карти {mapPrefabPath}", "Loading");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при завантаженні префабу карти: {ex.Message}", "Loading", ex);
            }

            // Симулюємо завантаження даних
            _loadingStatus = "Обробка даних карти...";
            PublishProgressUpdate(_loadingProgress + 0.1f, _loadingStatus, _currentStage);
            await UniTask.Delay(100);
        }

        private async UniTask InitializePoolsAsync()
        {
            _logger.LogInfo("Ініціалізація пулів об'єктів...", "Loading");

            // Ініціалізуємо пули для героїв
            foreach (var entry in _prefabCache)
            {
                if (entry.Key != "map")
                {
                    string poolKey = $"Hero_{entry.Key}";
                    await _resourceManager.InitializePoolAsync<GameObject>(poolKey, entry.Value, 2);
                    _logger.LogInfo($"Створено пул для героя {entry.Key}", "Loading");
                }
            }

            // Ініціалізуємо загальні пули для ігрових об'єктів
            await InitializeCommonPoolsAsync();

            // Видаляємо зайві об'єкти з існуючих пулів
            _poolManager.TrimExcessObjects(10);
        }

        private async UniTask InitializeCommonPoolsAsync()
        {
            // Тут ініціалізуються загальні пули для ігрових об'єктів
            string[] commonPrefabPaths = new string[]
            {
                "Prefabs/Effects/HitEffect",
                "Prefabs/Items/Chest",
                "Prefabs/VFX/RuneActivation"
            };

            for (int i = 0; i < commonPrefabPaths.Length; i++)
            {
                string path = commonPrefabPaths[i];
                try
                {
                    // Завантажуємо префаб
                    var prefab = await _resourceManager.LoadAsync<GameObject>(path);

                    // Створюємо пул
                    if (prefab != null)
                    {
                        string poolKey = $"Common_{Path.GetFileNameWithoutExtension(path)}";
                        _poolManager.CreatePool<GameObject>(poolKey, prefab, 5);
                        _logger.LogInfo($"Створено загальний пул для {path}", "Loading");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Не вдалося створити пул для {path}: {ex.Message}", "Loading");
                }

                // Оновлюємо статус
                _loadingStatus = $"Підготовка ігрових об'єктів {i + 1}/{commonPrefabPaths.Length}...";
                PublishProgressUpdate(_loadingProgress, _loadingStatus, _currentStage);

                await UniTask.Delay(50);
            }
        }

        private async UniTask CreateEntitiesAsync()
        {
            _logger.LogInfo("Створення ігрових сутностей...", "Loading");

            // Створюємо карту
            int mapEntityId = CreateMapEntity();
            if (mapEntityId >= 0)
            {
                _createdEntityIds.Add(mapEntityId.ToString());
            }

            // Створюємо героїв
            for (int i = 0; i < _selectedHeroArchetypes.Length; i++)
            {
                string archetypeId = _selectedHeroArchetypes[i];
                _loadingStatus = $"Створення героя {i + 1}/{_selectedHeroArchetypes.Length}...";
                PublishProgressUpdate(_loadingProgress, _loadingStatus, _currentStage);

                // Створюємо сутність героя
                int heroEntityId = await CreateHeroEntityAsync(archetypeId, i);
                if (heroEntityId >= 0)
                {
                    _createdEntityIds.Add(heroEntityId.ToString());
                }

                await UniTask.Delay(50);
            }

            _logger.LogInfo($"Створено {_createdEntityIds.Count} сутностей", "Loading");
        }

        private int CreateMapEntity()
        {
            try
            {
                // Створюємо сутність карти через систему архетипів
                int mapEntityId = _archetypeSystem.CreateEntityFromArchetype("Map", null);

                if (mapEntityId >= 0)
                {
                    _logger.LogInfo($"Створено сутність карти з ID {mapEntityId}", "Loading");
                    return mapEntityId;
                }
                else
                {
                    _logger.LogWarning("Не вдалося створити сутність карти", "Loading");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при створенні сутності карти: {ex.Message}", "Loading", ex);
                return -1;
            }
        }

        private async UniTask<int> CreateHeroEntityAsync(string archetypeId, int index)
        {
            try
            {
                // Створюємо сутність героя через систему архетипів
                int heroEntityId = _archetypeSystem.CreateEntityFromArchetype(archetypeId, null);

                if (heroEntityId >= 0)
                {
                    _logger.LogInfo($"Створено сутність героя {archetypeId} з ID {heroEntityId}", "Loading");

                    // Додаємо додаткові компоненти або налаштування
                    // ...

                    return heroEntityId;
                }
                else
                {
                    _logger.LogWarning($"Не вдалося створити сутність героя {archetypeId}", "Loading");
                    return -1;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при створенні сутності героя {archetypeId}: {ex.Message}", "Loading", ex);
                return -1;
            }
        }

        private async UniTask SetupSystemsAsync()
        {
            _logger.LogInfo("Налаштування ігрових систем...", "Loading");

            try
            {
                // Ініціалізуємо системи категорії Gameplay
                _systemRegistry.InitializeSystemsByCategory(SystemInitializationCategory.Gameplay);

                // Симулюємо роботу
                _loadingStatus = "Налаштування ігрової логіки...";
                PublishProgressUpdate(_loadingProgress, _loadingStatus, _currentStage);
                await UniTask.Delay(100);

                _logger.LogInfo("Ігрові системи успішно налаштовано", "Loading");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при налаштуванні ігрових систем: {ex.Message}", "Loading", ex);
                throw;
            }
        }

        private async UniTask FinalSetupAsync()
        {
            _logger.LogInfo("Завершення налаштування...", "Loading");

            _loadingStatus = "Оптимізація ресурсів...";
            PublishProgressUpdate(_loadingProgress, _loadingStatus, _currentStage);

            // Очищаємо кеш завантажених префабів
            _prefabCache.Clear();

            // Очищаємо надлишкові ресурси
            await _resourceManager.CleanupMemoryAsync();

            _loadingStatus = "Завантаження завершено";
            PublishProgressUpdate(_loadingProgress, _loadingStatus, _currentStage);

            _logger.LogInfo("Фінальне налаштування завершено", "Loading");
        }

        public float GetLoadingProgress()
        {
            return _loadingProgress;
        }

        public string GetLoadingStatus()
        {
            return _loadingStatus;
        }

        public bool IsLoadingComplete()
        {
            return _isLoadingComplete;
        }

        public async UniTask FinishLoadingAsync()
        {
            if (!_isLoadingComplete)
            {
                _logger.LogWarning("Спроба завершити завантаження, яке ще не закінчилося", "Loading");
                return;
            }

            _logger.LogInfo("Завершення процесу завантаження", "Loading");

            // Публікуємо фінальне оновлення прогресу
            Publish(new LoadingProgressEvent
            {
                Progress = 1.0f,
                Status = "Перехід до гри...",
                Stage = LoadingStage.FinalSetup,
                Timestamp = DateTime.UtcNow
            });

            // Очищаємо стан
            _isLoadingInProgress = false;
            _selectedHeroArchetypes = null;
            _mapId = null;
        }

        public async UniTask CancelLoadingAsync()
        {
            if (!_isLoadingInProgress)
            {
                return;
            }

            _logger.LogInfo("Скасування процесу завантаження", "Loading");

            // Відміняємо процес завантаження
            _isLoadingInProgress = false;
            _isLoadingComplete = false;
            _loadingCompletionSource.TrySetResult(false);

            // Очищаємо ресурси
            _prefabCache.Clear();

            // Публікуємо подію скасування завантаження
            Publish(new LoadingProgressEvent
            {
                Progress = 0f,
                Status = "Завантаження скасовано",
                Stage = _currentStage,
                Timestamp = DateTime.UtcNow
            });

            await UniTask.Delay(100); // Затримка для відображення повідомлення
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            _prefabCache.Clear();
            _createdEntityIds.Clear();
            _isLoadingInProgress = false;
            _isLoadingComplete = false;
            base.Dispose();
        }
        // Допоміжний клас для роботи з шляхами
        private static class Path
        {
            public static string GetFileNameWithoutExtension(string path)
            {
                if (string.IsNullOrEmpty(path))
                    return string.Empty;

                int lastSlashIndex = path.LastIndexOf('/');
                int lastDotIndex = path.LastIndexOf('.');

                if (lastSlashIndex < 0)
                    lastSlashIndex = -1;

                if (lastDotIndex < 0 || lastDotIndex < lastSlashIndex)
                    lastDotIndex = path.Length;

                return path.Substring(lastSlashIndex + 1, lastDotIndex - lastSlashIndex - 1);
            }
        }
    }
   
}


