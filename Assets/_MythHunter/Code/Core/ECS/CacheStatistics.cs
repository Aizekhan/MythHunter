using System;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Структура для зберігання статистики кешу компонентів
    /// </summary>
    public class CacheStatistics
    {
        public string ComponentType
        {
            get; set;
        }
        public int CachedCount
        {
            get; set;
        }
        public int UpdateCount
        {
            get; set;
        }
        public int HitCount
        {
            get; set;
        }
        public int MissCount
        {
            get; set;
        }
        public float HitRatio
        {
            get; set;
        }

        public override string ToString()
        {
            return $"Type: {ComponentType}, Cached: {CachedCount}, Updates: {UpdateCount}, " +
                   $"Hits: {HitCount}, Misses: {MissCount}, HitRatio: {HitRatio:P2}";
        }
    }
}
