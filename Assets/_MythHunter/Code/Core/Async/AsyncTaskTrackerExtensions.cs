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
        /// Track a UniTaskVoid operation and forget it
        /// Usage: tracker.TrackAndForget(token => MyAsyncMethod(token), "MyOperation");
        /// </summary>
        public static void TrackAndForget(
            this IAsyncTaskTracker tracker,
            Func<CancellationToken, UniTaskVoid> operation,
            string operationName = null)
        {
            tracker.Track(operation, operationName);
        }

        /// <summary>
        /// Track a UniTask operation (converts to UniTaskVoid) and forget it
        /// Usage: tracker.TrackAndForget(token => MyAsyncMethod(token).AsUniTaskVoid(), "MyOperation");
        /// </summary>
        public static void TrackAndForget(
            this IAsyncTaskTracker tracker,
            Func<CancellationToken, UniTask> operation,
            string operationName = null)
        {
            tracker.Track(async token =>
            {
                await operation(token);
            }, operationName);
        }

        /// <summary>
        /// Track a parameterless UniTaskVoid operation
        /// Usage: tracker.TrackAndForget(() => MyAsyncMethod().AsUniTaskVoid(), "MyOperation");
        /// </summary>
        public static void TrackAndForget(
            this IAsyncTaskTracker tracker,
            Func<UniTaskVoid> operation,
            string operationName = null)
        {
            tracker.Track(_ => operation(), operationName);
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
