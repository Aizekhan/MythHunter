// Assets/_MythHunter/Code/Systems/Movement/MovementSystem.cs
using MythHunter.Components.Movement;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events.Domain;
using MythHunter.Events;
using MythHunter.Systems.Core;
using MythHunter.Systems.Phase;
using MythHunter.Utils.Logging;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
namespace MythHunter.Systems.Movement
{
    /// <summary>
    /// Система руху
    /// </summary>
    public class MovementSystem : SystemBase, IMovementSystem, IPhaseFilteredSystem
    {
        private readonly IEntityManager _entityManager;
        private readonly IPhaseSystem _phaseSystem;
        private readonly IPathfindingSystem _pathfindingSystem;
        private readonly ComponentCache<PositionComponent> _positionCache;
        private readonly ComponentCache<MovementComponent> _movementCache;
        private readonly ComponentCache<PathComponent> _pathCache;
        private string[] _activePhaseIds;

        [Inject]
        public MovementSystem(
            IEntityManager entityManager,
            IEventBus eventBus,
            IMythLogger logger,
            IPhaseSystem phaseSystem,
            IPathfindingSystem pathfindingSystem)
            : base(logger, eventBus)
        {
            _entityManager = entityManager;
            _phaseSystem = phaseSystem;
            _pathfindingSystem = pathfindingSystem;

            _positionCache = new ComponentCache<PositionComponent>(_entityManager);
            _movementCache = new ComponentCache<MovementComponent>(_entityManager);
            _pathCache = new ComponentCache<PathComponent>(_entityManager);

            // Активно тільки у фазі руху (Active phase)
            SetActivePhases(new[] { GamePhase.Active });
        }

        public override void Initialize()
        {
            base.Initialize();
            _positionCache.Update();
            _movementCache.Update();
            _pathCache.Update();
        }

        public override void Update(float deltaTime)
        {
            if (!IsActiveInPhase(_phaseSystem.CurrentPhase))
                return;

            _positionCache.Update();
            _movementCache.Update();
            _pathCache.Update();

            // Проходимо по всіх сутностях з компонентами шляху і руху
            foreach (var entityId in _pathCache.GetEntityIdsArray())
            {
                if (!_movementCache.Contains(entityId) || !_positionCache.Contains(entityId))
                    continue;

                var path = _pathCache.Get(entityId);
                var movement = _movementCache.Get(entityId);
                var position = _positionCache.Get(entityId);

                // Пропускаємо нерухомі сутності
                if (!movement.IsMoving || path.IsPathComplete)
                    continue;

                // Обробляємо рух
                ProcessEntityMovement(entityId, deltaTime, ref path, ref movement, ref position);

                // Оновлюємо компоненти
                _pathCache.Add(entityId, path);
                _movementCache.Add(entityId, movement);
                _positionCache.Add(entityId, position);
            }
        }

        private void ProcessEntityMovement(int entityId, float deltaTime, ref PathComponent path,
            ref MovementComponent movement, ref PositionComponent position)
        {
            // Основна логіка руху сутності за шляхом
            if (path.CurrentWaypointIndex >= path.Waypoints.Count)
            {
                CompleteMovement(entityId, ref path, ref movement);
                return;
            }

            // Отримуємо поточну цільову точку
            Vector3 targetPoint = path.Waypoints[path.CurrentWaypointIndex];

            // Розраховуємо відстань до цільової точки
            Vector3 direction = targetPoint - position.Position;
            float distance = direction.magnitude;

            // Якщо досягли точки, переходимо до наступної
            if (distance < 0.1f)
            {
                path.CurrentWaypointIndex++;

                // Якщо це була остання точка, завершуємо рух
                if (path.CurrentWaypointIndex >= path.Waypoints.Count)
                {
                    CompleteMovement(entityId, ref path, ref movement);
                    return;
                }

                // Оновлюємо цільову точку
                targetPoint = path.Waypoints[path.CurrentWaypointIndex];
                direction = targetPoint - position.Position;
                distance = direction.magnitude;
            }

            // Нормалізуємо напрямок
            if (distance > 0)
                direction /= distance;

            // Розраховуємо швидкість руху з врахуванням витривалості
            float speed = movement.MoveSpeed;
            float moveAmount = speed * deltaTime;

            // Перевіряємо витривалість
            if (movement.MovementPoints <= 0)
            {
                StopMovement(entityId, true);
                return;
            }

            // Якщо відстань менша за можливе пересування, просто переміщуємося в точку
            if (distance <= moveAmount)
            {
                position.PreviousPosition = position.Position;
                position.Position = targetPoint;

                // Віднімаємо витрачену витривалість
                movement.MovementPoints -= distance;

                // Публікуємо подію оновлення позиції
                Publish(new PositionUpdatedEvent
                {
                    EntityId = entityId,
                    Position = position.Position,
                    Rotation = position.Rotation,
                    Timestamp = DateTime.UtcNow
                });

                return;
            }

            // Зберігаємо попередню позицію
            position.PreviousPosition = position.Position;

            // Розраховуємо нову позицію
            Vector3 newPosition = position.Position + direction * moveAmount;

            // Оновлюємо позицію
            position.Position = newPosition;

            // Оновлюємо витривалість
            movement.MovementPoints -= moveAmount;

            // Оновлюємо напрямок руху
            movement.Direction = direction;

            // Розраховуємо обертання
            if (direction != Vector3.zero)
            {
                // Плавний поворот до напрямку руху
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                position.Rotation = Quaternion.Slerp(
                    position.Rotation,
                    targetRotation,
                    movement.RotationSpeed * deltaTime
                );
            }

            // Публікуємо подію оновлення позиції
            Publish(new PositionUpdatedEvent
            {
                EntityId = entityId,
                Position = position.Position,
                Rotation = position.Rotation,
                Timestamp = DateTime.UtcNow
            });

            // Збільшуємо час на шляху
            path.TimeOnPath += deltaTime;
        }

