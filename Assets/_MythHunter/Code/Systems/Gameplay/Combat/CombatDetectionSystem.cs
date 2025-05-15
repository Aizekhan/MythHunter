// Шлях: Assets/_MythHunter/Code/Systems/Combat/CombatDetectionSystem.cs
using System;
using System.Collections.Generic;
using MythHunter.Components.Combat;
using MythHunter.Components.Core;
using MythHunter.Components.Movement;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Systems.Combat
{
    /// <summary>
    /// Система виявлення бойових ситуацій
    /// </summary>
    public class CombatDetectionSystem : SystemBase, ICombatDetectionSystem, IEventSubscriber
    {
        private readonly IEntityManager _entityManager;

        // Відстань, на якій можна починати бій
        private const float COMBAT_DETECTION_DISTANCE = 1.5f;

        [Inject]
        public CombatDetectionSystem(
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
            _logger.LogInfo("CombatDetectionSystem initialized", "Combat");
        }

        protected override void OnSubscribeToEvents()
        {
            Subscribe<EntityDetectedEvent>(OnEntityDetected);
        }

        protected override void OnUnsubscribeFromEvents()
        {
            Unsubscribe<EntityDetectedEvent>(OnEntityDetected);
        }

        private void OnEntityDetected(EntityDetectedEvent evt)
        {
            // Коли сутність виявляє іншу, перевіряємо можливість бою
            if (CanInitiateCombat(evt.ObserverEntityId, evt.DetectedEntityId))
            {
                // Публікуємо подію з запитом на початок бою
                Publish(new CombatStartRequestEvent
                {
                    AttackerEntityId = evt.ObserverEntityId,
                    DefenderEntityId = evt.DetectedEntityId,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        public override void Update(float deltaTime)
        {
            // У фазі руху постійно скануємо на предмет нових бойових ситуацій
            CheckForNewCombatSituations();
        }

        private void CheckForNewCombatSituations()
        {
            // Отримуємо всі сутності з компонентами позиції та бойової статистики
            int[] entities = _entityManager.GetEntitiesWith<PositionComponent>();

            foreach (int entityId in entities)
            {
                if (!_entityManager.HasComponent<CombatStatsComponent>(entityId) ||
                    _entityManager.GetComponent<CombatStatsComponent>(entityId).IsInCombat)
                {
                    continue; // Пропускаємо, якщо вже в бою
                }

                // Отримуємо найближчого ворога
                int nearestEnemyId = GetNearestEnemy(entityId);

                if (nearestEnemyId != -1 && CanInitiateCombat(entityId, nearestEnemyId))
                {
                    // Публікуємо подію з запитом на початок бою
                    Publish(new CombatStartRequestEvent
                    {
                        AttackerEntityId = entityId,
                        DefenderEntityId = nearestEnemyId,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
        }

        public bool CanInitiateCombat(int attackerEntityId, int defenderEntityId)
        {
            // Перевіряємо, чи є обидві сутності
            if (!_entityManager.HasComponent<PositionComponent>(attackerEntityId) ||
                !_entityManager.HasComponent<PositionComponent>(defenderEntityId))
            {
                return false;
            }

            // Перевіряємо, чи є необхідні компоненти для бою
            if (!_entityManager.HasComponent<CombatStatsComponent>(attackerEntityId) ||
                !_entityManager.HasComponent<CombatStatsComponent>(defenderEntityId))
            {
                return false;
            }

            // Перевіряємо, чи живі обидві сутності
            if (_entityManager.HasComponent<HealthComponent>(attackerEntityId) &&
                _entityManager.GetComponent<HealthComponent>(attackerEntityId).IsDead)
            {
                return false;
            }

            if (_entityManager.HasComponent<HealthComponent>(defenderEntityId) &&
                _entityManager.GetComponent<HealthComponent>(defenderEntityId).IsDead)
            {
                return false;
            }

            // Перевіряємо, чи є сутності ворогами
            if (!AreEntitiesEnemies(attackerEntityId, defenderEntityId))
            {
                return false;
            }

            // Перевіряємо відстань між сутностями
            PositionComponent attackerPos = _entityManager.GetComponent<PositionComponent>(attackerEntityId);
            PositionComponent defenderPos = _entityManager.GetComponent<PositionComponent>(defenderEntityId);

            float distance = Vector3.Distance(attackerPos.Position, defenderPos.Position);

            if (distance > COMBAT_DETECTION_DISTANCE)
            {
                return false;
            }

            // Перевіряємо, чи бачить атакуючий захисника
            // (тут потрібна система видимості, ми використовуємо спрощену версію)
            bool canSeeDefender = IsEntityInSight(attackerEntityId, defenderEntityId);

            return canSeeDefender;
        }

        public int[] GetEnemiesInSight(int entityId)
        {
            List<int> enemiesInSight = new List<int>();

            if (!_entityManager.HasComponent<PositionComponent>(entityId))
            {
                return Array.Empty<int>();
            }

            PositionComponent entityPos = _entityManager.GetComponent<PositionComponent>(entityId);

            // Отримуємо всі сутності з компонентом позиції
            int[] potentialEnemies = _entityManager.GetEntitiesWith<PositionComponent>();

            foreach (int potentialEnemyId in potentialEnemies)
            {
                if (potentialEnemyId == entityId || !AreEntitiesEnemies(entityId, potentialEnemyId))
                {
                    continue;
                }

                if (_entityManager.HasComponent<HealthComponent>(potentialEnemyId) &&
                    _entityManager.GetComponent<HealthComponent>(potentialEnemyId).IsDead)
                {
                    continue; // Пропускаємо мертвих
                }

                PositionComponent enemyPos = _entityManager.GetComponent<PositionComponent>(potentialEnemyId);

                float distance = Vector3.Distance(entityPos.Position, enemyPos.Position);

                if (distance <= COMBAT_DETECTION_DISTANCE && IsEntityInSight(entityId, potentialEnemyId))
                {
                    enemiesInSight.Add(potentialEnemyId);
                }
            }

            return enemiesInSight.ToArray();
        }

        public int GetNearestEnemy(int entityId)
        {
            if (!_entityManager.HasComponent<PositionComponent>(entityId))
            {
                return -1;
            }

            PositionComponent entityPos = _entityManager.GetComponent<PositionComponent>(entityId);
            float minDistance = float.MaxValue;
            int nearestEnemyId = -1;

            // Отримуємо всі сутності з компонентом позиції
            int[] potentialEnemies = _entityManager.GetEntitiesWith<PositionComponent>();

            foreach (int potentialEnemyId in potentialEnemies)
            {
                if (potentialEnemyId == entityId || !AreEntitiesEnemies(entityId, potentialEnemyId))
                {
                    continue;
                }

                if (_entityManager.HasComponent<HealthComponent>(potentialEnemyId) &&
                    _entityManager.GetComponent<HealthComponent>(potentialEnemyId).IsDead)
                {
                    continue; // Пропускаємо мертвих
                }

                PositionComponent enemyPos = _entityManager.GetComponent<PositionComponent>(potentialEnemyId);

                float distance = Vector3.Distance(entityPos.Position, enemyPos.Position);

                if (distance < minDistance && IsEntityInSight(entityId, potentialEnemyId))
                {
                    minDistance = distance;
                    nearestEnemyId = potentialEnemyId;
                }
            }

            return nearestEnemyId;
        }

        public bool AreEntitiesEnemies(int entityId1, int entityId2)
        {
            // Перевіряємо, чи належать сутності до різних команд
            if (!_entityManager.HasComponent<TeamComponent>(entityId1) ||
                !_entityManager.HasComponent<TeamComponent>(entityId2))
            {
                return false; // Якщо немає компоненту команди, не вважаємо ворогами
            }

            TeamComponent team1 = _entityManager.GetComponent<TeamComponent>(entityId1);
            TeamComponent team2 = _entityManager.GetComponent<TeamComponent>(entityId2);

            // Нейтральні сутності не є ворогами
            if (team1.IsNeutral || team2.IsNeutral)
            {
                return false;
            }

            // Різні команди = вороги
            return team1.TeamId != team2.TeamId;
        }

        private bool IsEntityInSight(int observerEntityId, int targetEntityId)
        {
            // Спрощена перевірка видимості (у реальній системі потрібно враховувати кут огляду та перешкоди)
            if (!_entityManager.HasComponent<PositionComponent>(observerEntityId) ||
                !_entityManager.HasComponent<PositionComponent>(targetEntityId))
            {
                return false;
            }

            PositionComponent observerPos = _entityManager.GetComponent<PositionComponent>(observerEntityId);
            PositionComponent targetPos = _entityManager.GetComponent<PositionComponent>(targetEntityId);

            float distance = Vector3.Distance(observerPos.Position, targetPos.Position);

            // TODO: Додати перевірку кута огляду та перешкод для більш реалістичної системи

            // Поки що просто перевіряємо відстань
            return distance <= COMBAT_DETECTION_DISTANCE;
        }
    }
}
