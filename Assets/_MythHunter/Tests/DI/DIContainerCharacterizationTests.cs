using System;
using System.Linq;
using System.Reflection;
using NUnit.Framework;

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

        public void Dispose()
        {
            DisposeCount++;
        }
    }
}
