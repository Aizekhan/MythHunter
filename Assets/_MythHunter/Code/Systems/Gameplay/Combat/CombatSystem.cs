// Шлях: Assets/_MythHunter/Code/Systems/Combat/CombatSystem.cs
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Components.Combat;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Events.Domain;
using MythHunter.Systems.Phase;
using MythHunter.Utils.Logging;
using UnityEngine;
using RPGFramework.ECS;

namespace MythHunter.Systems.Combat
{
    /// <summary>
    /// Основна система бою
    /// </summary>
    public class CombatSystem : SystemBase, ICombatSystem, IEventSubscriber
    {
        private readonly IEntityManager _entityManager;
        private readonly IPhaseSystem _phaseSystem;
        private readonly ICombatDetectionSystem _combatDetectionSystem;
        private readonly ICombatAbilitySystem _combatAbilitySystem;
        private readonly MythHunter.GameLogic.Combat.Adapters.CombatSystemIntegration _combatIntegration;
        // Шлях: Assets/_MythHunter/Code/Systems/Combat/CombatSystem.cs (продовження)

        // Словник активних боїв: ID бою -> (ID атакуючого, ID захисника)
        private readonly Dictionary<int, (int attacker, int defender)> _activeCombats = new Dictionary<int, (int attacker, int defender)>();
        private int _nextCombatId = 1;

        // Частота оновлення бою
        private const float COMBAT_UPDATE_INTERVAL = 0.1f;

        // Коефіцієнти накопичення і втрати люті та концентрації
        private const float RAGE_GAIN_ON_HIT = 10f;
        private const float RAGE_GAIN_ON_CRIT = 25f;
        private const float RAGE_DECAY_RATE = 5f; // Втрата люті за секунду
        private const float CONCENTRATION_GAIN_ON_DAMAGE = 5f;
        private const float CONCENTRATION_REGEN_RATE = 2f; // Регенерація концентрації за секунду
        private const float CONCENTRATION_COST_DODGE = 15f;
        private const float CONCENTRATION_COST_BLOCK = 10f;

        // Час кулдауну для обміну люті на концентрацію
        private const float RAGE_EXCHANGE_COOLDOWN = 3f;

        // Мапа для відстеження часу останнього обміну для кожної сутності
        private readonly Dictionary<int, float> _lastRageExchangeTime = new Dictionary<int, float>();

        // Мапа для зберігання асинхронних операцій бою
        private readonly Dictionary<int, UniTaskCompletionSource<bool>> _combatTasks = new Dictionary<int, UniTaskCompletionSource<bool>>();

        [Inject]
        public CombatSystem(
            IEntityManager entityManager,
            IEventBus eventBus,
            IMythLogger logger,
            IPhaseSystem phaseSystem,
            ICombatDetectionSystem combatDetectionSystem,
            ICombatAbilitySystem combatAbilitySystem,
            MythHunter.GameLogic.Combat.Adapters.CombatSystemIntegration combatIntegration)
            : base(logger, eventBus)
        {
            _entityManager = entityManager;
            _phaseSystem = phaseSystem;
            _combatDetectionSystem = combatDetectionSystem;
            _combatAbilitySystem = combatAbilitySystem;
            _combatIntegration = combatIntegration;
        }

        public override void Initialize()
        {
            base.Initialize();
            _logger.LogInfo("CombatSystem initialized", "Combat");
        }

        protected override void OnSubscribeToEvents()
        {
            Subscribe<PhaseChangedEvent>(OnPhaseChanged);
            Subscribe<EntityDetectedEvent>(OnEntityDetected);
            Subscribe<EntityLostEvent>(OnEntityLost);
            Subscribe<CombatStartRequestEvent>(OnCombatStartRequest);
            Subscribe<DamageAppliedEvent>(OnDamageApplied);
            Subscribe<EntityDeathEvent>(OnEntityDeath);
        }

