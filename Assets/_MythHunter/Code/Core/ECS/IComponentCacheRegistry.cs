// Файл: Assets/_MythHunter/Code/Core/ECS/IComponentCacheRegistry.cs

using System;
using System.Collections.Generic;
using MythHunter.Core.ECS;

namespace MythHunter.Entities
{
    /// <summary>
    /// Інтерфейс для реєстру кешів компонентів
    /// </summary>
    public interface IComponentCacheRegistry
    {
        // Базові методи роботи з кешем
        ComponentCache<T> GetCache<T>() where T : struct, IComponent;
        void UpdateCache<T>() where T : struct, IComponent;
        void UpdateAllCaches();
        void ClearAllCaches();
        Dictionary<string, CacheStatistics> GetCacheStatistics();

        // Методи для розширеного управління кешем
        void SetAutoUpdate(bool value);
        void SetUpdateInterval(int frames);
        void Update();
        void RegisterTypeForAutoCreate<T>() where T : struct, IComponent;

        // Методи для роботи з фазами (використовують стрінг замість GamePhase)
        void RegisterComponentForPhase<T>(string phaseId) where T : struct, IComponent;
        void UpdateCachesForCurrentPhase();
    }
}
