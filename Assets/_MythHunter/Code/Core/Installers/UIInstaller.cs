using MythHunter.Core.DI;
using MythHunter.UI.Core;
using MythHunter.UI.Presenters;
using MythHunter.UI.Models;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Resources.Core;

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

            var eventBus = container.Resolve<IEventBus>();
            var resourceProvider = container.Resolve<IResourceProvider>();

            // Реєстрація основної UI системи
            BindSingleton<IUISystem, UISystem>(container);

            // Реєстрація моделей
            BindSingleton<IMainMenuModel, MainMenuModel>(container);
            BindSingleton<IGameplayUIModel, GameplayUIModel>(container);
            BindSingleton<IInventoryModel, InventoryModel>(container);

            // Реєстрація презентерів
            Bind<IMainMenuPresenter, MainMenuPresenter>(container);
            Bind<IGameplayUIPresenter, GameplayUIPresenter>(container);
            Bind<IInventoryPresenter, InventoryPresenter>(container);

            // Реєстрація фабрики представлень
            BindSingleton<IUIViewFactory, UIViewFactory>(container);

            logger.LogInfo("Встановлення залежностей UISystem завершено", "Installer");
        }
    }
}