        protected override void OnUnsubscribeFromEvents()
        {
            Unsubscribe<PhaseChangedEvent>(OnPhaseChanged);
            Unsubscribe<EntityDetectedEvent>(OnEntityDetected);
            Unsubscribe<EntityLostEvent>(OnEntityLost);
            Unsubscribe<CombatStartRequestEvent>(OnCombatStartRequest);
            Unsubscribe<DamageAppliedEvent>(OnDamageApplied);
            Unsubscribe<EntityDeathEvent>(OnEntityDeath);
        }

        private void OnPhaseChanged(PhaseChangedEvent evt)
        {
            // Якщо почалася фаза Active - можна проводити бойові дії
            if (evt.CurrentPhase == GamePhase.Active)
            {
                // Можливо, почати сканування для виявлення потенційних боїв
            }
            // Якщо змінилася фаза (вже не Active), завершити всі активні бої
            if (evt.PreviousPhase == GamePhase.Active)
            {
                EndAllActiveCombats(CombatEndReason.PhaseEnded);
            }
        }

        private void OnEntityDetected(EntityDetectedEvent evt)
        {
            // Перевіряємо, чи можна почати бій між сутностями
            if (_combatDetectionSystem.CanInitiateCombat(evt.ObserverEntityId, evt.DetectedEntityId))
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

        private void OnEntityLost(EntityLostEvent evt)
        {
            // Можливо тут треба логіка, що робити, якщо ціль загубилася з поля зору
            // Наприклад, завершити бій з певною затримкою, якщо втрачено візуальний контакт
        }

        private void OnCombatStartRequest(CombatStartRequestEvent evt)
        {
            StartCombat(evt.AttackerEntityId, evt.DefenderEntityId);
        }

        private void OnDamageApplied(DamageAppliedEvent evt)
        {
            // Оновлюємо лють при нанесенні пошкодження
            float oldRage = _combatIntegration.GetRage(evt.SourceEntityId);
            float rageGain = evt.IsCritical ? RAGE_GAIN_ON_CRIT : RAGE_GAIN_ON_HIT;
            _combatIntegration.AddRage(evt.SourceEntityId, rageGain);
            float newRage = _combatIntegration.GetRage(evt.SourceEntityId);

            // Публікуємо подію про зміну люті
            if (Mathf.Abs(newRage - oldRage) > 0.01f)
            {
                Publish(new RageUpdatedEvent
                {
                    EntityId = evt.SourceEntityId,
                    OldValue = oldRage,
                    NewValue = newRage,
                    Timestamp = DateTime.UtcNow
                });
            }

            // Оновлюємо концентрацію при отриманні пошкодження
            if (!evt.IsDodged && !evt.IsBlocked)
            {
                float oldConcentration = _combatIntegration.GetConcentration(evt.TargetEntityId);
                _combatIntegration.AddConcentration(evt.TargetEntityId, CONCENTRATION_GAIN_ON_DAMAGE);
                float newConcentration = _combatIntegration.GetConcentration(evt.TargetEntityId);

                // Публікуємо подію про зміну концентрації
                if (Mathf.Abs(newConcentration - oldConcentration) > 0.01f)
                {
                    Publish(new ConcentrationUpdatedEvent
                    {
                        EntityId = evt.TargetEntityId,
                        OldValue = oldConcentration,
                        NewValue = newConcentration,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
        }

        private void OnEntityDeath(EntityDeathEvent evt)
        {
            // При смерті сутності завершуємо всі бої, в яких вона бере участь
            foreach (var combatEntry in _activeCombats)
            {
                if (combatEntry.Value.attacker == evt.EntityId || combatEntry.Value.defender == evt.EntityId)
                {
                    EndCombat(combatEntry.Key, CombatEndReason.Death);
                }
            }
        }

        public override void Update(float deltaTime)
        {
            // Обробка активних боїв
            ProcessActiveCombats(deltaTime);

            // Оновлення регенерації/деградації ресурсів для всіх сутностей
            UpdateCombatResources(deltaTime);
        }

        public override void Dispose()
        {
            // Завершення всіх активних боїв при знищенні системи
            EndAllActiveCombats(CombatEndReason.SystemForced);

            base.Dispose();
        }

        public int StartCombat(int attackerEntityId, int defenderEntityId)
        {
            // Перевіряємо, чи можливий бій між цими сутностями
            if (!_combatDetectionSystem.CanInitiateCombat(attackerEntityId, defenderEntityId))
            {
                _logger.LogWarning($"Cannot start combat between entities {attackerEntityId} and {defenderEntityId}", "Combat");
                return -1;
            }

            // Перевіряємо, чи вже є активний бій між цими сутностями
            foreach (var combat in _activeCombats)
            {
                if ((combat.Value.attacker == attackerEntityId && combat.Value.defender == defenderEntityId) ||
                    (combat.Value.attacker == defenderEntityId && combat.Value.defender == attackerEntityId))
                {
                    _logger.LogInfo($"Combat between entities {attackerEntityId} and {defenderEntityId} is already active", "Combat");
                    return combat.Key;
                }
            }

            int combatId = _nextCombatId++;
            _activeCombats[combatId] = (attackerEntityId, defenderEntityId);

            // Встановлюємо стан бою для обох сутностей
            _combatIntegration.SetCombatState(attackerEntityId, true, defenderEntityId);
            _combatIntegration.SetCombatState(defenderEntityId, true, attackerEntityId);

            // Публікуємо подію початку бою
            Publish(new CombatStartedEvent
            {
                CombatId = combatId,
                ParticipantEntityIds = new[] { attackerEntityId, defenderEntityId },
                Timestamp = DateTime.UtcNow
            });

            // Запускаємо асинхронний процес бою
            ProcessCombatAsync(combatId).Forget();

            _logger.LogInfo($"Combat started between entities {attackerEntityId} and {defenderEntityId}, combat ID: {combatId}", "Combat");

            return combatId;
        }

        public void EndCombat(int combatId, CombatEndReason reason)
        {
            if (!_activeCombats.TryGetValue(combatId, out var participants))
            {
                _logger.LogWarning($"Cannot end combat {combatId}: combat not found", "Combat");
                return;
            }

            // Визначаємо переможця (для спрощення, це завжди атакуючий, якщо він живий)
            int winnerEntityId = -1;

            // Перевіряємо, хто переміг
            if (_entityManager.HasComponent<HealthComponent>(participants.attacker))
            {
                HealthComponent attackerHealth = _entityManager.GetComponent<HealthComponent>(participants.attacker);
                if (!attackerHealth.IsDead)
                {
                    winnerEntityId = participants.attacker;
                }
            }

            if (winnerEntityId == -1 && _entityManager.HasComponent<HealthComponent>(participants.defender))
            {
                HealthComponent defenderHealth = _entityManager.GetComponent<HealthComponent>(participants.defender);
                if (!defenderHealth.IsDead)
                {
                    winnerEntityId = participants.defender;
                }
            }

            // Скидаємо стан бою для обох сутностей
            _combatIntegration.SetCombatState(participants.attacker, false);
            _combatIntegration.SetCombatState(participants.defender, false);

            // Скидаємо стан використання активної здібності
            _combatAbilitySystem.ResetActiveAbilityUse(participants.attacker);
            _combatAbilitySystem.ResetActiveAbilityUse(participants.defender);

            // Публікуємо подію завершення бою
            Publish(new CombatEndedEvent
            {
                CombatId = combatId,
                WinnerEntityId = winnerEntityId,
                ParticipantEntityIds = new[] { participants.attacker, participants.defender },
                EndReason = reason,
                Timestamp = DateTime.UtcNow
            });

            // Завершуємо асинхронну задачу бою, якщо вона є
            if (_combatTasks.TryGetValue(combatId, out var taskSource))
            {
                taskSource.TrySetResult(true);
                _combatTasks.Remove(combatId);
            }

            _activeCombats.Remove(combatId);

            _logger.LogInfo($"Combat {combatId} ended. Reason: {reason}, Winner: {winnerEntityId}", "Combat");
        }

        private void EndAllActiveCombats(CombatEndReason reason)
        {
            // Копіюємо список активних боїв, щоб уникнути проблем при модифікації колекції
            var combatIds = new List<int>(_activeCombats.Keys);

            foreach (var combatId in combatIds)
            {
                EndCombat(combatId, reason);
            }
        }

        public async UniTask ProcessCombatAsync(int combatId)
        {
            if (!_activeCombats.TryGetValue(combatId, out var participants))
            {
                _logger.LogWarning($"Cannot process combat {combatId}: combat not found", "Combat");
                return;
            }

            // Створюємо джерело завершення для асинхронної задачі
            var completionSource = new UniTaskCompletionSource<bool>();
            _combatTasks[combatId] = completionSource;

            // Ініціалізуємо параметри бою
            int attackerId = participants.attacker;
            int defenderId = participants.defender;

            float combatElapsedTime = 0f;

            // Цикл бою
            while (_activeCombats.ContainsKey(combatId))
            {
                // Перевіряємо, чи не закінчилася активна фаза
                if (_phaseSystem.CurrentPhase != GamePhase.Active)
                {
                    EndCombat(combatId, CombatEndReason.PhaseEnded);
                    break;
                }

                // Перевіряємо, чи обидві сутності живі
                if (!AreEntitiesAlive(attackerId, defenderId))
                {
                    EndCombat(combatId, CombatEndReason.Death);
                    break;
                }

                // Перевіряємо, чи бачать сутності одна одну
                if (!_combatDetectionSystem.CanInitiateCombat(attackerId, defenderId))
                {
                    // Можливо, вони втратили одна одну з поля зору
                    // Додаємо затримку перед завершенням бою
                    await UniTask.Delay(TimeSpan.FromSeconds(1));

                    // Повторна перевірка після затримки
                    if (!_combatDetectionSystem.CanInitiateCombat(attackerId, defenderId))
                    {
                        EndCombat(combatId, CombatEndReason.Retreat);
                        break;
                    }
                }

                // Обробка дій бою
                ProcessCombatActions(attackerId, defenderId);

                // Оновлення часу бою
                combatElapsedTime += COMBAT_UPDATE_INTERVAL;

                // Затримка між оновленнями бою
                await UniTask.Delay(TimeSpan.FromSeconds(COMBAT_UPDATE_INTERVAL));
            }

            await completionSource.Task;
        }

        private bool AreEntitiesAlive(int entityId1, int entityId2)
        {
            bool entity1Alive = true;
            bool entity2Alive = true;

            if (_entityManager.HasComponent<HealthComponent>(entityId1))
            {
                entity1Alive = !_entityManager.GetComponent<HealthComponent>(entityId1).IsDead;
            }

            if (_entityManager.HasComponent<HealthComponent>(entityId2))
            {
                entity2Alive = !_entityManager.GetComponent<HealthComponent>(entityId2).IsDead;
            }

            return entity1Alive && entity2Alive;
        }

        private void ProcessCombatActions(int attackerId, int defenderId)
        {
            // Перевіряємо, чи є компоненти для обох учасників
            if (!_combatIntegration.IsAlive(attackerId) || !_combatIntegration.IsAlive(defenderId))
            {
                return;
            }

            // Перевіряємо, чи минув час для наступної атаки атакуючого
            float currentTime = Time.time;

            if (_combatIntegration.CanAttack(attackerId, currentTime))
            {
                // Обробляємо атаку
                ProcessAttack(attackerId, defenderId);

                // Оновлюємо час останньої атаки
                _combatIntegration.UpdateLastAttackTime(attackerId, currentTime);
            }

            // Перевіряємо, чи минув час для наступної атаки захисника
            if (_combatIntegration.CanAttack(defenderId, currentTime))
            {
                // Обробляємо атаку у зворотньому напрямку
                ProcessAttack(defenderId, attackerId);

                // Оновлюємо час останньої атаки
                _combatIntegration.UpdateLastAttackTime(defenderId, currentTime);
            }
        }

        private void ProcessAttack(int attackerId, int defenderId)
        {
            // Перевіряємо, чи не мертві сутності
            if (!_combatIntegration.IsAlive(attackerId) || !_combatIntegration.IsAlive(defenderId))
            {
                return;
            }

            // Визначаємо, чи точний удар
            float accuracy = _combatIntegration.GetAccuracy(attackerId);
            bool isAccurate = UnityEngine.Random.value <= accuracy;

            if (!isAccurate)
            {
                // Промах, нічого не робимо
                return;
            }

            // Визначаємо, чи ухилився захисник
            bool isDodged = false;
            float defenderConcentration = _combatIntegration.GetConcentration(defenderId);

            if (defenderConcentration >= CONCENTRATION_COST_DODGE)
            {
                float dodgeChance = _combatIntegration.GetDodgeChance(defenderId);
                isDodged = UnityEngine.Random.value <= dodgeChance;

                if (isDodged)
                {
                    // Віднімаємо концентрацію при ухиленні
                    float oldConcentration = defenderConcentration;
                    float newConcentration = defenderConcentration - CONCENTRATION_COST_DODGE;
                    _combatIntegration.SetConcentration(defenderId, newConcentration);

                    // Публікуємо подію про зміну концентрації
                    Publish(new ConcentrationUpdatedEvent
                    {
                        EntityId = defenderId,
                        OldValue = oldConcentration,
                        NewValue = newConcentration,
                        Timestamp = DateTime.UtcNow
                    });

                    return; // Ухилився, пропускаємо подальші розрахунки
                }
            }

            // Визначаємо, чи заблокував захисник
            bool isBlocked = false;
            defenderConcentration = _combatIntegration.GetConcentration(defenderId); // Оновлюємо значення

            if (defenderConcentration >= CONCENTRATION_COST_BLOCK)
            {
                float blockChance = _combatIntegration.GetBlockChance(defenderId);
                isBlocked = UnityEngine.Random.value <= blockChance;

                if (isBlocked)
                {
                    // Віднімаємо концентрацію при блокуванні
                    float oldConcentration = defenderConcentration;
                    float newConcentration = defenderConcentration - CONCENTRATION_COST_BLOCK;
                    _combatIntegration.SetConcentration(defenderId, newConcentration);

                    // Публікуємо подію про зміну концентрації
                    Publish(new ConcentrationUpdatedEvent
                    {
                        EntityId = defenderId,
                        OldValue = oldConcentration,
                        NewValue = newConcentration,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            // Визначаємо, чи критичний удар
            float critChance = _combatIntegration.GetCriticalChance(attackerId);
            bool isCritical = UnityEngine.Random.value <= critChance;

            // Перевіряємо, чи наповнена шкала люті для критичного удару
            float attackerRage = _combatIntegration.GetRage(attackerId);
            float maxRage = _combatIntegration.GetMaxRage(attackerId);

            if (attackerRage >= maxRage)
            {
                isCritical = true;

                // Обнуляємо лють після критичного удару
                _combatIntegration.SetRage(attackerId, 0f);

                // Публікуємо подію про зміну люті
                Publish(new RageUpdatedEvent
                {
                    EntityId = attackerId,
                    OldValue = attackerRage,
                    NewValue = 0f,
                    Timestamp = DateTime.UtcNow
                });
            }

            // Розраховуємо базовий урон
            float baseDamage = _combatIntegration.GetAttackPower(attackerId);

            // Модифікуємо урон залежно від критичного удару
            float critMultiplier = _combatIntegration.GetCriticalMultiplier(attackerId);
            float damageMultiplier = isCritical ? critMultiplier : 1f;

            // Модифікуємо урон залежно від блокування
            if (isBlocked)
            {
                damageMultiplier *= 0.5f; // Зменшуємо урон на 50% при блокуванні
            }

            // Розраховуємо фінальний урон з урахуванням захисту
            float defense = _combatIntegration.GetDefense(defenderId);
            float finalDamage = (baseDamage * damageMultiplier) * (100f / (100f + defense));

            // Запобігаємо від'ємному урону
            finalDamage = Mathf.Max(1f, finalDamage);

            // Застосовуємо урон
            ApplyDamage(attackerId, defenderId, finalDamage, Events.Domain.DamageType.Physical);
        }

        private void ProcessActiveCombats(float deltaTime)
        {
            // Нічого додаткового тут не робимо, бо вся обробка відбувається в асинхронній функції ProcessCombatAsync
        }

        private void UpdateCombatResources(float deltaTime)
        {
            // Отримуємо всі сутності з компонентами бою
            var entities = _combatIntegration.GetEntitiesWithCombatComponents();

            foreach (int entityId in entities)
            {
                float oldRage = _combatIntegration.GetRage(entityId);
                float oldConcentration = _combatIntegration.GetConcentration(entityId);

                // Оновлюємо лють (зменшується з часом, якщо в бою)
                if (_combatIntegration.IsInCombat(entityId))
                {
                    float newRage = Mathf.Max(0f, oldRage - (RAGE_DECAY_RATE * deltaTime));
                    _combatIntegration.SetRage(entityId, newRage);
                }

                // Оновлюємо концентрацію (регенерується з часом)
                float maxConcentration = _combatIntegration.GetMaxConcentration(entityId);
                float newConcentration = Mathf.Min(maxConcentration, oldConcentration + (CONCENTRATION_REGEN_RATE * deltaTime));
                _combatIntegration.SetConcentration(entityId, newConcentration);

                // Публікуємо події про зміну ресурсів, якщо вони значно змінилися
                float currentRage = _combatIntegration.GetRage(entityId);
                float currentConcentration = _combatIntegration.GetConcentration(entityId);

                if (Mathf.Abs(oldRage - currentRage) > 1f)
                {
                    Publish(new RageUpdatedEvent
                    {
                        EntityId = entityId,
                        OldValue = oldRage,
                        NewValue = currentRage,
                        Timestamp = DateTime.UtcNow
                    });
                }

                if (Mathf.Abs(oldConcentration - currentConcentration) > 1f)
                {
                    Publish(new ConcentrationUpdatedEvent
                    {
                        EntityId = entityId,
                        OldValue = oldConcentration,
                        NewValue = currentConcentration,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
        }

        public bool IsEntityInCombat(int entityId)
        {
            return _combatIntegration.IsInCombat(entityId);
        }

        public int GetEntityCombatTarget(int entityId)
        {
            return _combatIntegration.GetCombatTarget(entityId);
        }

        public float ApplyDamage(int sourceEntityId, int targetEntityId, float amount, Events.Domain.DamageType damageType)
        {
            if (!_entityManager.HasComponent<HealthComponent>(targetEntityId))
            {
                _logger.LogWarning($"Cannot apply damage to entity {targetEntityId}: it has no HealthComponent", "Combat");
                return 0f;
            }

            HealthComponent health = _entityManager.GetComponent<HealthComponent>(targetEntityId);

            // Перевіряємо, чи не мертва сутність і чи не невразлива
            if (health.IsDead || health.IsInvulnerable)
            {
                return 0f;
            }

            // Застосовуємо пошкодження
            float oldHealth = health.CurrentHealth;
            health.CurrentHealth = Mathf.Max(0f, health.CurrentHealth - amount);
            health.LastDamageTime = Time.time;

            // Перевіряємо, чи сутність померла
            if (health.CurrentHealth <= 0f)
            {
                health.CurrentHealth = 0f;
                health.IsDead = true;

                // Публікуємо подію смерті
                Publish(new EntityDeathEvent
                {
                    EntityId = targetEntityId,
                    KillerEntityId = sourceEntityId,
                    Timestamp = DateTime.UtcNow
                });
            }

            // Оновлюємо компонент здоров'я
            _entityManager.AddComponent(targetEntityId, health);

            // Визначаємо критичне попадання
            float critChance = _combatIntegration.GetCriticalChance(sourceEntityId);
            float sourceRage = _combatIntegration.GetRage(sourceEntityId);
            float sourceMaxRage = _combatIntegration.GetMaxRage(sourceEntityId);
            bool isCritical = UnityEngine.Random.value <= critChance || sourceRage >= sourceMaxRage;

            // Визначаємо, чи була атака заблокована або ухилена
            bool isBlocked = false;
            bool isDodged = false;

            float targetConcentration = _combatIntegration.GetConcentration(targetEntityId);
            float blockChance = _combatIntegration.GetBlockChance(targetEntityId);
            float dodgeChance = _combatIntegration.GetDodgeChance(targetEntityId);

            if (targetConcentration >= CONCENTRATION_COST_BLOCK)
            {
                isBlocked = UnityEngine.Random.value <= blockChance;
            }

            if (targetConcentration >= CONCENTRATION_COST_DODGE && !isBlocked)
            {
                isDodged = UnityEngine.Random.value <= dodgeChance;
            }

            // Публікуємо подію про нанесення пошкодження
            Publish(new DamageAppliedEvent
            {
                SourceEntityId = sourceEntityId,
                TargetEntityId = targetEntityId,
                DamageAmount = amount,
                DamageType = damageType,
                IsCritical = isCritical,
                IsBlocked = isBlocked,
                IsDodged = isDodged,
                Timestamp = DateTime.UtcNow
            });

            return amount;
        }

        public bool UseActiveAbility(int entityId, int targetEntityId = -1)
        {
            return _combatAbilitySystem.UseActiveAbility(entityId, targetEntityId);
        }

        public void ChangeCombatStance(int entityId, CombatStance newStance)
        {
            if (!_entityManager.HasComponent<CombatStyleComponent>(entityId))
            {
                _logger.LogWarning($"Cannot change combat stance for entity {entityId}: it has no CombatStyleComponent", "Combat");
                return;
            }

            CombatStyleComponent styleComponent = _entityManager.GetComponent<CombatStyleComponent>(entityId);

            // Запам'ятовуємо стару стійку
            CombatStance oldStance = styleComponent.CurrentStance;

            // Якщо стійка така ж сама, нічого не робимо
            if (oldStance == newStance)
            {
                return;
            }

            // Змінюємо стійку
            styleComponent.CurrentStance = newStance;
            styleComponent.LastStanceChangeTime = Time.time;

            // Оновлюємо компонент стилю бою
            _entityManager.AddComponent(entityId, styleComponent);

            // Оновлюємо бойові характеристики відповідно до нової стійки
            UpdateCombatStatsBasedOnStance(entityId);

            // Публікуємо подію про зміну стійки
            Publish(new CombatStanceChangedEvent
            {
                EntityId = entityId,
                OldStance = oldStance,
                NewStance = newStance,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInfo($"Entity {entityId} changed combat stance from {oldStance} to {newStance}", "Combat");
        }

        public CombatStance GetEntityCombatStance(int entityId)
        {
            if (_entityManager.HasComponent<CombatStyleComponent>(entityId))
            {
                return _entityManager.GetComponent<CombatStyleComponent>(entityId).CurrentStance;
            }

            return CombatStance.Balanced; // За замовчуванням
        }

        public void UpdateCombatStatsBasedOnStance(int entityId)
        {
            // TODO EPIC 12 Phase 3: This method needs to be redesigned to use StatModifiers
            // instead of directly modifying base stats. For now, it only works with old components.
            // New components should use the modifier system from RPGFramework.Stats.

            if (!_entityManager.HasComponent<CombatStyleComponent>(entityId))
            {
                return;
            }

            // Only apply stance modifiers to old components
            // New components should use stat modifiers instead
            if (!_entityManager.HasComponent<CombatStatsComponent>(entityId))
            {
                _logger.LogWarning(
                    $"UpdateCombatStatsBasedOnStance called for entity {entityId} with new components. " +
                    "This method is deprecated for new components - use stat modifiers instead.",
                    "Combat");
                return;
            }

            CombatStyleComponent styleComponent = _entityManager.GetComponent<CombatStyleComponent>(entityId);
            CombatStatsComponent statsComponent = _entityManager.GetComponent<CombatStatsComponent>(entityId);

            // Скидаємо модифікатори до базових значень
            // Тут можна використовувати оригінальні базові значення, якщо зберігаємо їх

            // Застосовуємо модифікатори відповідно до стійки
            switch (styleComponent.CurrentStance)
            {
                case CombatStance.Aggressive:
                    // Збільшуємо атаку, зменшуємо захист
                    statsComponent.AttackPower *= styleComponent.AggressiveAttackMod;
                    statsComponent.Defense *= styleComponent.AggressiveDefenseMod;
                    statsComponent.DodgeChance *= styleComponent.AggressiveDodgeMod;
                    statsComponent.BlockChance *= styleComponent.AggressiveBlockMod;
                    break;

                case CombatStance.Defensive:
                    // Зменшуємо атаку, збільшуємо захист
                    statsComponent.AttackPower *= styleComponent.DefensiveAttackMod;
                    statsComponent.Defense *= styleComponent.DefensiveDefenseMod;
                    statsComponent.DodgeChance *= styleComponent.DefensiveDodgeMod;
                    statsComponent.BlockChance *= styleComponent.DefensiveBlockMod;
                    break;

                case CombatStance.Balanced:
                    // Нічого не змінюємо, залишаємо базові значення
                    break;
            }

            // Оновлюємо компонент бойових характеристик
            _entityManager.AddComponent(entityId, statsComponent);
        }

        public float ExchangeRageForConcentration(int entityId, float rageAmount)
        {
            // Перевіряємо, чи пройшов кулдаун
            float currentTime = Time.time;

            if (_lastRageExchangeTime.TryGetValue(entityId, out float lastExchangeTime) &&
                currentTime - lastExchangeTime < RAGE_EXCHANGE_COOLDOWN)
            {
                _logger.LogInfo($"Cannot exchange rage yet for entity {entityId}: cooldown active", "Combat");
                return 0f;
            }

            // Отримуємо поточні значення
            float currentRage = _combatIntegration.GetRage(entityId);
            float currentConcentration = _combatIntegration.GetConcentration(entityId);
            float maxConcentration = _combatIntegration.GetMaxConcentration(entityId);

            // Обмежуємо кількість люті для обміну
            float actualRageAmount = Mathf.Min(rageAmount, currentRage);

            if (actualRageAmount <= 0f)
            {
                return 0f;
            }

            // Обмінний курс (скільки концентрації за 1 люті)
            const float EXCHANGE_RATE = 0.75f;

            // Розраховуємо отриману концентрацію
            float concentrationGained = actualRageAmount * EXCHANGE_RATE;

            // Оновлюємо значення люті та концентрації
            float newRage = currentRage - actualRageAmount;
            float newConcentration = Mathf.Min(maxConcentration, currentConcentration + concentrationGained);

            _combatIntegration.SetRage(entityId, newRage);
            _combatIntegration.SetConcentration(entityId, newConcentration);

            // Запам'ятовуємо час обміну
            _lastRageExchangeTime[entityId] = currentTime;

            // Публікуємо події про зміну ресурсів
            Publish(new RageUpdatedEvent
            {
                EntityId = entityId,
                OldValue = currentRage,
                NewValue = newRage,
                Timestamp = DateTime.UtcNow
            });

            Publish(new ConcentrationUpdatedEvent
            {
                EntityId = entityId,
                OldValue = currentConcentration,
                NewValue = newConcentration,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInfo($"Entity {entityId} exchanged {actualRageAmount} rage for {concentrationGained} concentration", "Combat");

            return concentrationGained;
        }
    }
}
