// Assets/_MythHunter/Code/Data/StatRegistry.cs

using System.Collections.Generic;
using System.Linq;
using MythHunter.Components.Character;
using UnityEngine;

namespace MythHunter.Data
{
    /// <summary>
    /// Реєстр характеристик з документацією та категоризацією
    /// </summary>
    public static class StatRegistry
    {
        private static readonly Dictionary<StatType, StatDefinition> _definitions = new Dictionary<StatType, StatDefinition>();

        static StatRegistry()
        {


            // Базові характеристики
            RegisterStat(StatType.ManaCost, "Вартість мани", "Вартість виклику героя в мані", 1, 4, StatCategory.Basic);
            RegisterStat(StatType.Level, "Рівень", "Загальний рівень героя", 1, 100, StatCategory.Basic);
            RegisterStat(StatType.Experience,"Досвід","Кількість досвіду, набраного героєм",0, 999999,StatCategory.Basic);
            RegisterStat(StatType.HP, "Здоров'я", "Максимальна кількість здоров'я", 1, 10000, StatCategory.Basic);
            RegisterStat(StatType.HpRegen, "Регенерація здоров'я", "Кількість здоров'я, що відновлюється за секунду", 0, 1000, StatCategory.Basic);
            RegisterStat(StatType.Stamina, "Витривалість", "Максимальна кількість витривалості", 1, 1000, StatCategory.Basic);
            RegisterStat(StatType.StaminaRegen, "Регенерація витривалості", "Кількість витривалості, що відновлюється за секунду", 0, 100, StatCategory.Basic);

            // Характеристики для системи огляду
            RegisterStat(StatType.VisionRadius, "Радіус огляду", "Максимальна відстань, на якій герой бачить об'єкти", 1, 10, StatCategory.Basic);
            RegisterStat(StatType.VisionAngle, "Кут огляду", "Кут огляду героя у градусах", 30, 360, StatCategory.Basic);

            // Характеристики для системи бою
            RegisterStat(StatType.Rage, "Лють", "Поточний рівень люті для бойових здібностей", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.RageMax, "Максимальна лють", "Максимально можливий рівень люті", 50, 200, StatCategory.Combat);
            RegisterStat(StatType.Concentration, "Концентрація", "Поточний рівень концентрації для захисних маневрів", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.ConcentrationMax, "Максимальна концентрація", "Максимально можливий рівень концентрації", 50, 200, StatCategory.Combat);

            // Магічні характеристики
            RegisterStat(StatType.MagicChance, "Шанс магії", "Ймовірність використання магії", 0, 100, StatCategory.Magic);
            RegisterStat(StatType.MagicPower, "Сила магії", "Сила магічних здібностей", 0, 1000, StatCategory.Magic);

            // Бойові характеристики - атака
            RegisterStat(StatType.Damage, "Пошкодження", "Базова сила атаки", 1, 1000, StatCategory.Combat);
            RegisterStat(StatType.Weakness, "Слабкість", "Зниження здібностей при низькому здоров'ї", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.AttackSpeed, "Швидкість атаки", "Швидкість проведення атак", 0.1f, 10, StatCategory.Combat);
            RegisterStat(StatType.AttackRate, "Частота атаки", "Кількість атак за одиницю часу", 0.1f, 10, StatCategory.Combat);
            RegisterStat(StatType.MultipleAttackChance, "Шанс множинної атаки", "Ймовірність нанести кілька ударів за один раз", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.AmountOfMultipleHits, "Кількість множинних ударів", "Скільки додаткових ударів завдається при множинній атаці", 1, 10, StatCategory.Combat);
            RegisterStat(StatType.Slowdown, "Уповільнення", "Здатність уповільнювати ворога", 0, 10, StatCategory.Combat);

            // Бойові характеристики - захист
            RegisterStat(StatType.ArmorDurability, "Міцність броні", "Наскільки довго броня може захищати", 0, 1000, StatCategory.Combat);
            RegisterStat(StatType.ArmorResistance, "Стійкість броні", "Наскільки добре броня знижує пошкодження", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.ArmorReduction, "Зниження броні", "Здатність знижувати броню ворога", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.ArmorCorrosion, "Корозія броні", "Здатність руйнувати броню ворога з часом", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.BlockChance, "Шанс блоку", "Ймовірність заблокувати атаку", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.BlockPenetrationChance, "Шанс пробиття блоку", "Ймовірність пробити блок ворога", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.ArmorPenetrationChance, "Шанс пробиття броні", "Ймовірність пробити броню ворога", 0, 100, StatCategory.Combat);

            // Бойові характеристики - критичні удари
            RegisterStat(StatType.CritChance, "Шанс критичного удару", "Ймовірність нанести критичний удар", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.CritPower, "Сила критичного удару", "Множник пошкодження критичного удару", 1, 1000, StatCategory.Combat);

            // Бойові характеристики - ухилення/точність
            RegisterStat(StatType.EvasionChance, "Шанс ухилення", "Ймовірність ухилитися від атаки", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.AccuracyChance, "Шанс влучення", "Ймовірність влучення по цілі", 0, 100, StatCategory.Combat);

            // Бойові характеристики - станові ефекти
            RegisterStat(StatType.StunChance, "Шанс оглушення", "Ймовірність оглушити ворога", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.StunPower, "Сила оглушення", "Тривалість оглушення в секундах", 0, 10, StatCategory.Combat);
            RegisterStat(StatType.ReturnDamageChance, "Шанс повернення пошкодження", "Ймовірність повернути частину пошкодження", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.ReturnDamagePower, "Сила повернення пошкодження", "Відсоток пошкодження, що повертається", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.CounterattackChance, "Шанс контратаки", "Ймовірність контратакувати після блоку", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.DeathHitChance, "Шанс смертельного удару", "Ймовірність миттєво вбити ціль", 0, 100, StatCategory.Combat);

            // Бойові характеристики - ефекти отрути та кровотечі
            RegisterStat(StatType.Poison, "Отрута", "Шанс отруїти ворога", 0, 100, StatCategory.Status);
            RegisterStat(StatType.PoisonPower, "Сила отрути", "Пошкодження від отрути за секунду", 0, 100, StatCategory.Status);
            RegisterStat(StatType.Bleeding, "Кровотеча", "Шанс викликати кровотечу", 0, 100, StatCategory.Status);
            RegisterStat(StatType.BleedingPower, "Сила кровотечі", "Пошкодження від кровотечі за секунду", 0, 100, StatCategory.Status);
            RegisterStat(StatType.Plague, "Чума", "Шанс викликати чуму", 0, 100, StatCategory.Status);
            RegisterStat(StatType.PlaguePower, "Сила чуми", "Пошкодження та ослаблення від чуми", 0, 100, StatCategory.Status);

            // Бойові характеристики - вампіризм та крадіжка
            RegisterStat(StatType.VampirikChance, "Шанс вампіризму", "Ймовірність викрасти здоров'я", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.VampirikPower, "Сила вампіризму", "Відсоток викраденого здоров'я", 0, 100, StatCategory.Combat);
            RegisterStat(StatType.ThiefChance, "Шанс крадіжки", "Ймовірність викрасти золото або предмети", 0, 100, StatCategory.Combat);

            // Економічні характеристики
            RegisterStat(StatType.GoldPerTap, "Золото за клік", "Кількість золота, отриманого за клік", 0, 1000, StatCategory.Economy);
            RegisterStat(StatType.GoldPer8Hours, "Золото за 8 годин", "Кількість золота, отриманого за 8 годин пасивно", 0, 10000, StatCategory.Economy);
            RegisterStat(StatType.CommonItemChance, "Шанс звичайного предмета", "Ймовірність отримати звичайний предмет", 0, 100, StatCategory.Economy);
            RegisterStat(StatType.RareItemChance, "Шанс рідкісного предмета", "Ймовірність отримати рідкісний предмет", 0, 100, StatCategory.Economy);
            RegisterStat(StatType.EpicItemChance, "Шанс епічного предмета", "Ймовірність отримати епічний предмет", 0, 100, StatCategory.Economy);
            RegisterStat(StatType.LegendaryItemChance, "Шанс легендарного предмета", "Ймовірність отримати легендарний предмет", 0, 100, StatCategory.Economy);
            RegisterStat(StatType.UniqueItemChance, "Шанс унікального предмета", "Ймовірність отримати унікальний предмет", 0, 100, StatCategory.Economy);
            RegisterStat(StatType.TotalUpgradeCost, "Загальна вартість поліпшення", "Базова вартість поліпшення героя", 1, 1000, StatCategory.Economy);
            RegisterStat(StatType.UpgradeScale, "Масштаб поліпшення", "Множник збільшення вартості при поліпшенні", 1, 10, StatCategory.Economy);

            // Інші характеристики
            RegisterStat(StatType.BagSlots, "Слоти сумки", "Кількість слотів в сумці", 1, 4, StatCategory.Inventory);
        }

