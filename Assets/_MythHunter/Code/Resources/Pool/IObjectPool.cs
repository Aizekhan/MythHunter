namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Інтерфейс пулу об'єктів
    /// </summary>
    public interface IObjectPool<T> where T : UnityEngine.Object
    {
        T Get();
        void Return(T instance);
        void Clear();
        int CountActive
        {
            get;
        }
        int CountInactive
        {
            get;
        }
    }
}
