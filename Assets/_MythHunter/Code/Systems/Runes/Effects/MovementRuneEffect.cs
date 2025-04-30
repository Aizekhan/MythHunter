// MovementRuneEffect.cs
using MythHunter.Core.ECS;
using MythHunter.Components.Movement;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.Runes.Effects
{
    /// <summary>
    /// Ефект руни, що впливає на пересування
    /// </summary>
    public class MovementRuneEffect : RuneEffect
    {
        public MovementRuneEffect(IEntityManager entityManager, IMythLogger logger) : base(entityManager, logger)
        {
        }

        public override void Apply(int entityId, int runeValue)
        {
            if (!CanApply(entityId))
                return;

            // Отримуємо поточний компонент
            var movement = EntityManager.GetComponent<MovementComponent>(entityId);

            // Модифікуємо на основі значення руни
            switch (runeValue)
            {
                case 2: // Мінімальне значення (2d6)
                    movement.Speed *= 0.6f; // Сильне зменшення швидкості
                    break;
                case 3:
                case 4:
                    movement.Speed *= 0.8f; // Помірне зменшення швидкості
                    break;
                case 5:
                case 6:
                case 7: // Найбільш ймовірні значення
                    // Нейтральний ефект, можна додати мінітік
                    break;
                case 8:
                case 9:
                    movement.Speed *= 1.2f; // Помірне збільшення швидкості
                    break;
                case 10:
                case 11:
                    movement.Speed *= 1.4f; // Значне збільшення швидкості
                    break;
                case 12: // Максимальне значення (2d6)
                    movement.Speed *= 2f; // Подвійна швидкість
                    break;
            }

            // Оновлюємо компонент
            EntityManager.AddComponent(entityId, movement);

            // Записуємо логи
            Logger.LogInfo($"Applied Movement Rune effect (value: {runeValue}) to entity {entityId}", "Runes");
        }

        public override bool CanApply(int entityId)
        {
            return EntityManager.HasComponent<MovementComponent>(entityId);
        }

        public override string GetDescription(int runeValue)
        {
            switch (runeValue)
            {
                case 2:
                    return "Закляття сповільнення: зменшує швидкість на 40%";
                case 3:
                case 4:
                    return "Обтяження: зменшує швидкість на 20%";
                case 5:
                case 6:
                case 7:
                    return "Нейтральний ефект на рух";
                case 8:
                case 9:
                    return "Прискорення: збільшує швидкість на 20%";
                case 10:
                case 11:
                    return "Значне прискорення: збільшує швидкість на 40%";
                case 12:
                    return "Благословення швидкості: подвоює швидкість";
                default:
                    return "Невідомий ефект руни руху";
            }
        }
    }
}
