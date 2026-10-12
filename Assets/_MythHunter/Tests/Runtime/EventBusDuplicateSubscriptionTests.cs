// Assets/_MythHunter/Tests/Runtime/EventBusDuplicateSubscriptionTests.cs
using NUnit.Framework;
using System.Collections;
using Cysharp.Threading.Tasks;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using UnityEngine.TestTools;

namespace MythHunter.Tests.Runtime
{
    /// <summary>
    /// EPIC 08 Phase 3: Duplicate subscription detection tests for EventBus
    /// </summary>
    public class EventBusDuplicateSubscriptionTests
    {
        private class TestEventPool : IEventPool
        {
            public T Get<T>() where T : struct, IEvent => default;
            public void Return<T>(T evt) where T : struct, IEvent { }
        }

        private class TestLogger : IMythLogger
        {
            public System.Collections.Generic.List<string> Warnings = new();
            public System.Collections.Generic.List<string> Errors = new();

            public void Log(string message, string context = null) { }
            public void LogInfo(string message, string context = null) { }
            public void LogDebug(string message, string context = null) { }
            public void LogWarning(string message, string context = null)
            {
                Warnings.Add(message);
            }
            public void LogError(string message, string context = null, System.Exception exception = null)
            {
                Errors.Add(message);
            }
            public void LogFatal(string message, string context = null, System.Exception exception = null) { }
        }

        private struct TestEvent : IEvent
        {
            public int Value;
            public System.DateTime Timestamp { get; set; }
        }

        private EventBus _eventBus;
        private TestLogger _logger;
        private int _handlerCallCount;

        [SetUp]
        public void Setup()
        {
            _logger = new TestLogger();
            _eventBus = new EventBus(new TestEventPool(), _logger);
            _handlerCallCount = 0;
        }

        [TearDown]
        public void TearDown()
        {
            _eventBus?.Dispose();
        }

        [Test]
        public void Subscribe_DuplicateHandler_LogsWarning()
        {
            // Arrange
            void Handler(TestEvent evt) => _handlerCallCount++;

            // Act: Subscribe same handler twice
            _eventBus.Subscribe<TestEvent>(Handler);
            _eventBus.Subscribe<TestEvent>(Handler);

            // Assert: Warning was logged
            Assert.That(_logger.Warnings.Count, Is.EqualTo(1));
            Assert.That(_logger.Warnings[0], Does.Contain("Handler already subscribed"));
            Assert.That(_logger.Warnings[0], Does.Contain(nameof(TestEvent)));
        }

        [Test]
        public void Subscribe_DuplicateHandler_DoesNotAddTwice()
        {
            // Arrange
            void Handler(TestEvent evt) => _handlerCallCount++;

            // Act: Subscribe same handler twice
            _eventBus.Subscribe<TestEvent>(Handler);
            _eventBus.Subscribe<TestEvent>(Handler);

            // Publish event
            _eventBus.Publish(new TestEvent { Value = 42 });

            // Assert: Handler was called only once
            Assert.That(_handlerCallCount, Is.EqualTo(1), "Handler should be called only once");
        }

        [Test]
        public void Subscribe_DifferentHandlers_BothAdded()
        {
            // Arrange
            int handler1CallCount = 0;
            int handler2CallCount = 0;

            void Handler1(TestEvent evt) => handler1CallCount++;
            void Handler2(TestEvent evt) => handler2CallCount++;

            // Act: Subscribe different handlers
            _eventBus.Subscribe<TestEvent>(Handler1);
            _eventBus.Subscribe<TestEvent>(Handler2);

            // Publish event
            _eventBus.Publish(new TestEvent { Value = 42 });

            // Assert: Both handlers were called
            Assert.That(handler1CallCount, Is.EqualTo(1));
            Assert.That(handler2CallCount, Is.EqualTo(1));
            Assert.That(_logger.Warnings.Count, Is.EqualTo(0), "No warnings should be logged for different handlers");
        }

        [Test]
        public void Subscribe_SameHandlerDifferentPriorities_LogsWarning()
        {
            // Arrange
            void Handler(TestEvent evt) => _handlerCallCount++;

            // Act: Subscribe same handler with different priorities
            _eventBus.Subscribe<TestEvent>(Handler, EventPriority.High);
            _eventBus.Subscribe<TestEvent>(Handler, EventPriority.Normal);

            // Assert: Warning was logged (duplicate is detected regardless of priority)
            Assert.That(_logger.Warnings.Count, Is.EqualTo(1));
            Assert.That(_logger.Warnings[0], Does.Contain("Handler already subscribed"));
        }

