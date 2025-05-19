// Шлях: Assets/_MythHunter/Code/Core/Installers/LobbySystemsInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Systems.Core;
using MythHunter.Systems.Lobby;
using MythHunter.Utils.Logging;
using MythHunter.Systems.Heroes;
using MythHunter.Systems.Groups;
using MythHunter.UI.Presenters;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для систем лобі
    /// </summary>
    public class LobbySystemsInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення систем лобі...", "Installer");

            var systemRegistry = container.Resolve<ISystemRegistry>();
            BindSingleton<ILobbySystem, LobbySystem>(container);
            BindSingleton<IHeroSelectionSystem, HeroSelectionSystem>(container);
            BindSingleton<ILobbyPresenter, LobbyPresenter>(container);
            // Реєстрація групи систем лобі
            systemRegistry.RegisterGroupWithCategory<SystemGroup, ILobbySystem, IHeroSelectionSystem>(
                "LobbySystems",
                SystemPriorities.UI + 10,
                SystemInitializationCategory.Lobby,
                logger
            );

            // Реєстрація групи систем героїв
            BindSingleton<IHeroSystem, HeroSystem>(container);
            BindSingleton<IRaceClassBonusSystem, RaceClassBonusSystem>(container);
            systemRegistry.RegisterGroupWithCategory<SystemGroup, IRaceClassBonusSystem, IHeroSystem>(
                "HeroSystems",
                SystemPriorities.Active - 5,
                SystemInitializationCategory.Lobby,
                logger
            );

            logger.LogInfo("Системи лобі встановлено успішно", "Installer");
        }
    }
}
