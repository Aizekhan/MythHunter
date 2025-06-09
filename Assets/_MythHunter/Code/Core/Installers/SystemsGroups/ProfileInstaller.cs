// Assets/_MythHunter/Code/Core/Installers/ProfileInstaller.cs
using MythHunter.Core.DI;
using MythHunter.UI.Presenters;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Installers
{
    public class ProfileInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення Profile компонентів...", "Installer");

            // Реєструємо Profile Presenter
            BindSingleton<IProfilePresenter, ProfilePresenter>(container);

            logger.LogInfo("Profile компоненти встановлено успішно", "Installer");
        }
    }
}
