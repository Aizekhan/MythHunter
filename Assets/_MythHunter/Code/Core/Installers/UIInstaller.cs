using MythHunter.Core.DI;
using MythHunter.UI.Core;
using MythHunter.UI.Presenters;
using MythHunter.UI.Models;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Resources.Core;
using System;

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

            try
            {
                // Перевіряємо, що потрібні залежності вже зареєстровані
                var resourceProvider = container.Resolve<IResourceProvider>();

                // Створюємо UIViewFactory вручну і реєструємо його як інстанс
                var viewFactory = new UIViewFactory(resourceProvider, logger);
                container.RegisterInstance<IUIViewFactory>(viewFactory);

                logger.LogInfo("IUIViewFactory успішно зареєстровано", "Installer");
            }
            catch (Exception ex)
            {
                logger.LogError($"Помилка реєстрації IUIViewFactory: {ex.Message}", "Installer", ex);
                throw; // Перекидаємо помилку далі, щоб не приховувати її
            }

            // Реєстрація інших UI-залежностей, які можуть залежати від IUIViewFactory
            BindSingleton<IUISystem, UISystem>(container);

            // Реєстрація моделей
            BindSingleton<IMainMenuModel, MainMenuModel>(container);
            BindSingleton<IGameplayUIModel, GameplayUIModel>(container);
            BindSingleton<IInventoryModel, InventoryModel>(container);

            // Реєстрація презентерів
            Bind<IMainMenuPresenter, MainMenuPresenter>(container);
            Bind<IGameplayUIPresenter, GameplayUIPresenter>(container);
            Bind<IInventoryPresenter, InventoryPresenter>(container);

            logger.LogInfo("Встановлення залежностей UISystem завершено", "Installer");
        }
    }
}
