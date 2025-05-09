// Шлях: Assets/_MythHunter/Code/Resources/Pool/ObjectPoolAdapter.cs
using MythHunter.Resources.Pool;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Адаптер для GenericObjectPool, що реалізує IObjectPool
    /// </summary>
    public class ObjectPoolAdapter<T> : BaseObjectPool where T : UnityEngine.Object
    {
        private readonly GenericObjectPool<T> _pool;

        public override int CountInactive => _pool.CountInactive;

        public ObjectPoolAdapter(GenericObjectPool<T> pool, IMythLogger logger = null)
            : base(logger, pool != null ? $"{typeof(T).Name}Adapter" : "UnnamedPoolAdapter")
        {
            _pool = pool;
        }

        public override void Clear()
        {
            _pool.Clear();
        }

        protected override void ReturnObjectInternal(UnityEngine.Object obj)
        {
            if (obj is T typedObj)
            {
                _pool.Release(typedObj);
            }
            else
            {
                LogWarning($"Cannot return object of type {obj.GetType().Name} to pool of type {typeof(T).Name}");
            }
        }
    }
}
