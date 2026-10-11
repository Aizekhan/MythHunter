using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
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
            var registry = Activator.CreateInstance(registryType, new[]
            {
                logger,
                eventBus,
                container
            });
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
            var registry = Activator.CreateInstance(registryType, new[] { logger, eventBus, container });
            var systemInterface = assembly.GetType("MythHunter.Core.ECS.ISystem", true);

            var highPriority = (SystemSpyProxy)CreateSystemSpy(systemInterface, typeof(HighPrioritySystemSpyProxy));
            highPriority.ProbeName = "high";
            var lowPriority = (SystemSpyProxy)CreateSystemSpy(systemInterface, typeof(LowPrioritySystemSpyProxy));
            lowPriority.ProbeName = "low";

            try
            {
                var register = registryType.GetMethod("RegisterSystemWithPriority", new[] { systemInterface, typeof(int) });
                register.Invoke(registry, new object[] { highPriority, 100 });
                register.Invoke(registry, new object[] { lowPriority, 0 });
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

            LogAssert.Expect(LogType.Error, new Regex("Error initializing system .*Expected initialize failure\\."));
            Assert.DoesNotThrow(() => groupType.GetMethod("Initialize").Invoke(group, null));

            LogAssert.Expect(LogType.Error, new Regex("Error updating system .*Expected update failure\\."));
            Assert.DoesNotThrow(() => groupType.GetMethod("Update", new[] { typeof(float) })
                .Invoke(group, new object[] { 0.016f }));

            LogAssert.Expect(LogType.Error, new Regex("Error disposing system .*Expected dispose failure\\."));
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

        [Test]
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
