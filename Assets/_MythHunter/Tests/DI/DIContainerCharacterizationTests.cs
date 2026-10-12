using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace MythHunter.DI.CharacterizationTests
{
    // Reflection is used because the current MythHunter runtime compiles into
    // Assembly-CSharp, which a custom asmdef test assembly cannot reference directly.
    public sealed class DIContainerCharacterizationTests
    {
        [Test]
        public void SingletonRegistration_ReturnsSameInstance()
        {
            var container = CreateContainer();
            Register(container, "RegisterSingleton", typeof(ITestService), typeof(TestService));

            var first = Resolve(container, typeof(ITestService));
            var second = Resolve(container, typeof(ITestService));

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void TransientRegistration_ReturnsDifferentInstances()
        {
            var container = CreateContainer();
            Register(container, "RegisterTransient", typeof(ITestService), typeof(TestService));

            var first = Resolve(container, typeof(ITestService));
            var second = Resolve(container, typeof(ITestService));

            Assert.That(first, Is.Not.SameAs(second));
        }

        [Test]
        public void ScopedRegistration_CachesByServiceTypeWithinScope()
        {
            var container = CreateContainer();
            Register(container, "RegisterScoped", typeof(ITestService), typeof(TestService));

            var first = Resolve(container, typeof(ITestService));
            var second = Resolve(container, typeof(ITestService));

            Assert.That(second, Is.SameAs(first));
        }

        [Test]
        public void ScopedRegistration_UsesSeparateInstancesForSeparateScopes()
        {
            var container = CreateContainer();
            Register(container, "RegisterScoped", typeof(ITestService), typeof(TestService));

            var firstScope = CreateScope(container, "first");
            SetCurrentScope(container, firstScope);
            var first = Resolve(container, typeof(ITestService));

            var secondScope = CreateScope(container, "second");
            SetCurrentScope(container, secondScope);
            var second = Resolve(container, typeof(ITestService));

            Assert.That(second, Is.Not.SameAs(first));
        }

        [Test]
        public void ResolveByType_ReturnsResolvedServiceInsteadOfRegistrationMetadata()
        {
            var container = CreateContainer();
            Register(container, "RegisterTransient", typeof(ITestService), typeof(TestService));

            var resolved = ResolveByType(container, typeof(ITestService));

            Assert.That(resolved, Is.InstanceOf<ITestService>());
        }

        [Test]
        public void TypeBasedRegistrationCheck_IncludesRegisteredInstances()
        {
            var container = CreateContainer();
            var instance = new TestService();
            RegisterInstance(container, typeof(ITestService), instance);

            Assert.That(IsRegisteredByType(container, typeof(ITestService)), Is.True);
            Assert.That(ResolveByType(container, typeof(ITestService)), Is.SameAs(instance));
        }

        [Test]
        public void ScopeDisposal_DisposesScopedServiceExactlyOnce()
        {
            ScopedDisposableService.DisposeCount = 0;
            var container = CreateContainer();
            Register(container, "RegisterScoped", typeof(IScopedDisposableService), typeof(ScopedDisposableService));

            var scope = CreateScope(container, "disposable");
            SetCurrentScope(container, scope);
            var first = Resolve(container, typeof(IScopedDisposableService));
            var second = Resolve(container, typeof(IScopedDisposableService));
            Assert.That(second, Is.SameAs(first));

            scope.GetType().GetMethod("Dispose", Type.EmptyTypes).Invoke(scope, null);

            Assert.That(ScopedDisposableService.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void LazySingleton_CreatesAndCachesValueOnFirstAccess()
        {
            var container = CreateContainer();
            Register(container, "RegisterLazySingleton", typeof(ITestService), typeof(TestService));

            var lazy = InvokeGeneric(container, "ResolveLazy", new[] { typeof(ITestService) });
            var first = lazy.GetType().GetProperty("Value").GetValue(lazy, null);
            var second = lazy.GetType().GetProperty("Value").GetValue(lazy, null);

            Assert.That(second, Is.SameAs(first));
            Assert.That(Resolve(container, typeof(ITestService)), Is.SameAs(first));
        }

        [Test]
        public void SetCurrentScope_OnWorkerThread_DoesNotReplaceMainThreadScope()
        {
            var container = CreateContainer();
            Register(container, "RegisterScoped", typeof(ITestService), typeof(TestService));

            var mainScope = CreateScope(container, "main");
            SetCurrentScope(container, mainScope);
            var mainInstance = Resolve(container, typeof(ITestService));

            object workerInstance = null;
            object workerSecond = null;
            Exception workerError = null;

            var worker = new Thread(() =>
            {
                try
                {
                    var workerScope = CreateScope(container, "worker");
                    SetCurrentScope(container, workerScope);
                    workerInstance = Resolve(container, typeof(ITestService));
                    workerSecond = Resolve(container, typeof(ITestService));
                }
                catch (Exception exception)
                {
                    workerError = exception;
                }
            });

            worker.Start();
            worker.Join();

            Assert.That(workerError, Is.Null);
            Assert.That(workerInstance, Is.Not.Null);
            Assert.That(workerSecond, Is.SameAs(workerInstance));
            Assert.That(workerInstance, Is.Not.SameAs(mainInstance));
            Assert.That(Resolve(container, typeof(ITestService)), Is.SameAs(mainInstance));
        }

        [Test]
        public void ChildScope_HasIndependentScopedInstance_AndDoesNotDisposeParentInstance()
        {
            ScopedDisposableService.DisposeCount = 0;
            var container = CreateContainer();
            Register(container, "RegisterScoped", typeof(IScopedDisposableService), typeof(ScopedDisposableService));

            var parentScope = CreateScope(container, "parent");
            SetCurrentScope(container, parentScope);
            var parentInstance = (ScopedDisposableService)Resolve(container, typeof(IScopedDisposableService));

            var childScope = parentScope.GetType()
                .GetMethod("CreateChildScope", new[] { typeof(string) })
                .Invoke(parentScope, new object[] { "child" });
            SetCurrentScope(container, childScope);
            var childInstance = (ScopedDisposableService)Resolve(container, typeof(IScopedDisposableService));

            Assert.That(childInstance, Is.Not.SameAs(parentInstance));

            childScope.GetType().GetMethod("Dispose", Type.EmptyTypes).Invoke(childScope, null);
            Assert.That(childInstance.IsDisposed, Is.True);
            Assert.That(parentInstance.IsDisposed, Is.False);
            Assert.That(ScopedDisposableService.DisposeCount, Is.EqualTo(1));

            SetCurrentScope(container, parentScope);
            Assert.That(Resolve(container, typeof(IScopedDisposableService)), Is.SameAs(parentInstance));
            parentScope.GetType().GetMethod("Dispose", Type.EmptyTypes).Invoke(parentScope, null);
            Assert.That(ScopedDisposableService.DisposeCount, Is.EqualTo(2));
        }

        [Test]
        public void ScopeDisposal_UsesReverseCreationOrder_AndContinuesAfterDisposeThrows()
        {
            ScopeDisposalProbe.Order.Clear();
            var container = CreateContainer();
            Register(container, "RegisterScoped", typeof(IFirstDisposable), typeof(FirstDisposable));
            Register(container, "RegisterScoped", typeof(IThrowingDisposable), typeof(ThrowingDisposable));

            var scope = CreateScope(container, "order");
            SetCurrentScope(container, scope);
            Resolve(container, typeof(IFirstDisposable));
            Resolve(container, typeof(IThrowingDisposable));

            scope.GetType().GetMethod("Dispose", Type.EmptyTypes).Invoke(scope, null);

            CollectionAssert.AreEqual(new[] { "throwing", "first" }, ScopeDisposalProbe.Order);
        }

        [Test]
        public void ScopeDisposal_DoesNotDisposeSameSharedInstanceTwice()
        {
            SharedDualService.DisposeCount = 0;
            var container = CreateContainer();
            var shared = new SharedDualService();
            RegisterInstance(container, typeof(IFirstDisposable), shared);
            RegisterInstance(container, typeof(IThrowingDisposable), shared);

            var scope = CreateScope(container, "shared");
            var first = InvokeGeneric(scope, "GetOrCreateInstance", new[] { typeof(IFirstDisposable) });
            var second = InvokeGeneric(scope, "GetOrCreateInstance", new[] { typeof(IThrowingDisposable) });
            Assert.That(second, Is.SameAs(first));

            scope.GetType().GetMethod("Dispose", Type.EmptyTypes).Invoke(scope, null);

            Assert.That(SharedDualService.DisposeCount, Is.EqualTo(1));
        }

        [Test]
        public void EventNetworkAuthorityPolicy_OnlyAllowsConfiguredAuthority()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == "Assembly-CSharp")
                ?? Assembly.Load("Assembly-CSharp");
            var modeType = assembly.GetType("MythHunter.Events.Network.EventNetworkMode", true);
            var authorityType = assembly.GetType("MythHunter.Events.Network.NetworkEventAuthority", true);
            var policyType = assembly.GetType("MythHunter.Events.Network.EventNetworkAuthorityPolicy", true);
            var method = policyType.GetMethod("CanTransmit", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            bool CanTransmit(string mode, string authority)
            {
                return (bool)method.Invoke(null, new[]
                {
                    Enum.Parse(modeType, mode),
                    Enum.Parse(authorityType, authority)
                });
            }

            Assert.That(CanTransmit("Offline", "Server"), Is.False);
            Assert.That(CanTransmit("Offline", "Client"), Is.False);
            Assert.That(CanTransmit("Offline", "Both"), Is.False);
            Assert.That(CanTransmit("Client", "Client"), Is.True);
            Assert.That(CanTransmit("Client", "Server"), Is.False);
            Assert.That(CanTransmit("Client", "Both"), Is.True);
            Assert.That(CanTransmit("Server", "Server"), Is.True);
            Assert.That(CanTransmit("Server", "Client"), Is.False);
            Assert.That(CanTransmit("Server", "Both"), Is.True);
        }

        [TestCase("Offline", false, null)]
        [TestCase("Client", true, "MythHunter.Networking.Core.ClientNetworkSystem")]
        [TestCase("Server", true, "MythHunter.Networking.Core.ServerNetworkSystem")]
        public void NetworkingInstaller_RegistersOnlyTheExplicitRoleAsINetworkSystem(
            string modeName, bool expectedRegistration, string expectedImplementation)
        {
            var container = CreateContainer();
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == "Assembly-CSharp")
                ?? Assembly.Load("Assembly-CSharp");
            RegisterErrorOnlyLogger(container, assembly);

            var modeType = assembly.GetType("MythHunter.Events.Network.EventNetworkMode", true);
            RegisterInstance(container, modeType, Enum.Parse(modeType, modeName));

            var installerType = assembly.GetType("MythHunter.Core.Installers.NetworkingInstaller", true);
            var installer = Activator.CreateInstance(installerType);
            installerType.GetMethod("InstallBindings").Invoke(installer, new[] { container });

            var networkType = assembly.GetType("MythHunter.Networking.Core.INetworkSystem", true);
            Assert.That(IsRegisteredByType(container, networkType), Is.EqualTo(expectedRegistration));

            var authorityType = assembly.GetType("MythHunter.Networking.Core.NetworkAuthority", true);
            Assert.That(IsRegisteredByType(container, authorityType), Is.EqualTo(expectedRegistration));

            if (!expectedRegistration)
                return;

            var selected = Resolve(container, networkType);
            var expectedRole = Resolve(container, assembly.GetType(
                modeName == "Client"
                    ? "MythHunter.Networking.Core.IClientNetworkSystem"
                    : "MythHunter.Networking.Core.IServerNetworkSystem", true));

            Assert.That(selected.GetType().FullName, Is.EqualTo(expectedImplementation));
            Assert.That(selected, Is.SameAs(expectedRole));
        }

        [TestCase("Offline", "MythHunter.Events.EventBus")]
        [TestCase("Client", "MythHunter.Events.Network.NetworkEventBus")]
        [TestCase("Server", "MythHunter.Events.Network.NetworkEventBus")]
        [Ignore("EventsInstaller removed - logic now in GameCoreInstaller (EPIC 07 Phase 2)")]
        public void EventsInstaller_SelectsBusFromExplicitMode_AndRegistryCapturesSameInstance(
            string modeName, string expectedBusType)
        {
            var container = CreateContainer();
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == "Assembly-CSharp")
                ?? Assembly.Load("Assembly-CSharp");
            RegisterErrorOnlyLogger(container, assembly);

            var modeType = assembly.GetType("MythHunter.Events.Network.EventNetworkMode", true);
            RegisterInstance(container, modeType, Enum.Parse(modeType, modeName));

            var networkingInstallerType = assembly.GetType("MythHunter.Core.Installers.NetworkingInstaller", true);
            var networkingInstaller = Activator.CreateInstance(networkingInstallerType);
            networkingInstallerType.GetMethod("InstallBindings").Invoke(networkingInstaller, new[] { container });

            Register(container, "RegisterSingleton",
                assembly.GetType("MythHunter.Events.IEventPool", true),
                assembly.GetType("MythHunter.Events.EventPool", true));
            // Match CoreInstaller's fallback; EventsInstaller must register its final choice
            // before constructing SystemRegistry.
            Register(container, "RegisterSingleton",
                assembly.GetType("MythHunter.Events.IEventBus", true),
                assembly.GetType("MythHunter.Events.SimpleEventBus", true));
            Register(container, "RegisterSingleton",
                assembly.GetType("MythHunter.Systems.Core.ISystemRegistry", true),
                assembly.GetType("MythHunter.Systems.Core.SystemRegistry", true));

            var eventsInstallerType = assembly.GetType("MythHunter.Core.Installers.EventsInstaller", true);
            var eventsInstaller = Activator.CreateInstance(eventsInstallerType);
            eventsInstallerType.GetMethod("InstallBindings").Invoke(eventsInstaller, new[] { container });

            var selectedBus = Resolve(container, assembly.GetType("MythHunter.Events.IEventBus", true));
            var registry = Resolve(container, assembly.GetType("MythHunter.Systems.Core.ISystemRegistry", true));
            var registryBusField = registry.GetType().GetField("_eventBus", BindingFlags.Instance | BindingFlags.NonPublic);
            var registryBus = registryBusField.GetValue(registry);

            try
            {
                Assert.That(selectedBus.GetType().FullName, Is.EqualTo(expectedBusType));
                Assert.That(registryBus, Is.SameAs(selectedBus));
                if (modeName != "Offline")
                {
                    var networkBusInterface = assembly.GetType("MythHunter.Events.Network.INetworkEventBus", true);
                    Assert.That(Resolve(container, networkBusInterface), Is.SameAs(selectedBus));
                }
            }
            finally
            {
                selectedBus.GetType().GetMethod("Dispose", Type.EmptyTypes)?.Invoke(selectedBus, null);
            }
        }


        [TestCase("OnBoot")]
        [TestCase("Lobby")]
        [TestCase("Gameplay")]
        [TestCase("Manual")]
        public void RegistryExtensions_RegisterGroupWithCategory_ForwardsRequestedCategory(string categoryName)
        {
            var container = CreateContainer();
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == "Assembly-CSharp")
                ?? Assembly.Load("Assembly-CSharp");
            RegisterErrorOnlyLogger(container, assembly);

            var loggerInterface = assembly.GetType("MythHunter.Utils.Logging.IMythLogger", true);
            var logger = Resolve(container, loggerInterface);
            var registryInterface = assembly.GetType("MythHunter.Systems.Core.ISystemRegistry", true);
            var categoryType = assembly.GetType("MythHunter.Systems.Core.SystemInitializationCategory", true);
            var registry = (RegistryContractSpyProxy)CreateSystemSpy(
                registryInterface,
                typeof(RegistryContractSpyProxy));
            var extensionType = assembly.GetType("MythHunter.Systems.Core.SystemRegistryExtensions", true);
            var method = extensionType.GetMethod(
                "RegisterGroupWithCategory",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { registryInterface, typeof(string), typeof(int), categoryType, loggerInterface },
                null);
            Assert.That(method, Is.Not.Null);

            var generic = method.MakeGenericMethod(assembly.GetType("MythHunter.Systems.Groups.SystemGroup", true));
            generic.Invoke(null, new[]
            {
                (object)registry,
                "ExplicitCategoryGroup",
                10,
                Enum.Parse(categoryType, categoryName),
                logger
            });

            Assert.That(registry.RegisteredSystem, Is.Not.Null);
            Assert.That(registry.RegisteredPriority, Is.EqualTo(10));
            Assert.That(registry.RegisteredCategory, Is.Not.Null);
            Assert.That(registry.RegisteredCategory.ToString(), Is.EqualTo(categoryName),
                "The extension must pass the caller's explicit category through the ISystemRegistry contract.");
        }

        [Test]
        public void SystemRegistry_RegisterSystemWithPriority_HonorsExplicitCategory()
        {
            var container = CreateContainer();
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == "Assembly-CSharp")
                ?? Assembly.Load("Assembly-CSharp");
            RegisterErrorOnlyLogger(container, assembly);

            var logger = Resolve(container, assembly.GetType("MythHunter.Utils.Logging.IMythLogger", true));
            var eventPoolType = assembly.GetType("MythHunter.Events.EventPool", true);
            var eventBusType = assembly.GetType("MythHunter.Events.SimpleEventBus", true);
            var eventBus = Activator.CreateInstance(eventBusType, new[]
            {
                Activator.CreateInstance(eventPoolType, new[] { logger }),
                logger
            });
            var registryType = assembly.GetType("MythHunter.Systems.Core.SystemRegistry", true);
            var registry = Activator.CreateInstance(registryType, new[] { logger, eventBus });
            var systemInterface = assembly.GetType("MythHunter.Core.ECS.ISystem", true);
            var categoryType = assembly.GetType("MythHunter.Systems.Core.SystemInitializationCategory", true);
            var probe = (SystemSpyProxy)CreateSystemSpy(systemInterface);
            var requestedCategory = Enum.Parse(categoryType, "Gameplay");

            try
            {
                registryType.GetMethod(
                    "RegisterSystemWithPriority",
                    new[] { systemInterface, typeof(int), categoryType })
                    .Invoke(registry, new[] { (object)probe, 7, requestedCategory });

                var wrongCategory = Enum.Parse(categoryType, "OnDemand");
                registryType.GetMethod("InitializeSystemsByCategory").Invoke(registry, new[] { wrongCategory });
                Assert.That(probe.InitializeCount, Is.EqualTo(0),
                    "An explicit category must not initialize the system in a different category.");

                registryType.GetMethod("InitializeSystemsByCategory").Invoke(registry, new[] { requestedCategory });
                Assert.That(probe.InitializeCount, Is.EqualTo(1),
                    "The system must initialize when its explicitly requested category is selected.");
            }
            finally
            {
                registryType.GetMethod("DisposeAll").Invoke(registry, null);
                eventBusType.GetMethod("Clear").Invoke(eventBus, null);
            }
        }

        [Test]
        public void ComponentCache_FullRefreshesChangedValuesAndRemovesMissingMembership()
        {
            var fixture = CreateComponentCacheFixture("Full");
            var entityId = fixture.CreateEntity();
            fixture.AddPosition(entityId, 1f);
            fixture.Update();
            Assert.That(fixture.GetPositionX(entityId), Is.EqualTo(1f));

            fixture.AddPosition(entityId, 2f);
            fixture.Update();
            Assert.That(fixture.GetPositionX(entityId), Is.EqualTo(2f));

            fixture.RemovePosition(entityId);
            fixture.Update();
            Assert.That(fixture.Contains(entityId), Is.False);
            Assert.That(fixture.Count, Is.EqualTo(0));

            fixture.AddPosition(entityId, 5f);
            fixture.Update();
            fixture.DestroyEntity(entityId);
            fixture.Update();
            Assert.That(fixture.Contains(entityId), Is.False,
                "Destroyed entities must be absent after a full refresh.");
        }

        [Test]
        public void ComponentCache_OnDemandContainsFindsUncachedAuthoritativeMembership()
        {
            var fixture = CreateComponentCacheFixture("OnDemand");
            var entityId = fixture.CreateEntity();
            fixture.AddPosition(entityId, 12f);

            Assert.That(fixture.Contains(entityId), Is.True,
                "OnDemand Contains must query authoritative membership even before Get caches the component.");
            Assert.That(fixture.Count, Is.EqualTo(1),
                "A successful OnDemand membership query should refresh its cached snapshot.");
            Assert.That(fixture.GetPositionX(entityId), Is.EqualTo(12f));
        }

        [Test]
        public void ComponentCache_OnDemandRevalidatesValuesAndMembershipAgainstEntityManager()
        {
            var fixture = CreateComponentCacheFixture("OnDemand");
            var entityId = fixture.CreateEntity();
            fixture.AddPosition(entityId, 3f);
            Assert.That(fixture.GetPositionX(entityId), Is.EqualTo(3f));

            fixture.AddPosition(entityId, 4f);
            Assert.That(fixture.GetPositionX(entityId), Is.EqualTo(4f),
                "OnDemand Get must return the authoritative latest struct value.");

            fixture.RemovePosition(entityId);
            Assert.That(fixture.Contains(entityId), Is.False);
            Assert.That(fixture.Count, Is.EqualTo(0));

            fixture.AddPosition(entityId, 6f);
            Assert.That(fixture.GetPositionX(entityId), Is.EqualTo(6f));
            fixture.DestroyEntity(entityId);
            Assert.That(fixture.Contains(entityId), Is.False);
            Assert.That(fixture.Count, Is.EqualTo(0));
        }

        [Test]
        public void ComponentCache_IncrementalRefreshesValuesAndPrunesRemovedComponentsInBatches()
        {
            var fixture = CreateComponentCacheFixture("Incremental");
            var entityIds = new List<int>();
            for (int i = 0; i < 205; i++)
            {
                int id = fixture.CreateEntity();
                entityIds.Add(id);
                fixture.AddPosition(id, i);
            }

            int guard = 0;
            while (fixture.Count < entityIds.Count && guard++ < 10)
                fixture.Update();

            Assert.That(fixture.Count, Is.EqualTo(205),
                "All IDs must be populated after at most three 100-item batches.");

            fixture.AddPosition(entityIds[150], 999f);
            fixture.RemovePosition(entityIds[5]);
            int previousCount = fixture.Count;
            fixture.Update();

            Assert.That(fixture.Contains(entityIds[5]), Is.False,
                "Membership removals must be applied on the next Incremental update.");
            Assert.That(fixture.Count, Is.EqualTo(previousCount - 1));

            guard = 0;
            while (fixture.GetPositionX(entityIds[150]) != 999f && guard++ < 10)
                fixture.Update();
            Assert.That(fixture.GetPositionX(entityIds[150]), Is.EqualTo(999f),
                "Existing cached values must refresh as the incremental cursor advances.");

            fixture.AddPosition(entityIds[5], 555f);
            guard = 0;
            while (!fixture.Contains(entityIds[5]) && guard++ < 10)
                fixture.Update();
            Assert.That(fixture.Contains(entityIds[5]), Is.True);

            fixture.DestroyEntity(entityIds[10]);
            fixture.Update();
            Assert.That(fixture.Contains(entityIds[10]), Is.False,
                "Destroyed entities must be pruned during the next incremental update.");
        }

        [Test]
        public void MovementSystemUpdate_PersistsMutatedComponentsToAuthoritativeEntityManager()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var frameworkAssembly = assemblies.FirstOrDefault(a => a.GetType("RPGFramework.ECS.EntityManager", false) != null)
                ?? Assembly.Load("RPGFramework.ECS.Runtime");
            var gameAssembly = assemblies.FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp")
                ?? Assembly.Load("Assembly-CSharp");

            var entityManagerType = frameworkAssembly.GetType("RPGFramework.ECS.EntityManager", true);
            var entityManager = Activator.CreateInstance(entityManagerType);
            var entityId = (int)entityManagerType.GetMethod("CreateEntity").Invoke(entityManager, null);

            var positionType = gameAssembly.GetType("MythHunter.Components.Movement.PositionComponent", true);
            var movementType = gameAssembly.GetType("MythHunter.Components.Movement.MovementComponent", true);
            var pathType = gameAssembly.GetType("MythHunter.Components.Movement.PathComponent", true);

            var position = Activator.CreateInstance(positionType);
            var positionField = positionType.GetField("Position");
            var vectorType = positionField.FieldType;
            positionField.SetValue(position, Activator.CreateInstance(vectorType, new object[] { 0f, 0f, 0f }));
            InvokeGeneric(entityManager, "AddComponent", new[] { positionType }, new[] { entityId, position });

            var movement = Activator.CreateInstance(movementType);
            movementType.GetField("MoveSpeed").SetValue(movement, 1f);
            movementType.GetField("RotationSpeed").SetValue(movement, 0f);
            movementType.GetField("IsMoving").SetValue(movement, true);
            movementType.GetField("MovementPoints").SetValue(movement, 10f);
            movementType.GetField("MaxMovementPoints").SetValue(movement, 10f);
            InvokeGeneric(entityManager, "AddComponent", new[] { movementType }, new[] { entityId, movement });

            var path = Activator.CreateInstance(pathType);
            var waypointsField = pathType.GetField("Waypoints");
            var waypoints = (System.Collections.IList)Activator.CreateInstance(waypointsField.FieldType);
            waypoints.Add(Activator.CreateInstance(vectorType, new object[] { 10f, 0f, 0f }));
            waypointsField.SetValue(path, waypoints);
            pathType.GetField("CurrentWaypointIndex").SetValue(path, 0);
            pathType.GetField("IsPathComplete").SetValue(path, false);
            pathType.GetField("TimeOnPath").SetValue(path, 0f);
            InvokeGeneric(entityManager, "AddComponent", new[] { pathType }, new[] { entityId, path });

            var eventBusType = gameAssembly.GetType("MythHunter.Events.IEventBus", true);
            var loggerType = gameAssembly.GetType("MythHunter.Utils.Logging.IMythLogger", true);
            var phaseType = gameAssembly.GetType("MythHunter.Systems.Phase.IPhaseSystem", true);
            var pathfindingType = gameAssembly.GetType("MythHunter.Systems.Movement.IPathfindingSystem", true);

            var eventBus = CreateSystemSpy(eventBusType);
            var logger = CreateSystemSpy(loggerType);
            var pathfinding = CreateSystemSpy(pathfindingType);
            var phase = CreateSystemSpy(phaseType, typeof(ActivePhaseSpyProxy));

            var movementSystemType = gameAssembly.GetType("MythHunter.Systems.Movement.MovementSystem", true);
            var ctor = movementSystemType.GetConstructors()
                .Single(candidate => candidate.GetParameters().Length == 5);
            var system = ctor.Invoke(new[] { entityManager, eventBus, logger, phase, pathfinding });

            movementSystemType.GetMethod("Update", new[] { typeof(float) })
                .Invoke(system, new object[] { 1f });

            var storedPosition = entityManagerType.GetMethod("GetComponent").MakeGenericMethod(positionType)
                .Invoke(entityManager, new object[] { entityId });
            var storedMovement = entityManagerType.GetMethod("GetComponent").MakeGenericMethod(movementType)
                .Invoke(entityManager, new object[] { entityId });
            var storedPath = entityManagerType.GetMethod("GetComponent").MakeGenericMethod(pathType)
                .Invoke(entityManager, new object[] { entityId });

            var storedPositionVector = positionType.GetField("Position").GetValue(storedPosition);
            var storedX = (float)vectorType.GetField("x").GetValue(storedPositionVector);
            var storedMovementPoints = (float)movementType.GetField("MovementPoints").GetValue(storedMovement);
            var storedPathTime = (float)pathType.GetField("TimeOnPath").GetValue(storedPath);

            Assert.That(storedX, Is.EqualTo(1f).Within(0.001f),
                "A movement tick must persist PositionComponent to the authoritative EntityManager.");
            Assert.That(storedMovementPoints, Is.EqualTo(9f).Within(0.001f),
                "A movement tick must persist spent movement points to the authoritative EntityManager.");
            Assert.That(storedPathTime, Is.EqualTo(1f).Within(0.001f),
                "A movement tick must persist PathComponent progress to the authoritative EntityManager.");

            movementSystemType.GetMethod("Dispose", Type.EmptyTypes).Invoke(system, null);
        }

        private sealed class ComponentCacheFixture
        {
            private readonly object _entityManager;
            private readonly object _cache;
            private readonly Type _componentType;
            private readonly Type _cacheType;
            private readonly MethodInfo _update;
            private readonly MethodInfo _contains;
            private readonly PropertyInfo _count;

            public ComponentCacheFixture(string strategyName)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                var frameworkAssembly = assemblies.FirstOrDefault(a => a.GetType("RPGFramework.ECS.EntityManager", false) != null)
                    ?? Assembly.Load("RPGFramework.ECS.Runtime");
                var gameAssembly = assemblies.FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp")
                    ?? Assembly.Load("Assembly-CSharp");
                var entityManagerType = frameworkAssembly.GetType("RPGFramework.ECS.EntityManager", true);
                _entityManager = Activator.CreateInstance(entityManagerType);
                _componentType = gameAssembly.GetType("MythHunter.Components.Movement.PositionComponent", true);
                var cacheDefinition = gameAssembly.GetType("MythHunter.Core.ECS.ComponentCache`1", true);
                _cacheType = cacheDefinition.MakeGenericType(_componentType);
                _cache = Activator.CreateInstance(_cacheType, new[] { _entityManager });
                var setStrategyMethod = _cacheType.GetMethod("SetCacheStrategy");
                // Resolve the parameter type from the closed generic method to avoid
                // passing the open nested enum type ComponentCache<T>.CacheStrategy.
                var strategyType = setStrategyMethod.GetParameters()[0].ParameterType;
                var strategy = Enum.Parse(strategyType, strategyName);
                setStrategyMethod.Invoke(_cache, new[] { strategy });
                _update = _cacheType.GetMethod("Update");
                _contains = _cacheType.GetMethod("Contains");
                _count = _cacheType.GetProperty("Count");
            }

            public int Count => (int)_count.GetValue(_cache);
            public int CreateEntity() => (int)_entityManager.GetType().GetMethod("CreateEntity").Invoke(_entityManager, null);
            public void Update() => _update.Invoke(_cache, null);
            public bool Contains(int id) => (bool)_contains.Invoke(_cache, new object[] { id });

            public void AddPosition(int id, float x)
            {
                var component = Activator.CreateInstance(_componentType);
                var positionField = _componentType.GetField("Position");
                var vector = Activator.CreateInstance(positionField.FieldType, new object[] { x, 0f, 0f });
                positionField.SetValue(component, vector);
                InvokeGeneric(_entityManager, "AddComponent", new[] { _componentType }, new object[] { id, component });
            }

            public void RemovePosition(int id)
            {
                InvokeGeneric(_entityManager, "RemoveComponent", new[] { _componentType }, new object[] { id });
            }

            public void DestroyEntity(int id)
            {
                _entityManager.GetType().GetMethod("DestroyEntity").Invoke(_entityManager, new object[] { id });
            }

            public float GetPositionX(int id)
            {
                var component = _cacheType.GetMethod("Get").Invoke(_cache, new object[] { id });
                var positionField = _componentType.GetField("Position");
                var vector = positionField.GetValue(component);
                return (float)positionField.FieldType.GetField("x").GetValue(vector);
            }
        }

        private static ComponentCacheFixture CreateComponentCacheFixture(string strategyName)
        {
            return new ComponentCacheFixture(strategyName);
        }
        [Test]
        public void SystemRegistry_DoesNotUpdateRegisteredSystemBeforeInitialization()
        {
            var container = CreateContainer();
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == "Assembly-CSharp")
                ?? Assembly.Load("Assembly-CSharp");
            RegisterErrorOnlyLogger(container, assembly);

            var loggerInterface = assembly.GetType("MythHunter.Utils.Logging.IMythLogger", true);
            var logger = Resolve(container, loggerInterface);
            var eventPoolType = assembly.GetType("MythHunter.Events.EventPool", true);
            var eventBusType = assembly.GetType("MythHunter.Events.SimpleEventBus", true);
            var eventBus = Activator.CreateInstance(eventBusType, new[]
            {
                Activator.CreateInstance(eventPoolType, new[] { logger }),
                logger
            });
            var registryType = assembly.GetType("MythHunter.Systems.Core.SystemRegistry", true);
            var registry = Activator.CreateInstance(registryType, new[] { logger, eventBus });
            var systemInterface = assembly.GetType("MythHunter.Core.ECS.ISystem", true);
            var probe = CreateSystemSpy(systemInterface);
            var probeState = (SystemSpyProxy)probe;

            try
            {
                registryType.GetMethod("RegisterSystem", new[] { systemInterface })
                    .Invoke(registry, new[] { probe });
                registryType.GetMethod("UpdateAll").Invoke(registry, new object[] { 0.016f });

                Assert.That(probeState.UpdateCount, Is.EqualTo(0),
                    "A registered system must not update before its Initialize callback succeeds.");

                var onDemand = Enum.Parse(
                    assembly.GetType("MythHunter.Systems.Core.SystemInitializationCategory", true),
                    "OnDemand");
                registryType.GetMethod("InitializeSystemsByCategory")
                    .Invoke(registry, new[] { onDemand });
                registryType.GetMethod("UpdateAll").Invoke(registry, new object[] { 0.016f });

                Assert.That(probeState.InitializeCount, Is.EqualTo(1));
                Assert.That(probeState.UpdateCount, Is.EqualTo(1));
            }
            finally
            {
                registryType.GetMethod("DisposeAll").Invoke(registry, null);
                eventBusType.GetMethod("Clear").Invoke(eventBus, null);
            }
        }

        [Test]
        public void SystemRegistry_DisposesSystemsInReverseInitializationOrder()
        {
            SystemSpyProxy.DisposeOrder.Clear();
            var container = CreateContainer();
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == "Assembly-CSharp")
                ?? Assembly.Load("Assembly-CSharp");
            RegisterErrorOnlyLogger(container, assembly);

            var logger = Resolve(container, assembly.GetType("MythHunter.Utils.Logging.IMythLogger", true));
            var eventPoolType = assembly.GetType("MythHunter.Events.EventPool", true);
            var eventBusType = assembly.GetType("MythHunter.Events.SimpleEventBus", true);
            var eventBus = Activator.CreateInstance(eventBusType, new[]
            {
                Activator.CreateInstance(eventPoolType, new[] { logger }),
                logger
            });
            var registryType = assembly.GetType("MythHunter.Systems.Core.SystemRegistry", true);
            var registry = Activator.CreateInstance(registryType, new[] { logger, eventBus });
            var systemInterface = assembly.GetType("MythHunter.Core.ECS.ISystem", true);

            var highPriority = (SystemSpyProxy)CreateSystemSpy(systemInterface, typeof(HighPrioritySystemSpyProxy));
            highPriority.ProbeName = "high";
            var lowPriority = (SystemSpyProxy)CreateSystemSpy(systemInterface, typeof(LowPrioritySystemSpyProxy));
            lowPriority.ProbeName = "low";

            // DispatchProxy implementations may share the same generated runtime type.
            // Distinct SystemGroup names avoid the duplicate-system-type guard while
            // still exercising reverse-priority disposal through the registry.
            var groupType = assembly.GetType("MythHunter.Systems.Groups.SystemGroup", true);
            var highGroup = Activator.CreateInstance(groupType, new object[] { "HighPriorityGroup", logger });
            var lowGroup = Activator.CreateInstance(groupType, new object[] { "LowPriorityGroup", logger });
            groupType.GetMethod("AddSystem", new[] { systemInterface }).Invoke(highGroup, new[] { highPriority });
            groupType.GetMethod("AddSystem", new[] { systemInterface }).Invoke(lowGroup, new[] { lowPriority });

            try
            {
                var register = registryType.GetMethod("RegisterSystemWithPriority", new[] { systemInterface, typeof(int) });
                register.Invoke(registry, new object[] { highGroup, 100 });
                register.Invoke(registry, new object[] { lowGroup, 0 });
                var onDemand = Enum.Parse(
                    assembly.GetType("MythHunter.Systems.Core.SystemInitializationCategory", true),
                    "OnDemand");
                registryType.GetMethod("InitializeSystemsByCategory").Invoke(registry, new[] { onDemand });
                registryType.GetMethod("DisposeAll").Invoke(registry, null);

                CollectionAssert.AreEqual(new[] { "low", "high" }, SystemSpyProxy.DisposeOrder);
            }
            finally
            {
                eventBusType.GetMethod("Clear").Invoke(eventBus, null);
            }
        }

        [Test]
        public void SystemGroup_IsolatesInitializeUpdateAndDisposeFailures()
        {
            var container = CreateContainer();
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == "Assembly-CSharp")
                ?? Assembly.Load("Assembly-CSharp");
            RegisterErrorOnlyLogger(container, assembly);
            var logger = Resolve(container, assembly.GetType("MythHunter.Utils.Logging.IMythLogger", true));
            var systemInterface = assembly.GetType("MythHunter.Core.ECS.ISystem", true);
            var groupType = assembly.GetType("MythHunter.Systems.Groups.SystemGroup", true);
            var group = Activator.CreateInstance(groupType, new object[] { "LifecycleProbe", logger });

            var updateFailure = (SystemSpyProxy)CreateSystemSpy(systemInterface);
            updateFailure.ThrowOnUpdate = true;
            var healthy = (SystemSpyProxy)CreateSystemSpy(systemInterface);
            var initializeFailure = (SystemSpyProxy)CreateSystemSpy(systemInterface);
            initializeFailure.ThrowOnInitialize = true;
            var disposeFailure = (SystemSpyProxy)CreateSystemSpy(systemInterface);
            disposeFailure.ThrowOnDispose = true;

            var addSystem = groupType.GetMethod("AddSystem", new[] { systemInterface });
            addSystem.Invoke(group, new object[] { updateFailure });
            addSystem.Invoke(group, new object[] { healthy });
            addSystem.Invoke(group, new object[] { initializeFailure });
            addSystem.Invoke(group, new object[] { disposeFailure });

            LogAssert.Expect(new Regex("(?s).*Error initializing system .*Expected initialize failure\\..*"));
            Assert.DoesNotThrow(() => groupType.GetMethod("Initialize").Invoke(group, null));

            LogAssert.Expect(new Regex("(?s).*Error updating system .*Expected update failure\\..*"));
            Assert.DoesNotThrow(() => groupType.GetMethod("Update", new[] { typeof(float) })
                .Invoke(group, new object[] { 0.016f }));

            LogAssert.Expect(new Regex("(?s).*Error disposing system .*Expected dispose failure\\..*"));
            Assert.DoesNotThrow(() => groupType.GetMethod("Dispose").Invoke(group, null));

            Assert.That(updateFailure.InitializeCount, Is.EqualTo(1));
            Assert.That(updateFailure.UpdateCount, Is.EqualTo(1));
            Assert.That(healthy.InitializeCount, Is.EqualTo(1));
            Assert.That(healthy.UpdateCount, Is.EqualTo(1),
                "A child update exception must not prevent sibling updates.");
            Assert.That(initializeFailure.InitializeCount, Is.EqualTo(1));
            Assert.That(initializeFailure.UpdateCount, Is.EqualTo(0),
                "A child whose initialization failed must not be updated.");
            Assert.That(disposeFailure.InitializeCount, Is.EqualTo(1));
            Assert.That(disposeFailure.UpdateCount, Is.EqualTo(1));
            Assert.That(updateFailure.DisposeCount, Is.EqualTo(1));
            Assert.That(healthy.DisposeCount, Is.EqualTo(1));
            Assert.That(initializeFailure.DisposeCount, Is.EqualTo(1));
            Assert.That(disposeFailure.DisposeCount, Is.EqualTo(1),
                "A child dispose exception must not prevent remaining sibling cleanup.");
        }

        [Test]
        public void SystemRegistryContract_DoesNotExposeTheDependencyContainer()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == "Assembly-CSharp")
                ?? Assembly.Load("Assembly-CSharp");
            var registryInterface = assembly.GetType("MythHunter.Systems.Core.ISystemRegistry", true);

            Assert.That(registryInterface.GetMethod("GetContainer"), Is.Null,
                "The system registry contract must not expose the DI container as a service locator.");
        }

        [Test]
        public void RegistryExtensions_RegisterPhaseGroupThroughTheInterfaceContract()
        {
            var container = CreateContainer();
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == "Assembly-CSharp")
                ?? Assembly.Load("Assembly-CSharp");
            RegisterErrorOnlyLogger(container, assembly);

            var loggerInterface = assembly.GetType("MythHunter.Utils.Logging.IMythLogger", true);
            var logger = Resolve(container, loggerInterface);
            var registryInterface = assembly.GetType("MythHunter.Systems.Core.ISystemRegistry", true);
            var providerInterface = assembly.GetType("MythHunter.Core.ECS.IPhaseProvider", true);
            var registry = (RegistryContractSpyProxy)CreateSystemSpy(
                registryInterface,
                typeof(RegistryContractSpyProxy));
            var phaseProvider = CreateSystemSpy(providerInterface, typeof(PhaseProviderSpyProxy));
            var extensionType = assembly.GetType("MythHunter.Systems.Core.SystemRegistryExtensions", true);
            var method = extensionType.GetMethod(
                "RegisterPhaseSystemGroup",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[]
                {
                    registryInterface,
                    typeof(string),
                    typeof(int),
                    loggerInterface,
                    providerInterface,
                    typeof(string[])
                },
                null);

            Assert.That(method, Is.Not.Null);
            var group = method.Invoke(null, new object[]
            {
                registry,
                "ContractPhaseGroup",
                42,
                logger,
                phaseProvider,
                new[] { "Planning" }
            });

            Assert.That(group, Is.Not.Null);
            Assert.That(registry.GetAllSystemsCallCount, Is.EqualTo(1),
                "Group discovery must use ISystemRegistry.GetAllSystems, not a concrete registry cast.");
            Assert.That(registry.RegisteredSystem, Is.SameAs(group));
            Assert.That(registry.RegisteredPriority, Is.EqualTo(42));
        }

        [Test]
        public void LegacyPhaseGroupRegistration_FailsFastWhenNoProviderIsRegistered()
        {
            var container = CreateContainer();
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == "Assembly-CSharp")
                ?? Assembly.Load("Assembly-CSharp");
            RegisterErrorOnlyLogger(container, assembly);

            var loggerInterface = assembly.GetType("MythHunter.Utils.Logging.IMythLogger", true);
            var logger = Resolve(container, loggerInterface);
            var registryInterface = assembly.GetType("MythHunter.Systems.Core.ISystemRegistry", true);
            var registry = (RegistryContractSpyProxy)CreateSystemSpy(
                registryInterface,
                typeof(RegistryContractSpyProxy));
            var gamePhaseType = assembly.GetType("MythHunter.Events.Domain.GamePhase", true);
            var phaseValues = Array.CreateInstance(gamePhaseType, 1);
            phaseValues.SetValue(Enum.Parse(gamePhaseType, "Planning"), 0);
            var extensionType = assembly.GetType("MythHunter.Systems.Core.SystemRegistryExtensions", true);
            var method = extensionType.GetMethod(
                "RegisterPhaseSystemGroup",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[]
                {
                    registryInterface,
                    typeof(string),
                    typeof(int),
                    loggerInterface,
                    gamePhaseType.MakeArrayType()
                },
                null);

            Assert.That(method, Is.Not.Null);
            LogAssert.Expect(new Regex("(?s).*No IPhaseProvider is registered.*Pass an IPhaseProvider explicitly.*"));
            var invocation = Assert.Throws<TargetInvocationException>(() => method.Invoke(null, new[]
            {
                (object)registry,
                "MissingProviderGroup",
                5,
                logger,
                phaseValues
            }));
            Assert.That(invocation.InnerException, Is.TypeOf<InvalidOperationException>());
            Assert.That(invocation.InnerException.Message, Does.Contain("Pass an IPhaseProvider explicitly"));
        }

        private static object CreateSystemSpy(Type systemInterface)
        {
            return CreateSystemSpy(systemInterface, typeof(SystemSpyProxy));
        }

        private static object CreateSystemSpy(Type systemInterface, Type proxyType)
        {
            var create = typeof(DispatchProxy).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(method =>
                    method.Name == "Create" &&
                    !method.IsGenericMethodDefinition &&
                    method.GetParameters().Length == 2);
            if (create != null)
                return create.Invoke(null, new object[] { systemInterface, proxyType });

            var genericCreate = typeof(DispatchProxy).GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Single(method =>
                    method.Name == "Create" &&
                    method.IsGenericMethodDefinition &&
                    method.GetGenericArguments().Length == 2 &&
                    method.GetParameters().Length == 0);
            return genericCreate.MakeGenericMethod(systemInterface, proxyType)
                .Invoke(null, null);
        }

        public class SystemSpyProxy : DispatchProxy
        {
            public int InitializeCount { get; private set; }
            public int UpdateCount { get; private set; }
            public int DisposeCount { get; private set; }
            public string ProbeName { get; set; }
            public static readonly List<string> DisposeOrder = new List<string>();
            public bool ThrowOnInitialize { get; set; }
            public bool ThrowOnUpdate { get; set; }
            public bool ThrowOnDispose { get; set; }

            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                switch (targetMethod.Name)
                {
                    case "Initialize":
                        InitializeCount++;
                        if (ThrowOnInitialize)
                            throw new InvalidOperationException("Expected initialize failure.");
                        break;
                    case "Update":
                        UpdateCount++;
                        if (ThrowOnUpdate)
                            throw new InvalidOperationException("Expected update failure.");
                        break;
                    case "Dispose":
                        DisposeCount++;
                        if (!string.IsNullOrEmpty(ProbeName))
                            DisposeOrder.Add(ProbeName);
                        if (ThrowOnDispose)
                            throw new InvalidOperationException("Expected dispose failure.");
                        break;
                }

                return null;
            }
        }

        public class HighPrioritySystemSpyProxy : SystemSpyProxy { }
        public class LowPrioritySystemSpyProxy : SystemSpyProxy { }

        public class ActivePhaseSpyProxy : DispatchProxy
        {
            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                if (targetMethod.Name == "get_CurrentPhase")
                    return Enum.Parse(targetMethod.ReturnType, "Active");

                var returnType = targetMethod.ReturnType;
                if (returnType == typeof(void))
                    return null;
                return returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
            }
        }

        public class RegistryContractSpyProxy : DispatchProxy
        {
            public object RegisteredSystem { get; private set; }
            public int RegisteredPriority { get; private set; }
            public object RegisteredCategory { get; private set; }
            public int GetAllSystemsCallCount { get; private set; }

            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                switch (targetMethod.Name)
                {
                    case "GetAllSystems":
                        GetAllSystemsCallCount++;
                        var systemType = targetMethod.ReturnType.GetGenericArguments()[0];
                        return Array.CreateInstance(systemType, 0);
                    case "RegisterSystemWithPriority":
                        RegisteredSystem = args[0];
                        RegisteredPriority = (int)args[1];
                        if (args.Length > 2)
                            RegisteredCategory = args[2];
                        return null;
                    case "LogInfo":
                        return null;
                }

                var returnType = targetMethod.ReturnType;
                if (returnType == typeof(void))
                    return null;
                return returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
            }
        }

        public class PhaseProviderSpyProxy : DispatchProxy
        {
            protected override object Invoke(MethodInfo targetMethod, object[] args)
            {
                var returnType = targetMethod.ReturnType;
                if (returnType == typeof(void))
                    return null;
                return returnType.IsValueType ? Activator.CreateInstance(returnType) : null;
            }
        }

        [Test]
        [Ignore("EventsInstaller removed - logic now in GameCoreInstaller (EPIC 07 Phase 2)")]
        public void EventsInstaller_RegistersFinalEventBusBeforeResolvingSystemRegistry()
        {
            var container = CreateContainer();
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == "Assembly-CSharp")
                ?? Assembly.Load("Assembly-CSharp");

            var loggerType = assembly.GetType("MythHunter.Utils.Logging.MythLogger", true);
            var loggerInterface = assembly.GetType("MythHunter.Utils.Logging.IMythLogger", true);
            var logLevelType = assembly.GetType("MythHunter.Utils.Logging.LogLevel", true);
            var logger = Activator.CreateInstance(loggerType, new[]
            {
                Enum.Parse(logLevelType, "Error"),
                (object)false,
                "EventBusRegistrationRegression"
            });
            RegisterInstance(container, loggerInterface, logger);
            var modeType = assembly.GetType("MythHunter.Events.Network.EventNetworkMode", true);
            RegisterInstance(container, modeType, Enum.Parse(modeType, "Offline"));

            Register(container, "RegisterSingleton",
                assembly.GetType("MythHunter.Events.IEventPool", true),
                assembly.GetType("MythHunter.Events.EventPool", true));
            // Mirror CoreInstaller's earlier fallback binding. EventsInstaller must
            // replace it before SystemRegistry's constructor resolves/caches the bus.
            Register(container, "RegisterSingleton",
                assembly.GetType("MythHunter.Events.IEventBus", true),
                assembly.GetType("MythHunter.Events.SimpleEventBus", true));
            Register(container, "RegisterSingleton",
                assembly.GetType("MythHunter.Systems.Core.ISystemRegistry", true),
                assembly.GetType("MythHunter.Systems.Core.SystemRegistry", true));

            var installerType = assembly.GetType("MythHunter.Core.Installers.EventsInstaller", true);
            var installer = Activator.CreateInstance(installerType);
            installerType.GetMethod("InstallBindings").Invoke(installer, new[] { container });

            var selectedBus = Resolve(container, assembly.GetType("MythHunter.Events.IEventBus", true));
            var registry = Resolve(container, assembly.GetType("MythHunter.Systems.Core.ISystemRegistry", true));
            var registryBusField = registry.GetType().GetField("_eventBus", BindingFlags.Instance | BindingFlags.NonPublic);
            var registryBus = registryBusField.GetValue(registry);

            try
            {
                Assert.That(selectedBus.GetType().FullName, Is.EqualTo("MythHunter.Events.EventBus"));
                Assert.That(registryBus, Is.SameAs(selectedBus));
            }
            finally
            {
                selectedBus.GetType().GetMethod("Dispose", Type.EmptyTypes)?.Invoke(selectedBus, null);
            }
        }

        private static void RegisterErrorOnlyLogger(object container, Assembly assembly)
        {
            var loggerType = assembly.GetType("MythHunter.Utils.Logging.MythLogger", true);
            var loggerInterface = assembly.GetType("MythHunter.Utils.Logging.IMythLogger", true);
            var logLevelType = assembly.GetType("MythHunter.Utils.Logging.LogLevel", true);
            var logger = Activator.CreateInstance(loggerType, new[]
            {
                Enum.Parse(logLevelType, "Error"),
                (object)false,
                "NetworkEventBusSelectionTests"
            });
            RegisterInstance(container, loggerInterface, logger);
        }

        private static object CreateContainer()
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(candidate => candidate.GetName().Name == "Assembly-CSharp")
                ?? Assembly.Load("Assembly-CSharp");
            var containerType = assembly.GetType("MythHunter.Core.DI.DIContainer", true);
            return Activator.CreateInstance(containerType, new object[] { null });
        }

        private static object Resolve(object container, Type serviceType)
        {
            return InvokeGeneric(container, "Resolve", new[] { serviceType });
        }

        private static void Register(object container, string methodName, Type serviceType, Type implementationType)
        {
            InvokeGeneric(container, methodName, new[] { serviceType, implementationType });
        }

        private static void RegisterInstance(object container, Type serviceType, object instance)
        {
            InvokeGeneric(container, "RegisterInstance", new[] { serviceType }, new[] { instance });
        }

        private static object CreateScope(object container, string scopeId)
        {
            return container.GetType().GetMethod("CreateScope", new[] { typeof(string) })
                .Invoke(container, new object[] { scopeId });
        }

        private static void SetCurrentScope(object container, object scope)
        {
            container.GetType().GetMethod("SetCurrentScope").Invoke(container, new[] { scope });
        }

        private static object ResolveByType(object container, Type serviceType)
        {
            return container.GetType().GetMethod("Resolve", new[] { typeof(Type) })
                .Invoke(container, new object[] { serviceType });
        }

        private static bool IsRegisteredByType(object container, Type serviceType)
        {
            return (bool)container.GetType().GetMethod("IsRegistered", new[] { typeof(Type) })
                .Invoke(container, new object[] { serviceType });
        }

        private static object InvokeGeneric(object target, string methodName, Type[] genericArguments, object[] arguments = null)
        {
            arguments = arguments ?? Array.Empty<object>();
            var method = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(candidate =>
                    candidate.Name == methodName &&
                    candidate.IsGenericMethodDefinition &&
                    candidate.GetGenericArguments().Length == genericArguments.Length &&
                    candidate.GetParameters().Length == arguments.Length);
            return method.MakeGenericMethod(genericArguments).Invoke(target, arguments);
        }
    }

    public interface ITestService { }

    public sealed class TestService : ITestService { }

    public interface IScopedDisposableService { }

    public sealed class ScopedDisposableService : IScopedDisposableService, IDisposable
    {
        public static int DisposeCount { get; set; }
        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            DisposeCount++;
        }
    }

    public interface IFirstDisposable { }

    public sealed class FirstDisposable : IFirstDisposable, IDisposable
    {
        public void Dispose()
        {
            ScopeDisposalProbe.Order.Add("first");
        }
    }

    public interface IThrowingDisposable { }

    public sealed class ThrowingDisposable : IThrowingDisposable, IDisposable
    {
        public void Dispose()
        {
            ScopeDisposalProbe.Order.Add("throwing");
            throw new InvalidOperationException("Expected test disposal failure.");
        }
    }

    public static class ScopeDisposalProbe
    {
        public static readonly List<string> Order = new List<string>();
    }

    public sealed class SharedDualService : IFirstDisposable, IThrowingDisposable, IDisposable
    {
        public static int DisposeCount { get; set; }

        public void Dispose()
        {
            DisposeCount++;
        }
    }
}
