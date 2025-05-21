// Assets/_MythHunter/Code/Core/Game/States/LoadingState.cs
using System;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Systems.Loading;
using MythHunter.UI.Core;
using MythHunter.UI.Navigation;
using MythHunter.Utils.Logging;
using System.Threading;
using MythHunter.Systems.Core;

namespace MythHunter.States
{
    /// <summary>
    /// Стан для управління процесом завантаження гри
    /// </summary>
    public class LoadingState : BaseState<GameStateType>
    {
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private readonly ILoadingSystem _loadingSystem;
        private readonly INavigationService _navigationService;
        private readonly ISystemRegistry _systemRegistry;

        private string[] _selectedHeroArchetypes;
        private string _mapId;
        private bool _isEntered = false;

        // Семафор для синхронізації
        private readonly SemaphoreSlim _enterSemaphore = new SemaphoreSlim(1, 1);

        public LoadingState(IDIContainer container) : base(container)
        {
            _eventBus = container.Resolve<IEventBus>();
            _logger = container.Resolve<IMythLogger>();
            _loadingSystem = container.Resolve<ILoadingSystem>();
            _navigationService = container.Resolve<INavigationService>();
            _systemRegistry = container.Resolve<ISystemRegistry>();
        }

        public override GameStateType StateId => GameStateType.Loading;

        public override void Enter(GameStateType previousState)
        {
            _logger.LogInfo("[LIFECYCLE] Початок входу в LoadingState", "GameState");

            // Запускаємо асинхронний процес входу
            EnterAsyncProcess(previousState).Forget();
        }

        private async UniTaskVoid EnterAsyncProcess(GameStateType previousState)
        {
            try
            {
                // Захоплюємо семафор
                await _enterSemaphore.WaitAsync();

                if (_isEntered)
                {
                    _logger.LogWarning("Повторний вхід у LoadingState проігноровано", "GameState");
                    return;
                }

                _isEntered = true;

                // Публікуємо подію зміни стану гри
                _eventBus.Publish(new GameStateChangedEvent
                {
                    PreviousState = previousState,
                    NewState = GameStateType.Loading,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInfo("GameStateChangedEvent опубліковано", "LoadingState");

                // Отримуємо параметри з контексту
                var context = GetStateContext();
                ExtractContextParameters(context);

                // Налаштовуємо навігацію для екрану завантаження
                var parameters = new NavigationParameters();
                parameters.Add("PreviousState", previousState.ToString());
                parameters.Add("SelectedHeroes", _selectedHeroArchetypes);
                parameters.Add("MapId", _mapId);

                await _navigationService.SetupForSceneAsync("LoadingScene", parameters);

                // Ініціалізуємо системи завантаження
                await InitializeLoadingSystemsAsync();

                // Починаємо процес завантаження
                await StartLoadingProcessAsync();

                _logger.LogInfo("[LIFECYCLE] Завершено вхід у LoadingState", "GameState");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при вході в LoadingState: {ex.Message}", "GameState", ex);
            }
            finally
            {
                _enterSemaphore.Release();
            }
        }

        private void ExtractContextParameters(object context)
        {
            if (context is LoadingStateContext loadingContext)
            {
                _selectedHeroArchetypes = loadingContext.SelectedHeroArchetypes;
                _mapId = loadingContext.MapId;

                _logger.LogInfo($"Отримано параметри: {_selectedHeroArchetypes?.Length ?? 0} героїв, карта: {_mapId}", "LoadingState");
            }
            else
            {
                _logger.LogWarning("Контекст стану відсутній або має неправильний тип", "LoadingState");
                _selectedHeroArchetypes = new string[0];
                _mapId = "default";
            }
        }

        private async UniTask InitializeLoadingSystemsAsync()
        {
            _logger.LogInfo("Ініціалізація систем завантаження", "LoadingState");

            try
            {
                // Ініціалізуємо системи категорії Loading
                _systemRegistry.InitializeSystemsByCategory(SystemInitializationCategory.OnDemand);

                // Даємо можливість Unity завершити оновлення UI
                await UniTask.DelayFrame(2);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при ініціалізації систем завантаження: {ex.Message}", "LoadingState", ex);
                throw;
            }
        }

        private async UniTask StartLoadingProcessAsync()
        {
            _logger.LogInfo("Початок процесу завантаження", "LoadingState");

            try
            {
                // Починаємо процес завантаження
                bool result = await _loadingSystem.StartLoadingGameAsync(_selectedHeroArchetypes, _mapId);

                if (result)
                {
                    // Завантаження успішне, переходимо до ігрової сцени
                    await _loadingSystem.FinishLoadingAsync();

                    // Переходимо в ігровий стан
                    StateMachine.ChangeState(GameStateType.Gameplay);
                }
                else
                {
                    // Завантаження не вдалося, повертаємося в лобі
                    _logger.LogWarning("Завантаження не вдалося, повернення в лобі", "LoadingState");
                    StateMachine.ChangeState(GameStateType.Lobby);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка в процесі завантаження: {ex.Message}", "LoadingState", ex);

                // При критичній помилці повертаємося в лобі
                StateMachine.ChangeState(GameStateType.Lobby);
            }
        }

        public override void Exit()
        {
            _logger.LogInfo("[LIFECYCLE] Вихід зі стану LoadingState", "GameState");

            // Скидаємо стан
            _isEntered = false;
            _selectedHeroArchetypes = null;
            _mapId = null;
        }
    }

    /// <summary>
    /// Контекст для передачі параметрів у стан завантаження
    /// </summary>
    public class LoadingStateContext
    {
        public string[] SelectedHeroArchetypes
        {
            get; set;
        }
        public string MapId { get; set; } = "default";
    }
}
