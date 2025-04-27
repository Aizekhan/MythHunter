using MythHunter.Core.ECS;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Інтерфейс для систем з пізнім оновленням
    /// </summary>
    public interface ILateUpdateSystem : ISystem
    {
        void LateUpdate(float deltaTime);
    }
}