// Assets/_MythHunter/Code/Core/Composition/Modules/DebugToolsModule.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using RPGFramework.Core.Composition;

namespace MythHunter.Core.Composition.Modules
{
    /// <summary>
    /// Optional Debug Tools module for development and testing.
    /// Should only be enabled in editor or development builds.
    /// </summary>
    public class DebugToolsModule : BaseFeatureModule
    {
        public override string Name => "MythHunter.DebugTools";

        public override IEnumerable<Type> Dependencies => new[]
        {
            typeof(GameLayerModule) // Needs game services to debug
        };

        protected override void OnConfigure()
        {
            var container = Container as IDIContainer;
            if (container == null)
            {
                throw new InvalidOperationException(
                    "DebugToolsModule requires IDIContainer");
            }

            // Install debug tools
            var debugToolsInstaller = new Installers.CoreServices.DebugToolsInstaller();
            debugToolsInstaller.InstallBindings(container);

            var debugSystemsInstaller = new Installers.SystemsGroups.DebugSystemsInstaller();
            debugSystemsInstaller.InstallBindings(container);
        }
    }
}
