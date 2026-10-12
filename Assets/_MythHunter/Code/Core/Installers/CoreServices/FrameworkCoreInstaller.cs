using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Core.ECS.Jobs;
using MythHunter.Entities;
using MythHunter.Events;
using MythHunter.Systems.Core;
using MythHunter.Utils.Logging;
using RPGFramework.ECS;

namespace MythHunter.Core.Installers
{
    /// <summary>
    /// Framework Core installer - registers universal RPG Framework services.
    /// These services are reusable across any RPG game and have no game-specific dependencies.
    /// </summary>
    public class FrameworkCoreInstaller : DIInstaller
    {
        public override void InstallBindings(IDIContainer container)
        {
            var logger = container.Resolve<IMythLogger>();
            logger.LogInfo("Installing Framework Core services...", "FrameworkCoreInstaller");

            // Entity and component management
            BindSingleton<IEntityManager, EntityManager>(container);
            BindSingleton<IEcsWorld, EcsWorld>(container);

            // Event infrastructure
            BindSingleton<IEventPool, EventPool>(container);

            // Dependency injection lifecycle
            BindSingleton<IDILifecycleManager, DILifecycleManager>(container);

            // System execution infrastructure
            BindSingleton<ISystemJobScheduler, SystemJobScheduler>(container);
            BindSingleton<ISystemRegistry, ParallelSystemRegistry>(container);

            logger.LogInfo("Framework Core services installed successfully", "FrameworkCoreInstaller");
        }
    }
}
