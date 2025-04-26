using System;
using System.IO;
using UnityEngine;

namespace MythHunter.Utils.Logging
{
    /// <summary>
    /// Реалізація логера через файл
    /// </summary>
    public class FileLogger : ILogger
    {
        private readonly string _logFilePath;
        private LogLevel _logLevel = LogLevel.Info;
        
        public FileLogger(string fileName = "MythHunter.log")
        {
            _logFilePath = Path.Combine(Application.persistentDataPath, fileName);
            
            // Створення файлу та запис заголовка
            using (StreamWriter writer = new StreamWriter(_logFilePath, false))
            {
                writer.WriteLine($"=== MythHunter Log Started {DateTime.Now} ===");
            }
        }
        
        public void LogInfo(string message)
        {
            if (_logLevel <= LogLevel.Info)
                WriteToFile("INFO", message);
        }
        
        public void LogWarning(string message)
        {
            if (_logLevel <= LogLevel.Warning)
                WriteToFile("WARNING", message);
        }
        
        public void LogError(string message, Exception exception = null)
        {
            if (_logLevel <= LogLevel.Error)
            {
                if (exception != null)
                    WriteToFile("ERROR", $"{message}\nException: {exception}");
                else
                    WriteToFile("ERROR", message);
            }
        }
        
        public void LogDebug(string message)
        {
            if (_logLevel <= LogLevel.Debug)
                WriteToFile("DEBUG", message);
        }
        
        public void SetLogLevel(LogLevel level)
        {
            _logLevel = level;
        }
        
        private void WriteToFile(string level, string message)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(_logFilePath, true))
                {
                    writer.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")}] [{level}] {message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to write to log file: {ex.Message}");
            }
        }
    }
}