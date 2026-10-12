// Assets/_Framework/ECS/Tests/ModularCompositionTests.cs
using NUnit.Framework;
using RPGFramework.Core.Composition;
using RPGFramework.Core.Composition.Modules;
using MythHunter.Core.Composition.Modules;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace RPGFramework.ECS.Tests
{
    [TestFixture]
    [Category("Architecture")]
    [Category("Composition")]
    public class ModularCompositionTests
    {
        [Test]
        public void FrameworkCoreModule_CanBeInstantiated()
        {
            var module = new FrameworkCoreModule();
            Assert.IsNotNull(module);
            Assert.AreEqual("Framework.Core", module.Name);
        }

        [Test]
        public void GameLayerModule_DependsOnFrameworkCore()
        {
            var module = new GameLayerModule();
            Assert.IsNotNull(module.Dependencies);
            Assert.IsTrue(System.Linq.Enumerable.Any(module.Dependencies,
                d => d == typeof(FrameworkCoreModule)));
        }

        [Test]
        public void NetworkingModule_DependsOnGameLayer()
        {
            var module = new NetworkingModule();
            Assert.IsNotNull(module.Dependencies);
            Assert.IsTrue(System.Linq.Enumerable.Any(module.Dependencies,
                d => d == typeof(GameLayerModule)));
        }

        [Test]
        public void LobbyModule_DependsOnNetworking()
        {
            var module = new LobbyModule();
            Assert.IsNotNull(module.Dependencies);
            Assert.IsTrue(System.Linq.Enumerable.Any(module.Dependencies,
                d => d == typeof(NetworkingModule)));
        }

        [Test]
        public void GameCompositionBuilder_CanBuildMinimalComposition()
        {
            var logger = MythLogger.CreateDefaultLogger();
            var container = new DIContainer(logger);
            container.BindSingleton<RPGFramework.Logging.ILogger>(logger);
            container.BindSingleton<MythHunter.Utils.Logging.IMythLogger>(logger);

            var builder = new GameCompositionBuilder(container);
            builder.UseModule<FrameworkCoreModule>();

            var composition = builder.Build();

            Assert.IsNotNull(composition);
            Assert.AreEqual(1, composition.Modules.Count);
            Assert.IsInstanceOf<FrameworkCoreModule>(composition.Modules[0]);
        }

        [Test]
        public void GameCompositionBuilder_ResolvesDependenciesInOrder()
        {
            var logger = MythLogger.CreateDefaultLogger();
            var container = new DIContainer(logger);
            container.BindSingleton<RPGFramework.Logging.ILogger>(logger);
            container.BindSingleton<MythHunter.Utils.Logging.IMythLogger>(logger);

            var builder = new GameCompositionBuilder(container);
            // Register in wrong order - builder should fix it
            builder.UseModule<GameLayerModule>();
            builder.UseModule<FrameworkCoreModule>();

            var composition = builder.Build();

            Assert.AreEqual(2, composition.Modules.Count);
            // Framework must be first (dependency of GameLayer)
            Assert.IsInstanceOf<FrameworkCoreModule>(composition.Modules[0]);
            Assert.IsInstanceOf<GameLayerModule>(composition.Modules[1]);
        }

        [Test]
        public void GameCompositionBuilder_DetectsMissingDependency()
        {
            var logger = MythLogger.CreateDefaultLogger();
            var container = new DIContainer(logger);

            var builder = new GameCompositionBuilder(container);
            // GameLayer depends on FrameworkCore, but FrameworkCore not registered
            builder.UseModule<GameLayerModule>();

            Assert.Throws<System.InvalidOperationException>(() => builder.Build());
        }

        [Test]
        public void GameCompositionBuilder_CanShutdownModules()
        {
            var logger = MythLogger.CreateDefaultLogger();
            var container = new DIContainer(logger);
            container.BindSingleton<RPGFramework.Logging.ILogger>(logger);
            container.BindSingleton<MythHunter.Utils.Logging.IMythLogger>(logger);

            var builder = new GameCompositionBuilder(container);
            builder.UseModule<FrameworkCoreModule>();

            var composition = builder.Build();

            // Shutdown should not throw
            Assert.DoesNotThrow(() => composition.Shutdown());
        }
    }
}
