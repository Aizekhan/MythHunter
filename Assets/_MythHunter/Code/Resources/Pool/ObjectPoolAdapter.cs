/// <summary>
/// Адаптер для ObjectPool, що реалізує IObjectPool
/// </summary>
 using MythHunter.Resources.Pool; 
public class ObjectPoolAdapter<T> : IObjectPool where T : UnityEngine.Object
{
    private readonly ObjectPool _pool;

    public int CountActive => 0; // Заглушка або реалізація
    public int CountInactive => 0; // Заглушка або реалізація

    public ObjectPoolAdapter(ObjectPool pool)
    {
        _pool = pool;
    }

    public void Clear()
    {
        _pool.Clear();
    }

    public void ReturnObject(UnityEngine.Object obj)
    {
        if (obj is T typedObj)
        {
            _pool.Return(typedObj);
        }
    }
}
