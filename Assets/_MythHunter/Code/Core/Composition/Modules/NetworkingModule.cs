// Assets/_MythHunter/Code/Core/Composition/Modules/NetworkingModule.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using RPGFramework.Core.Composition;

namespace MythHunter.Core.Composition.Modules
{
    /// <summary>
    /// Optional Networking module for multiplayer functionality.
    /// Only loaded when networking is required.
    /// </summary>
    public class NetworkingModule : BaseFeatureModule
    {
        public override string Name => "MythHunter.Networking";

        public override IEnumerable<Type> Dependencies => new[]
        {
            typeof(GameLayerModule) // Needs EventBus
        };

        protected override void OnConfigure()
        {
            var container = Container as IDIContainer;
            if (container == null)
            {
                throw new InvalidOperationException(
                    "NetworkingModule requires IDIContainer");
            }

            // Install networking infrastructure
            var networkingInstaller = new MythHunter.Core.Installers.NetworkingInstaller();
            networkingInstaller.InstallBindings(container);

            // Install networking systems
            var networkingSystemsInstaller = new MythHunter.Core.Installers.NetworkingSystemsInstaller();
            networkingSystemsInstaller.InstallBindings(container);
        }
    }
}
