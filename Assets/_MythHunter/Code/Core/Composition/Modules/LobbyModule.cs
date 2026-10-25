// Assets/_MythHunter/Code/Core/Composition/Modules/LobbyModule.cs
using System;
using System.Collections.Generic;
using RPGFramework.DI;
using RPGFramework.Core.Composition;

namespace MythHunter.Core.Composition.Modules
{
    /// <summary>
    /// Optional Lobby module for pre-game matchmaking and room management.
    /// Requires Networking module.
    /// </summary>
    public class LobbyModule : BaseFeatureModule
    {
        public override string Name => "MythHunter.Lobby";

        public override IEnumerable<Type> Dependencies => new[]
        {
            typeof(NetworkingModule) // Lobby requires networking
        };

        protected override void OnConfigure()
        {
            var container = Container as IDIContainer;
            if (container == null)
            {
                throw new InvalidOperationException(
                    "LobbyModule requires IDIContainer");
            }

            // Install lobby systems
            var lobbySystemsInstaller = new MythHunter.Core.Installers.LobbySystemsInstaller();
            lobbySystemsInstaller.InstallBindings(container);
        }
    }
}
