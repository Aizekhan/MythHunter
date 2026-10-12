// Assets/_MythHunter/Code/Core/Composition/Modules/GameLayerModule.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Core.Game;
using MythHunter.Core.MonoBehaviours;
using MythHunter.Core.SceneManagement;
using MythHunter.Core.StateMachine;
using MythHunter.Events;
using MythHunter.Resources;
using MythHunter.Resources.SceneManagement;
using MythHunter.Services.GameSettings;
using MythHunter.Systems.Core;
using MythHunter.Systems.Phase;
using RPGFramework.Core.Composition;
using RPGFramework.Core.Composition.Modules;

namespace MythHunter.Core.Composition.Modules
{
    /// <summary>
    /// MythHunter Game Layer module: game-specific flow, state machine, scene management.
    /// Depends on Framework Core for ECS and DI infrastructure.
    /// </summary>
    public class GameLayerModule : BaseFeatureModule
    {
        public override string Name => "MythHunter.GameLayer";

        public override IEnumerable<Type> Dependencies => new[]
        {
            typeof(FrameworkCoreModule)
        };

        protected override void OnConfigure()
        {
            var container = Container as IDIContainer;
            if (container == null)
            {
                throw new InvalidOperationException(
                    "GameLayerModule requires IDIContainer");
            }

            // Use existing CoreInstaller which registers game flow, state machine, ECS, and scene management
            var coreInstaller = new MythHunter.Core.Installers.CoreInstaller();
            coreInstaller.InstallBindings(container);
        }
    }
}
