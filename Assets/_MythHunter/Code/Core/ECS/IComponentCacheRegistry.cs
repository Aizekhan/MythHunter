using System.Collections.Generic;
using MythHunter.Core.ECS;

namespace MythHunter.Entities
{
    public interface IComponentCacheRegistry
    {
        ComponentCache<T> GetCache<T>() where T : struct, IComponent;
        void UpdateCache<T>() where T : struct, IComponent;
        void UpdateAllCaches();
        void ClearAllCaches();
        Dictionary<string, CacheStatistics> GetCacheStatistics();
    }
}
