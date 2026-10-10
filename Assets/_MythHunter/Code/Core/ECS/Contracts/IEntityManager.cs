using System;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Identifies an entity in an ECS world through its integer handle.
    /// This compatibility API intentionally keeps integer IDs until callers can be migrated coherently.
    /// </summary>
    public interface IEntityManager
    {
        int CreateEntity();
        void DestroyEntity(int entityId);

        void AddComponent<TComponent>(int entityId, TComponent component)
            where TComponent : IComponent;

        bool HasComponent<TComponent>(int entityId)
            where TComponent : IComponent;

        TComponent GetComponent<TComponent>(int entityId)
            where TComponent : IComponent;

        bool TryGetComponent<TComponent>(int entityId, out TComponent component)
            where TComponent : struct, IComponent;

        void RemoveComponent<TComponent>(int entityId)
            where TComponent : IComponent;

        int[] GetAllEntities();

        int[] GetEntitiesWith<TComponent>()
            where TComponent : IComponent;
    }
}
