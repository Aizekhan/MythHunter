// CombatRuneEffect.cs
using MythHunter.Core.ECS;
using MythHunter.Components.Combat;
using MythHunter.Utils.Logging;

namespace MythHunter.Systems.Runes.Effects
{
    /// <summary>
    /// Ефект руни, що впливає на бойові характеристики
    /// </summary>
    public class CombatRuneEffect : RuneEffect
    {
        public CombatRuneEffect(IEntityManager entityManager, IMythLogger logger) : base(entityManager, logger)
        {
        }

        public override void Apply(int entityId, int runeValue)
        {
            if (!CanApply(entityId))
                return;

            // Отримуємо поточний компонент
            var combatStats = EntityManager.GetComponent<CombatStatsComponent>(entityId);

            // Модифікуємо на основі значення руни
            switch (runeValue)
            {
                case 2: // Мінімальне значення (2d6)
                    combatStats.AttackPower *= 0.5f; // Зменшення атаки наполовину
                    break;
                case 3:
                case 4:
                    combatStats.AttackPower *= 0.8f; // Незначне зменшення атаки
                    combatStats.CriticalChance += 0.1f; // Компенсація збільшенням шансу крита
                    break;
                case 5:
                case 6:
                case 7: // Найбільш ймовірні значення
                    combatStats.CriticalMultiplier += 0.2f; // Незначне збільшення множника критів
                    break;
                case 8:
                case 9:
                    combatStats.AttackPower *= 1.1f; // Невелике збільшення атаки
                    break;
                case 10:
                case 11:
                    combatStats.AttackPower *= 1.2f; // Помітне збільшення атаки
                    break;
                case 12: // Максимальне значення (2d6)
                    combatStats.AttackPower *= 1.5f; // Значне збільшення атаки
                    combatStats.CriticalChance += 0.05f; // Невеликий бонус до шансу крита
                    break;
            }

            // Оновлюємо компонент
            EntityManager.AddComponent(entityId, combatStats);
            // Записуємо логи
            Logger.LogInfo($"Applied Combat Rune effect (value: {runeValue}) to entity {entityId}", "Runes");
        }

        public override bool CanApply(int entityId)
        {
            return EntityManager.HasComponent<CombatStatsComponent>(entityId);
        }

        public override string GetDescription(int runeValue)
        {
            switch (runeValue)
            {
                case 2:
                    return "Прокляття зброї: зменшує силу атаки на 50%";
                case 3:
                case 4:
                    return "Ослаблення зброї: зменшує силу атаки на 20%, але збільшує шанс критів на 10%";
                case 5:
                case 6:
                case 7:
                    return "Посилення критів: збільшує множник критичної шкоди на 20%";
                case 8:
                case 9:
                    return "Посилення зброї: збільшує силу атаки на 10%";
                case 10:
                case 11:
                    return "Значне посилення зброї: збільшує силу атаки на 20%";
                case 12:
                    return "Благословення зброї: збільшує силу атаки на 50% і шанс криту на 5%";
                default:
                    return "Невідомий ефект бойової руни";
            }
        }
    }
}