        [UnityTest]
        public IEnumerator SubscribeAsync_DuplicateHandler_LogsWarning()
        {
            // Arrange
            async UniTask Handler(TestEvent evt)
            {
                await UniTask.Yield();
                _handlerCallCount++;
            }

            // Act: Subscribe same async handler twice
            _eventBus.SubscribeAsync<TestEvent>(Handler);
            _eventBus.SubscribeAsync<TestEvent>(Handler);

            yield return null;

            // Assert: Warning was logged
            Assert.That(_logger.Warnings.Count, Is.EqualTo(1));
            Assert.That(_logger.Warnings[0], Does.Contain("Async handler already subscribed"));
            Assert.That(_logger.Warnings[0], Does.Contain(nameof(TestEvent)));
        }

        [UnityTest]
        public IEnumerator SubscribeAsync_DuplicateHandler_DoesNotAddTwice()
        {
            // Arrange
            async UniTask Handler(TestEvent evt)
            {
                await UniTask.Yield();
                _handlerCallCount++;
            }

            // Act: Subscribe same async handler twice
            _eventBus.SubscribeAsync<TestEvent>(Handler);
            _eventBus.SubscribeAsync<TestEvent>(Handler);

            // Publish event
            _eventBus.PublishAsync(new TestEvent { Value = 42 });

            // Wait for async processing
            yield return new UnityEngine.WaitForSeconds(0.1f);

            // Assert: Handler was called only once
            Assert.That(_handlerCallCount, Is.EqualTo(1), "Async handler should be called only once");
        }

        [UnityTest]
        public IEnumerator SubscribeAsync_DifferentHandlers_BothAdded()
        {
            // Arrange
            int handler1CallCount = 0;
            int handler2CallCount = 0;

            async UniTask Handler1(TestEvent evt)
            {
                await UniTask.Yield();
                handler1CallCount++;
            }

            async UniTask Handler2(TestEvent evt)
            {
                await UniTask.Yield();
                handler2CallCount++;
            }

            // Act: Subscribe different async handlers
            _eventBus.SubscribeAsync<TestEvent>(Handler1);
            _eventBus.SubscribeAsync<TestEvent>(Handler2);

            // Publish event
            _eventBus.PublishAsync(new TestEvent { Value = 42 });

            // Wait for async processing
            yield return new UnityEngine.WaitForSeconds(0.1f);

            // Assert: Both handlers were called
            Assert.That(handler1CallCount, Is.EqualTo(1));
            Assert.That(handler2CallCount, Is.EqualTo(1));
            Assert.That(_logger.Warnings.Count, Is.EqualTo(0), "No warnings for different async handlers");
        }

        [Test]
        public void Subscribe_MultipleEvents_EachTrackedSeparately()
        {
            // Arrange
            struct AnotherTestEvent : IEvent
            {
                public int Value;
                public System.DateTime Timestamp { get; set; }
            }

            int testEventCount = 0;
            int anotherEventCount = 0;

            void TestHandler(TestEvent evt) => testEventCount++;
            void AnotherHandler(AnotherTestEvent evt) => anotherEventCount++;

            // Act: Subscribe same method name but different event types
            _eventBus.Subscribe<TestEvent>(TestHandler);
            _eventBus.Subscribe<TestEvent>(TestHandler); // Duplicate

            _eventBus.Subscribe<AnotherTestEvent>(AnotherHandler);
            _eventBus.Subscribe<AnotherTestEvent>(AnotherHandler); // Duplicate

            // Assert: Warnings logged for both event types
            Assert.That(_logger.Warnings.Count, Is.EqualTo(2));

            // Publish events
            _eventBus.Publish(new TestEvent { Value = 1 });
            _eventBus.Publish(new AnotherTestEvent { Value = 2 });

            // Assert: Each handler called only once
            Assert.That(testEventCount, Is.EqualTo(1));
            Assert.That(anotherEventCount, Is.EqualTo(1));
        }

        [Test]
        public void Subscribe_LambdaHandlers_ConsideredDifferent()
        {
            // Arrange & Act: Subscribe using lambda expressions
            _eventBus.Subscribe<TestEvent>(evt => _handlerCallCount++);
            _eventBus.Subscribe<TestEvent>(evt => _handlerCallCount++);

            // Publish event
            _eventBus.Publish(new TestEvent { Value = 42 });

            // Assert: Lambda expressions are different instances, so both are added
            // (This is expected behavior - lambdas create new delegate instances)
            Assert.That(_handlerCallCount, Is.EqualTo(2));
            Assert.That(_logger.Warnings.Count, Is.EqualTo(0), "Lambdas are different instances");
        }
    }
}
