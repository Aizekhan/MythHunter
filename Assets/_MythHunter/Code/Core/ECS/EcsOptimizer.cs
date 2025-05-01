using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Сервіс для оптимізації роботи ECS системи
    /// </summary>
    public class EcsOptimizer
    {
        private readonly IEntityManager _entityManager;
        private readonly IMythLogger _logger;

        // Кеш масивів сутностей для зменшення алокацій
        private readonly Dictionary<int, int[]> _entityArraysCache = new Dictionary<int, int[]>();
        private readonly Dictionary<System.Type, int[]> _componentEntityArraysCache = new Dictionary<System.Type, int[]>();

        [Inject]
        public EcsOptimizer(IEntityManager entityManager, IMythLogger logger)
        {
            _entityManager = entityManager;
            _logger = logger;
        }

        /// <summary>
        /// Виконує оптимізацію роботи ECS системи
        /// </summary>
        public void Optimize()
        {
            // Очищення кешу для запобігання витокам пам'яті
            _entityArraysCache.Clear();
            _componentEntityArraysCache.Clear();

            _logger.LogInfo("ECS system optimized", "ECS");
        }

        /// <summary>
        /// Отримує кешований масив сутностей заданої довжини
        /// </summary>
        public int[] GetEntityArray(int size)
        {
            if (_entityArraysCache.TryGetValue(size, out var array))
            {
                return array;
            }

            var newArray = new int[size];
            _entityArraysCache[size] = newArray;
            return newArray;
        }

        /// <summary>
        /// Кешує масив сутностей для конкретного типу компонента
        /// </summary>
        public void CacheEntitiesWithComponent<T>(int[] entities) where T : struct, IComponent
        {
            _componentEntityArraysCache[typeof(T)] = entities;
        }

        /// <summary>
        /// Отримує кешований масив сутностей з компонентом заданого типу
        /// </summary>
        public int[] GetCachedEntitiesWithComponent<T>() where T : struct, IComponent
        {
            if (_componentEntityArraysCache.TryGetValue(typeof(T), out var entities))
            {
                return entities;
            }

            return null;
        }

        /// <summary>
        /// Оптимізує структуру сутностей
        /// </summary>
        public void OptimizeEntityStructure()
        {
            // Ця функція може виконувати більш глибоку оптимізацію структури сутностей
            // Наприклад, дефрагментацію ID, упаковку компонентів і т.д.

            // Заглушка для майбутньої реалізації
            _logger.LogInfo("Entity structure optimization performed", "ECS");
        }
    }
}
