// Шлях: Assets/_MythHunter/Code/Resources/Pool/IPoolManager.cs
using System.Collections.Generic;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Інтерфейс менеджера об'єктних пулів
    /// </summary>
    public interface IPoolManager
    {
        T GetFromPool<T>(string key) where T : UnityEngine.Object;
        void ReturnToPool(string key, UnityEngine.Object instance);
        void CreatePool<T>(string key, T prefab, int initialSize) where T : UnityEngine.Object;
        void ClearPool(string key);
        void ClearAllPools();

        // Нові методи для відстеження та аналізу пулів
        Dictionary<string, PoolStatistics> GetAllPoolsStatistics();
        void CheckForLeaks();
        void TrimExcessObjects(int maxInactivePerPool = 20);
        int GetTotalActiveObjects();
        void SetAutoCleanupInterval(float seconds);
    }
}
