using System;

namespace MythHunter.Utils.Logging
{
    /// <summary>
    /// Інтерфейс для системи логування проекту MythHunter.
    /// Забезпечує методи для логування повідомлень різних рівнів важливості.
    /// </summary>
    public interface IMythLogger
    {
        /// <summary>
        /// Логує інформаційне повідомлення
        /// </summary>
        /// <param name="message">Текст повідомлення</param>
        /// <param name="category">Категорія повідомлення (за замовчуванням - загальна)</param>
        void LogInfo(string message, string category = null);

        /// <summary>
        /// Логує попередження
        /// </summary>
        /// <param name="message">Текст попередження</param>
        /// <param name="category">Категорія повідомлення (за замовчуванням - загальна)</param>
        void LogWarning(string message, string category = null);

        /// <summary>
        /// Логує помилку
        /// </summary>
        /// <param name="message">Текст помилки</param>
        /// <param name="category">Категорія повідомлення (за замовчуванням - загальна)</param>
        void LogError(string message, string category = null);

        /// <summary>
        /// Логує фатальну помилку
        /// </summary>
        /// <param name="message">Текст фатальної помилки</param>
        /// <param name="category">Категорія повідомлення (за замовчуванням - загальна)</param>
        void LogFatal(string message, string category = null);

        /// <summary>
        /// Логує відлагоджувальне повідомлення
        /// </summary>
        /// <param name="message">Текст повідомлення</param>
        /// <param name="category">Категорія повідомлення (за замовчуванням - загальна)</param>
        void LogDebug(string message, string category = null);

        /// <summary>
        /// Логує трасувальне повідомлення
        /// </summary>
        /// <param name="message">Текст повідомлення</param>
        /// <param name="category">Категорія повідомлення (за замовчуванням - загальна)</param>
        void LogTrace(string message, string category = null);

        /// <summary>
        /// Асоціює контекстні дані з логером
        /// </summary>
        /// <param name="key">Ключ для контекстних даних</param>
        /// <param name="value">Значення контекстних даних</param>
        void WithContext(string key, object value);

        /// <summary>
        /// Очищує всі контекстні дані
        /// </summary>
        void ClearContext();

        /// <summary>
        /// Встановлює категорію за замовчуванням
        /// </summary>
        /// <param name="category">Нова категорія за замовчуванням</param>
        void SetDefaultCategory(string category);

        /// <summary>
        /// Змінює мінімальний рівень логування
        /// </summary>
        /// <param name="level">Новий мінімальний рівень логування</param>
        void SetMinLogLevel(LogLevel level);

        /// <summary>
        /// Включає або виключає файлове логування
        /// </summary>
        /// <param name="enable">Чи потрібно вмикати файлове логування</param>
        void EnableFileLogging(bool enable);
    }
}
