namespace MythHunter.Utils.Logging
{
    /// <summary>
    /// Інтерфейс логування
    /// </summary>
    public interface ILogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}