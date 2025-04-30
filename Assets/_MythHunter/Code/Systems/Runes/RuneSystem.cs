// RuneSystem.cs (оновлення)
using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Systems.Runes.Effects;
using System.Collections.Generic;
using MythHunter.Core.DI;

namespace MythHunter.Systems.Phase
{
    /// <summary>
    /// Розширена система керування рунами
    /// </summary>
    public class RuneSystem : MythHunter.Core.ECS.SystemBase, IRuneSystem, MythHunter.Events.IEventSubscriber
    {
        private readonly IEntityManager _entityManager;
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;

        private int _currentRuneValue = 0;
        private RuneEffectType _currentRuneType = RuneEffectType.Combat;
        private readonly System.Random _random = new System.Random();

        private readonly RuneEffectFactory _effectFactory;
        private readonly Dictionary<RuneEffectType, RuneEffect> _effects = new Dictionary<RuneEffectType, RuneEffect>();

        [Inject]
        public RuneSystem(IEntityManager entityManager, IEventBus eventBus, IMythLogger logger)
        {
            _entityManager = entityManager;
            _eventBus = eventBus;
            _logger = logger;

            // Створюємо фабрику ефектів
            _effectFactory = new RuneEffectFactory(entityManager, logger);

            // Ініціалізуємо ефекти
            _effects[RuneEffectType.Combat] = _effectFactory.CreateEffect(RuneEffectType.Combat);
            _effects[RuneEffectType.Movement] = _effectFactory.CreateEffect(RuneEffectType.Movement);
            _effects[RuneEffectType.Health] = _effectFactory.CreateEffect(RuneEffectType.Health);
        }

        public override void Initialize()
        {
            SubscribeToEvents();
            _logger.LogInfo("RuneSystem initialized", "Rune");
        }

        public void SubscribeToEvents()
        {
            // Підписуємось на початок фази рун
            _eventBus.Subscribe<Events.Domain.PhaseStartedEvent>(OnPhaseStarted);
        }

        public void UnsubscribeFromEvents()
        {
            // Відписуємось від подій
            _eventBus.Unsubscribe<Events.Domain.PhaseStartedEvent>(OnPhaseStarted);
        }

        private void OnPhaseStarted(Events.Domain.PhaseStartedEvent evt)
        {
            // Якщо почалась фаза рун - автоматично кидаємо руну
            if (evt.Phase == Events.Domain.GamePhase.Rune)
            {
                RollRune();
            }
        }

        public void RollRune()
        {
            // Генеруємо значення від 2 до 12 (як при киданні 2-х шестигранних кубиків)
            _currentRuneValue = _random.Next(1, 7) + _random.Next(1, 7);

            // Визначаємо випадковий тип руни
            _currentRuneType = (RuneEffectType)_random.Next(0, 3);

            _logger.LogInfo($"Rolled rune: Value={_currentRuneValue}, Type={_currentRuneType}", "Rune");

            // Публікуємо подію про кидання руни
            _eventBus.Publish(new Events.Domain.RuneRolledEvent
            {
                Value = _currentRuneValue,
                Type = (int)_currentRuneType,
                Timestamp = System.DateTime.UtcNow,
                Description = GetCurrentRuneDescription()
            });
        }

        public int GetRuneValue() => _currentRuneValue;

        public RuneEffectType GetRuneType() => _currentRuneType;

        public string GetCurrentRuneDescription()
        {
            if (_effects.TryGetValue(_currentRuneType, out var effect))
            {
                return effect.GetDescription(_currentRuneValue);
            }

            return "Невідомий ефект руни";
        }

        public void ApplyRuneEffect(int entityId)
        {
            _logger.LogInfo($"Applying rune effect to entity {entityId}: Value={_currentRuneValue}, Type={_currentRuneType}", "Rune");

            // Отримуємо відповідний ефект і застосовуємо
            if (_effects.TryGetValue(_currentRuneType, out var effect))
            {
                if (effect.CanApply(entityId))
                {
                    effect.Apply(entityId, _currentRuneValue);

                    // Публікуємо подію про застосування ефекту
                    _eventBus.Publish(new Events.Domain.RuneEffectAppliedEvent
                    {
                        EntityId = entityId,
                        Value = _currentRuneValue,
                        Type = (int)_currentRuneType,
                        Timestamp = System.DateTime.UtcNow
                    });
                }
                else
                {
                    _logger.LogWarning($"Cannot apply rune effect to entity {entityId}: incompatible entity", "Rune");
                }
            }
            else
            {
                _logger.LogError($"No effect found for rune type: {_currentRuneType}", "Rune");
            }
        }

        public override void Update(float deltaTime)
        {
            // Логіка оновлення системи рун
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            _logger.LogInfo("RuneSystem disposed", "Rune");
        }
    }
}
