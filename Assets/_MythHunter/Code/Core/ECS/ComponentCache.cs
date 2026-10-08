// Файл: Assets/_MythHunter/Code/Core/ECS/ComponentCache.cs

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Оптимізований кеш для швидкого доступу до компонентів певного типу
    /// </summary>
    public class ComponentCache<T> where T : struct, IComponent
    {
        private readonly Dictionary<int, T> _components = new Dictionary<int, T>();
        private readonly HashSet<int> _entityIds = new HashSet<int>();
        private readonly IEntityManager _entityManager;

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
        private readonly HashSet<int> _pendingEntities = new HashSet<int>();

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
        /// </summary>
        private void UpdateFullCache()
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

        /// <summary>
        /// Поступово оновлює кеш, обробляючи до 100 сутностей за раз
        /// </summary>
        private void UpdateIncrementalCache()
        {
            const int batchSize = 100;
            int processed = 0;

            // Якщо список очікуваних сутностей порожній, оновлюємо його
            if (_pendingEntities.Count == 0)
            {
                int[] entities = _entityManager.GetEntitiesWith<T>();
                foreach (int entityId in entities)
                {
                    if (!_components.ContainsKey(entityId))
                    {
                        _pendingEntities.Add(entityId);
                    }
                }
            }

            // Обробляємо до batchSize сутностей за один виклик
            var batch = new List<int>();
            foreach (int entityId in _pendingEntities)
            {
                batch.Add(entityId);
                processed++;

                if (processed >= batchSize)
                    break;
            }

            // Видаляємо оброблені сутності з очікуваних
            foreach (int entityId in batch)
            {
                _pendingEntities.Remove(entityId);

                T component = _entityManager.GetComponent<T>(entityId);
                _components[entityId] = component;
                _entityIds.Add(entityId);
            }

            // Позначаємо, що кеш потребує оновлення масиву ID
            _isDirty = true;
        }

        /// <summary>
        /// Додає компонент до кешу
        /// </summary>
        public void Add(int entityId, T component)
        {
            _components[entityId] = component;
            _entityIds.Add(entityId);
            _isDirty = true;
        }

        /// <summary>
        /// Видаляє компонент з кешу
        /// </summary>
        public void Remove(int entityId)
        {
            _components.Remove(entityId);
            _entityIds.Remove(entityId);
            _isDirty = true;
        }

        /// <summary>
        /// Отримує компонент з кешу за ідентифікатором сутності
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(int entityId)
        {
            if (_components.TryGetValue(entityId, out T component))
            {
                HitCount++;
                return component;
            }

            if (_strategy == CacheStrategy.OnDemand)
            {
                // Для стратегії OnDemand, кешуємо при запиті
                if (_entityManager.HasComponent<T>(entityId))
                {
                    component = _entityManager.GetComponent<T>(entityId);
                    _components[entityId] = component;
                    _entityIds.Add(entityId);
                    _isDirty = true;
                    HitCount++;
                    return component;
                }
            }

            MissCount++;
            return default;
        }

        /// <summary>
        /// Перевіряє, чи міститься компонент у кеші
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int entityId)
        {
            bool result = _components.ContainsKey(entityId);
            if (result)
                HitCount++;
            else
                MissCount++;
            return result;
        }

        /// <summary>
        /// Отримує всі ідентифікатори сутностей, що мають даний компонент
        /// </summary>
        public IReadOnlyCollection<int> GetAllEntityIds()
        {
            return _entityIds;
        }

        /// <summary>
        /// Отримує масив ID сутностей (кешований для швидкого доступу)
        /// </summary>
        public int[] GetEntityIdsArray()
        {
            if (_isDirty)
            {
                // Оновлюємо кешований масив
                _cachedEntityIds = new int[_entityIds.Count];
                _entityIds.CopyTo(_cachedEntityIds);
                _isDirty = false;
            }

            return _cachedEntityIds;
        }

        /// <summary>
        /// Отримує кількість кешованих компонентів
        /// </summary>
        public int Count => _components.Count;

        /// <summary>
        /// Повертає статистику кешу
        /// </summary>
        public CacheStatistics GetStatistics()
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
