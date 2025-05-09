// Файл: Assets/_MythHunter/Code/Core/ECS/IComponentCacheRegistry.cs

using System;
using System.Collections.Generic;
using MythHunter.Core.ECS;
using MythHunter.Events.Domain;

namespace MythHunter.Entities
{
    public interface IComponentCacheRegistry
    {
        // Існуючі методи
        ComponentCache<T> GetCache<T>() where T : struct, IComponent;
        void UpdateCache<T>() where T : struct, IComponent;
        void UpdateAllCaches();
        void ClearAllCaches();
        Dictionary<string, CacheStatistics> GetCacheStatistics();

        // Нові методи для розширеного управління кешем
        void SetAutoUpdate(bool value);
        void SetUpdateInterval(int frames);
        void Update();
        void RegisterTypeForAutoCreate<T>() where T : struct, IComponent;
        void RegisterComponentForPhase<T>(GamePhase phase) where T : struct, IComponent;
        void UpdateCachesForCurrentPhase();

        // Підтримка подій
        void SubscribeToEvents();
        void UnsubscribeFromEvents();
    }
}
