using UnityEngine;

namespace MythHunter.Utils.Logging
{
    /// <summary>
    /// Реалізація логера через Unity Debug
    /// </summary>
    public class UnityLogger : ILogger
    {
        private LogLevel _logLevel = LogLevel.Info;
        
        public void LogInfo(string message)
        {
            if (_logLevel <= LogLevel.Info)
                Debug.Log($"[INFO] {message}");
        }
        
        public void LogWarning(string message)
        {
            if (_logLevel <= LogLevel.Warning)
                Debug.LogWarning($"[WARNING] {message}");
        }
        
        public void LogError(string message, System.Exception exception = null)
        {
            if (_logLevel <= LogLevel.Error)
            {
                if (exception != null)
                    Debug.LogError($"[ERROR] {message}\nException: {exception}");
                else
                    Debug.LogError($"[ERROR] {message}");
            }
        }
        
        public void LogDebug(string message)
        {
            if (_logLevel <= LogLevel.Debug)
                Debug.Log($"[DEBUG] {message}");
        }
        
        public void SetLogLevel(LogLevel level)
        {
            _logLevel = level;
        }
    }
}