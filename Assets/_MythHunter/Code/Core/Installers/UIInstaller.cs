// Шлях: Assets/_MythHunter/Code/Core/Installers/UIInstaller.cs

using MythHunter.Core.DI;
using MythHunter.UI.Core;
using MythHunter.UI.Presenters;
using MythHunter.UI.Models;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Resources.Core;
using MythHunter.UI.Services;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для системи користувацького інтерфейсу
    /// </summary>
    public class UIInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення залежностей UISystem...", "Installer");

            // DI-реєстрація основних UI сервісів
            BindSingleton<ISpriteService, SpriteService>(container);
    

            BindSingleton<IUIComponentFactory, UIComponentFactory>(container);
            BindSingleton<IUIViewFactory, UIViewFactory>(container);
            BindSingleton<IUISystem, UISystem>(container);
            BindSingleton<IViewConfigRegistry, ViewConfigRegistry>(container);

            // Додаємо високорівневий UIService
            BindSingleton<IUIService, UIService>(container);

            // Моделі
            BindSingleton<IMainMenuModel, MainMenuModel>(container);
            BindSingleton<IGameplayUIModel, GameplayUIModel>(container);
            BindSingleton<IInventoryModel, InventoryModel>(container);
            BindSingleton<ILobbyModel, LobbyModel>(container);

            // Презентери
            BindSingleton<IMainMenuPresenter, MainMenuPresenter>(container);
            BindSingleton<IGameplayUIPresenter, GameplayUIPresenter>(container);
            BindSingleton<IInventoryPresenter, InventoryPresenter>(container);
            BindSingleton<ILobbyPresenter, LobbyPresenter>(container);

            logger.LogInfo("Встановлення залежностей UISystem завершено", "Installer");
        }
    }
}
