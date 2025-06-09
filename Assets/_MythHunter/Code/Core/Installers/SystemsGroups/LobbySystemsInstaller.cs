// Шлях: Assets/_MythHunter/Code/Core/Installers/LobbySystemsInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Systems.Core;
using MythHunter.Systems.Lobby;
using MythHunter.Utils.Logging;
using MythHunter.Systems.Groups;
using MythHunter.UI.Presenters;
using MythHunter.Systems.AI;


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
         
            // Реєстрація сервісів та класів
            BindSingleton<ILobbyPresenter, LobbyPresenter>(container);
            BindSingleton<ILobbySystem, LobbySystem>(container);
            BindSingleton<ISimpleLobbyAI, SimpleLobbyAI>(container);




            // Отримуємо інстанси для реєстрації
            var lobbyPresenter = container.Resolve<ILobbyPresenter>();
            var lobbySystem = container.Resolve<ILobbySystem>();
            var aiSystem = container.Resolve<ISimpleLobbyAI>();
         


            // Група Lobby - з явним порядком систем
            var lobbyGroup = systemRegistry.RegisterGroupWithCategory<SystemGroup>(
                "LobbySystems",
                SystemPriorities.UI + 10,
                SystemInitializationCategory.Lobby,
                logger
            );

            // Додаємо системи в чіткому порядку
            lobbyGroup.AddSystem(lobbySystem);
            lobbyGroup.AddSystem(aiSystem);

            // Група HeroSystems
            var heroGroup = systemRegistry.RegisterGroupWithCategory<SystemGroup>(
                "HeroSystems",
                SystemPriorities.Active - 5,
                SystemInitializationCategory.Lobby,
                logger
            );

           

            logger.LogInfo("Системи лобі встановлено успішно", "Installer");
        }
    }
}
