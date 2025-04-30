using MythHunter.Core.DI;
using MythHunter.Core.Installers;

namespace MythHunter.Core
{
    /// <summary>
    /// Реєстр інсталяторів для DI
    /// </summary>
    public static class InstallerRegistry
    {
        public static void RegisterInstallers(IDIContainer container)
        {
            // Core installers
           
            var coreInstaller = new Installers.CoreInstaller();
            coreInstaller.InstallBindings(container);

            // Resource installer
            var resourceInstaller = new ResourceInstaller();
            resourceInstaller.InstallBindings(container);

            // Networking installer
            var networkingInstaller = new NetworkingInstaller();
            networkingInstaller.InstallBindings(container);

            // UI installer
            var uiInstaller = new UIInstaller();
            uiInstaller.InstallBindings(container);

            // Gameplay installer (в останню чергу, оскільки він залежить від інших)
            var gameplayInstaller = new GameplayInstaller();
            gameplayInstaller.InstallBindings(container);

            // Debug tools installer (додано)
            var debugToolsInstaller = new DebugToolsInstaller();
            debugToolsInstaller.InstallBindings(container);

            // TODO: Wizard will automatically add installers here

        }
    }
}
