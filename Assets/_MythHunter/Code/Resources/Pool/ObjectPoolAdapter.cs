// Шлях: Assets/_MythHunter/Code/Resources/Pool/ObjectPoolAdapter.cs

using MythHunter.Resources.Pool;
using UnityEngine;

/// <summary>
/// Адаптер для GenericObjectPool, що реалізує IObjectPool
/// </summary>
public class ObjectPoolAdapter<T> : IObjectPool where T : UnityEngine.Object
{
    private readonly GenericObjectPool<T> _pool;

    public int CountActive => _pool.ActiveCount;
    public int CountInactive => _pool.InactiveCount;

    public ObjectPoolAdapter(GenericObjectPool<T> pool)
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
            _pool.Release(typedObj);
        }
    }
}
