using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using System.Collections.Generic;
using UnityEngine;
// Assets/_MythHunter/Code/Systems/Movement/PathfindingSystem.cs
namespace MythHunter.Systems.Movement
{
    /// <summary>
    /// Система пошуку шляху
    /// </summary>
    public class PathfindingSystem : SystemBase, IPathfindingSystem
    {
        private readonly IEntityManager _entityManager;

        [Inject]
        public PathfindingSystem(
            IEntityManager entityManager,
            IEventBus eventBus,
            IMythLogger logger)
            : base(logger, eventBus)
        {
            _entityManager = entityManager;
        }

        public override void Initialize()
        {
            base.Initialize();
            _logger.LogInfo("PathfindingSystem initialized", "Pathfinding");
        }

        public List<Vector3> FindPath(Vector3 start, Vector3 end, bool avoidObstacles = true)
        {
            // Спрощена реалізація пошуку шляху - пряма лінія
            // В майбутньому може бути замінена на A* або інший алгоритм пошуку шляху
            var path = new List<Vector3> { end };

            _logger.LogDebug($"Path found from {start} to {end}, path length: {path.Count}", "Pathfinding");

            return path;
        }

        public bool IsPathValid(List<Vector3> path)
        {
            // Проста перевірка валідності шляху
            // В майбутньому може включати перевірку на перешкоди, колізії тощо
            return path != null && path.Count > 0;
        }

        public float CalculatePathLength(List<Vector3> path)
        {
            if (path == null || path.Count < 2)
                return 0;

            float length = 0;

            for (int i = 0; i < path.Count - 1; i++)
            {
                length += Vector3.Distance(path[i], path[i + 1]);
            }

            return length;
        }

        public bool IsPointReachable(Vector3 point, float movementPoints)
        {
            // Проста перевірка досяжності точки
            // В майбутньому може включати перевірку на перешкоди, витривалість тощо
            return movementPoints > 0;
        }
    }
}
