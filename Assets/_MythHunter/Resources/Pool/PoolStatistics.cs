using System;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Статистика використання пулу об'єктів
    /// </summary>
    public class PoolStatistics
    {
        public string PoolType { get; set; } = "";
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public int InitialSize { get; set; } = 0;
        public int ActiveCount { get; set; } = 0;
        public int InactiveCount { get; set; } = 0;
        public int TotalSize => ActiveCount + InactiveCount;
        public long TotalGetCount { get; set; } = 0;
        public long TotalReturnCount { get; set; } = 0;
    }
}
