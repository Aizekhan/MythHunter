// Assets/_MythHunter/Code/Systems/Movement/IPathfindingSystem.cs
using MythHunter.Core.ECS;
using System.Collections.Generic;
using UnityEngine;
namespace MythHunter.Systems.Movement
{
    /// <summary>
    /// Інтерфейс системи пошуку шляху
    /// </summary>
    public interface IPathfindingSystem : ISystem
    {
        List<Vector3> FindPath(Vector3 start, Vector3 end, bool avoidObstacles = true);
        bool IsPathValid(List<Vector3> path);
        float CalculatePathLength(List<Vector3> path);
        bool IsPointReachable(Vector3 point, float movementPoints);
    }
}
