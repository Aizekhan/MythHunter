// Assets/_MythHunter/Tests/Progression/TestLogger.cs

using MythHunter.Utils.Logging;
using System;

namespace MythHunter.Tests.Progression
{
    /// <summary>
    /// Simple logger implementation for testing.
    /// Writes to Console instead of Unity's Debug.Log.
    /// </summary>
    public class TestLogger : IMythLogger
    {
        public void Log(RPGFramework.Logging.LogSeverity severity, string message, string category = null, Exception exception = null)
        {
            Console.WriteLine($"[{severity}] [{category ?? "Test"}] {message}");
            if (exception != null)
                Console.WriteLine($"  Exception: {exception.Message}\n{exception.StackTrace}");
        }

        public void LogInfo(string message, string category = null)
        {
            Console.WriteLine($"[INFO] [{category ?? "Test"}] {message}");
        }

        public void LogWarning(string message, string category = null)
        {
            Console.WriteLine($"[WARNING] [{category ?? "Test"}] {message}");
        }

        public void LogError(string message, string category = null, Exception exception = null)
        {
            Console.WriteLine($"[ERROR] [{category ?? "Test"}] {message}");
            if (exception != null)
                Console.WriteLine($"  Exception: {exception.Message}\n{exception.StackTrace}");
        }

        public void LogDebug(string message, string category = null)
        {
            Console.WriteLine($"[DEBUG] [{category ?? "Test"}] {message}");
        }

        public void LogTrace(string message, string category = null)
        {
            Console.WriteLine($"[TRACE] [{category ?? "Test"}] {message}");
        }

        public void LogFatal(string message, string category = null)
        {
            Console.WriteLine($"[FATAL] [{category ?? "Test"}] {message}");
        }

        public void LogException(Exception exception, string category = null)
        {
            Console.WriteLine($"[EXCEPTION] [{category ?? "Test"}] {exception.Message}\n{exception.StackTrace}");
        }

        public void WithContext(string key, object value) { }
        public void ClearContext() { }
        public void SetDefaultCategory(string category) { }
        public void SetMinLogLevel(LogLevel level) { }
        public void EnableFileLogging(bool enable) { }
    }
}
