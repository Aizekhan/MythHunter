// Шлях: Assets/_MythHunter/Code/Systems/Combat/CombatAbilitySystem.cs
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Components.Combat;
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
    /// Система бойових здібностей
    /// </summary>
    public class CombatAbilitySystem : SystemBase, ICombatAbilitySystem, IEventSubscriber
    {
        private readonly IEntityManager _entityManager;

        // Словник для зберігання реалізацій активних здібностей
        private readonly Dictionary<string, Func<int, int, bool>> _activeAbilityHandlers = new Dictionary<string, Func<int, int, bool>>();

        // Словник для зберігання реалізацій пасивних здібностей
        private readonly Dictionary<string, Action<int>> _passiveAbilityHandlers = new Dictionary<string, Action<int>>();

        [Inject]
        public CombatAbilitySystem(
            IEntityManager entityManager,
            IEventBus eventBus,
            IMythLogger logger)
            : base(logger, eventBus)
        {
            _entityManager = entityManager;

            // Реєструємо стандартні здібності
            RegisterDefaultAbilities();
        }

        public override void Initialize()
        {
            base.Initialize();
            _logger.LogInfo("CombatAbilitySystem initialized", "Combat");
        }

        protected override void OnSubscribeToEvents()
        {
            Subscribe<CombatStartedEvent>(OnCombatStarted);
            Subscribe<CombatEndedEvent>(OnCombatEnded);
        }

        protected override void OnUnsubscribeFromEvents()
        {
            Unsubscribe<CombatStartedEvent>(OnCombatStarted);
            Unsubscribe<CombatEndedEvent>(OnCombatEnded);
        }

        private void OnCombatStarted(CombatStartedEvent evt)
        {
            // При початку бою оновлюємо пасивні ефекти для учасників
            foreach (int entityId in evt.ParticipantEntityIds)
            {
                UpdatePassiveEffects(entityId);
            }
        }

        private void OnCombatEnded(CombatEndedEvent evt)
        {
            // При завершенні бою скидаємо стан використання активних здібностей
            foreach (int entityId in evt.ParticipantEntityIds)
            {
                ResetActiveAbilityUse(entityId);
            }
        }

        public override void Update(float deltaTime)
        {
            // Оновлюємо пасивні ефекти для всіх сутностей
            UpdateAllPassiveEffects(deltaTime);

            // Перевіряємо автоматичне використання активних здібностей
            CheckAutoAbilityUsage();
        }

        private void RegisterDefaultAbilities()
        {
            // Реєстрація активних здібностей
            RegisterActiveAbility("shield_block", OnShieldBlockAbility);
            RegisterActiveAbility("backstab", OnBackstabAbility);
            RegisterActiveAbility("rage_burst", OnRageBurstAbility);
            RegisterActiveAbility("teleport", OnTeleportAbility);

            // Реєстрація пасивних здібностей
            RegisterPassiveAbility("poison_attack", OnPoisonAttackPassive);
            RegisterPassiveAbility("counter_attack", OnCounterAttackPassive);
            RegisterPassiveAbility("health_regen", OnHealthRegenPassive);
            RegisterPassiveAbility("combat_master", OnCombatMasterPassive);
        }

        public void RegisterActiveAbility(string abilityId, Func<int, int, bool> handler)
        {
            _activeAbilityHandlers[abilityId] = handler;
        }

        public void RegisterPassiveAbility(string abilityId, Action<int> handler)
        {
            _passiveAbilityHandlers[abilityId] = handler;
        }

        public bool UseActiveAbility(int entityId, int targetEntityId = -1)
        {
            if (!_entityManager.HasComponent<CombatAbilityComponent>(entityId))
            {
                _logger.LogWarning($"Cannot use active ability for entity {entityId}: it has no CombatAbilityComponent", "Combat");
                return false;
            }

            CombatAbilityComponent abilityComponent = _entityManager.GetComponent<CombatAbilityComponent>(entityId);

            // Перевіряємо, чи не використана вже активна здібність
            if (abilityComponent.IsActiveAbilityUsed)
            {
                _logger.LogWarning($"Entity {entityId} already used active ability in this combat", "Combat");
                return false;
            }

            string abilityId = abilityComponent.ActiveAbilityId;

            if (string.IsNullOrEmpty(abilityId))
            {
                _logger.LogWarning($"Entity {entityId} has no active ability", "Combat");
                return false;
            }

            // Шукаємо обробника для цієї здібності
            if (!_activeAbilityHandlers.TryGetValue(abilityId, out var handler))
            {
                _logger.LogWarning($"No handler found for active ability '{abilityId}'", "Combat");
                return false;
            }

            // Викликаємо обробника здібності
            bool success = handler(entityId, targetEntityId);

            if (success)
            {
                // Помічаємо, що здібність використана
                abilityComponent.IsActiveAbilityUsed = true;
                abilityComponent.LastActiveUseTime = Time.time;
                _entityManager.AddComponent(entityId, abilityComponent);

                // Публікуємо подію використання активної здібності
                Publish(new ActiveAbilityUsedEvent
                {
                    EntityId = entityId,
                    AbilityId = abilityId,
                    TargetEntityId = targetEntityId,
                    IsSuccess = true,
                    Timestamp = DateTime.UtcNow
                });

                _logger.LogInfo($"Entity {entityId} used active ability '{abilityId}' successfully", "Combat");
            }

            return success;
        }

        public bool CanUseActiveAbility(int entityId)
        {
            if (!_entityManager.HasComponent<CombatAbilityComponent>(entityId))
            {
                return false;
            }

            CombatAbilityComponent abilityComponent = _entityManager.GetComponent<CombatAbilityComponent>(entityId);

            // Перевіряємо, чи не використана вже активна здібність
            if (abilityComponent.IsActiveAbilityUsed)
            {
                return false;
            }

            // Перевіряємо наявність ідентифікатора здібності
            return !string.IsNullOrEmpty(abilityComponent.ActiveAbilityId);
        }

        public void ResetActiveAbilityUse(int entityId)
        {
            if (_entityManager.HasComponent<CombatAbilityComponent>(entityId))
            {
                CombatAbilityComponent abilityComponent = _entityManager.GetComponent<CombatAbilityComponent>(entityId);

                if (abilityComponent.IsActiveAbilityUsed)
                {
                    abilityComponent.IsActiveAbilityUsed = false;
                    _entityManager.AddComponent(entityId, abilityComponent);

                    _logger.LogInfo($"Reset active ability use for entity {entityId}", "Combat");
                }
            }
        }

        public string GetEntityActiveAbilityId(int entityId)
        {
            if (_entityManager.HasComponent<CombatAbilityComponent>(entityId))
            {
                return _entityManager.GetComponent<CombatAbilityComponent>(entityId).ActiveAbilityId;
            }

            return string.Empty;
        }

        public void UpdatePassiveEffects(int entityId)
        {
            if (!_entityManager.HasComponent<CombatAbilityComponent>(entityId))
            {
                return;
            }

            CombatAbilityComponent abilityComponent = _entityManager.GetComponent<CombatAbilityComponent>(entityId);

            if (abilityComponent.PassiveAbilityIds == null || abilityComponent.PassiveAbilityIds.Length == 0)
            {
                return;
            }

            // Застосовуємо всі пасивні ефекти
            foreach (string passiveId in abilityComponent.PassiveAbilityIds)
            {
                if (_passiveAbilityHandlers.TryGetValue(passiveId, out var handler))
                {
                    handler(entityId);
                }
            }
        }

        private void UpdateAllPassiveEffects(float deltaTime)
        {
            // Оновлюємо пасивні ефекти для всіх сутностей з компонентом здібностей
            int[] entities = _entityManager.GetEntitiesWith<CombatAbilityComponent>();

            foreach (int entityId in entities)
            {
                UpdatePassiveEffects(entityId);
            }
        }

        private void CheckAutoAbilityUsage()
        {
            // Перевіряємо автоматичне використання здібностей
            int[] entities = _entityManager.GetEntitiesWith<CombatAbilityComponent>();

            foreach (int entityId in entities)
            {
                // Перевіряємо, чи сутність знаходиться в бою
                if (!_entityManager.HasComponent<CombatStatsComponent>(entityId) ||
                    !_entityManager.GetComponent<CombatStatsComponent>(entityId).IsInCombat)
                {
                    continue;
                }

                CombatAbilityComponent abilityComponent = _entityManager.GetComponent<CombatAbilityComponent>(entityId);

                // Перевіряємо, чи вже використана активна здібність
                if (abilityComponent.IsActiveAbilityUsed || !abilityComponent.UseAutoActivation)
                {
                    continue;
                }

                // Перевіряємо умови для автоматичного використання
                if (_entityManager.HasComponent<HealthComponent>(entityId))
                {
                    HealthComponent healthComponent = _entityManager.GetComponent<HealthComponent>(entityId);

                    float healthPercentage = healthComponent.CurrentHealth / healthComponent.MaxHealth;

                    // Шлях: Assets/_MythHunter/Code/Systems/Combat/CombatAbilitySystem.cs (продовження)

                    // Якщо здоров'я нижче порогу, використовуємо здібність
                    if (healthPercentage <= abilityComponent.AutoActivationHealthThreshold)
                    {
                        int targetId = _entityManager.GetComponent<CombatStatsComponent>(entityId).CombatTargetEntityId;
                        UseActiveAbility(entityId, targetId);
                    }
                }
            }
        }

        #region Active Ability Handlers

        private bool OnShieldBlockAbility(int entityId, int targetEntityId)
        {
            // Реалізація здібності "Блок щитом" - повний блок атак на 2 секунди
            if (!_entityManager.HasComponent<HealthComponent>(entityId))
            {
                return false;
            }

            HealthComponent health = _entityManager.GetComponent<HealthComponent>(entityId);

            // Встановлюємо невразливість на 2 секунди
            health.IsInvulnerable = true;
            _entityManager.AddComponent(entityId, health);

            // Скасовуємо невразливість через 2 секунди
            UniTask.Delay(TimeSpan.FromSeconds(2)).ContinueWith(() =>
            {
                if (_entityManager.HasComponent<HealthComponent>(entityId))
                {
                    HealthComponent updatedHealth = _entityManager.GetComponent<HealthComponent>(entityId);
                    updatedHealth.IsInvulnerable = false;
                    _entityManager.AddComponent(entityId, updatedHealth);
                }
            }).Forget();

            return true;
        }

        private bool OnBackstabAbility(int entityId, int targetEntityId)
        {
            // Реалізація здібності "Удар в спину" - додатковий урон при атаці ззаду
            if (targetEntityId == -1 || !_entityManager.HasComponent<CombatStatsComponent>(entityId))
            {
                return false;
            }

            // Тут у повній реалізації перевіряємо, чи атакуємо ззаду

            // Наносимо подвійний урон
            float damage = _entityManager.GetComponent<CombatStatsComponent>(entityId).AttackPower * 2;

            // Публікуємо подію нанесення шкоди
            Publish(new DamageAppliedEvent
            {
                SourceEntityId = entityId,
                TargetEntityId = targetEntityId,
                DamageAmount = damage,
                DamageType = DamageType.Physical,
                IsCritical = true,
                IsBlocked = false,
                IsDodged = false,
                Timestamp = DateTime.UtcNow
            });

            return true;
        }

        private bool OnRageBurstAbility(int entityId, int targetEntityId)
        {
            // Реалізація здібності "Вибух люті" - миттєво наповнює шкалу люті
            if (!_entityManager.HasComponent<CombatStatsComponent>(entityId))
            {
                return false;
            }

            CombatStatsComponent stats = _entityManager.GetComponent<CombatStatsComponent>(entityId);
            float oldRage = stats.Rage;

            // Наповнюємо лють
            stats.Rage = stats.MaxRage;
            _entityManager.AddComponent(entityId, stats);

            // Публікуємо подію зміни люті
            Publish(new RageUpdatedEvent
            {
                EntityId = entityId,
                OldValue = oldRage,
                NewValue = stats.Rage,
                Timestamp = DateTime.UtcNow
            });

            return true;
        }

        private bool OnTeleportAbility(int entityId, int targetEntityId)
        {
            // Реалізація здібності "Телепорт" - переміщення на коротку відстань
            if (!_entityManager.HasComponent<PositionComponent>(entityId))
            {
                return false;
            }

            PositionComponent position = _entityManager.GetComponent<PositionComponent>(entityId);

            // Вибираємо напрямок телепорту
            Vector3 teleportDirection;

            if (targetEntityId != -1 && _entityManager.HasComponent<PositionComponent>(targetEntityId))
            {
                // Телепорт до цілі
                PositionComponent targetPosition = _entityManager.GetComponent<PositionComponent>(targetEntityId);
                teleportDirection = (targetPosition.Position - position.Position).normalized;
            }
            else
            {
                // Телепорт вперед
                teleportDirection = Vector3.forward; // Це потрібно замінити на актуальний напрямок персонажа
            }

            // Виконуємо телепорт на 3 одиниці відстані
            position.Position += teleportDirection * 3f;
            _entityManager.AddComponent(entityId, position);

            return true;
        }

        #endregion

        #region Passive Ability Handlers

        private void OnPoisonAttackPassive(int entityId)
        {
            // Реалізація пасивної здібності "Отруйна атака"
            if (!_entityManager.HasComponent<CombatStatsComponent>(entityId))
            {
                return;
            }

            // У повній реалізації додаємо ефект отрути до атак
            // Тут можна, наприклад, модифікувати характеристики

            CombatStatsComponent stats = _entityManager.GetComponent<CombatStatsComponent>(entityId);
            // Модифікація характеристик, якщо потрібно
            _entityManager.AddComponent(entityId, stats);
        }

        private void OnCounterAttackPassive(int entityId)
        {
            // Реалізація пасивної здібності "Контратака"
            // У повній реалізації додаємо шанс контратаки при ухиленні
        }

        private void OnHealthRegenPassive(int entityId)
        {
            // Реалізація пасивної здібності "Регенерація здоров'я"
            if (!_entityManager.HasComponent<HealthComponent>(entityId))
            {
                return;
            }

            HealthComponent health = _entityManager.GetComponent<HealthComponent>(entityId);

            // Збільшуємо регенерацію
            health.RegenRate *= 1.5f;
            _entityManager.AddComponent(entityId, health);
        }

        private void OnCombatMasterPassive(int entityId)
        {
            // Реалізація пасивної здібності "Майстер бою"
            if (!_entityManager.HasComponent<CombatStatsComponent>(entityId))
            {
                return;
            }

            CombatStatsComponent stats = _entityManager.GetComponent<CombatStatsComponent>(entityId);

            // Збільшуємо шанс критичного удару
            stats.CriticalChance += 0.05f;
            _entityManager.AddComponent(entityId, stats);
        }

        #endregion
    }
}
