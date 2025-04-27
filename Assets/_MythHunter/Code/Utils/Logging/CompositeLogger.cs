using System;
using System.Collections.Generic;

namespace MythHunter.Utils.Logging
{
    /// <summary>
    /// Комбінований логер, який використовує кілька логерів
    /// </summary>
    public class CompositeLogger : IMythLogger
    {
        private readonly List<IMythLogger> _loggers = new List<IMythLogger>();
        private LogLevel _logLevel = LogLevel.Info;
        
        public CompositeLogger(params IMythLogger[] loggers)
        {
            _loggers.AddRange(loggers);
        }
        
        public void AddLogger(IMythLogger logger)
        {
            _loggers.Add(logger);
        }
        
        public void RemoveLogger(IMythLogger logger)
        {
            _loggers.Remove(logger);
        }
        
        public void LogInfo(string message)
        {
            if (_logLevel <= LogLevel.Info)
            {
                foreach (var logger in _loggers)
                {
                    logger.LogInfo(message);
                }
            }
        }
        
        public void LogWarning(string message)
        {
            if (_logLevel <= LogLevel.Warning)
            {
                foreach (var logger in _loggers)
                {
                    logger.LogWarning(message);
                }
            }
        }
        
        public void LogError(string message, Exception exception = null)
        {
            if (_logLevel <= LogLevel.Error)
            {
                foreach (var logger in _loggers)
                {
                    logger.LogError(message, exception);
                }
            }
        }
        
        public void LogDebug(string message)
        {
            if (_logLevel <= LogLevel.Debug)
            {
                foreach (var logger in _loggers)
                {
                    logger.LogDebug(message);
                }
            }
        }
        
        public void SetLogLevel(LogLevel level)
        {
            _logLevel = level;
            
            foreach (var logger in _loggers)
            {
                logger.SetLogLevel(level);
            }
        }
    }
}