        private void CompleteMovement(int entityId, ref PathComponent path, ref MovementComponent movement)
        {
            path.IsPathComplete = true;
            movement.IsMoving = false;

            // Публікуємо подію завершення руху
            Publish(new MovementStoppedEvent
            {
                EntityId = entityId,
                Position = _positionCache.Get(entityId).Position,
                PathCompleted = true,
                Timestamp = DateTime.UtcNow
            });
        }

        // Реалізація інтерфейсу IMovementSystem
        public void PlanPath(int entityId, List<Vector3> waypoints)
        {
            // Перевіряємо чи має сутність необхідні компоненти
            if (!_entityManager.HasComponent<PositionComponent>(entityId) ||
                !_entityManager.HasComponent<MovementComponent>(entityId))
            {
                _logger.LogWarning($"Entity {entityId} lacks required components for movement", "Movement");
                return;
            }

            // Перевіряємо валідність шляху
            if (!_pathfindingSystem.IsPathValid(waypoints))
            {
                _logger.LogWarning($"Invalid path for entity {entityId}", "Movement");
                return;
            }

            // Створюємо компонент шляху, якщо він відсутній
            if (!_entityManager.HasComponent<PathComponent>(entityId))
            {
                var pathComponent = new PathComponent
                {
                    Waypoints = waypoints,
                    CurrentWaypointIndex = 0,
                    IsPathComplete = false,
                    TimeOnPath = 0
                };

                _entityManager.AddComponent(entityId, pathComponent);
            }
            else
            {
                // Оновлюємо існуючий компонент шляху
                var pathComponent = _entityManager.GetComponent<PathComponent>(entityId);
                pathComponent.Waypoints = waypoints;
                pathComponent.CurrentWaypointIndex = 0;
                pathComponent.IsPathComplete = false;
                pathComponent.TimeOnPath = 0;

                _entityManager.AddComponent(entityId, pathComponent);
            }

            // Публікуємо подію планування шляху
            Publish(new PathPlannedEvent
            {
                EntityId = entityId,
                Waypoints = waypoints,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInfo($"Path planned for entity {entityId} with {waypoints.Count} waypoints", "Movement");
        }

        public void StartMovement(int entityId)
        {
            if (!_entityManager.HasComponent<MovementComponent>(entityId) ||
                !_entityManager.HasComponent<PathComponent>(entityId) ||
                !_entityManager.HasComponent<PositionComponent>(entityId))
            {
                _logger.LogWarning($"Entity {entityId} lacks required components for movement", "Movement");
                return;
            }

            var movement = _entityManager.GetComponent<MovementComponent>(entityId);
            var path = _entityManager.GetComponent<PathComponent>(entityId);

            // Перевіряємо чи є шлях і витривалість
            if (path.Waypoints.Count == 0 || movement.MovementPoints <= 0)
            {
                _logger.LogWarning($"Cannot start movement for entity {entityId}: No path or no movement points", "Movement");
                return;
            }

            // Активуємо рух
            movement.IsMoving = true;
            path.IsPathComplete = false;

            _entityManager.AddComponent(entityId, movement);
            _entityManager.AddComponent(entityId, path);

            // Публікуємо подію початку руху
            Publish(new MovementStartedEvent
            {
                EntityId = entityId,
                StartPosition = _entityManager.GetComponent<PositionComponent>(entityId).Position,
                TargetPosition = path.Waypoints[path.CurrentWaypointIndex],
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInfo($"Movement started for entity {entityId}", "Movement");
        }

        public void StopMovement(int entityId, bool forceStop = false)
        {
            if (!_entityManager.HasComponent<MovementComponent>(entityId))
                return;

            var movement = _entityManager.GetComponent<MovementComponent>(entityId);

            // Якщо сутність не рухається, нічого не робимо
            if (!movement.IsMoving && !forceStop)
                return;

            movement.IsMoving = false;

            _entityManager.AddComponent(entityId, movement);

            // Якщо є шлях, позначаємо його як завершений
            if (_entityManager.HasComponent<PathComponent>(entityId))
            {
                var path = _entityManager.GetComponent<PathComponent>(entityId);
                path.IsPathComplete = true;
                _entityManager.AddComponent(entityId, path);
            }

            // Публікуємо подію зупинки руху
            Publish(new MovementStoppedEvent
            {
                EntityId = entityId,
                Position = _entityManager.HasComponent<PositionComponent>(entityId) ?
                    _entityManager.GetComponent<PositionComponent>(entityId).Position : Vector3.zero,
                PathCompleted = false,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInfo($"Movement stopped for entity {entityId}", "Movement");
        }

        public bool IsEntityMoving(int entityId)
        {
            if (!_entityManager.HasComponent<MovementComponent>(entityId))
                return false;

            return _entityManager.GetComponent<MovementComponent>(entityId).IsMoving;
        }

        public float GetRemainingDistance(int entityId)
        {
            if (!_entityManager.HasComponent<PathComponent>(entityId) ||
                !_entityManager.HasComponent<PositionComponent>(entityId))
                return 0;

            var path = _entityManager.GetComponent<PathComponent>(entityId);
            var position = _entityManager.GetComponent<PositionComponent>(entityId);

            if (path.CurrentWaypointIndex >= path.Waypoints.Count)
                return 0;

            float distance = 0;

            // Відстань до поточної точки
            distance += Vector3.Distance(position.Position, path.Waypoints[path.CurrentWaypointIndex]);

            // Додаємо відстані між рештою точок
            for (int i = path.CurrentWaypointIndex; i < path.Waypoints.Count - 1; i++)
            {
                distance += Vector3.Distance(path.Waypoints[i], path.Waypoints[i + 1]);
            }

            return distance;
        }

        public float GetRemainingMovementPoints(int entityId)
        {
            if (!_entityManager.HasComponent<MovementComponent>(entityId))
                return 0;

            return _entityManager.GetComponent<MovementComponent>(entityId).MovementPoints;
        }

        public Vector3 GetCurrentPosition(int entityId)
        {
            if (!_entityManager.HasComponent<PositionComponent>(entityId))
                return Vector3.zero;

            return _entityManager.GetComponent<PositionComponent>(entityId).Position;
        }

        public Vector3 GetTargetPosition(int entityId)
        {
            if (!_entityManager.HasComponent<PathComponent>(entityId))
                return Vector3.zero;

            var path = _entityManager.GetComponent<PathComponent>(entityId);

            if (path.Waypoints.Count == 0 || path.CurrentWaypointIndex >= path.Waypoints.Count)
                return Vector3.zero;

            return path.Waypoints[path.CurrentWaypointIndex];
        }

        // Реалізація інтерфейсу IPhaseFilteredSystem
        public void SetActivePhaseIds(string[] phaseIds)
        {
            _activePhaseIds = phaseIds;
        }

        public void SetActivePhases(GamePhase[] phases)
        {
            _activePhaseIds = phases.Select(p => p.ToString()).ToArray();
        }

        public bool IsActiveInPhase(GamePhase phase)
        {
            return _activePhaseIds?.Contains(phase.ToString()) ?? false;
        }

        protected override void OnSubscribeToEvents()
        {
            Subscribe<PhaseChangedEvent>(OnPhaseChanged);
        }

        protected override void OnUnsubscribeFromEvents()
        {
            Unsubscribe<PhaseChangedEvent>(OnPhaseChanged);
        }

        private void OnPhaseChanged(PhaseChangedEvent evt)
        {
            // Якщо фаза змінилася на Movement, запускаємо рух для всіх сутностей
            if (evt.CurrentPhase == GamePhase.Active)
            {
                StartAllPlannedMovements();
            }
            // Якщо фаза змінилася з Movement, зупиняємо всі рухи
            else if (evt.PreviousPhase == GamePhase.Active)
            {
                StopAllMovements();
            }
        }

        private void StartAllPlannedMovements()
        {
            // Запускаємо рух для всіх сутностей з компонентом шляху
            foreach (var entityId in _entityManager.GetEntitiesWith<PathComponent>())
            {
                var path = _entityManager.GetComponent<PathComponent>(entityId);

                // Пропускаємо сутності з порожнім шляхом
                if (path.Waypoints.Count == 0 || path.IsPathComplete)
                    continue;

                StartMovement(entityId);
            }
        }

        private void StopAllMovements()
        {
            // Зупиняємо рух для всіх сутностей з компонентом руху
            foreach (var entityId in _entityManager.GetEntitiesWith<MovementComponent>())
            {
                var movement = _entityManager.GetComponent<MovementComponent>(entityId);

                if (movement.IsMoving)
                {
                    StopMovement(entityId);
                }
            }
        }
    }
}


