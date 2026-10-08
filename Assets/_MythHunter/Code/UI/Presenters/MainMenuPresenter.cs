// Assets/_MythHunter/Code/UI/Presenters/MainMenuPresenter.cs
using MythHunter.Core.DI;
using MythHunter.Core.Game;
using MythHunter.Events;
using MythHunter.Events.Domain.Menu;
using MythHunter.Services.GameSettings;
using MythHunter.UI.Core;
using MythHunter.UI.Models;
using MythHunter.UI.Presenters;
using MythHunter.UI.Views;
using MythHunter.Utils.Logging;
using System;
using UnityEngine;
using Cysharp.Threading.Tasks;
using MythHunter.Events.Domain.Profile;
public class MainMenuPresenter : BasePresenter, IMainMenuPresenter
{
    private readonly IMainMenuModel _model;
    private readonly IGameSettingsService _gameSettings;
    private readonly IGameFlowManager _gameFlowManager;

    private IMainMenuView _typedView => _view as IMainMenuView;

    [Inject]
    public MainMenuPresenter(
        IMainMenuModel model,
        IEventBus eventBus,
        IMythLogger logger,
        IGameSettingsService gameSettings,
        IGameFlowManager gameFlowManager)
        : base(eventBus, logger)
    {
        _model = model;
        _gameSettings = gameSettings;
        _gameFlowManager = gameFlowManager;
        _viewId = ViewId.MainMenu;
    }

    // ✅ НОВІ методи для кнопок
    public void OnOnlinePvPClicked()
    {
        _logger.LogInfo("Online PvP режим вибрано", "MainMenu");
        SelectGameMode(GameMode.OnlinePvP);
    }

    public void OnLocalPvPClicked()
    {
        _logger.LogInfo("Local PvP режим вибрано", "MainMenu");
        SelectGameMode(GameMode.LocalPvP);
    }

    public void OnPvAIClicked()
    {
        _logger.LogInfo("PvAI режим вибрано", "MainMenu");
        SelectGameMode(GameMode.PvAI);
    }
    // ✅ ДОДАЙТЕ ЦЕЙ МЕТОД:
    public void OnProfileClicked()
    {
        _logger.LogInfo("👤 Profile button clicked!", "MainMenu");

        // Публікуємо подію
        _eventBus.Publish(new ProfileOpenedEvent
        {
            PlayerId = "LocalPlayer", // Поки що хардкод, пізніше отримуватимемо з UserService
            Timestamp = DateTime.UtcNow
        });

        // Переходимо до профілю
        _gameFlowManager.EnterProfileAsync().Forget();
    }
    private void SelectGameMode(GameMode mode)
    {
        // Зберігаємо режим
        _gameSettings.CurrentGameMode = mode;

        // Публікуємо подію
        _eventBus.Publish(new GameModeSelectedEvent
        {
            SelectedMode = mode,
            Timestamp = DateTime.UtcNow
        });

        // Переходимо до лобі
        _gameFlowManager.EnterLobbyAsync().Forget();
    }

   
   

    public void OnSettingsClicked()
    {
        _logger.LogInfo("Settings button clicked", "MainMenu");
        // TODO: Show settings
    }

    public void OnExitClicked()
    {
        _logger.LogInfo("Exit button clicked", "MainMenu");
        Application.Quit();
    }
}
