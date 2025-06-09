namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Базовий інтерфейс для систем ECS
    /// </summary>
    public interface ISystem
    {
        void Initialize();
        void Update(float deltaTime);
        void Dispose();
    }
}