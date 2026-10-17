// Assets/_MythHunter/Code/Core/Installers/ProfileInstaller.cs
using MythHunter.Core.DI;
using MythHunter.UI.Presenters;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// [OBSOLETE] Profile installer - now integrated into InfrastructureModule.
    /// This class will be removed in a future version.
    /// </summary>
    [System.Obsolete("Use InfrastructureModule instead.", false)]
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
