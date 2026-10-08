// Assets/_MythHunter/Code/Systems/Movement/VisibilitySystem.cs (продовження)
using MythHunter.Components.Movement;
using MythHunter.Core.ECS;
using MythHunter.Events.Domain;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Utils.Logging;
namespace MythHunter.Systems.Movement
{
    /// <summary>
    /// Система видимості
    /// </summary>
    public class VisibilitySystem : SystemBase, IVisibilitySystem
    {
        private readonly IEntityManager _entityManager;
        private readonly ComponentCache<PositionComponent> _positionCache;
        private readonly ComponentCache<VisibilityComponent> _visibilityCache;

        // Кеш видимих сутностей для оптимізації
        private readonly Dictionary<int, HashSet<int>> _visibleEntities = new Dictionary<int, HashSet<int>>();

        [Inject]
        public VisibilitySystem(
            IEntityManager entityManager,
            IEventBus eventBus,
            IMythLogger logger)
            : base(logger, eventBus)
        {
            _entityManager = entityManager;
            _positionCache = new ComponentCache<PositionComponent>(_entityManager);
            _visibilityCache = new ComponentCache<VisibilityComponent>(_entityManager);
        }

        public override void Initialize()
        {
            base.Initialize();
            _positionCache.Update();
            _visibilityCache.Update();
            _logger.LogInfo("VisibilitySystem initialized", "Visibility");
        }

        public override void Update(float deltaTime)
        {
            _positionCache.Update();
            _visibilityCache.Update();

            // Оновлюємо видимість для всіх сутностей з компонентами видимості
            foreach (var observerId in _visibilityCache.GetEntityIdsArray())
            {
                if (!_positionCache.Contains(observerId))
                    continue;

                var observerPosition = _positionCache.Get(observerId).Position;
                var observerVisibility = _visibilityCache.Get(observerId);

                // Створюємо або очищаємо кеш видимих сутностей
                if (!_visibleEntities.ContainsKey(observerId))
                {
                    _visibleEntities[observerId] = new HashSet<int>();
                }
                else
                {
                    _visibleEntities[observerId].Clear();
                }

                // Перевіряємо всі сутності з позицією на видимість
                foreach (var targetId in _positionCache.GetEntityIdsArray())
                {
                    // Пропускаємо спостерігача
                    if (targetId == observerId)
                        continue;

                    // Отримуємо позицію цілі
                    var targetPosition = _positionCache.Get(targetId).Position;

                    // Перевіряємо чи ціль видима
                    if (IsInFieldOfView(observerPosition, observerVisibility.LookDirection,
                        observerVisibility.VisionRadius, observerVisibility.VisionAngle, targetPosition))
                    {
                        // Додаємо цільову сутність до видимих
                        _visibleEntities[observerId].Add(targetId);

                        // Перевіряємо чи сутність була вже видима
                        bool wasVisible = _visibleEntities.ContainsKey(observerId) &&
                                         _visibleEntities[observerId].Contains(targetId);

                        // Якщо це нова видима сутність, публікуємо подію
                        if (!wasVisible)
                        {
                            Publish(new EntityDetectedEvent
                            {
                                ObserverEntityId = observerId,
                                DetectedEntityId = targetId,
                                Timestamp = DateTime.UtcNow
                            });
                        }
                    }
                    else
                    {
                        // Перевіряємо чи сутність була раніше видима
                        bool wasVisible = _visibleEntities.ContainsKey(observerId) &&
                                         _visibleEntities[observerId].Contains(targetId);

                        // Якщо сутність була видима, а тепер ні, публікуємо подію
                        if (wasVisible)
                        {
                            Publish(new EntityLostEvent
                            {
                                ObserverEntityId = observerId,
                                LostEntityId = targetId,
                                Timestamp = DateTime.UtcNow
                            });
                        }
                    }
                }
            }
        }

        private bool IsInFieldOfView(Vector3 observerPosition, Vector3 observerDirection,
            float radius, float angleInDegrees, Vector3 targetPosition)
        {
            // Отримуємо вектор до цілі
            Vector3 directionToTarget = targetPosition - observerPosition;

            // Перевіряємо відстань
            if (directionToTarget.magnitude > radius)
                return false;

            // Якщо кут зору 360 градусів, ціль завжди видима в межах радіуса
            if (angleInDegrees >= 360f)
                return true;

            // Нормалізуємо напрямки
            directionToTarget.Normalize();
            observerDirection.Normalize();

            // Кут між напрямком спостерігача і напрямком до цілі
            float angle = Vector3.Angle(observerDirection, directionToTarget);

            // Перевіряємо чи ціль в межах кута зору
            return angle <= angleInDegrees * 0.5f;
        }

        public void SetLookDirection(int entityId, Vector3 direction)
        {
            if (!_entityManager.HasComponent<VisibilityComponent>(entityId))
            {
                _logger.LogWarning($"Entity {entityId} doesn't have VisibilityComponent", "Visibility");
                return;
            }

            var visibility = _entityManager.GetComponent<VisibilityComponent>(entityId);

            // Зберігаємо попередній напрямок
            Vector3 previousDirection = visibility.LookDirection;

            // Оновлюємо напрямок
            visibility.LookDirection = direction.normalized;
            _entityManager.AddComponent(entityId, visibility);

            // Публікуємо подію зміни напрямку погляду
            Publish(new LookDirectionChangedEvent
            {
                EntityId = entityId,
                Direction = visibility.LookDirection,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogDebug($"Entity {entityId} look direction changed from {previousDirection} to {visibility.LookDirection}", "Visibility");
        }

        public bool IsEntityVisible(int observerId, int targetId)
        {
            // Перевіряємо чи ціль знаходиться в кеші видимих сутностей
            return _visibleEntities.ContainsKey(observerId) &&
                  _visibleEntities[observerId].Contains(targetId);
        }

        public bool IsPointInSight(int entityId, Vector3 point)
        {
            if (!_entityManager.HasComponent<VisibilityComponent>(entityId) ||
                !_entityManager.HasComponent<PositionComponent>(entityId))
            {
                return false;
            }

            var visibility = _entityManager.GetComponent<VisibilityComponent>(entityId);
            var position = _entityManager.GetComponent<PositionComponent>(entityId);

            return IsInFieldOfView(
                position.Position,
                visibility.LookDirection,
                visibility.VisionRadius,
                visibility.VisionAngle,
                point
            );
        }

        public List<int> GetVisibleEntities(int entityId)
        {
            if (!_visibleEntities.ContainsKey(entityId))
                return new List<int>();

            return _visibleEntities[entityId].ToList();
        }

        public float GetVisibilityRadius(int entityId)
        {
            if (!_entityManager.HasComponent<VisibilityComponent>(entityId))
                return 0f;

            return _entityManager.GetComponent<VisibilityComponent>(entityId).VisionRadius;
        }
    }
}
