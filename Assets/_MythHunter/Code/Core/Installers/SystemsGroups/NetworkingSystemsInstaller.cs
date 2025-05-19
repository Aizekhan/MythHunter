// Шлях: Assets/_MythHunter/Code/Core/Installers/NetworkingSystemsInstaller.cs
using MythHunter.Core.DI;
using MythHunter.Systems.Core;
using MythHunter.Utils.Logging;
using MythHunter.Networking.Core;
using MythHunter.Networking.Client;
using MythHunter.Networking.Server;
using MythHunter.Systems.Groups;
using MythHunter.Core.ECS;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Інсталятор для мережевих систем
    /// </summary>
    public class NetworkingSystemsInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Встановлення мережевих систем...", "Installer");

            var systemRegistry = container.Resolve<ISystemRegistry>();

            if (container.IsRegistered<INetworkSystem>())
            {
                var systems = new ISystem[]
                {
                };

                var networkGroup = systemRegistry.RegisterGroupWithCategory<SystemGroup>(
                    "NetworkSystems",
                    SystemPriorities.Network,
                    SystemInitializationCategory.OnBoot,
                    logger
                );

                foreach (var system in systems)
                    networkGroup.AddSystem(system);

                logger.LogInfo("Мережеві системи встановлено успішно", "Installer");
            }
            else
            {
                logger.LogInfo("Мережеві системи пропущено (NetworkSystem не зареєстрована)", "Installer");
            }
        }
    }
}
