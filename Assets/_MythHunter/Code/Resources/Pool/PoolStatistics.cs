// Шлях: Assets/_MythHunter/Code/Resources/Pool/PoolStatistics.cs
using System;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Статистика пулу об'єктів
    /// </summary>
    public class PoolStatistics
    {
        // Базова інформація
        public string PoolKey { get; set; } = string.Empty;
        public string PoolType { get; set; } = string.Empty;
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public int InitialSize { get; set; } = 0;

        // Поточний стан
        public int ActiveCount { get; set; } = 0;
        public int InactiveCount { get; set; } = 0;
        public int TotalSize => ActiveCount + InactiveCount;

        // Статистика використання
        public long TotalGetCount { get; set; } = 0;
        public long TotalReturnCount { get; set; } = 0;
        public long DiscardedCount { get; set; } = 0;

        // Додаткова інформація
        public string SceneName { get; set; } = string.Empty;
        public float AverageActiveTime { get; set; } = 0;
        public float PeakActiveTime { get; set; } = 0;

        public override string ToString()
        {
            return $"Pool: {PoolKey} ({PoolType}), Active: {ActiveCount}, Inactive: {InactiveCount}, " +
                   $"Total: {TotalSize}, Get: {TotalGetCount}, Return: {TotalReturnCount}, " +
                   (string.IsNullOrEmpty(SceneName) ? "" : $"Scene: {SceneName}");
        }
    }
}
