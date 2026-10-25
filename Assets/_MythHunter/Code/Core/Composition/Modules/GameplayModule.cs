// Assets/_MythHunter/Code/Core/Composition/Modules/GameplayModule.cs
using System;
using System.Collections.Generic;
using RPGFramework.DI;
using RPGFramework.Core.Composition;

namespace MythHunter.Core.Composition.Modules
{
    /// <summary>
    /// Core gameplay systems module: Movement, Visibility, Hero, etc.
    /// Always required for actual gameplay.
    /// </summary>
    public class GameplayModule : BaseFeatureModule
    {
        public override string Name => "MythHunter.Gameplay";

        public override IEnumerable<Type> Dependencies => new[]
        {
            typeof(GameLayerModule) // Needs game flow and ECS
        };

        protected override void OnConfigure()
        {
            var container = Container as IDIContainer;
            if (container == null)
            {
                throw new InvalidOperationException(
                    "GameplayModule requires IDIContainer");
            }

            // Install core gameplay systems
            var gameplaySystemsInstaller = new MythHunter.Core.Installers.GameplaySystemsInstaller();
            gameplaySystemsInstaller.InstallBindings(container);

            var heroSystemsInstaller = new MythHunter.Core.Installers.HeroSystemsInstaller();
            heroSystemsInstaller.InstallBindings(container);
        }
    }
}
