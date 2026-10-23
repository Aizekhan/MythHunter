using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MythHunter.Utils.Logging
{
    

    /// <summary>
    /// Сигнатура для методів, що будуть вставлятися в логи для додаткової обробки
    /// </summary>
    public delegate string LogEnricher(Dictionary<string, object> properties);

    /// <summary>
    /// Розширена реалізація логера для проекту MythHunter з підтримкою кольорів, категорій та контексту.
    /// </summary>
    public class MythLogger : IMythLogger
    {
        #region Fields and Properties

        private const string DEFAULT_CATEGORY = "General";
        private const string LOG_FILE_PREFIX = "mythgame_log_";
        private const string LOG_FILE_EXT = ".log";
        private const int MAX_LOG_FILES = 5;
        private const float MB = 1024 * 1024;
        private const float MAX_LOG_SIZE_MB = 10;

        // Статичні значки для різних типів логів
        private static readonly Dictionary<LogLevel, string> LogIcons = new Dictionary<LogLevel, string>
        {
            { LogLevel.Trace, "🔍" },
            { LogLevel.Debug, "🐞" },
            { LogLevel.Info, "ℹ️" },
            { LogLevel.Warning, "⚠️" },
            { LogLevel.Error, "❌" },
            { LogLevel.Fatal, "☠️" }
        };

        // Статичні кольори для різних типів логів (кольори у форматі для Unity Console)
        private static readonly Dictionary<LogLevel, string> LogColors = new Dictionary<LogLevel, string>
        {
            { LogLevel.Trace, "#AAAAAA" },  // Світло-сірий
            { LogLevel.Debug, "#DDDDDD" },  // Сірий
            { LogLevel.Info, "#FFFFFF" },   // Білий
            { LogLevel.Warning, "#FFCC00" },// Жовтий
            { LogLevel.Error, "#FF6666" },  // Червоний
            { LogLevel.Fatal, "#FF0000" }   // Яскраво-червоний
        };

        // Колекція категорій логів з їхніми назвами та ярликами
        private static readonly Dictionary<string, string> LogCategories = new Dictionary<string, string>
        {
            { "General", "🌐" },
            { "Network", "🌍" },
            { "Combat", "⚔️" },
            { "Movement", "🏃" },
            { "AI", "🧠" },
            { "UI", "🖥️" },
            { "Performance", "⚡" },
            { "Physics", "🔄" },
            { "Audio", "🔊" },
            { "Input", "🎮" },
            { "Resource", "📦" },
            { "Database", "💾" },
            { "Replay", "📼" },
            { "Analytics", "📊" },
            { "Phase", "⏱️" },
            { "Rune", "🔮" },
            { "Item", "🎒" },
            { "Character", "👤" },
            { "Startup", "🚀" },
            { "Cloud", "☁️" }
        };

        // Мінімальний рівень логування
        private LogLevel _minLogLevel;

        // Флаг для файлового логування
        private bool _logToFile;

        // Шлях до файлу логу
        private string _logFilePath;

        // Збагачувачі логу
        private List<LogEnricher> _enrichers = new List<LogEnricher>();

        // Контекст логування
        private Dictionary<string, object> _context = new Dictionary<string, object>();

        // Категорія за замовчуванням
        private string _defaultCategory = DEFAULT_CATEGORY;

        // Об'єкт блокування для потокобезпечного запису у файл
        private readonly object _fileLock = new object();

        #endregion

        #region Constructors

        /// <summary>
        /// Створює новий екземпляр MythLogger
        /// </summary>
        /// <param name="minLogLevel">Мінімальний рівень логування</param>
        /// <param name="logToFile">Чи потрібно писати логи у файл</param>
        /// <param name="defaultCategory">Категорія за замовчуванням</param>
        public MythLogger(LogLevel minLogLevel = LogLevel.Info, bool logToFile = false, string defaultCategory = DEFAULT_CATEGORY)
        {
            _minLogLevel = minLogLevel;
            _logToFile = logToFile;
            _defaultCategory = defaultCategory;

            if (logToFile)
            {
                InitializeFileLogging();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Implements the platform-neutral Framework logging contract while preserving MythHunter formatting and sinks.
        /// </summary>
        public void Log(RPGFramework.Logging.LogSeverity severity, string message, string category = null, Exception exception = null)
        {
            LogLevel mythHunterLevel;
            switch (severity)
            {
                case RPGFramework.Logging.LogSeverity.Trace:
                    mythHunterLevel = LogLevel.Trace;
                    break;
                case RPGFramework.Logging.LogSeverity.Debug:
                    mythHunterLevel = LogLevel.Debug;
                    break;
                case RPGFramework.Logging.LogSeverity.Information:
                    mythHunterLevel = LogLevel.Info;
                    break;
                case RPGFramework.Logging.LogSeverity.Warning:
                    mythHunterLevel = LogLevel.Warning;
                    break;
                case RPGFramework.Logging.LogSeverity.Error:
                    mythHunterLevel = LogLevel.Error;
                    break;
                case RPGFramework.Logging.LogSeverity.Critical:
                    mythHunterLevel = LogLevel.Fatal;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(severity), severity, "Unknown logging severity.");
            }

            if (exception != null)
            {
                message += $"\nException: {exception.Message}";
                if (exception.StackTrace != null)
                {
                    message += $"\nStackTrace: {exception.StackTrace}";
                }
            }

            Log(mythHunterLevel, message, category ?? _defaultCategory);
        }


        /// <summary>
        /// Логує інформаційне повідомлення
        /// </summary>
        public void LogInfo(string message, string category = null)
        {
            Log(LogLevel.Info, message, category ?? _defaultCategory);
        }

        /// <summary>
        /// Логує попередження
        /// </summary>
        public void LogWarning(string message, string category = null)
        {
            Log(LogLevel.Warning, message, category ?? _defaultCategory);
        }

        /// <summary>
        /// Логує помилку
        /// </summary>
        public void LogError(string message, string category = null, Exception exception = null)
{
    if (_minLogLevel <= LogLevel.Error)
    {
        string errorMsg = message;
        if (exception != null)
        {
            errorMsg += $"\nException: {exception.Message}";
            if (exception.StackTrace != null)
            {
                errorMsg += $"\nStackTrace: {exception.StackTrace}";
            }
        }
        Log(LogLevel.Error, errorMsg, category ?? _defaultCategory);
    }
}

        /// <summary>
        /// Логує фатальну помилку
        /// </summary>
        public void LogFatal(string message, string category = null)
        {
            Log(LogLevel.Fatal, message, category ?? _defaultCategory);
        }

        /// <summary>
        /// Логує відлагоджувальне повідомлення
        /// </summary>
        public void LogDebug(string message, string category = null)
        {
            Log(LogLevel.Debug, message, category ?? _defaultCategory);
        }

        /// <summary>
        /// Логує трасувальне повідомлення
        /// </summary>
        public void LogTrace(string message, string category = null)
        {
            Log(LogLevel.Trace, message, category ?? _defaultCategory);
        }

        /// <summary>
        /// Асоціює контекстні дані з логером
        /// </summary>
        public void WithContext(string key, object value)
        {
            if (_context.ContainsKey(key))
            {
                _context[key] = value;
            }
            else
            {
                _context.Add(key, value);
            }
        }

        /// <summary>
        /// Очищує всі контекстні дані
        /// </summary>
        public void ClearContext()
        {
            _context.Clear();
        }

        /// <summary>
        /// Додає збагачувач до логера
        /// </summary>
        public void AddEnricher(LogEnricher enricher)
        {
            if (enricher != null && !_enrichers.Contains(enricher))
            {
                _enrichers.Add(enricher);
            }
        }

        /// <summary>
        /// Видаляє збагачувач з логера
        /// </summary>
        public void RemoveEnricher(LogEnricher enricher)
        {
            if (enricher != null)
            {
                _enrichers.Remove(enricher);
            }
        }

        /// <summary>
        /// Встановлює категорію за замовчуванням
        /// </summary>
        public void SetDefaultCategory(string category)
        {
            _defaultCategory = !string.IsNullOrEmpty(category) ? category : DEFAULT_CATEGORY;
        }

        /// <summary>
        /// Змінює мінімальний рівень логування
        /// </summary>
        public void SetMinLogLevel(LogLevel level)
        {
            _minLogLevel = level;
        }

        /// <summary>
        /// Включає або виключає файлове логування
        /// </summary>
        public void EnableFileLogging(bool enable)
        {
            if (enable && !_logToFile)
            {
                _logToFile = true;
                InitializeFileLogging();
            }
            else if (!enable && _logToFile)
            {
                _logToFile = false;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Основний метод логування
        /// </summary>
        private void Log(
            LogLevel level,
            string message,
            string category,
            [CallerMemberName] string callerMember = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0
        )
        {
            if (level < _minLogLevel)
                return;

            string callerFile = Path.GetFileName(callerFilePath);

            // Підготовка контексту для збагачувачів
            var properties = new Dictionary<string, object>(_context)
            {
                { "level", level },
                { "message", message },
                { "category", category },
                { "timestamp", DateTime.Now },
                { "caller", $"{callerFile}:{callerMember}:{callerLineNumber}" }
            };

            // Застосування збагачувачів
            foreach (var enricher in _enrichers)
            {
                try
                {
                    string enrichment = enricher(properties);
                    if (!string.IsNullOrEmpty(enrichment))
                    {
                        message += " " + enrichment;
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = $"Error in log enricher: {ex.Message}";
                    UnityEngine.Debug.LogError(errorMsg);
                }
            }

            // Отримання значка для категорії
            string categoryIcon = GetCategoryIcon(category);

            // Отримання значка для рівня логування
            string levelIcon = LogIcons.ContainsKey(level) ? LogIcons[level] : "";

            // Форматування повідомлення з часом, категорією, рівнем та значками
            string timeStr = DateTime.Now.ToString("HH:mm:ss.fff");
            string colorTag = GetColorTag(level);

            // Форматування повного повідомлення для логу
            string fullMessage = $"{timeStr} {categoryIcon} {levelIcon} <b>[{category}]</b> {message}";

            // Якщо є детальний контекст, додати його
            if (properties.Count > 5) // Базовий контекст містить 5 елементів
            {
                StringBuilder contextStr = new StringBuilder(" {");
                bool first = true;

                foreach (var kv in properties.Where(p =>
                   p.Key != "level" &&
                   p.Key != "message" &&
                   p.Key != "category" &&
                   p.Key != "timestamp" &&
                   p.Key != "caller"))
                {
                    if (!first)
                        contextStr.Append(", ");
                    first = false;

                    contextStr.Append($"{kv.Key}={FormatContextValue(kv.Value)}");
                }

                contextStr.Append("}");
                fullMessage += contextStr.ToString();
            }

            // Додавання інформації про виклик для рівнів Debug та Trace
            if (level <= LogLevel.Debug)
            {
                fullMessage += $" [{callerFile}:{callerMember}():{callerLineNumber}]";
            }

            // Виведення у консоль Unity
            LogToUnityConsole(level, $"{colorTag}{fullMessage}</color>");

            // Виведення у файл, якщо увімкнено
            if (_logToFile)
            {
                LogToFile(level, $"{timeStr} [{level}] [{category}] {message}");
            }
        }

        /// <summary>
        /// Отримує відповідний значок для категорії логу
        /// </summary>
        private string GetCategoryIcon(string category)
        {
            return LogCategories.ContainsKey(category) ? LogCategories[category] : "📝";
        }

        /// <summary>
        /// Повертає тег кольору для рівня логування
        /// </summary>
        private string GetColorTag(LogLevel level)
        {
            return LogColors.ContainsKey(level) ? $"<color={LogColors[level]}>" : "<color=white>";
        }

        /// <summary>
        /// Виводить повідомлення у консоль Unity з відповідним рівнем логування
        /// </summary>
        private void LogToUnityConsole(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Error:
                case LogLevel.Fatal:
                    UnityEngine.Debug.LogError(message);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(message);
                    break;
                default:
                    UnityEngine.Debug.Log(message);
                    break;
            }
        }

        /// <summary>
        /// Записує повідомлення у файл логу
        /// </summary>
        private void LogToFile(LogLevel level, string message)
        {
            if (string.IsNullOrEmpty(_logFilePath))
                return;

            try
            {
                lock (_fileLock)
                {
                    File.AppendAllText(_logFilePath, message + Environment.NewLine);

                    // Перевірка розміру файлу
                    FileInfo logFile = new FileInfo(_logFilePath);
                    if (logFile.Length > MAX_LOG_SIZE_MB * MB)
                    {
                        RotateLogFiles();
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Помилка при записі в файл логу: {ex.Message}");
                _logToFile = false;
            }
        }

        /// <summary>
        /// Ініціалізує файлове логування
        /// </summary>
        private void InitializeFileLogging()
        {
            try
            {
                string logsDir = Path.Combine(Application.persistentDataPath, "Logs");

                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }

                // Створення нового файлу логу з поточною датою та часом
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                _logFilePath = Path.Combine(logsDir, $"{LOG_FILE_PREFIX}{timestamp}{LOG_FILE_EXT}");

                // Запис заголовка логу
                File.WriteAllText(_logFilePath, $"=== MythHunter Log Started at {DateTime.Now} ===\n" +
                    $"Application Version: {Application.version}\n" +
                    $"Unity Version: {Application.unityVersion}\n" +
                    $"Platform: {Application.platform}\n" +
                    $"System Language: {Application.systemLanguage}\n" +
                    $"Device Model: {SystemInfo.deviceModel}\n" +
                    $"Device Name: {SystemInfo.deviceName}\n" +
                    $"Operating System: {SystemInfo.operatingSystem}\n" +
                    $"Processor: {SystemInfo.processorType}\n" +
                    $"Memory: {SystemInfo.systemMemorySize} MB\n" +
                    $"Graphics Device: {SystemInfo.graphicsDeviceName}\n" +
                    $"Graphics Memory: {SystemInfo.graphicsMemorySize} MB\n" +
                    $"=== Log Entries ===\n\n");

                // Очищення старих логів
                CleanupOldLogs(logsDir);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Не вдалося ініціалізувати файлове логування: {ex.Message}");
                _logToFile = false;
            }
        }

        /// <summary>
        /// Ротація файлів логу при досягненні максимального розміру
        /// </summary>
        private void RotateLogFiles()
        {
            try
            {
                string directory = Path.GetDirectoryName(_logFilePath);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(_logFilePath);
                string extension = Path.GetExtension(_logFilePath);

                // Створення нового файлу з номером
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string newLogPath = Path.Combine(directory, $"{fileNameWithoutExt}_{timestamp}{extension}");

                // Закриття поточного файлу і створення нового
                _logFilePath = newLogPath;

                // Запис заголовка у новий файл
                File.WriteAllText(_logFilePath, $"=== MythHunter Log Continued at {DateTime.Now} ===\n\n");

                // Очищення старих логів
                CleanupOldLogs(directory);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Помилка при ротації файлів логу: {ex.Message}");
            }
        }

        /// <summary>
        /// Видаляє старі файли логів, залишаючи тільки останні
        /// </summary>
        private void CleanupOldLogs(string directory)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(directory);
                FileInfo[] logFiles = di.GetFiles($"{LOG_FILE_PREFIX}*{LOG_FILE_EXT}")
                                      .OrderByDescending(f => f.LastWriteTime)
                                      .ToArray();

                // Залишаємо тільки останні MAX_LOG_FILES файлів
                for (int i = MAX_LOG_FILES; i < logFiles.Length; i++)
                {
                    logFiles[i].Delete();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Помилка при очищенні старих логів: {ex.Message}");
            }
        }

        /// <summary>
        /// Форматує значення для контексту логу
        /// </summary>
        private string FormatContextValue(object value)
        {
            if (value == null)
                return "null";

            if (value is string)
                return $"\"{value}\"";

            if (value is DateTime dt)
                return dt.ToString("yyyy-MM-dd HH:mm:ss.fff");

            return value.ToString();
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Створює стандартний логер з конфігурацією за замовчуванням
        /// </summary>
        public static MythLogger CreateDefaultLogger()
        {
            // В режимі редактора використовуємо розширений режим з файловим логуванням
            if (Application.isEditor)
            {
                return new MythLogger(LogLevel.Debug, true, "General");
            }

            // В релізній збірці використовуємо більш обмежений режим
            bool isDevelopmentBuild = UnityEngine.Debug.isDebugBuild;
            LogLevel level = isDevelopmentBuild ? LogLevel.Info : LogLevel.Warning;
            bool logToFile = isDevelopmentBuild;

            return new MythLogger(level, logToFile, "General");
        }

        #endregion
    }

    /// <summary>
    /// Фабрика для створення логерів з різними налаштуваннями
    /// </summary>
    public static class MythLoggerFactory
    {
        private static IMythLogger _defaultLogger;
        private static Dictionary<string, IMythLogger> _loggers = new Dictionary<string, IMythLogger>();

        /// <summary>
        /// Створює або повертає логер за замовчуванням
        /// </summary>
        public static IMythLogger GetDefaultLogger()
        {
            if (_defaultLogger == null)
            {
                _defaultLogger = MythLogger.CreateDefaultLogger();
            }

            return _defaultLogger;
        }

        /// <summary>
        /// Створює або повертає логер для конкретної підсистеми
        /// </summary>
        public static IMythLogger GetLogger(string subsystem)
        {
            if (string.IsNullOrEmpty(subsystem))
            {
                return GetDefaultLogger();
            }

            if (!_loggers.TryGetValue(subsystem, out var logger))
            {
                logger = new MythLogger(defaultCategory: subsystem);
                _loggers[subsystem] = logger;
            }

            return logger;
        }

        /// <summary>
        /// Створює спеціалізований логер з конкретними параметрами
        /// </summary>
        public static IMythLogger CreateCustomLogger(LogLevel level, bool logToFile, string category)
        {
            return new MythLogger(level, logToFile, category);
        }
    }
}
