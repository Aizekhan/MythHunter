namespace MythHunter.Utils.Logging
{
    /// <summary>
    /// Фабрика логерів
    /// </summary>
    public static class LoggerFactory
    {
        public static IMythLogger CreateUnityLogger(LogLevel level = LogLevel.Info)
        {
            var logger = new UnityLogger();
            logger.SetLogLevel(level);
            return logger;
        }
        
        public static IMythLogger CreateFileLogger(string fileName = "MythHunter.log", LogLevel level = LogLevel.Info)
        {
            var logger = new FileLogger(fileName);
            logger.SetLogLevel(level);
            return logger;
        }
        
        public static IMythLogger CreateCompositeLogger(LogLevel level = LogLevel.Info)
        {
            var unityLogger = CreateUnityLogger(level);
            var fileLogger = CreateFileLogger("MythHunter.log", level);
            
            return new CompositeLogger(unityLogger, fileLogger);
        }
        
        public static IMythLogger CreateDefaultLogger()
        {
            #if UNITY_EDITOR
            return CreateUnityLogger();
            #else
            return CreateCompositeLogger();
            #endif
        }
    }
}