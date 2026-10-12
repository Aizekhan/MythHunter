// Assets/_Framework/Core/Composition/Modules/FrameworkCoreModule.cs
using MythHunter.Core.DI;
using RPGFramework.ECS;
using RPGFramework.Logging;

namespace RPGFramework.Core.Composition.Modules
{
    /// <summary>
    /// Framework Core module: universal ECS runtime, DI, and logging.
    /// This module has zero game-specific dependencies and can be reused across any RPG game.
    /// </summary>
    public class FrameworkCoreModule : BaseFeatureModule
    {
        public override string Name => "Framework.Core";

        protected override void OnConfigure()
        {
            var container = Container as IDIContainer;
            if (container == null)
            {
                throw new System.InvalidOperationException(
                    "FrameworkCoreModule requires IDIContainer");
            }

            // Framework ECS Runtime (universal)
            container.BindSingleton<IEntityManager, EntityManager>();
            container.BindSingleton<MythHunter.Systems.Core.ISystemRegistry, MythHunter.Systems.Core.SystemRegistry>();

            // Framework Component Cache Registry
            container.BindSingleton<MythHunter.Entities.IComponentCacheRegistry, MythHunter.Entities.ComponentCacheRegistry>();

            // Note: Logger is already registered in GameBootstrapper before module system
            // DI Container itself is the hosting container, doesn't need self-registration
        }

        public override void Initialize()
        {
            // Framework core has no initialization logic
            // Entities and systems are initialized by EcsWorld (Game Layer responsibility)
        }
    }
}
