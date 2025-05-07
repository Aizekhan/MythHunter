// Шлях: Assets/_MythHunter/Code/Resources/Pool/PoolStatistics.cs
using System;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Структура для зберігання статистики пулу об'єктів
    /// </summary>
    public class PoolStatistics
    {
        /// <summary>
        /// Тип пулу об'єктів
        /// </summary>
        public string PoolType
        {
            get; set;
        }

        /// <summary>
        /// Час створення пулу
        /// </summary>
        public DateTime CreationTime
        {
            get; set;
        }

        /// <summary>
        /// Початковий розмір пулу
        /// </summary>
        public int InitialSize
        {
            get; set;
        }

        /// <summary>
        /// Кількість неактивних об'єктів у пулі
        /// </summary>
        public int InactiveCount
        {
            get; set;
        }

        /// <summary>
        /// Кількість активних об'єктів у пулі
        /// </summary>
        public int ActiveCount
        {
            get; set;
        }

        /// <summary>
        /// Загальний розмір пулу (активні + неактивні)
        /// </summary>
        public int TotalSize
        {
            get; set;
        }

        /// <summary>
        /// Загальна кількість запитів об'єктів (Get)
        /// </summary>
        public int TotalGetCount
        {
            get; set;
        }

        /// <summary>
        /// Загальна кількість повернень об'єктів (Return)
        /// </summary>
        public int TotalReturnCount
        {
            get; set;
        }

        /// <summary>
        /// Відображення статистики у вигляді рядка
        /// </summary>
        public override string ToString()
        {
            return $"Пул {PoolType}: Активні: {ActiveCount}, Неактивні: {InactiveCount}, " +
                   $"Всього: {TotalSize} (Запитів: {TotalGetCount}, Повернень: {TotalReturnCount})";
        }
    }
}
