// IMovementSystem.cs
namespace MythHunter.Systems.Movement
{
    /// <summary>
    /// Інтерфейс для системи руху
    /// </summary>
    public interface IMovementSystem : MythHunter.Core.ECS.ISystem
    {
        void MoveEntity(int entityId, UnityEngine.Vector3 destination);
        void StopEntity(int entityId);
        float GetEntitySpeed(int entityId);
        bool IsEntityMoving(int entityId);
    }
}
