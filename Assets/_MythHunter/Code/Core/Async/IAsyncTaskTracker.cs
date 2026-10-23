// Assets/_MythHunter/Code/Core/Async/IAsyncTaskTracker.cs
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MythHunter.Core.Async
{
    /// <summary>
    /// EPIC 08: Interface for tracking fire-and-forget async operations
    /// </summary>
    public interface IAsyncTaskTracker : IDisposable
    {
        /// <summary>
        /// Track a fire-and-forget operation with cancellation support
        /// </summary>
        void Track(Func<CancellationToken, UniTask> asyncOperation, string operationName = null);

        /// <summary>
        /// Cancel all tracked operations
        /// </summary>
        void CancelAll();

        /// <summary>
        /// Cancel a specific operation by name
        /// </summary>
        void Cancel(string operationName);

        /// <summary>
        /// Wait for all tracked operations to complete (with timeout)
        /// </summary>
        UniTask WaitForAllAsync(int timeoutMs = 5000);

        /// <summary>
        /// Get count of currently active operations
        /// </summary>
        int GetActiveCount();

        /// <summary>
        /// Get names of all active operations
        /// </summary>
        string[] GetActiveOperationNames();
    }
}
