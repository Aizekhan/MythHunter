// Assets/_MythHunter/Tests/Runtime/InitializationTransactionTests.cs
using NUnit.Framework;
using System;
using System.Collections.Generic;
using MythHunter.Core.Initialization;
using MythHunter.Utils.Logging;

namespace MythHunter.Tests.Runtime
{
    /// <summary>
    /// EPIC 08 Phase 4: Tests for initialization rollback mechanism
    /// </summary>
    public class InitializationTransactionTests
    {
        private class TestLogger : IMythLogger
        {
            public List<string> Logs = new();
            public List<string> Warnings = new();
            public List<string> Errors = new();

            public void Log(RPGFramework.Logging.LogSeverity severity, string message, string category = null, Exception exception = null) => Logs.Add(message);
            public void LogInfo(string message, string category = null) => Logs.Add(message);
            public void LogDebug(string message, string category = null) => Logs.Add(message);
            public void LogTrace(string message, string category = null) => Logs.Add(message);
            public void LogWarning(string message, string category = null) => Warnings.Add(message);
            public void LogError(string message, string category = null, Exception exception = null) => Errors.Add(message);
            public void LogFatal(string message, string category = null) { }
            public void WithContext(string key, object value) { }
            public void ClearContext() { }
            public void SetDefaultCategory(string category) { }
            public void SetMinLogLevel(Utils.Logging.LogLevel level) { }
            public void EnableFileLogging(bool enable) { }
        }

        private TestLogger _logger;

        [SetUp]
        public void Setup()
        {
            _logger = new TestLogger();
        }

        [Test]
        public void Transaction_Commit_NoRollback()
        {
            // Arrange
            bool rollbackCalled = false;

            // Act
            using (var transaction = new InitializationTransaction(_logger))
            {
                transaction.RegisterRollback(() => rollbackCalled = true, "Test rollback");
                transaction.Commit();
            }

            // Assert
            Assert.That(rollbackCalled, Is.False, "Rollback should not be called after commit");
            Assert.That(_logger.Logs, Does.Contain("Initialization transaction committed"));
        }

        [Test]
        public void Transaction_NoCommit_AutomaticRollback()
        {
            // Arrange
            bool rollbackCalled = false;

            // Act
            using (var transaction = new InitializationTransaction(_logger))
            {
                transaction.RegisterRollback(() => rollbackCalled = true, "Test rollback");
                // No commit - should auto-rollback on dispose
            }

            // Assert
            Assert.That(rollbackCalled, Is.True, "Rollback should be called on dispose if not committed");
            Assert.That(_logger.Warnings, Does.Contain("Transaction disposed without commit - rolling back"));
        }

        [Test]
        public void Transaction_ManualRollback_ExecutesActions()
        {
            // Arrange
            int rollbackCount = 0;

            using (var transaction = new InitializationTransaction(_logger))
            {
                transaction.RegisterRollback(() => rollbackCount++, "Rollback 1");
                transaction.RegisterRollback(() => rollbackCount++, "Rollback 2");
                transaction.RegisterRollback(() => rollbackCount++, "Rollback 3");

                // Act
                transaction.Rollback();

                // Assert
                Assert.That(rollbackCount, Is.EqualTo(3), "All rollback actions should execute");
            }
        }

        [Test]
        public void Transaction_Rollback_LIFOOrder()
        {
            // Arrange
            var executionOrder = new List<int>();

            using (var transaction = new InitializationTransaction(_logger))
            {
                transaction.RegisterRollback(() => executionOrder.Add(1), "Step 1");
                transaction.RegisterRollback(() => executionOrder.Add(2), "Step 2");
                transaction.RegisterRollback(() => executionOrder.Add(3), "Step 3");

                // Act
                transaction.Rollback();

                // Assert: Should execute in reverse order (LIFO)
                Assert.That(executionOrder, Is.EqualTo(new[] { 3, 2, 1 }),
                    "Rollback should execute in reverse order (LIFO)");
            }
        }

