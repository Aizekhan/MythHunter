// Assets/_MythHunter/Code/Core/Async/AsyncTaskTrackerExtensions.cs
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace MythHunter.Core.Async
{
    /// <summary>
    /// EPIC 08: Extension methods for AsyncTaskTracker
    /// Provides convenient syntax for tracking fire-and-forget operations
    /// </summary>
    public static class AsyncTaskTrackerExtensions
    {
        /// <summary>
        /// Track a UniTask operation and forget it
        /// Usage: tracker.TrackAndForget(token => MyAsyncMethod(token), "MyOperation");
        /// </summary>
        public static void TrackAndForget(
            this IAsyncTaskTracker tracker,
            Func<CancellationToken, UniTask> operation,
            string operationName = null)
        {
            tracker.Track(operation, operationName);
        }

        /// <summary>
        /// Track a parameterless UniTask operation
        /// Usage: tracker.TrackAndForget(async () => await MyAsyncMethod(), "MyOperation");
        /// </summary>
        public static void TrackAndForget(
            this IAsyncTaskTracker tracker,
            Func<UniTask> operation,
            string operationName = null)
        {
            tracker.Track(async _ => await operation(), operationName);
        }
    }
}
