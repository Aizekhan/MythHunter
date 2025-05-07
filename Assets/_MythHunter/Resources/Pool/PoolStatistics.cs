// Шлях: Assets/_MythHunter/Code/Resources/Pool/PoolStatistics.cs
using System;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Статистика використання пулу об'єктів
    /// </summary>
    public class PoolStatistics
    {
        // Базова інформація
        public string PoolType { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public int InitialSize { get; set; } = 0;

        // Поточний стан
        public int ActiveCount { get; set; } = 0;
        public int InactiveCount { get; set; } = 0;
        public int TotalSize { get; set; } = 0;

        // Метрики використання
        public long TotalGetCount { get; set; } = 0;
        public long TotalReturnCount { get; set; } = 0;

        // Додаткова інформація для діагностики
        public float GetPerSecond { get; set; } = 0;
        public float ReturnPerSecond { get; set; } = 0;

        public override string ToString()
        {
            return $"Pool: {PoolType}, Active: {ActiveCount}, Inactive: {InactiveCount}, " +
                   $"Total: {TotalSize}, Gets: {TotalGetCount}, Returns: {TotalReturnCount}";
        }
    }
}
