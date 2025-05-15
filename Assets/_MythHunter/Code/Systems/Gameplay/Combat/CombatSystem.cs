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
            ICombatAbilitySystem combatAbilitySystem)
            : base(logger, eventBus)
        {
            _entityManager = entityManager;
            _phaseSystem = phaseSystem;
            _combatDetectionSystem = combatDetectionSystem;
            _combatAbilitySystem = combatAbilitySystem;
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
            if (_entityManager.HasComponent<CombatStatsComponent>(evt.SourceEntityId))
            {
                CombatStatsComponent stats = _entityManager.GetComponent<CombatStatsComponent>(evt.SourceEntityId);
                float oldRage = stats.Rage;

                // Збільшуємо лють при нанесенні урону
                float rageGain = evt.IsCritical ? RAGE_GAIN_ON_CRIT : RAGE_GAIN_ON_HIT;
                stats.Rage = Mathf.Min(stats.MaxRage, stats.Rage + rageGain);

                _entityManager.AddComponent(evt.SourceEntityId, stats);

                // Публікуємо подію про зміну люті
                Publish(new RageUpdatedEvent
                {
                    EntityId = evt.SourceEntityId,
                    OldValue = oldRage,
                    NewValue = stats.Rage,
                    Timestamp = DateTime.UtcNow
                });
            }

            // Оновлюємо концентрацію при отриманні пошкодження
            if (_entityManager.HasComponent<CombatStatsComponent>(evt.TargetEntityId))
            {
                CombatStatsComponent stats = _entityManager.GetComponent<CombatStatsComponent>(evt.TargetEntityId);
                float oldConcentration = stats.Concentration;

                // Збільшуємо концентрацію при отриманні урону
                if (!evt.IsDodged && !evt.IsBlocked)
                {
                    stats.Concentration = Mathf.Min(stats.MaxConcentration, stats.Concentration + CONCENTRATION_GAIN_ON_DAMAGE);
                }

                _entityManager.AddComponent(evt.TargetEntityId, stats);

                // Публікуємо подію про зміну концентрації
                Publish(new ConcentrationUpdatedEvent
                {
                    EntityId = evt.TargetEntityId,
                    OldValue = oldConcentration,
                    NewValue = stats.Concentration,
                    Timestamp = DateTime.UtcNow
                });
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

            // Встановлюємо компоненти бою для обох сутностей
            if (_entityManager.HasComponent<CombatStatsComponent>(attackerEntityId))
            {
                CombatStatsComponent attackerStats = _entityManager.GetComponent<CombatStatsComponent>(attackerEntityId);
                attackerStats.IsInCombat = true;
                attackerStats.CombatTargetEntityId = defenderEntityId;
                _entityManager.AddComponent(attackerEntityId, attackerStats);
            }

            if (_entityManager.HasComponent<CombatStatsComponent>(defenderEntityId))
            {
                CombatStatsComponent defenderStats = _entityManager.GetComponent<CombatStatsComponent>(defenderEntityId);
                defenderStats.IsInCombat = true;
                defenderStats.CombatTargetEntityId = attackerEntityId;
                _entityManager.AddComponent(defenderEntityId, defenderStats);
            }

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
            if (_entityManager.HasComponent<CombatStatsComponent>(participants.attacker))
            {
                CombatStatsComponent attackerStats = _entityManager.GetComponent<CombatStatsComponent>(participants.attacker);
                attackerStats.IsInCombat = false;
                attackerStats.CombatTargetEntityId = -1;
                _entityManager.AddComponent(participants.attacker, attackerStats);

                // Скидаємо стан використання активної здібності
                _combatAbilitySystem.ResetActiveAbilityUse(participants.attacker);
            }

            if (_entityManager.HasComponent<CombatStatsComponent>(participants.defender))
            {
                CombatStatsComponent defenderStats = _entityManager.GetComponent<CombatStatsComponent>(participants.defender);
                defenderStats.IsInCombat = false;
                defenderStats.CombatTargetEntityId = -1;
                _entityManager.AddComponent(participants.defender, defenderStats);

                // Скидаємо стан використання активної здібності
                _combatAbilitySystem.ResetActiveAbilityUse(participants.defender);
            }

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
            // Отримуємо компоненти бою для атакуючого
            if (!_entityManager.HasComponent<CombatStatsComponent>(attackerId) ||
                !_entityManager.HasComponent<HealthComponent>(attackerId))
            {
                return;
            }

            // Отримуємо компоненти бою для захисника
            if (!_entityManager.HasComponent<CombatStatsComponent>(defenderId) ||
                !_entityManager.HasComponent<HealthComponent>(defenderId))
            {
                return;
            }

            CombatStatsComponent attackerStats = _entityManager.GetComponent<CombatStatsComponent>(attackerId);
            HealthComponent attackerHealth = _entityManager.GetComponent<HealthComponent>(attackerId);

            CombatStatsComponent defenderStats = _entityManager.GetComponent<CombatStatsComponent>(defenderId);
            HealthComponent defenderHealth = _entityManager.GetComponent<HealthComponent>(defenderId);

            // Перевіряємо, чи минув час для наступної атаки атакуючого
            float currentTime = Time.time;

            if (currentTime - attackerStats.LastAttackTime >= (1f / attackerStats.AttackSpeed))
            {
                // Обробляємо атаку
                ProcessAttack(attackerId, defenderId, attackerStats, defenderStats);

                // Оновлюємо час останньої атаки
                attackerStats.LastAttackTime = currentTime;
                _entityManager.AddComponent(attackerId, attackerStats);
            }

            // Перевіряємо, чи минув час для наступної атаки захисника
            if (currentTime - defenderStats.LastAttackTime >= (1f / defenderStats.AttackSpeed))
            {
                // Обробляємо атаку у зворотньому напрямку
                ProcessAttack(defenderId, attackerId, defenderStats, attackerStats);

                // Оновлюємо час останньої атаки
                defenderStats.LastAttackTime = currentTime;
                _entityManager.AddComponent(defenderId, defenderStats);
            }
        }

        private void ProcessAttack(int attackerId, int defenderId, CombatStatsComponent attackerStats, CombatStatsComponent defenderStats)
        {
            // Перевіряємо, чи не мертві сутності
            if (_entityManager.HasComponent<HealthComponent>(attackerId) &&
                _entityManager.GetComponent<HealthComponent>(attackerId).IsDead)
            {
                return;
            }

            if (_entityManager.HasComponent<HealthComponent>(defenderId) &&
                _entityManager.GetComponent<HealthComponent>(defenderId).IsDead)
            {
                return;
            }

            // Визначаємо, чи точний удар
            bool isAccurate = UnityEngine.Random.value <= attackerStats.Accuracy;

            if (!isAccurate)
            {
                // Промах, нічого не робимо
                return;
            }

            // Визначаємо, чи ухилився захисник
            bool isDodged = false;

            if (defenderStats.Concentration >= CONCENTRATION_COST_DODGE)
            {
                isDodged = UnityEngine.Random.value <= defenderStats.DodgeChance;

                if (isDodged)
                {
                    // Віднімаємо концентрацію при ухиленні
                    float oldConcentration = defenderStats.Concentration;
                    defenderStats.Concentration -= CONCENTRATION_COST_DODGE;
                    _entityManager.AddComponent(defenderId, defenderStats);

                    // Публікуємо подію про зміну концентрації
                    Publish(new ConcentrationUpdatedEvent
                    {
                        EntityId = defenderId,
                        OldValue = oldConcentration,
                        NewValue = defenderStats.Concentration,
                        Timestamp = DateTime.UtcNow
                    });

                    return; // Ухилився, пропускаємо подальші розрахунки
                }
            }

            // Визначаємо, чи заблокував захисник
            bool isBlocked = false;

            if (defenderStats.Concentration >= CONCENTRATION_COST_BLOCK)
            {
                isBlocked = UnityEngine.Random.value <= defenderStats.BlockChance;

                if (isBlocked)
                {
                    // Віднімаємо концентрацію при блокуванні
                    float oldConcentration = defenderStats.Concentration;
                    defenderStats.Concentration -= CONCENTRATION_COST_BLOCK;
                    _entityManager.AddComponent(defenderId, defenderStats);

                    // Публікуємо подію про зміну концентрації
                    Publish(new ConcentrationUpdatedEvent
                    {
                        EntityId = defenderId,
                        OldValue = oldConcentration,
                        NewValue = defenderStats.Concentration,
                        Timestamp = DateTime.UtcNow
                    });
                }
            }

            // Визначаємо, чи критичний удар
            bool isCritical = UnityEngine.Random.value <= attackerStats.CriticalChance;

            // Перевіряємо, чи наповнена шкала люті для критичного удару
            if (attackerStats.Rage >= attackerStats.MaxRage)
            {
                isCritical = true;

                // Обнуляємо лють після критичного удару
                float oldRage = attackerStats.Rage;
                attackerStats.Rage = 0f;
                _entityManager.AddComponent(attackerId, attackerStats);

                // Публікуємо подію про зміну люті
                Publish(new RageUpdatedEvent
                {
                    EntityId = attackerId,
                    OldValue = oldRage,
                    NewValue = 0f,
                    Timestamp = DateTime.UtcNow
                });
            }

            // Розраховуємо базовий урон
            float baseDamage = attackerStats.AttackPower;

            // Модифікуємо урон залежно від критичного удару
            float damageMultiplier = isCritical ? attackerStats.CriticalMultiplier : 1f;

            // Модифікуємо урон залежно від блокування
            if (isBlocked)
            {
                damageMultiplier *= 0.5f; // Зменшуємо урон на 50% при блокуванні
            }

            // Розраховуємо фінальний урон з урахуванням захисту
            float finalDamage = (baseDamage * damageMultiplier) * (100f / (100f + defenderStats.Defense));

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
            // Отримуємо всі сутності з компонентом бою
            int[] entities = _entityManager.GetEntitiesWith<CombatStatsComponent>();

            foreach (int entityId in entities)
            {
                if (_entityManager.HasComponent<CombatStatsComponent>(entityId))
                {
                    CombatStatsComponent stats = _entityManager.GetComponent<CombatStatsComponent>(entityId);
                    float oldRage = stats.Rage;
                    float oldConcentration = stats.Concentration;

                    // Оновлюємо лють (зменшується з часом)
                    if (stats.IsInCombat)
                    {
                        stats.Rage = Mathf.Max(0f, stats.Rage - (RAGE_DECAY_RATE * deltaTime));
                    }

                    // Оновлюємо концентрацію (регенерується з часом)
                    stats.Concentration = Mathf.Min(stats.MaxConcentration, stats.Concentration + (CONCENTRATION_REGEN_RATE * deltaTime));

                    // Застосовуємо зміни
                    _entityManager.AddComponent(entityId, stats);

                    // Публікуємо події про зміну ресурсів, якщо вони значно змінилися
                    if (Mathf.Abs(oldRage - stats.Rage) > 1f)
                    {
                        Publish(new RageUpdatedEvent
                        {
                            EntityId = entityId,
                            OldValue = oldRage,
                            NewValue = stats.Rage,
                            Timestamp = DateTime.UtcNow
                        });
                    }

                    if (Mathf.Abs(oldConcentration - stats.Concentration) > 1f)
                    {
                        Publish(new ConcentrationUpdatedEvent
                        {
                            EntityId = entityId,
                            OldValue = oldConcentration,
                            NewValue = stats.Concentration,
                            Timestamp = DateTime.UtcNow
                        });
                    }
                }
            }
        }

        public bool IsEntityInCombat(int entityId)
        {
            if (_entityManager.HasComponent<CombatStatsComponent>(entityId))
            {
                return _entityManager.GetComponent<CombatStatsComponent>(entityId).IsInCombat;
            }

            return false;
        }

        public int GetEntityCombatTarget(int entityId)
        {
            if (_entityManager.HasComponent<CombatStatsComponent>(entityId))
            {
                return _entityManager.GetComponent<CombatStatsComponent>(entityId).CombatTargetEntityId;
            }

            return -1;
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
            bool isCritical = false;
            if (_entityManager.HasComponent<CombatStatsComponent>(sourceEntityId))
            {
                CombatStatsComponent sourceStats = _entityManager.GetComponent<CombatStatsComponent>(sourceEntityId);
                isCritical = UnityEngine.Random.value <= sourceStats.CriticalChance || sourceStats.Rage >= sourceStats.MaxRage;
            }

            // Визначаємо, чи була атака заблокована або ухилена
            bool isBlocked = false;
            bool isDodged = false;

            if (_entityManager.HasComponent<CombatStatsComponent>(targetEntityId))
            {
                // Шлях: Assets/_MythHunter/Code/Systems/Combat/CombatSystem.cs (продовження)

                CombatStatsComponent targetStats = _entityManager.GetComponent<CombatStatsComponent>(targetEntityId);

                if (targetStats.Concentration >= CONCENTRATION_COST_BLOCK)
                {
                    isBlocked = UnityEngine.Random.value <= targetStats.BlockChance;
                }

                if (targetStats.Concentration >= CONCENTRATION_COST_DODGE && !isBlocked)
                {
                    isDodged = UnityEngine.Random.value <= targetStats.DodgeChance;
                }
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
            if (!_entityManager.HasComponent<CombatStyleComponent>(entityId) ||
                !_entityManager.HasComponent<CombatStatsComponent>(entityId))
            {
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
            if (!_entityManager.HasComponent<CombatStatsComponent>(entityId))
            {
                _logger.LogWarning($"Cannot exchange rage for entity {entityId}: it has no CombatStatsComponent", "Combat");
                return 0f;
            }

            // Перевіряємо, чи пройшов кулдаун
            float currentTime = Time.time;

            if (_lastRageExchangeTime.TryGetValue(entityId, out float lastExchangeTime) &&
                currentTime - lastExchangeTime < RAGE_EXCHANGE_COOLDOWN)
            {
                _logger.LogInfo($"Cannot exchange rage yet for entity {entityId}: cooldown active", "Combat");
                return 0f;
            }

            CombatStatsComponent stats = _entityManager.GetComponent<CombatStatsComponent>(entityId);

            // Обмежуємо кількість люті для обміну
            float actualRageAmount = Mathf.Min(rageAmount, stats.Rage);

            if (actualRageAmount <= 0f)
            {
                return 0f;
            }

            // Обмінний курс (скільки концентрації за 1 люті)
            const float EXCHANGE_RATE = 0.75f;

            // Розраховуємо отриману концентрацію
            float concentrationGained = actualRageAmount * EXCHANGE_RATE;

            // Оновлюємо значення люті та концентрації
            float oldRage = stats.Rage;
            float oldConcentration = stats.Concentration;

            stats.Rage -= actualRageAmount;
            stats.Concentration = Mathf.Min(stats.MaxConcentration, stats.Concentration + concentrationGained);

            // Оновлюємо компонент
            _entityManager.AddComponent(entityId, stats);

            // Запам'ятовуємо час обміну
            _lastRageExchangeTime[entityId] = currentTime;

            // Публікуємо події про зміну ресурсів
            Publish(new RageUpdatedEvent
            {
                EntityId = entityId,
                OldValue = oldRage,
                NewValue = stats.Rage,
                Timestamp = DateTime.UtcNow
            });

            Publish(new ConcentrationUpdatedEvent
            {
                EntityId = entityId,
                OldValue = oldConcentration,
                NewValue = stats.Concentration,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInfo($"Entity {entityId} exchanged {actualRageAmount} rage for {concentrationGained} concentration", "Combat");

            return concentrationGained;
        }
    }
}
