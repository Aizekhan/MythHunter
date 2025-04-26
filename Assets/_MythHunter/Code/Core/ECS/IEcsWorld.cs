namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Інтерфейс світу ECS
    /// </summary>
    public interface IEcsWorld
    {
        IEntityManager EntityManager { get; }
        void Initialize();
        void Update(float deltaTime);
        void Dispose();
    }
}