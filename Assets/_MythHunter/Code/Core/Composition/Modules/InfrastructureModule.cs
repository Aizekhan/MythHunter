// Assets/_MythHunter/Code/Core/Composition/Modules/InfrastructureModule.cs
using System;
using System.Collections.Generic;
using RPGFramework.DI;
using RPGFramework.Core.Composition;

namespace MythHunter.Core.Composition.Modules
{
    /// <summary>
    /// Infrastructure module: Resources, Pooling, Serialization, UI.
    /// Provides foundational services needed by gameplay.
    /// </summary>
    public class InfrastructureModule : BaseFeatureModule
    {
        public override string Name => "MythHunter.Infrastructure";

        public override IEnumerable<Type> Dependencies => new[]
        {
            typeof(GameLayerModule) // Needs basic game services
        };

        protected override void OnConfigure()
        {
            var container = Container as IDIContainer;
            if (container == null)
            {
                throw new InvalidOperationException(
                    "InfrastructureModule requires IDIContainer");
            }

            // Install infrastructure services
            var resourceInstaller = new MythHunter.Core.Installers.ResourceInstaller();
            resourceInstaller.InstallBindings(container);

            var serializationInstaller = new MythHunter.Core.Installers.SerializationInstaller();
            serializationInstaller.InstallBindings(container);

            var poolInstaller = new MythHunter.Core.Installers.PoolInstaller();
            poolInstaller.InstallBindings(container);

            var entitiesInstaller = new MythHunter.Core.Installers.EntitiesInstaller();
            entitiesInstaller.InstallBindings(container);

            var uiInstaller = new MythHunter.Core.Installers.UIInstaller();
            uiInstaller.InstallBindings(container);

            // MonoBehaviour DI support
            var lazyDIInstaller = new MythHunter.Core.Installers.LazyDependencyInjectorInstaller();
            lazyDIInstaller.InstallBindings(container);

            // Profile UI presenter
            var profileInstaller = new MythHunter.Core.Installers.ProfileInstaller();
            profileInstaller.InstallBindings(container);
        }
    }
}
