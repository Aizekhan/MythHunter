// IPoolManager.cs
namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Інтерфейс менеджера об'єктних пулів
    /// </summary>
    public interface IPoolManager
    {
        T GetFromPool<T>(string key) where T : UnityEngine.Object;
        void ReturnToPool<T>(string key, T instance) where T : UnityEngine.Object;
        void CreatePool<T>(string key, T prefab, int initialSize) where T : UnityEngine.Object;
        void ClearPool(string key);
        void ClearAllPools();
    }
}
