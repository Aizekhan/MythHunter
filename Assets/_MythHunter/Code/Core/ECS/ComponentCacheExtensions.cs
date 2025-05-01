// Шлях: Assets/_MythHunter/Code/Core/ECS/ComponentCacheExtensions.cs

using System.Collections.Generic;
using System.Linq;
using MythHunter.Core.ECS;

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
        public static List<int> FindEntities<T>(this ComponentCache<T> cache, System.Predicate<T> predicate)
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
        public static int FindEntity<T>(this ComponentCache<T> cache, System.Predicate<T> predicate)
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
    }
}
