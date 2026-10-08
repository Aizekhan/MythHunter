using System;
using System.Collections.Generic;
using System.Linq;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Розширення для ComponentCache з додатковими функціями пошуку
    /// </summary>
    public static class ComponentCacheExtensions
    {
        /// <summary>
        /// Знаходить всі сутності з компонентом, що задовольняє певну умову
        /// </summary>
        public static List<int> FindEntities<T>(this ComponentCache<T> cache, Predicate<T> predicate)
            where T : struct, IComponent
        {
            var result = new List<int>();

            foreach (int entityId in cache.GetAllEntityIds())
            {
                T component = cache.Get(entityId);
                if (predicate(component))
                {
                    result.Add(entityId);
                }
            }

            return result;
        }

        /// <summary>
        /// Знаходить першу сутність з компонентом, що задовольняє певну умову
        /// </summary>
        public static int FindEntity<T>(this ComponentCache<T> cache, Predicate<T> predicate)
            where T : struct, IComponent
        {
            foreach (int entityId in cache.GetAllEntityIds())
            {
                T component = cache.Get(entityId);
                if (predicate(component))
                {
                    return entityId;
                }
            }

            return -1; // Повертаємо невалідний ID, якщо сутність не знайдено
        }

        /// <summary>
        /// Виконує дію для кожного компонента у кеші
        /// </summary>
        public static void ForEach<T>(this ComponentCache<T> cache, Action<int, T> action)
            where T : struct, IComponent
        {
            foreach (int entityId in cache.GetAllEntityIds())
            {
                T component = cache.Get(entityId);
                action(entityId, component);
            }
        }

        /// <summary>
        /// Фільтрує сутності за наявністю іншого компонента
        /// </summary>
        public static List<int> WithComponent<T, TOther>(this ComponentCache<T> cache, IEntityManager entityManager)
            where T : struct, IComponent
            where TOther : struct, IComponent
        {
            var result = new List<int>();

            foreach (int entityId in cache.GetAllEntityIds())
            {
                if (entityManager.HasComponent<TOther>(entityId))
                {
                    result.Add(entityId);
                }
            }

            return result;
        }

        /// <summary>
        /// Фільтрує сутності за відсутністю іншого компонента
        /// </summary>
        public static List<int> WithoutComponent<T, TOther>(this ComponentCache<T> cache, IEntityManager entityManager)
            where T : struct, IComponent
            where TOther : struct, IComponent
        {
            var result = new List<int>();

            foreach (int entityId in cache.GetAllEntityIds())
            {
                if (!entityManager.HasComponent<TOther>(entityId))
                {
                    result.Add(entityId);
                }
            }

            return result;
        }
    }
}
