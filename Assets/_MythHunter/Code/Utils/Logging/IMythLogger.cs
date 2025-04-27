namespace MythHunter.Utils.Logging
{
    /// <summary>
    /// Інтерфейс логування
    /// </summary>
    public interface IMythLogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, System.Exception exception = null);
        void LogDebug(string message);
        void SetLogLevel(LogLevel level);
    }
    
    /// <summary>
    /// Рівні логування
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        None
    }
}
