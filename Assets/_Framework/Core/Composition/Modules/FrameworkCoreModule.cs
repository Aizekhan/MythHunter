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
            // Framework core dependencies are registered by CoreInstaller (called from GameLayerModule)
            // This module exists primarily for dependency tracking and organization
            // The actual Framework types (IEntityManager, ISystemRegistry, IComponentCacheRegistry)
            // are part of CoreInstaller which will be called by GameLayerModule
        }

        public override void Initialize()
        {
            // Framework core has no initialization logic
            // Entities and systems are initialized by EcsWorld (Game Layer responsibility)
        }
    }
}