        private static void RegisterStat(StatType type, string name, string description, float min, float max, StatCategory category)
        {
            _definitions[type] = new StatDefinition
            {
                Type = type,
                Name = name,
                Description = description,
                MinValue = min,
                MaxValue = max,
                Category = category,
                Color = GetColorForCategory(category)
            };
        }

        public static StatDefinition GetDefinition(StatType type)
        {
            return _definitions.TryGetValue(type, out var def) ? def : null;
        }

        public static IEnumerable<StatDefinition> GetAllDefinitions()
        {
            return _definitions.Values;
        }

        public static IEnumerable<StatDefinition> GetDefinitionsByCategory(StatCategory category)
        {
            return _definitions.Values.Where(d => d.Category == category);
        }

        private static Color GetColorForCategory(StatCategory category)
        {
            switch (category)
            {
                case StatCategory.Basic:
                    return new Color(0.2f, 0.6f, 1f); // Блакитний
                case StatCategory.Combat:
                    return new Color(1f, 0.3f, 0.3f); // Червоний
                case StatCategory.Magic:
                    return new Color(0.6f, 0.4f, 1f); // Фіолетовий
                case StatCategory.Status:
                    return new Color(0.5f, 0.9f, 0.4f); // Зелений
                case StatCategory.Economy:
                    return new Color(1f, 0.9f, 0.3f); // Золотий
                case StatCategory.Inventory:
                    return new Color(0.8f, 0.5f, 0.2f); // Коричневий
                default:
                    return Color.white;
            }
        }
    }

    /// <summary>
    /// Опис та властивості характеристики
    /// </summary>
    public class StatDefinition
    {
        public StatType Type
        {
            get; set;
        }
        public string Name
        {
            get; set;
        }
        public string Description
        {
            get; set;
        }
        public float MinValue
        {
            get; set;
        }
        public float MaxValue
        {
            get; set;
        }
        public StatCategory Category
        {
            get; set;
        }
        public Color Color { get; set; } = Color.white;
    }
}
