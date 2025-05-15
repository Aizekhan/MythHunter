// Assets/_MythHunter/Code/Core/Installers/LobbyInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Systems.Core;
using MythHunter.Systems.Lobby;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для систем лоббі
    /// </summary>
    public class LobbyInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing Lobby systems", "Installer");

            // Реєструємо системи лоббі
            BindSingleton<IHeroSelectionSystem, HeroSelectionSystem>(container);
            BindSingleton<ILobbySystem, LobbySystem>(container);

            // Отримуємо реєстр систем
            var systemRegistry = container.Resolve<ISystemRegistry>();

            // Реєструємо системи з пріоритетами
            systemRegistry.RegisterSystemWithPriority(
                container.Resolve<IHeroSelectionSystem>(),
                SystemPriorities.UI + 10 // Високий пріоритет для UI
            );

            systemRegistry.RegisterSystemWithPriority(
                container.Resolve<ILobbySystem>(),
                SystemPriorities.UI + 5
            );

            logger.LogInfo("Lobby systems installed successfully", "Installer");
        }
    }
}
