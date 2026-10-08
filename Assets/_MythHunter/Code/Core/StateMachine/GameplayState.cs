// Assets/_MythHunter/Code/Core/Game/GameplayState.cs
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Core.SceneManagement;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Events.Domain.Gameplay;
using MythHunter.Systems.Core;
using MythHunter.UI.Navigation;
using MythHunter.Utils.Logging;

namespace MythHunter.States
{
    /// <summary>
    /// Стан ігрового процесу
    /// </summary>
    public class GameplayState : BaseState<GameStateType>
    {
        private readonly IMythLogger _logger;
        private readonly IEventBus _eventBus;
        private readonly IGameFlowManager _gameFlowManager;

        public GameplayState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<IMythLogger>();
            _eventBus = container.Resolve<IEventBus>();
            _gameFlowManager = container.Resolve<IGameFlowManager>();
        }

        public override GameStateType StateId => GameStateType.Gameplay;


        public override void Enter(GameStateType previousState)
        {
            _logger.LogInfo("🎮 GameplayState: Завантаження GameScene та початок гри", "GameplayState");

            EnterGameplayAsync(previousState).Forget();
        }
        private async UniTaskVoid EnterGameplayAsync(GameStateType previousState)
        {
            try
            {
                // 1. Завантажуємо GameScene
                var sceneDispatcher = _container.Resolve<ISceneDispatcher>();
                await sceneDispatcher.LoadSceneAsync("GameScene");

                // 2. Налаштовуємо навігацію для гри
                var heroes = sceneDispatcher.GetSceneData<string[]>("SelectedHeroArchetypes");
                var parameters = new NavigationParameters();
                parameters.Add("SelectedHeroArchetypes", heroes);

                var navigationService = _container.Resolve<INavigationService>();
                await navigationService.SetupForSceneAsync("GameScene", parameters);

                // 3. Ініціалізуємо ігрові системи
                var systemRegistry = _container.Resolve<ISystemRegistry>();
                systemRegistry.InitializeSystemsByCategory(SystemInitializationCategory.Gameplay);

                // 4. Публікуємо події початку гри
                _eventBus.Publish(new GameStartedEvent { Timestamp = DateTime.UtcNow });

                _logger.LogInfo("✅ GameplayState: Гра почалася", "GameplayState");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ GameplayState помилка: {ex.Message}", "GameplayState", ex);
            }
        }
        public override void Exit()
        {
            _logger.LogInfo("Exiting gameplay state");
        }
    }
}
