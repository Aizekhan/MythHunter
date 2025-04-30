// HealthRuneEffect.cs
using MythHunter.Core.ECS;
using MythHunter.Components.Character;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.Runes.Effects
{
    /// <summary>
    /// Ефект руни, що впливає на здоров'я
    /// </summary>
    public class HealthRuneEffect : RuneEffect
    {
        public HealthRuneEffect(IEntityManager entityManager, IMythLogger logger) : base(entityManager, logger)
        {
        }

        public override void Apply(int entityId, int runeValue)
        {
            if (!CanApply(entityId))
                return;

            // Отримуємо поточний компонент здоров'я
            var health = EntityManager.GetComponent<HealthComponent>(entityId);

            // Модифікуємо на основі значення руни
            switch (runeValue)
            {
                case 2: // Мінімальне значення (2d6)
                    // Знижуємо здоров'я на 30% від поточного
                    health.CurrentHealth = System.Math.Max(1, health.CurrentHealth * 0.7f);
                    break;
                case 3:
                case 4:
                    // Зниження здоров'я на 15%
                    health.CurrentHealth = System.Math.Max(1, health.CurrentHealth * 0.85f);
                    break;
                case 5:
                case 6:
                case 7: // Найбільш ймовірні значення
                    // Добавляємо регенерацію здоров'я
                    health.HasRegeneration = true;
                    health.RegenAmount = 1;
                    health.RegenInterval = 5f;
                    break;
                case 8:
                case 9:
                    // Підвищуємо максимальне здоров'я на 10% і відновлюємо частину поточного
                    health.MaxHealth *= 1.1f;
                    health.CurrentHealth += health.MaxHealth * 0.1f;
                    if (health.CurrentHealth > health.MaxHealth)
                        health.CurrentHealth = health.MaxHealth;
                    break;
                case 10:
                case 11:
                    // Підвищуємо максимальне здоров'я на 20% і відновлюємо 20% здоров'я
                    health.MaxHealth *= 1.2f;
                    health.CurrentHealth += health.MaxHealth * 0.2f;
                    if (health.CurrentHealth > health.MaxHealth)
                        health.CurrentHealth = health.MaxHealth;
                    break;
                case 12: // Максимальне значення (2d6)
                    // Значне підвищення максимального здоров'я і повне відновлення
                    health.MaxHealth *= 1.5f;
                    health.CurrentHealth = health.MaxHealth;
                    health.HasRegeneration = true;
                    health.RegenAmount = 2;
                    health.RegenInterval = 3f;
                    break;
            }

            // Оновлюємо компонент
            EntityManager.AddComponent(entityId, health);

            // Записуємо логи
            Logger.LogInfo($"Applied Health Rune effect (value: {runeValue}) to entity {entityId}", "Runes");
        }

        public override bool CanApply(int entityId)
        {
            return EntityManager.HasComponent<HealthComponent>(entityId);
        }

        public override string GetDescription(int runeValue)
        {
            switch (runeValue)
            {
                case 2:
                    return "Прокляття життя: зменшує поточне здоров'я на 30%";
                case 3:
                case 4:
                    return "Виснаження: зменшує поточне здоров'я на 15%";
                case 5:
                case 6:
                case 7:
                    return "Регенерація: додає повільну регенерацію здоров'я";
                case 8:
                case 9:
                    return "Посилення життя: підвищує максимальне здоров'я на 10% і відновлює частину поточного";
                case 10:
                case 11:
                    return "Значне посилення життя: підвищує максимальне здоров'я на 20% і відновлює частину поточного";
                case 12:
                    return "Благословення життя: збільшує максимальне здоров'я на 50%, повністю відновлює і додає швидку регенерацію";
                default:
                    return "Невідомий ефект руни здоров'я";
            }
        }
    }
}
