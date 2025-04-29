using MythHunter.Components.Character;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Systems.Core;
using MythHunter.Utils.Logging;
using MythHunter.Events.Domain.Combat;
namespace MythHunter.Systems.Combat
{
    /// <summary>
    /// Система для обробки здоров'я персонажів з використанням кешу компонентів
    /// </summary>
    public class HealthSystem : SystemBase
    {
        private readonly IEntityManager _entityManager;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        // Кеш для швидкого доступу до компонентів здоров'я
        private readonly ComponentCache<HealthComponent> _healthCache;

        [Inject]
        public HealthSystem(
            IEntityManager entityManager,
            IEventBus eventBus,
            IMythLogger logger)
        {
            _entityManager = entityManager;
            _eventBus = eventBus;
            _logger = logger;

            // Створення кешу компонентів
            _healthCache = new ComponentCache<HealthComponent>(_entityManager);
        }

        public override void Initialize()
        {
            // Підписка на події
            _eventBus.Subscribe<Events.Domain.Combat.DamageAppliedEvent>(OnDamageApplied);
            _eventBus.Subscribe<Events.Domain.Combat.HealingAppliedEvent>(OnHealingApplied);

            // Початкове наповнення кешу
            _healthCache.Update();

            _logger.LogInfo("HealthSystem initialized");
        }

        public override void Update(float deltaTime)
        {
            // Оновлення кешу перед обробкою
            _healthCache.Update();

            // Перевірка стану здоров'я для всіх сутностей
            foreach (var entityId in _healthCache.GetAllEntityIds())
            {
                var health = _healthCache.Get(entityId);

                // Перевірка смерті
                if (health.CurrentHealth <= 0 && !health.IsDead)
                {
                    health.IsDead = true;
                    _entityManager.AddComponent(entityId, health);

                    // Публікація події смерті
                    _eventBus.Publish(new Events.Domain.Combat.EntityDeathEvent
                    {
                        EntityId = entityId,
                        Timestamp = System.DateTime.UtcNow
                    });
                }

                // Регенерація здоров'я, якщо є
                if (health.HasRegeneration && !health.IsDead)
                {
                    health.RegenTimer += deltaTime;

                    if (health.RegenTimer >= health.RegenInterval)
                    {
                        health.RegenTimer = 0;
                        health.CurrentHealth = System.Math.Min(health.CurrentHealth + health.RegenAmount, health.MaxHealth);
                        _entityManager.AddComponent(entityId, health);
                    }
                }
            }
        }

        private void OnDamageApplied(Events.Domain.Combat.DamageAppliedEvent evt)
        {
            if (!_entityManager.HasComponent<HealthComponent>(evt.TargetEntityId))
                return;

            var health = _entityManager.GetComponent<HealthComponent>(evt.TargetEntityId);
            health.CurrentHealth = System.Math.Max(0, health.CurrentHealth - evt.DamageAmount);

            _entityManager.AddComponent(evt.TargetEntityId, health);
            _healthCache.Add(evt.TargetEntityId, health);

            _logger.LogDebug($"Entity {evt.TargetEntityId} took {evt.DamageAmount} damage. " +
                          $"Current health: {health.CurrentHealth}/{health.MaxHealth}");
        }

        private void OnHealingApplied(Events.Domain.Combat.HealingAppliedEvent evt)
        {
            if (!_entityManager.HasComponent<HealthComponent>(evt.TargetEntityId))
                return;

            var health = _entityManager.GetComponent<HealthComponent>(evt.TargetEntityId);

            if (health.IsDead)
                return;

            health.CurrentHealth = System.Math.Min(health.MaxHealth, health.CurrentHealth + evt.HealingAmount);

            _entityManager.AddComponent(evt.TargetEntityId, health);
            _healthCache.Add(evt.TargetEntityId, health);

            _logger.LogDebug($"Entity {evt.TargetEntityId} healed for {evt.HealingAmount}. " +
                          $"Current health: {health.CurrentHealth}/{health.MaxHealth}");
        }

        public override void Dispose()
        {
            // Відписка від подій
            _eventBus.Unsubscribe<Events.Domain.Combat.DamageAppliedEvent>(OnDamageApplied);
            _eventBus.Unsubscribe<Events.Domain.Combat.HealingAppliedEvent>(OnHealingApplied);

            _logger.LogInfo("HealthSystem disposed");
        }
    }
}
