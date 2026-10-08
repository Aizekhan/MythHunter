using System;

namespace MythHunter.Utils.Logging
{
    /// <summary>
    /// Рівні логування, від найменш до найбільш важливого
    /// </summary>
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Fatal = 5,
        Off = 6 // Вимкнути логування
    }

    /// <summary>
    /// Інтерфейс для системи логування проекту MythHunter.
    /// Забезпечує методи для логування повідомлень різних рівнів важливості.
    /// </summary>
    public interface IMythLogger
    {
        void LogInfo(string message, string category = null);
        void LogWarning(string message, string category = null);
        void LogError(string message, string category = null, Exception exception = null);
        void LogFatal(string message, string category = null);
        void LogDebug(string message, string category = null);
        void LogTrace(string message, string category = null);
        void WithContext(string key, object value);
        void ClearContext();
        void SetDefaultCategory(string category);
        void SetMinLogLevel(LogLevel level);
        void EnableFileLogging(bool enable);
    }
}