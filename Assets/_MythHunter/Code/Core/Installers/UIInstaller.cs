// Файл: Assets/_MythHunter/Code/Core/Installers/UIInstaller.cs
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
            InstallWithLogging(container, "UISystem", (c) => {
                // Базові залежності
                var eventBus = c.Resolve<IEventBus>();
                var resourceProvider = c.Resolve<IResourceProvider>();

                // Реєстрація основної UI системи
                BindSingleton<IUISystem, UISystem>(c);

                // Реєстрація основних моделей
                BindSingleton<IMainMenuModel, MainMenuModel>(c);
                BindSingleton<IGameplayUIModel, GameplayUIModel>(c);
                BindSingleton<IInventoryModel, InventoryModel>(c);

                // Реєстрація основних презентерів
                Bind<IMainMenuPresenter, MainMenuPresenter>(c);
                Bind<IGameplayUIPresenter, GameplayUIPresenter>(c);
                Bind<IInventoryPresenter, InventoryPresenter>(c);

                // Реєстрація фабрики представлень
                BindSingleton<IUIViewFactory, UIViewFactory>(c);
            });
        }
    }
}
