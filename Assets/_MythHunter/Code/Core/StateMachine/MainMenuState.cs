// Assets/_MythHunter/Code/Core/StateMachine/MainMenuState.cs
using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.UI.Navigation;
using MythHunter.Systems.Core;
using Cysharp.Threading.Tasks;
using System;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан MainMenu з підтримкою новою архітектури
    /// </summary>
    public class MainMenuState : BaseState<GameStateType>
    {
        private readonly IMythLogger _logger;
        private readonly IEventBus _eventBus;
        private readonly INavigationService _navigationService;
        private readonly ISystemRegistry _systemRegistry;

        public override GameStateType StateId => GameStateType.MainMenu;

        public MainMenuState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<IMythLogger>();
            _eventBus = container.Resolve<IEventBus>();
            _navigationService = container.Resolve<INavigationService>();
            _systemRegistry = container.Resolve<ISystemRegistry>();
        }

        public override void Enter(GameStateType previousState)
        {
            _logger.LogInfo("🏠 Entering MainMenu state", "GameState");

            // Асинхронна ініціалізація з новою архітектурою
            InitializeAsync(previousState).Forget();
        }

        private async UniTaskVoid InitializeAsync(GameStateType previousState)
        {
            try
            {
                // 1. Публікуємо подію зміни стану
                _eventBus.Publish(new GameStateChangedEvent
                {
                    PreviousState = previousState,
                    NewState = GameStateType.MainMenu,
                    Timestamp = DateTime.UtcNow
                });

                // 2. Ініціалізуємо UI системи для MainMenu
                _systemRegistry.InitializeSystemsByCategory(SystemInitializationCategory.OnBoot);

                // 3. Налаштовуємо навігацію для MainMenuScene
                var parameters = new NavigationParameters();
                parameters.Add("PreviousState", previousState.ToString());
                parameters.Add("ShowWelcome", previousState == GameStateType.Boot);

                await _navigationService.SetupForSceneAsync("MainMenuScene", parameters);

                // 4. Невелика затримка для завершення ініціалізації UI
                await UniTask.DelayFrame(3);

                _logger.LogInfo("✅ MainMenu state initialized successfully", "GameState");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error in MainMenu initialization: {ex.Message}", "GameState", ex);
            }
        }

        public override void Update()
        {
            // Тут можна додати логіку оновлення MainMenu, якщо потрібно
        }

        public override void Exit()
        {
            _logger.LogInfo("🚪 Exiting MainMenu state", "GameState");

            // Очищуємо навігацію при виході (якщо потрібно)
            try
            {
                _navigationService.PrepareForSceneChangeAsync().Forget();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"⚠️ Warning during MainMenu exit: {ex.Message}", "GameState");
            }
        }
    }
}