        [Test]
        public void Transaction_RollbackWithError_ContinuesOtherRollbacks()
        {
            // Arrange
            var executedRollbacks = new List<int>();

            using (var transaction = new InitializationTransaction(_logger))
            {
                transaction.RegisterRollback(() => executedRollbacks.Add(1), "Step 1");
                transaction.RegisterRollback(() => throw new Exception("Rollback error"), "Failing step");
                transaction.RegisterRollback(() => executedRollbacks.Add(3), "Step 3");

                // Act
                transaction.Rollback();

                // Assert: Both non-failing rollbacks should execute despite error
                Assert.That(executedRollbacks, Does.Contain(1));
                Assert.That(executedRollbacks, Does.Contain(3));
                Assert.That(_logger.Errors.Count, Is.EqualTo(1), "Error should be logged");
            }
        }

        [Test]
        public void Transaction_MultipleRollbacks_AllExecute()
        {
            // Arrange
            const int rollbackCount = 10;
            int executedCount = 0;

            using (var transaction = new InitializationTransaction(_logger))
            {
                for (int i = 0; i < rollbackCount; i++)
                {
                    transaction.RegisterRollback(() => executedCount++, $"Rollback {i}");
                }

                // Act
                transaction.Rollback();

                // Assert
                Assert.That(executedCount, Is.EqualTo(rollbackCount), "All rollbacks should execute");
            }
        }

        [Test]
        public void Transaction_RegisterAfterCommit_ThrowsException()
        {
            // Arrange
            var transaction = new InitializationTransaction(_logger);
            transaction.Commit();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                transaction.RegisterRollback(() => { }, "Test"),
                "Cannot register rollback after commit");

            transaction.Dispose();
        }

        [Test]
        public void Transaction_DisposeAfterCommit_NoRollback()
        {
            // Arrange
            bool rollbackCalled = false;
            var transaction = new InitializationTransaction(_logger);

            transaction.RegisterRollback(() => rollbackCalled = true, "Test");
            transaction.Commit();

            // Act
            transaction.Dispose();
            transaction.Dispose(); // Second dispose should be safe

            // Assert
            Assert.That(rollbackCalled, Is.False, "No rollback after commit and dispose");
        }

        [Test]
        public void Extensions_RegisterDisposableRollback_DisposesOnRollback()
        {
            // Arrange
            var disposable = new TestDisposable();

            using (var transaction = new InitializationTransaction(_logger))
            {
                transaction.RegisterDisposableRollback(disposable, "Test disposable");

                // Act: Rollback on dispose
            }

            // Assert
            Assert.That(disposable.IsDisposed, Is.True, "Disposable should be disposed on rollback");
        }

        [Test]
        public void Extensions_RegisterSafeRollback_HandlesErrors()
        {
            // Arrange
            using (var transaction = new InitializationTransaction(_logger))
            {
                transaction.RegisterSafeRollback(
                    () => throw new Exception("Safe rollback error"),
                    "Failing rollback",
                    _logger);

                // Act
                transaction.Rollback();

                // Assert
                Assert.That(_logger.Errors.Count, Is.GreaterThan(0), "Error should be logged");
            }
        }

        [Test]
        public void Transaction_RealWorldScenario_PartialInitialization()
        {
            // Arrange: Simulate game initialization
            var resource1 = new TestDisposable();
            var resource2 = new TestDisposable();
            var resource3 = new TestDisposable();

            try
            {
                using (var transaction = new InitializationTransaction(_logger))
                {
                    // Step 1: Initialize resource 1
                    transaction.RegisterDisposableRollback(resource1, "Resource 1");

                    // Step 2: Initialize resource 2
                    transaction.RegisterDisposableRollback(resource2, "Resource 2");

                    // Step 3: Simulate failure during resource 3 initialization
                    throw new Exception("Initialization failed at step 3");

                    // This would never execute
                    transaction.RegisterDisposableRollback(resource3, "Resource 3");
                    transaction.Commit();
                }
            }
            catch
            {
                // Exception expected
            }

            // Assert: Resources 1 and 2 should be disposed (rolled back)
            Assert.That(resource1.IsDisposed, Is.True, "Resource 1 should be cleaned up");
            Assert.That(resource2.IsDisposed, Is.True, "Resource 2 should be cleaned up");
            Assert.That(resource3.IsDisposed, Is.False, "Resource 3 was never initialized");
        }

        private class TestDisposable : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}
