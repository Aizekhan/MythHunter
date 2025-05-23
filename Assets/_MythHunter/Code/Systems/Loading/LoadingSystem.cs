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
using MythHunter.Core.Game;
using MythHunter.Resources;
using MythHunter.Systems.Lobby;
using MythHunter.UI.Core;
using MythHunter.UI.ViewConfigs;
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
        private readonly IDIContainer _container;
        private readonly IPrefabProvider _prefabProvider;
       

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
            IMythLogger logger,
            IDIContainer container,
            IPrefabProvider prefabProvider)
            : base(logger, eventBus)
        {
            _resourceManager = resourceManager;
            _poolManager = poolManager;
            _entityManager = entityManager;
            _archetypeSystem = archetypeSystem;
            _systemRegistry = systemRegistry;
            _eventThrottler = eventThrottler;
            _container = container;

            // Реєстрація обмеження для подій прогресу (максимум 4 рази на секунду)
            _eventThrottler.RegisterThrottle<LoadingProgressEvent>(0.25f);
            _container = container;
            _prefabProvider = prefabProvider;
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
            _logger.LogInfo("🔄 LoadingSystem: Початок завантаження", "Loading");

            if (_isLoadingInProgress)
                return false;

            _isLoadingInProgress = true;
            _isLoadingComplete = false;
            _selectedHeroArchetypes = selectedHeroArchetypes ?? Array.Empty<string>();
            _mapId = mapId;

            // Визначаємо тип завантаження
            LoadingType loadingType = DetermineLoadingType(mapId, selectedHeroArchetypes);

            _logger.LogInfo($"🎯 Тип завантаження: {loadingType}", "Loading");

            try
            {
                bool result = loadingType switch
                {
                    LoadingType.LobbyPreparation => await ExecuteLobbyLoadingAsync(),
                    LoadingType.GameplayFull => await ExecuteGameplayLoadingAsync(),
                    _ => await ExecuteMinimalLoadingAsync()
                };

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ LoadingSystem помилка: {ex.Message}", "Loading", ex);
                return false;
            }
        }
        private LoadingType DetermineLoadingType(string mapId, string[] heroArchetypes)
        {
            // Якщо це перехід до лобі - потрібно підготувати все для вибору героїв
            if (mapId.Contains("lobby", StringComparison.OrdinalIgnoreCase) ||
                heroArchetypes.Length == 0)
            {
                return LoadingType.LobbyPreparation;
            }

            // Якщо є вибрані герої - повне завантаження для гри
            if (heroArchetypes.Length > 0)
            {
                return LoadingType.GameplayFull;
            }

            return LoadingType.Minimal;
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


        /// <summary>
        /// Завантаження для Lobby - підготовка вибору героїв
        /// </summary>
        private async UniTask<bool> ExecuteLobbyLoadingAsync()
        {
            _logger.LogInfo("🏠 LoadingSystem: Підготовка Lobby сцени", "Loading");

            try
            {
                // Етап 1: Підготовка базових ресурсів (20%)
                await ExecuteLoadingStageAsync(LoadingStage.PreparingResources, 0.2f,
                    "Підготовка системи...", PrepareBasicResourcesAsync);

                // ✅ ЗАТРИМКА для візуалізації
                await UniTask.Delay(300);

                // Етап 2: Завантаження архетипів героїв (40%)
                await ExecuteLoadingStageAsync(LoadingStage.LoadingHeroPrefabs, 0.4f,
                    "Завантаження архетипів героїв...", LoadAllHeroArchetypesAsync);

                // ✅ ЗАТРИМКА
                await UniTask.Delay(300);

                // Етап 3: Підготовка UI ресурсів (20%)
                await ExecuteLoadingStageAsync(LoadingStage.LoadingMapData, 0.2f,
                    "Підготовка UI лобі...", LoadLobbyUIResourcesAsync);

                // ✅ ЗАТРИМКА
                await UniTask.Delay(200);

                // Етап 4: Ініціалізація пулів (10%)
                await ExecuteLoadingStageAsync(LoadingStage.InitializingPools, 0.1f,
                    "Підготовка карток героїв...", InitializeLobbyPoolsAsync);

                // ✅ ЗАТРИМКА
                await UniTask.Delay(200);

                // Етап 5: Фінальне налаштування (10%)
                await ExecuteLoadingStageAsync(LoadingStage.FinalSetup, 0.1f,
                    "Завершення підготовки...", FinalLobbySetupAsync);

                // ✅ ОБОВ'ЯЗКОВА ЗАТРИМКА перед завершенням
                await UniTask.Delay(500);

                _isLoadingComplete = true;
                _loadingProgress = 1.0f;

                Publish(new LoadingCompletedEvent
                {
                    Success = true,
                    CreatedEntityIds = Array.Empty<string>(),
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInfo("✅ LoadingSystem: Lobby підготовлено", "Loading");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Помилка підготовки Lobby: {ex.Message}", "Loading", ex);
                return false;
            }
        }
        /// <summary>
        /// Завантаження всіх архетипів героїв для показу в лобі
        /// </summary>
        private async UniTask LoadAllHeroArchetypesAsync()
        {
            _logger.LogInfo("📋 Завантаження всіх архетипів героїв...", "Loading");

            try
            {
                // Отримуємо всі архетипи героїв з ресурсів
                var heroArchetypes = await _resourceManager.LoadAllAsync<HeroArchetypeSO>("ScriptableObjects/Heroes");

                _logger.LogInfo($"Знайдено {heroArchetypes.Count} архетипів героїв", "Loading");

                // Завантажуємо іконки для всіх героїв
                float progressStep = 1.0f / (heroArchetypes.Count + 1);

                for (int i = 0; i < heroArchetypes.Count; i++)
                {
                    var archetype = heroArchetypes[i];
                    _loadingStatus = $"Завантаження героя {archetype.HeroName}...";

                    try
                    {
                        // Завантажуємо іконку героя
                        if (!string.IsNullOrEmpty(archetype.IconPath))
                        {
                            var icon = await _resourceManager.LoadAsync<Sprite>(archetype.IconPath);
                            if (icon != null)
                            {
                                _logger.LogDebug($"Завантажено іконку для {archetype.HeroName}", "Loading");
                            }
                        }

                        // Можемо також завантажити базовий префаб героя для прев'ю
                        var prefab = await _prefabProvider.LoadPrefabByArchetypeAsync(archetype.ArchetypeId);

                        if (prefab != null)
                        {
                            _prefabCache[archetype.ArchetypeId] = prefab;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Не вдалося завантажити ресурси для {archetype.HeroName}: {ex.Message}", "Loading");
                    }

                    // Оновлюємо прогрес
                    float newProgress = _loadingProgress + progressStep * (i + 1);
                    PublishProgressUpdate(newProgress, _loadingStatus, _currentStage);

                    await UniTask.Delay(50); // Для плавності UI
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Критична помилка завантаження архетипів: {ex.Message}", "Loading", ex);
                throw;
            }
        }
        /// <summary>
        /// Завантаження UI ресурсів для лобі на основі ViewConfig категорій
        /// </summary>
        private async UniTask LoadLobbyUIResourcesAsync()
        {
            _logger.LogInfo("🎨 Завантаження UI ресурсів лобі через категорії...", "Loading");

            try
            {
                var viewConfigRegistry = _container.Resolve<IViewConfigRegistry>();
                var allConfigs = viewConfigRegistry.GetAll();

                // Завантажуємо всі UI з категорії Lobby + Common
                var targetConfigs = allConfigs.Where(config =>
                    config.category == UICategory.Lobby ||
                    config.category == UICategory.Common
                ).ToList();

                _logger.LogInfo($"Знайдено {targetConfigs.Count()} UI компонентів для завантаження", "Loading");

                if (targetConfigs.Count() == 0)
                {
                    _logger.LogWarning("Не знайдено жодного ViewConfig для Lobby/Common категорій", "Loading");
                    return;
                }

                float progressStep = 1.0f / targetConfigs.Count();

                for (int i = 0; i < targetConfigs.Count(); i++)
                {
                    var config = targetConfigs[i];

                    if (string.IsNullOrEmpty(config.prefabPath))
                    {
                        _logger.LogWarning($"ViewConfig {config.viewId} має порожній prefabPath", "Loading");
                        continue;
                    }

                    _loadingStatus = $"Завантаження UI: {config.viewId}...";

                    try
                    {
                        // Завантажуємо префаб за шляхом з конфігурації
                        var prefab = await _resourceManager.LoadAsync<GameObject>(config.prefabPath);

                        if (prefab != null)
                        {
                            _logger.LogDebug($"✅ Завантажено UI префаб для {config.viewId}: {config.prefabPath}", "Loading");

                            // Зберігаємо в кеш для можливого використання
                            _prefabCache[$"UI_{config.viewId}"] = prefab;
                        }
                        else
                        {
                            _logger.LogWarning($"❌ Не вдалося завантажити префаб для {config.viewId} з {config.prefabPath}", "Loading");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Помилка завантаження UI для {config.viewId}: {ex.Message}", "Loading");
                    }

                    // Оновлюємо прогрес
                    float newProgress = _loadingProgress + progressStep * (i + 1);
                    PublishProgressUpdate(newProgress, _loadingStatus, _currentStage);

                    await UniTask.Delay(50); // Для плавності UI
                }

                _logger.LogInfo($"✅ Завантажено UI ресурси для {targetConfigs.Count()} компонентів", "Loading");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Критична помилка завантаження UI лобі: {ex.Message}", "Loading", ex);
                throw;
            }
        }
        /// <summary>
        /// Ініціалізація пулів для лобі (особливо для карток героїв)
        /// </summary>
        private async UniTask InitializeLobbyPoolsAsync()
        {
            _logger.LogInfo("🏊 Ініціалізація пулів для лобі...", "Loading");

            try
            {
                // Пул для карток героїв - найважливіший
                var heroCardPrefab = await _resourceManager.LoadAsync<GameObject>("UI/Lobby/HeroCardUI");
                if (heroCardPrefab != null)
                {
                    _poolManager.CreatePool<GameObject>("HeroCardUI", heroCardPrefab, 20); // 20 карток в пулі
                    _logger.LogInfo("Створено пул для карток героїв (20 штук)", "Loading");
                }

                // Пул для selected hero cards
                var selectedCardPrefab = await _resourceManager.LoadAsync<GameObject>("UI/Lobby/SelectedHeroCard");
                if (selectedCardPrefab != null)
                {
                    _poolManager.CreatePool<GameObject>("SelectedHeroCard", selectedCardPrefab, 8);
                    _logger.LogInfo("Створено пул для вибраних карток (8 штук)", "Loading");
                }

                // Пул для tooltip-ів та інших UI елементів
                await InitializeCommonUIPoolsAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка ініціалізації пулів лобі: {ex.Message}", "Loading", ex);
                throw;
            }
        }

        /// <summary>
        /// Налаштування систем специфічних для лобі
        /// </summary>
        private async UniTask SetupLobbySystemsAsync()
        {
            _logger.LogInfo("⚙️ Налаштування систем лобі...", "Loading");

            try
            {
                // Ініціалізуємо системи категорії Lobby
                _systemRegistry.InitializeSystemsByCategory(SystemInitializationCategory.Lobby);

                // Додаткова ініціалізація HeroSelectionSystem
                var heroSelectionSystem = _systemRegistry.GetSystem<IHeroSelectionSystem>();
                if (heroSelectionSystem != null)
                {
                    await heroSelectionSystem.LoadAvailableHeroes();
                    _logger.LogInfo("HeroSelectionSystem ініціалізовано", "Loading");
                }

                await UniTask.Delay(200); // Даємо час системам на ініціалізацію
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка налаштування систем лобі: {ex.Message}", "Loading", ex);
                throw;
            }
        }

        private async UniTask FinalLobbySetupAsync()
        {
            _logger.LogInfo("🏁 Фінальне налаштування лобі...", "Loading");

            // Очищаємо тимчасовий кеш префабів (залишаємо тільки потрібне)
            _prefabCache.Clear();

            // Оптимізуємо пули
            _poolManager.TrimExcessObjects(15);

            await UniTask.Delay(100);
            _logger.LogInfo("Лобі готове до використання", "Loading");
        }

        // Enum для типів завантаження

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
                    // ✅ Використовуємо IPrefabProvider (архітектурно правильно)
                    var prefab = await _prefabProvider.LoadPrefabByArchetypeAsync(archetypeId);

                    if (prefab != null)
                    {
                        _prefabCache[archetypeId] = prefab;
                        _logger.LogInfo($"Завантажено префаб для {archetypeId}", "Loading");
                    }
                    else
                    {
                        _logger.LogWarning($"Не вдалося завантажити префаб для архетипу {archetypeId}", "Loading");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Помилка завантаження префабу для {archetypeId}: {ex.Message}", "Loading", ex);
                }

                // ✅ Додаємо прогрес та візуалізацію
                float newProgress = baseProgress + progressStep * (i + 1);
                PublishProgressUpdate(newProgress, _loadingStatus, _currentStage);

                // Невелика затримка для візуалізації
                await UniTask.Delay(50);
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
                    _poolManager.CreatePool<GameObject>(poolKey, entry.Value, 10);
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
        private async UniTask PrepareBasicResourcesAsync()
        {
            _logger.LogInfo("🔧 Заглушка: PrepareBasicResourcesAsync", "Loading");
            await UniTask.Delay(100);
        }

        private async UniTask InitializeCommonUIPoolsAsync()
        {
            _logger.LogInfo("🔧 Заглушка: InitializeCommonUIPoolsAsync", "Loading");
            await UniTask.Delay(100);
        }

        private async UniTask<bool> ExecuteMinimalLoadingAsync()
        {
            _logger.LogInfo("🔧 Заглушка: ExecuteMinimalLoadingAsync", "Loading");
            await UniTask.Delay(100);
            return true;
        }

        private async UniTask<bool> ExecuteGameplayLoadingAsync()
        {
            _logger.LogInfo("🔧 Заглушка: ExecuteGameplayLoadingAsync", "Loading");
            await ExecuteLoadingSequenceAsync();
            return true;
        }

    }
   
}


