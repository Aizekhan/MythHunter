using MythHunter.Core.ECS;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Інтерфейс для систем з фіксованим оновленням
    /// </summary>
    public interface IFixedUpdateSystem : ISystem
    {
        void FixedUpdate(float fixedDeltaTime);
    }
}