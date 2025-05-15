// Assets/_MythHunter/Code/Systems/Movement/IMovementSystem.cs
using MythHunter.Core.ECS;
using System.Collections.Generic;
using UnityEngine;
namespace MythHunter.Systems.Movement
{
    /// <summary>
    /// Інтерфейс системи руху
    /// </summary>
    public interface IMovementSystem : ISystem
    {
        void PlanPath(int entityId, List<Vector3> waypoints);
        void StartMovement(int entityId);
        void StopMovement(int entityId, bool forceStop = false);
        bool IsEntityMoving(int entityId);
        float GetRemainingDistance(int entityId);
        float GetRemainingMovementPoints(int entityId);
        Vector3 GetCurrentPosition(int entityId);
        Vector3 GetTargetPosition(int entityId);
    }
}
