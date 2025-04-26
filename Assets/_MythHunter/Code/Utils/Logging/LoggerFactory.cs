namespace MythHunter.Utils.Logging
{
    /// <summary>
    /// Фабрика логерів
    /// </summary>
    public static class LoggerFactory
    {
        public static ILogger CreateUnityLogger(LogLevel level = LogLevel.Info)
        {
            var logger = new UnityLogger();
            logger.SetLogLevel(level);
            return logger;
        }
        
        public static ILogger CreateFileLogger(string fileName = "MythHunter.log", LogLevel level = LogLevel.Info)
        {
            var logger = new FileLogger(fileName);
            logger.SetLogLevel(level);
            return logger;
        }
        
        public static ILogger CreateCompositeLogger(LogLevel level = LogLevel.Info)
        {
            var unityLogger = CreateUnityLogger(level);
            var fileLogger = CreateFileLogger("MythHunter.log", level);
            
            return new CompositeLogger(unityLogger, fileLogger);
        }
        
        public static ILogger CreateDefaultLogger()
        {
            #if UNITY_EDITOR
            return CreateUnityLogger();
            #else
            return CreateCompositeLogger();
            #endif
        }
    }
}