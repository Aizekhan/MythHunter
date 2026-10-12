// Файл: Assets/_MythHunter/Code/Core/ECS/ComponentCache.cs
using RPGFramework.ECS;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Оптимізований кеш для швидкого доступу до компонентів певного типу
    /// EPIC 08 Phase 3: Thread-safe implementation with ReaderWriterLockSlim
    /// </summary>
    public class ComponentCache<T> : IDisposable where T : struct, IComponent
    {
        private readonly Dictionary<int, T> _components = new Dictionary<int, T>();
        private readonly HashSet<int> _entityIds = new HashSet<int>();
        private readonly IEntityManager _entityManager;

        // EPIC 08 Phase 3: Thread safety
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        // Додаємо масиви для швидкого доступу (для часто використовуваних операцій)
        private int[] _cachedEntityIds = new int[0];
        private bool _isDirty = true;

        // Додаємо статистику
        public int UpdateCount { get; private set; } = 0;
        public int HitCount { get; private set; } = 0;
        public int MissCount { get; private set; } = 0;

        // Додаємо стратегію кешування
        public enum CacheStrategy
        {
            Full,       // Кешувати всі компоненти
            OnDemand,   // Кешувати тільки при запиті
            Incremental // Кешувати поступово
        }

        private CacheStrategy _strategy = CacheStrategy.Full;
        private int _incrementalCursor = 0;

        public ComponentCache(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        /// <summary>
        /// Встановлює стратегію кешування
        /// </summary>
        public void SetCacheStrategy(CacheStrategy strategy)
        {
            _strategy = strategy;
        }

        /// <summary>
        /// Оновлює кеш, знаходячи всі компоненти даного типу
        /// </summary>
        public void Update()
        {
            UpdateCount++;

            switch (_strategy)
            {
                case CacheStrategy.Full:
                    UpdateFullCache();
                    break;
                case CacheStrategy.OnDemand:
                    // Для OnDemand нічого не робимо при оновленні
                    break;
                case CacheStrategy.Incremental:
                    UpdateIncrementalCache();
                    break;
            }
        }

        // Файл: Assets/_MythHunter/Code/Core/ECS/ComponentCache.cs (продовження)

        /// <summary>
        /// Оновлює весь кеш за один раз
        /// EPIC 08 Phase 3: Thread-safe with write lock
        /// </summary>
        private void UpdateFullCache()
        {
            _lock.EnterWriteLock();
            try
            {
                // Очищення кешу перед оновленням
                _components.Clear();
                _entityIds.Clear();

                // Отримання всіх сутностей з компонентом типу T
                int[] entities = _entityManager.GetEntitiesWith<T>();

                // Заповнення кешу
                foreach (int entityId in entities)
                {
                    T component = _entityManager.GetComponent<T>(entityId);
                    _components[entityId] = component;
                    _entityIds.Add(entityId);
                }

                // Позначаємо, що кеш потребує оновлення масиву ID
                _isDirty = true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Поступово оновлює кеш, обробляючи до 100 сутностей за раз
        /// EPIC 08 Phase 3: Thread-safe with write lock
        /// </summary>
        private void UpdateIncrementalCache()
        {
            const int batchSize = 100;
            int[] entities = _entityManager.GetEntitiesWith<T>();
            var currentEntityIds = new HashSet<int>(entities);

            _lock.EnterWriteLock();
            try
            {
                // Membership is reconciled every update so removals and destroyed entities
                // cannot remain in the snapshot while value refresh stays batched.
                var cachedIds = new List<int>(_components.Keys);
                foreach (int entityId in cachedIds)
                {
                    if (!currentEntityIds.Contains(entityId))
                    {
                        _components.Remove(entityId);
                        _entityIds.Remove(entityId);
                    }
                }

                if (entities.Length == 0)
                {
                    _incrementalCursor = 0;
                    _isDirty = true;
                    return;
                }

                // Refresh at most 100 components per update. A rotating cursor ensures
                // cached values and newly-added components eventually converge without starvation.
                _incrementalCursor %= entities.Length;
                int processed = System.Math.Min(batchSize, entities.Length);

                for (int i = 0; i < processed; i++)
                {
                    int index = (_incrementalCursor + i) % entities.Length;
                    int entityId = entities[index];
                    T component = _entityManager.GetComponent<T>(entityId);

                    _components[entityId] = component;
                    _entityIds.Add(entityId);
                }

                _incrementalCursor = (_incrementalCursor + processed) % entities.Length;
                _isDirty = true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Додає компонент до кешу
        /// EPIC 08 Phase 3: Thread-safe with write lock
        /// </summary>
        public void Add(int entityId, T component)
        {
            _lock.EnterWriteLock();
            try
            {
                _components[entityId] = component;
                _entityIds.Add(entityId);
                _isDirty = true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Видаляє компонент з кешу
        /// EPIC 08 Phase 3: Thread-safe with write lock
        /// </summary>
        public void Remove(int entityId)
        {
            _lock.EnterWriteLock();
            try
            {
                _components.Remove(entityId);
                _entityIds.Remove(entityId);
                _isDirty = true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Отримує компонент з кешу за ідентифікатором сутності
        /// EPIC 08 Phase 3: Thread-safe with upgradeable read lock
        /// </summary>
        public T Get(int entityId)
        {
            if (_strategy == CacheStrategy.OnDemand)
            {
                _lock.EnterUpgradeableReadLock();
                try
                {
                    // OnDemand is a live read: validate membership and value against the source of truth.
                    if (_entityManager.HasComponent<T>(entityId))
                    {
                        T current = _entityManager.GetComponent<T>(entityId);

                        _lock.EnterWriteLock();
                        try
                        {
                            _components[entityId] = current;
                            _entityIds.Add(entityId);
                            _isDirty = true;
                            HitCount++;
                        }
                        finally
                        {
                            _lock.ExitWriteLock();
                        }

                        return current;
                    }

                    // Need to remove, upgrade to write lock
                    _lock.EnterWriteLock();
                    try
                    {
                        _components.Remove(entityId);
                        _entityIds.Remove(entityId);
                        _isDirty = true;
                        MissCount++;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }

                    return default;
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }

            _lock.EnterReadLock();
            try
            {
                if (_components.TryGetValue(entityId, out T component))
                {
                    HitCount++;
                    return component;
                }

                MissCount++;
                return default;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Перевіряє, чи міститься компонент у кеші
        /// EPIC 08 Phase 3: Thread-safe with upgradeable read lock
        /// </summary>
        public bool Contains(int entityId)
        {
            if (_strategy == CacheStrategy.OnDemand)
            {
                _lock.EnterUpgradeableReadLock();
                try
                {
                    // Contains is a membership query, not a query for cache residency.
                    // Resolve against the authoritative store even when this ID has not
                    // previously been observed through Get or enumeration.
                    if (_entityManager.HasComponent<T>(entityId))
                    {
                        T current = _entityManager.GetComponent<T>(entityId);

                        _lock.EnterWriteLock();
                        try
                        {
                            _components[entityId] = current;
                            _entityIds.Add(entityId);
                            _isDirty = true;
                            HitCount++;
                        }
                        finally
                        {
                            _lock.ExitWriteLock();
                        }

                        return true;
                    }

                    // Need to remove, upgrade to write lock
                    _lock.EnterWriteLock();
                    try
                    {
                        _components.Remove(entityId);
                        _entityIds.Remove(entityId);
                        _isDirty = true;
                        MissCount++;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }

                    return false;
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }

            _lock.EnterReadLock();
            try
            {
                bool result = _components.ContainsKey(entityId);
                if (result)
                    HitCount++;
                else
                    MissCount++;
                return result;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Отримує всі ідентифікатори сутностей, що мають даний компонент
        /// EPIC 08 Phase 3: Thread-safe with upgradeable read lock
        /// </summary>
        public IReadOnlyCollection<int> GetAllEntityIds()
        {
            if (_strategy == CacheStrategy.OnDemand)
            {
                _lock.EnterUpgradeableReadLock();
                try
                {
                    var cachedIds = new List<int>(_entityIds);
                    bool needsWrite = false;

                    foreach (int entityId in cachedIds)
                    {
                        if (!_entityManager.HasComponent<T>(entityId))
                        {
                            needsWrite = true;
                            break;
                        }
                    }

                    if (needsWrite)
                    {
                        _lock.EnterWriteLock();
                        try
                        {
                            foreach (int entityId in cachedIds)
                            {
                                if (!_entityManager.HasComponent<T>(entityId))
                                {
                                    _components.Remove(entityId);
                                    _entityIds.Remove(entityId);
                                    _isDirty = true;
                                }
                            }
                        }
                        finally
                        {
                            _lock.ExitWriteLock();
                        }
                    }

                    return new HashSet<int>(_entityIds);
                }
                finally
                {
                    _lock.ExitUpgradeableReadLock();
                }
            }

            _lock.EnterReadLock();
            try
            {
                return new HashSet<int>(_entityIds);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Отримує масив ID сутностей (кешований для швидкого доступу)
        /// EPIC 08 Phase 3: Thread-safe with upgradeable read lock
        /// </summary>
        public int[] GetEntityIdsArray()
        {
            // Ensure OnDemand enumeration does not expose cached IDs whose component was removed.
            if (_strategy == CacheStrategy.OnDemand)
            {
                GetAllEntityIds();
            }

            _lock.EnterUpgradeableReadLock();
            try
            {
                if (_isDirty)
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        // Оновлюємо кешований масив
                        _cachedEntityIds = new int[_entityIds.Count];
                        _entityIds.CopyTo(_cachedEntityIds);
                        _isDirty = false;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }

                return _cachedEntityIds;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Отримує кількість кешованих компонентів
        /// </summary>
        public int Count => _components.Count;

        /// <summary>
        /// Повертає статистику кешу
        /// EPIC 08 Phase 3: Thread-safe with read lock
        /// </summary>
        public CacheStatistics GetStatistics()
        {
            _lock.EnterReadLock();
            try
            {
                return new CacheStatistics
                {
                    ComponentType = typeof(T).Name,
                    CachedCount = Count,
                    UpdateCount = UpdateCount,
                    HitCount = HitCount,
                    MissCount = MissCount,
                    HitRatio = HitCount + MissCount > 0 ? (float)HitCount / (HitCount + MissCount) : 0
                };
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        /// <summary>
        /// Dispose lock resources
        /// EPIC 08 Phase 3: Proper cleanup of ReaderWriterLockSlim
        /// </summary>
        public void Dispose()
        {
            _lock?.Dispose();
        }

        /// <summary>
        /// Швидкий ітератор для обробки всіх компонентів без запитів до словника
        /// </summary>
        public ComponentEnumerator GetEnumerator()
        {
            return new ComponentEnumerator(this);
        }

        /// <summary>
        /// Структура для швидкої ітерації по компонентах
        /// </summary>
        public struct ComponentEnumerator
        {
            private readonly ComponentCache<T> _cache;
            private readonly int[] _entityIds;
            private int _index;
            private int _count;

            public ComponentEnumerator(ComponentCache<T> cache)
            {
                _cache = cache;
                _entityIds = cache.GetEntityIdsArray();
                _index = -1;
                _count = _entityIds.Length;
            }

            public bool MoveNext()
            {
                return ++_index < _count;
            }

            public (int EntityId, T Component) Current
            {
                get
                {
                    int entityId = _entityIds[_index];
                    return (entityId, _cache._components[entityId]);
                }
            }
        }
    }
}
