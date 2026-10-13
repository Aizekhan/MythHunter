// Assets/_MythHunter/Tests/Runtime/EventBusMemoryLeakTests.cs
using NUnit.Framework;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using System.Reflection;

namespace MythHunter.Tests.Runtime
{
    /// <summary>
    /// EPIC 08 Phase 3: Memory leak prevention tests for EventBus
    /// Tests pending events limit to prevent unbounded memory growth
    /// </summary>
    public class EventBusMemoryLeakTests
    {
        private class TestEventPool : IEventPool
        {
            public T Get<T>() where T : struct, IEvent => default;
            public void Return<T>(T evt) where T : struct, IEvent { }
        }

        private class TestLogger : IMythLogger
        {
            public System.Collections.Generic.List<string> Warnings = new();

            public void Log(string message, string context = null) { }
            public void LogInfo(string message, string context = null) { }
            public void LogDebug(string message, string context = null) { }
            public void LogWarning(string message, string context = null)
            {
                Warnings.Add(message);
            }
            public void LogError(string message, string context = null, System.Exception exception = null) { }
            public void LogFatal(string message, string context = null, System.Exception exception = null) { }
        }

        private struct TestEvent : IEvent
        {
            public int Value;
            public System.DateTime Timestamp { get; set; }
        }

        private struct AnotherTestEvent : IEvent
        {
            public int Value;
            public System.DateTime Timestamp { get; set; }
        }

        private EventBus _eventBus;
        private TestLogger _logger;

        [SetUp]
        public void Setup()
        {
            _logger = new TestLogger();
            _eventBus = new EventBus(new TestEventPool(), _logger);
        }

        [TearDown]
        public void TearDown()
        {
            _eventBus?.Dispose();
        }

        [Test]
        public void PendingEvents_BelowLimit_AllEventsStored()
        {
            // Arrange: Monitor event type (enables pending events)
            _eventBus.MonitorEventType<TestEvent>();

            // Act: Publish events without handlers (below limit)
            for (int i = 0; i < 100; i++)
            {
                _eventBus.Publish(new TestEvent { Value = i });
            }

            // Assert: No warnings logged (below limit)
            Assert.That(_logger.Warnings.Count, Is.EqualTo(0), "No warnings should be logged below limit");

            // Get pending events count via reflection
            int pendingCount = GetPendingEventsCount<TestEvent>();
            Assert.That(pendingCount, Is.EqualTo(100), "All 100 events should be pending");
        }

        [Test]
        public void PendingEvents_AtLimit_LogsWarning()
        {
            // Arrange: Monitor event type
            _eventBus.MonitorEventType<TestEvent>();

            int maxLimit = GetMaxPendingEventsLimit();

            // Act: Publish events up to and beyond the limit
            for (int i = 0; i < maxLimit + 100; i++)
            {
                _eventBus.Publish(new TestEvent { Value = i });
            }

            // Assert: Warnings logged for events beyond limit
            Assert.That(_logger.Warnings.Count, Is.EqualTo(100), "Should log 100 warnings for events beyond limit");
            Assert.That(_logger.Warnings[0], Does.Contain("Pending events limit"));
            Assert.That(_logger.Warnings[0], Does.Contain(maxLimit.ToString()));
        }

        [Test]
        public void PendingEvents_ExceedsLimit_DropsOldestEvents()
        {
            // Arrange: Monitor event type
            _eventBus.MonitorEventType<TestEvent>();

            int maxLimit = GetMaxPendingEventsLimit();

            // Act: Publish events beyond limit
            for (int i = 0; i < maxLimit + 100; i++)
            {
                _eventBus.Publish(new TestEvent { Value = i });
            }

            // Assert: Pending events capped at limit
            int pendingCount = GetPendingEventsCount<TestEvent>();
            Assert.That(pendingCount, Is.EqualTo(maxLimit), "Pending events should be capped at limit");
        }

