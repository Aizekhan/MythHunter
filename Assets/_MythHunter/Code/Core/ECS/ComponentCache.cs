using MythHunter.Core.ECS;
using System.Collections.Generic;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Кеш для швидкого доступу до компонентів певного типу
    /// </summary>
    public class ComponentCache<T> where T : struct, IComponent
    {
        private readonly Dictionary<int, T> _components = new Dictionary<int, T>();
        private readonly HashSet<int> _entityIds = new HashSet<int>();
        private readonly IEntityManager _entityManager;

        public ComponentCache(IEntityManager entityManager)
        {
            _entityManager = entityManager;
        }

        /// <summary>
        /// Оновлює кеш, знаходячи всі компоненти даного типу
        /// </summary>
        public void Update()
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
        }

        /// <summary>
        /// Додає компонент до кешу
        /// </summary>
        public void Add(int entityId, T component)
        {
            _components[entityId] = component;
            _entityIds.Add(entityId);
        }

        /// <summary>
        /// Видаляє компонент з кешу
        /// </summary>
        public void Remove(int entityId)
        {
            _components.Remove(entityId);
            _entityIds.Remove(entityId);
        }

        /// <summary>
        /// Отримує компонент з кешу за ідентифікатором сутності
        /// </summary>
        public T Get(int entityId)
        {
            return _components.TryGetValue(entityId, out T component) ? component : default;
        }

        /// <summary>
        /// Перевіряє, чи міститься компонент у кеші
        /// </summary>
        public bool Contains(int entityId)
        {
            return _components.ContainsKey(entityId);
        }

        /// <summary>
        /// Отримує всі ідентифікатори сутностей, що мають даний компонент
        /// </summary>
        public IReadOnlyCollection<int> GetAllEntityIds()
        {
            return _entityIds;
        }

        /// <summary>
        /// Отримує кількість кешованих компонентів
        /// </summary>
        public int Count => _components.Count;
    }
}
