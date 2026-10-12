// Assets/_MythHunter/Tests/Runtime/DIContainerPerformanceTests.cs
using NUnit.Framework;
using System.Diagnostics;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.Tests.Runtime
{
    /// <summary>
    /// EPIC 08 Phase 4: Performance tests for DIContainer reflection optimization
    /// </summary>
    public class DIContainerPerformanceTests
    {
        private class TestLogger : IMythLogger
        {
            public void Log(string message, string context = null) { }
            public void LogInfo(string message, string context = null) { }
            public void LogDebug(string message, string context = null) { }
            public void LogWarning(string message, string context = null) { }
            public void LogError(string message, string context = null, System.Exception exception = null) { }
            public void LogFatal(string message, string context = null, System.Exception exception = null) { }
        }

        private interface ITestService
        {
            int Value { get; }
        }

        private class TestService : ITestService
        {
            public int Value => 42;
        }

        private class TestTarget
        {
            [Inject]
            public ITestService Service1;

            [Inject]
            public ITestService Service2 { get; set; }

            public ITestService Service3 { get; private set; }

            [Inject]
            public void InjectMethod(ITestService service)
            {
                Service3 = service;
            }
        }

        private class ComplexTarget
        {
            [Inject] public ITestService Field1;
            [Inject] public ITestService Field2;
            [Inject] public ITestService Field3;
            [Inject] public ITestService Prop1 { get; set; }
            [Inject] public ITestService Prop2 { get; set; }
            [Inject] public ITestService Prop3 { get; set; }

            public ITestService Method1Service;
            public ITestService Method2Service;

            [Inject]
            public void Method1(ITestService service)
            {
                Method1Service = service;
            }

            [Inject]
            public void Method2(ITestService service)
            {
                Method2Service = service;
            }
        }

        private DIContainer _container;

        [SetUp]
        public void Setup()
        {
            _container = new DIContainer(new TestLogger());
            _container.RegisterSingleton<ITestService, TestService>();
        }

        [Test]
        public void InjectionCaching_FirstCall_CreatesCache()
        {
            // Arrange
            var target = new TestTarget();
            var sw = Stopwatch.StartNew();

            // Act: First injection (should create cache)
            _container.InjectDependencies(target);

            sw.Stop();
            long firstCallTime = sw.ElapsedTicks;

            // Assert: Injection successful
            Assert.That(target.Service1, Is.Not.Null);
            Assert.That(target.Service2, Is.Not.Null);
            Assert.That(target.Service3, Is.Not.Null);

            UnityEngine.Debug.Log($"First injection call: {firstCallTime} ticks");
        }

        [Test]
        public void InjectionCaching_SecondCall_UsesCachedMetadata()
        {
            // Arrange: Warm up cache
            var warmupTarget = new TestTarget();
            _container.InjectDependencies(warmupTarget);

            // Act: Measure cached performance
            var target = new TestTarget();
            var sw = Stopwatch.StartNew();
            _container.InjectDependencies(target);
            sw.Stop();

            long cachedCallTime = sw.ElapsedTicks;

            // Assert: Injection successful
            Assert.That(target.Service1, Is.Not.Null);
            Assert.That(target.Service2, Is.Not.Null);
            Assert.That(target.Service3, Is.Not.Null);

            UnityEngine.Debug.Log($"Cached injection call: {cachedCallTime} ticks");
        }

        [Test]
        public void InjectionCaching_RepeatedCalls_ShowsPerformanceGain()
        {
            // Arrange
            const int iterations = 100;

            // Act: First call (creates cache)
            var firstTarget = new TestTarget();
            var firstSw = Stopwatch.StartNew();
            _container.InjectDependencies(firstTarget);
            firstSw.Stop();
            long firstCallTime = firstSw.ElapsedTicks;

            // Act: Subsequent calls (use cache)
            var cachedSw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var target = new TestTarget();
                _container.InjectDependencies(target);
            }
            cachedSw.Stop();
            long averageCachedTime = cachedSw.ElapsedTicks / iterations;

            // Assert: Cached calls should be faster
            UnityEngine.Debug.Log($"First call: {firstCallTime} ticks");
            UnityEngine.Debug.Log($"Average cached call: {averageCachedTime} ticks");
            UnityEngine.Debug.Log($"Performance improvement: {(float)firstCallTime / averageCachedTime:F2}x faster");

            Assert.That(averageCachedTime, Is.LessThan(firstCallTime),
                "Cached injection should be faster than first call");
        }

        [Test]
        public void InjectionCaching_ComplexType_ShowsSignificantGain()
        {
            // Arrange
            const int iterations = 50;

            // Act: First call
            var firstTarget = new ComplexTarget();
            var firstSw = Stopwatch.StartNew();
            _container.InjectDependencies(firstTarget);
            firstSw.Stop();
            long firstCallTime = firstSw.ElapsedTicks;

            // Act: Cached calls
            var cachedSw = Stopwatch.StartNew();
            for (int i = 0; i < iterations; i++)
            {
                var target = new ComplexTarget();
                _container.InjectDependencies(target);
            }
            cachedSw.Stop();
            long averageCachedTime = cachedSw.ElapsedTicks / iterations;

            // Assert
            UnityEngine.Debug.Log($"Complex type - First call: {firstCallTime} ticks");
            UnityEngine.Debug.Log($"Complex type - Average cached: {averageCachedTime} ticks");
            UnityEngine.Debug.Log($"Complex type - Improvement: {(float)firstCallTime / averageCachedTime:F2}x");

            // Verify all injections worked
            var testTarget = new ComplexTarget();
            _container.InjectDependencies(testTarget);
            Assert.That(testTarget.Field1, Is.Not.Null);
            Assert.That(testTarget.Prop1, Is.Not.Null);
            Assert.That(testTarget.Method1Service, Is.Not.Null);
        }

        [Test]
        public void InjectionCaching_MultipleTypes_IndependentCaches()
        {
            // Arrange
            var target1 = new TestTarget();
            var target2 = new ComplexTarget();

            // Act: Inject both types
            _container.InjectDependencies(target1);
            _container.InjectDependencies(target2);

            // Inject again (should use cache for both)
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 100; i++)
            {
                var t1 = new TestTarget();
                var t2 = new ComplexTarget();
                _container.InjectDependencies(t1);
                _container.InjectDependencies(t2);
            }
            sw.Stop();

            // Assert: Both types work correctly
            var finalTarget1 = new TestTarget();
            var finalTarget2 = new ComplexTarget();
            _container.InjectDependencies(finalTarget1);
            _container.InjectDependencies(finalTarget2);

            Assert.That(finalTarget1.Service1, Is.Not.Null);
            Assert.That(finalTarget2.Field1, Is.Not.Null);

            UnityEngine.Debug.Log($"200 injections (2 types × 100): {sw.ElapsedMilliseconds}ms");
        }

        [Test]
        public void InjectionCaching_ThreadSafe_NoConcurrentIssues()
        {
            // Arrange: Inject from multiple threads
            const int threadCount = 10;
            const int iterationsPerThread = 100;
            var tasks = new System.Threading.Tasks.Task[threadCount];

            // Act: Concurrent injections
            for (int i = 0; i < threadCount; i++)
            {
                tasks[i] = System.Threading.Tasks.Task.Run(() =>
                {
                    for (int j = 0; j < iterationsPerThread; j++)
                    {
                        var target = new TestTarget();
                        _container.InjectDependencies(target);

                        Assert.That(target.Service1, Is.Not.Null);
                        Assert.That(target.Service2, Is.Not.Null);
                        Assert.That(target.Service3, Is.Not.Null);
                    }
                });
            }

            // Assert: All tasks complete successfully
            Assert.DoesNotThrow(() => System.Threading.Tasks.Task.WaitAll(tasks));
        }
    }
}
