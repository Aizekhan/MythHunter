namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Інтерфейс менеджера ентіті
    /// </summary>
    public interface IEntityManager
    {
        int CreateEntity();
        void DestroyEntity(int entityId);
        void AddComponent<TComponent>(int entityId, TComponent component) where TComponent : IComponent;
        bool HasComponent<TComponent>(int entityId) where TComponent : IComponent;
        TComponent GetComponent<TComponent>(int entityId) where TComponent : IComponent;
    }
}