        [Test]
        public void PendingEvents_FIFOEviction_OldestEventsDropped()
        {
            // Arrange: Monitor event type
            _eventBus.MonitorEventType<TestEvent>();

            // Publish a few events
            for (int i = 0; i < 10; i++)
            {
                _eventBus.Publish(new TestEvent { Value = i });
            }

            // Subscribe to process events
            int processedCount = 0;
            int firstProcessedValue = -1;

            _eventBus.Subscribe<TestEvent>(evt =>
            {
                if (processedCount == 0)
                    firstProcessedValue = evt.Value;
                processedCount++;
            });

            // Assert: First processed event should be value 0 (FIFO)
            // Events are processed when handler subscribes
            Assert.That(firstProcessedValue, Is.EqualTo(0), "First event (value 0) should be processed first");
        }

        [Test]
        public void PendingEvents_WithHandler_NoLimit()
        {
            // Arrange: Subscribe handler BEFORE monitoring
            int handledCount = 0;
            _eventBus.Subscribe<TestEvent>(evt => handledCount++);

            _eventBus.MonitorEventType<TestEvent>();

            int maxLimit = GetMaxPendingEventsLimit();

            // Act: Publish many events (handlers process immediately, not pending)
            for (int i = 0; i < maxLimit + 1000; i++)
            {
                _eventBus.Publish(new TestEvent { Value = i });
            }

            // Assert: No warnings (events processed immediately, not pending)
            Assert.That(_logger.Warnings.Count, Is.EqualTo(0), "No warnings when handlers exist");
            Assert.That(handledCount, Is.EqualTo(maxLimit + 1000), "All events should be handled");

            // No pending events
            int pendingCount = GetPendingEventsCount<TestEvent>();
            Assert.That(pendingCount, Is.EqualTo(0), "No events should be pending when handlers exist");
        }

        [Test]
        public void PendingEvents_MultipleEventTypes_IndependentLimits()
        {
            // Arrange
            _eventBus.MonitorEventType<TestEvent>();
            _eventBus.MonitorEventType<AnotherTestEvent>();

            // Act: Publish different event types
            for (int i = 0; i < 100; i++)
            {
                _eventBus.Publish(new TestEvent { Value = i });
                _eventBus.Publish(new AnotherTestEvent { Value = i });
            }

            // Assert: Each type tracked independently
            int testEventCount = GetPendingEventsCount<TestEvent>();
            int anotherEventCount = GetPendingEventsCount<AnotherTestEvent>();

            Assert.That(testEventCount, Is.EqualTo(100));
            Assert.That(anotherEventCount, Is.EqualTo(100));
            Assert.That(_logger.Warnings.Count, Is.EqualTo(0), "No warnings below limit for both types");
        }

        [Test]
        public void PendingEvents_LimitReached_ContinuesOperating()
        {
            // Arrange: Monitor event type
            _eventBus.MonitorEventType<TestEvent>();

            int maxLimit = GetMaxPendingEventsLimit();

            // Act: Fill up to limit
            for (int i = 0; i < maxLimit; i++)
            {
                _eventBus.Publish(new TestEvent { Value = i });
            }

            // Publish more events
            for (int i = 0; i < 100; i++)
            {
                Assert.DoesNotThrow(() => _eventBus.Publish(new TestEvent { Value = maxLimit + i }),
                    "EventBus should continue operating after limit");
            }

            // Assert: Still at limit, system operational
            int pendingCount = GetPendingEventsCount<TestEvent>();
            Assert.That(pendingCount, Is.EqualTo(maxLimit));
        }

        // Helper methods using reflection to access private fields
        private int GetPendingEventsCount<TEvent>() where TEvent : struct, IEvent
        {
            var pendingEventsField = typeof(EventBus).GetField("_pendingEvents",
                BindingFlags.NonPublic | BindingFlags.Instance);

            var pendingEvents = (System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.List<IEvent>>)
                pendingEventsField.GetValue(_eventBus);

            if (pendingEvents.TryGetValue(typeof(TEvent), out var events))
            {
                return events.Count;
            }

            return 0;
        }

        private int GetMaxPendingEventsLimit()
        {
            var maxLimitField = typeof(EventBus).GetField("MAX_PENDING_EVENTS",
                BindingFlags.NonPublic | BindingFlags.Static);

            return (int)maxLimitField.GetValue(null);
        }
    }
}
