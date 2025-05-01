// Шлях: Assets/_MythHunter/Code/Resources/Pool/IObjectPool.cs
namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Загальний інтерфейс пулу об'єктів
    /// </summary>
    public interface IObjectPool
    {
        int CountActive
        {
            get;
        }
        int CountInactive
        {
            get;
        }
        void Clear();
        void ReturnObject(UnityEngine.Object obj);
    }

    /// <summary>
    /// Типізований інтерфейс пулу об'єктів
    /// </summary>
    public interface IObjectPool<T> : IObjectPool where T : class
    {
        T Get();
        void Release(T instance);
    }
}
