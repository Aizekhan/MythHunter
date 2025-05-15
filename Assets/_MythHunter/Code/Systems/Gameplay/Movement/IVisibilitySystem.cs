// Assets/_MythHunter/Code/Systems/Movement/IVisibilitySystem.cs
using MythHunter.Core.ECS;
using System.Collections.Generic;
using UnityEngine;
namespace MythHunter.Systems.Movement
{
    /// <summary>
    /// Інтерфейс системи видимості
    /// </summary>
    public interface IVisibilitySystem : ISystem
    {
        void SetLookDirection(int entityId, Vector3 direction);
        bool IsEntityVisible(int observerId, int targetId);
        bool IsPointInSight(int entityId, Vector3 point);
        List<int> GetVisibleEntities(int entityId);
        float GetVisibilityRadius(int entityId);
    }
}
