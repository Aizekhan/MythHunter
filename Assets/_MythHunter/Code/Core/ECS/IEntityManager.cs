namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Інтерфейс менеджера сутностей
    /// </summary>
    public interface IEntityManager
    {
        int CreateEntity();
        void DestroyEntity(int entityId);
        void AddComponent<TComponent>(int entityId, TComponent component) where TComponent : IComponent;
        bool HasComponent<TComponent>(int entityId) where TComponent : IComponent;
        TComponent GetComponent<TComponent>(int entityId) where TComponent : IComponent;
        public bool TryGetComponent<T>(int entityId, out T component) where T : struct, IComponent;

        void RemoveComponent<TComponent>(int entityId) where TComponent : IComponent;
        int[] GetAllEntities();
        int[] GetEntitiesWith<TComponent>() where TComponent : IComponent;
    }
}
