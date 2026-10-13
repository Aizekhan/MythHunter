// Assets/_MythHunter/Code/Core/Async/AsyncTaskTracker.cs
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Async
{
    /// <summary>
    /// EPIC 08: Tracks and manages fire-and-forget async operations
    /// Provides cancellation and exception handling for .Forget() operations
    /// </summary>
    public class AsyncTaskTracker : IAsyncTaskTracker
    {
        private readonly IMythLogger _logger;
        private readonly List<TrackedTask> _activeTasks = new List<TrackedTask>();
        private readonly object _lock = new object();
        private CancellationTokenSource _globalCts = new CancellationTokenSource();
        private bool _isDisposed;

        private struct TrackedTask
        {
            public string Name;
            public DateTime StartTime;
            public CancellationTokenSource Cts;
        }

        public AsyncTaskTracker(IMythLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Track a fire-and-forget operation with cancellation support
        /// </summary>
        public void Track(Func<CancellationToken, UniTask> asyncOperation, string operationName = null)
        {
            if (_isDisposed)
            {
                _logger.LogWarning("AsyncTaskTracker is disposed, cannot track new operations", "AsyncTracker");
                return;
            }

            var taskName = operationName ?? $"Task_{Guid.NewGuid():N}";
            var cts = CancellationTokenSource.CreateLinkedTokenSource(_globalCts.Token);

            var trackedTask = new TrackedTask
            {
                Name = taskName,
                StartTime = DateTime.UtcNow,
                Cts = cts
            };

            lock (_lock)
            {
                _activeTasks.Add(trackedTask);
            }

            _logger.LogDebug($"Tracking async operation: {taskName}", "AsyncTracker");

            // Execute the operation with exception handling
            ExecuteTrackedAsync(asyncOperation, trackedTask).Forget();
        }

        /// <summary>
        /// Execute a tracked async operation with cleanup and exception handling
        /// </summary>
        private async UniTaskVoid ExecuteTrackedAsync(Func<CancellationToken, UniTask> operation, TrackedTask task)
        {
            try
            {
                await operation(task.Cts.Token);
                _logger.LogDebug($"Async operation completed: {task.Name} (Duration: {(DateTime.UtcNow - task.StartTime).TotalMilliseconds:F0}ms)", "AsyncTracker");
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug($"Async operation cancelled: {task.Name}", "AsyncTracker");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Async operation failed: {task.Name} - {ex.Message}", "AsyncTracker", ex);
            }
            finally
            {
                // Cleanup
                lock (_lock)
                {
                    _activeTasks.Remove(task);
                }
                task.Cts?.Dispose();
            }
        }

        /// <summary>
        /// Cancel all tracked operations
        /// </summary>
        public void CancelAll()
        {
            _logger.LogInfo("Cancelling all tracked async operations", "AsyncTracker");
            _globalCts?.Cancel();
        }

        /// <summary>
        /// Cancel a specific operation by name
        /// </summary>
        public void Cancel(string operationName)
        {
            lock (_lock)
            {
                for (int i = _activeTasks.Count - 1; i >= 0; i--)
                {
                    if (_activeTasks[i].Name == operationName)
                    {
                        _logger.LogDebug($"Cancelling async operation: {operationName}", "AsyncTracker");
                        _activeTasks[i].Cts?.Cancel();
                        return;
                    }
                }
            }
            _logger.LogWarning($"No active operation found with name: {operationName}", "AsyncTracker");
        }

        /// <summary>
        /// Wait for all tracked operations to complete (with timeout)
        /// </summary>
        public async UniTask WaitForAllAsync(int timeoutMs = 5000)
        {
            var startTime = DateTime.UtcNow;
            while (true)
            {
                int activeCount;
                lock (_lock)
                {
                    activeCount = _activeTasks.Count;
                }

                if (activeCount == 0)
                    break;

                if ((DateTime.UtcNow - startTime).TotalMilliseconds > timeoutMs)
                {
                    _logger.LogWarning($"Timeout waiting for {activeCount} async operations to complete", "AsyncTracker");
                    break;
                }

                await UniTask.Delay(50);
            }
        }

        /// <summary>
        /// Get count of currently active operations
        /// </summary>
        public int GetActiveCount()
        {
            lock (_lock)
            {
                return _activeTasks.Count;
            }
        }

        /// <summary>
        /// Get names of all active operations
        /// </summary>
        public string[] GetActiveOperationNames()
        {
            lock (_lock)
            {
                var names = new string[_activeTasks.Count];
                for (int i = 0; i < _activeTasks.Count; i++)
                {
                    names[i] = _activeTasks[i].Name;
                }
                return names;
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _logger.LogInfo("Disposing AsyncTaskTracker", "AsyncTracker");
            _isDisposed = true;

            // Cancel all operations
            _globalCts?.Cancel();
            _globalCts?.Dispose();
            _globalCts = null;

            // Wait briefly for cleanup
            WaitForAllAsync(1000).Forget();

            // Cleanup remaining task sources
            lock (_lock)
            {
                foreach (var task in _activeTasks)
                {
                    task.Cts?.Dispose();
                }
                _activeTasks.Clear();
            }
        }
    }
}
