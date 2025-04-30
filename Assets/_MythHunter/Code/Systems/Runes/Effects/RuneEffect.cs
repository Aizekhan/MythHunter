// RuneEffect.cs
using System;
using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.Runes.Effects
{
    /// <summary>
    /// Базовий клас для ефектів рун
    /// </summary>
    public abstract class RuneEffect
    {
        protected IEntityManager EntityManager;
        protected IMythLogger Logger;

        public RuneEffect(IEntityManager entityManager, IMythLogger logger)
        {
            EntityManager = entityManager;
            Logger = logger;
        }

        /// <summary>
        /// Застосовує ефект руни до сутності
        /// </summary>
        public abstract void Apply(int entityId, int runeValue);

        /// <summary>
        /// Перевіряє, чи можна застосувати ефект до сутності
        /// </summary>
        public abstract bool CanApply(int entityId);

        /// <summary>
        /// Отримує опис ефекту на основі значення руни
        /// </summary>
        public abstract string GetDescription(int runeValue);
    }

    /// <summary>
    /// Фабрика для створення ефектів рун на основі категорії
    /// </summary>
    public class RuneEffectFactory
    {
        private readonly IEntityManager _entityManager;
        private readonly IMythLogger _logger;

        public RuneEffectFactory(IEntityManager entityManager, IMythLogger logger)
        {
            _entityManager = entityManager;
            _logger = logger;
        }

        public RuneEffect CreateEffect(RuneEffectType effectType)
        {
            switch (effectType)
            {
                case RuneEffectType.Combat:
                    return new CombatRuneEffect(_entityManager, _logger);
                case RuneEffectType.Movement:
                    return new MovementRuneEffect(_entityManager, _logger);
                case RuneEffectType.Health:
                    return new HealthRuneEffect(_entityManager, _logger);
                default:
                    _logger.LogWarning($"Unknown rune effect type: {effectType}", "Runes");
                    return null;
            }
        }
    }

    /// <summary>
    /// Типи ефектів рун
    /// </summary>
    public enum RuneEffectType
    {
        Combat,
        Movement,
        Health
    }
}
