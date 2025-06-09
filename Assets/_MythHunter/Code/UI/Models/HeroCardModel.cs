// Assets/_MythHunter/Code/UI/Models/HeroCardModel.cs - РОЗШИРЕНА ВЕРСІЯ
using System;
using System.Collections.Generic;

namespace MythHunter.UI.Models
{
    /// <summary>
    /// Модель для картки героя з повною інформацією для нового лоббі
    /// </summary>
    [Serializable]
    public class HeroCardModel
    {
        // Базова інформація
        public string ArchetypeId;
        public string Name;
        public string Description;
        public string Race;
        public string Class;
        public string IconPath;

        // Нові поля для нового лоббі
        public int Level = 1; // Рівень героя (1-10+)
        public int ManaCost = 1; // Вартість в мані (зазвичай 1)
        public int Experience = 0; // Досвід героя
        public int MaxExperience = 100; // Максимальний досвід для поточного рівня

        // Стан у лоббі
        public bool IsSelectable = true;
        public bool IsSelected = false;
        public int PlayerIndex = -1; // Який гравець вибрав (-1 = не вибраний)

        // Базові характеристики (для вкладки Stats)
        public int Health = 100;
        public int Energy = 50;
        public int Damage = 10;
        public int Defense = 5;
        public int Speed = 5;

        // Конструктори
        public HeroCardModel()
        {
        }

        public HeroCardModel(string archetypeId, string name, string race, string heroClass)
        {
            ArchetypeId = archetypeId;
            Name = name;
            Race = race;
            Class = heroClass;
        }

        /// <summary>
        /// Створює модель з архетипу
        /// </summary>
        public static HeroCardModel FromArchetype(string archetypeId, string name, string race, string heroClass, string iconPath = null)
        {
            return new HeroCardModel
            {
                ArchetypeId = archetypeId,
                Name = name,
                Race = race,
                Class = heroClass,
                IconPath = iconPath ?? $"UI/Heroes/{archetypeId}",
                Level = 1,
                ManaCost = 1,
                IsSelectable = true,
                IsSelected = false
            };
        }

        /// <summary>
        /// Клонує модель
        /// </summary>
        public HeroCardModel Clone()
        {
            return new HeroCardModel
            {
                ArchetypeId = this.ArchetypeId,
                Name = this.Name,
                Description = this.Description,
                Race = this.Race,
                Class = this.Class,
                IconPath = this.IconPath,
                Level = this.Level,
                ManaCost = this.ManaCost,
                Experience = this.Experience,
                MaxExperience = this.MaxExperience,
                IsSelectable = this.IsSelectable,
                IsSelected = this.IsSelected,
                PlayerIndex = this.PlayerIndex,
                Health = this.Health,
                Energy = this.Energy,
                Damage = this.Damage,
                Defense = this.Defense,
                Speed = this.Speed
            };
        }

        /// <summary>
        /// Отримує прогрес досвіду (0.0 - 1.0)
        /// </summary>
        public float GetExperienceProgress()
        {
            if (MaxExperience <= 0)
                return 0f;
            return (float)Experience / MaxExperience;
        }

        /// <summary>
        /// Додає досвід та перевіряє на підвищення рівня
        /// </summary>
        public bool AddExperience(int amount)
        {
            Experience += amount;

            if (Experience >= MaxExperience)
            {
                LevelUp();
                return true; // Рівень підвищено
            }

            return false;
        }

        /// <summary>
        /// Підвищує рівень героя
        /// </summary>
        private void LevelUp()
        {
            Level++;
            Experience -= MaxExperience;
            MaxExperience = CalculateMaxExperience(Level);

            // Підвищуємо характеристики
            Health += 10;
            Energy += 5;
            Damage += 2;
            Defense += 1;
            Speed += 1;
        }

        /// <summary>
        /// Розраховує максимальний досвід для рівня
        /// </summary>
        private int CalculateMaxExperience(int level)
        {
            return 100 + (level - 1) * 50; // 100, 150, 200, 250...
        }

        /// <summary>
        /// Перевіряє чи герой доступний для вибору за маною
        /// </summary>
        public bool CanAfford(int availableMana)
        {
            return availableMana >= ManaCost;
        }

        /// <summary>
        /// Встановлює стан вибраності
        /// </summary>
        public void SetSelected(bool selected, int playerIndex = -1)
        {
            IsSelected = selected;
            PlayerIndex = selected ? playerIndex : -1;
            IsSelectable = !selected; // Якщо вибраний, то більше не доступний
        }

        /// <summary>
        /// Скидає стан вибору
        /// </summary>
        public void ResetSelection()
        {
            IsSelected = false;
            PlayerIndex = -1;
            IsSelectable = true;
        }

        public override string ToString()
        {
            return $"{Name} (Lv.{Level}, {Race} {Class}, Cost: {ManaCost})";
        }
    }

    /// <summary>
    /// Фабрика для створення тестових героїв
    /// </summary>
    public static class HeroCardModelFactory
    {
        public static HeroCardModel CreateTestWarrior()
        {
            return new HeroCardModel
            {
                ArchetypeId = "TestWarrior",
                Name = "Воїн-Захисник",
                Race = "Human",
                Class = "Warrior",
                IconPath = "UI/Heroes/warrior_icon",
                Level = 1,
                ManaCost = 1,
                Health = 120,
                Energy = 40,
                Damage = 15,
                Defense = 10,
                Speed = 3
            };
        }

        public static HeroCardModel CreateTestMage()
        {
            return new HeroCardModel
            {
                ArchetypeId = "TestMage",
                Name = "Маг-Руйнівник",
                Race = "Elf",
                Class = "Mage",
                IconPath = "UI/Heroes/mage_icon",
                Level = 1,
                ManaCost = 1,
                Health = 80,
                Energy = 80,
                Damage = 20,
                Defense = 3,
                Speed = 6
            };
        }

        public static HeroCardModel CreateTestArcher()
        {
            return new HeroCardModel
            {
                ArchetypeId = "TestArcher",
                Name = "Лучник-Снайпер",
                Race = "Elf",
                Class = "Archer",
                IconPath = "UI/Heroes/archer_icon",
                Level = 1,
                ManaCost = 1,
                Health = 90,
                Energy = 60,
                Damage = 18,
                Defense = 5,
                Speed = 8
            };
        }

        /// <summary>
        /// Створює список тестових героїв для демонстрації
        /// </summary>
        public static List<HeroCardModel> CreateTestHeroes(int count = 12)
        {
            var heroes = new List<HeroCardModel>();
            var random = new System.Random();

            string[] names = { "Аратор", "Селінда", "Торгрім", "Лісанна", "Векс", "Міранда", "Гаррок", "Елара", "Браннон", "Зефіра", "Кайн", "Ріана" };
            string[] races = { "Human", "Elf", "Dwarf", "Halfling" };
            string[] classes = { "Warrior", "Mage", "Archer", "Rogue", "Tank", "Support" };

            for (int i = 0; i < count; i++)
            {
                var hero = new HeroCardModel
                {
                    ArchetypeId = $"Hero_{i + 1:D2}",
                    Name = names[i % names.Length],
                    Race = races[random.Next(races.Length)],
                    Class = classes[random.Next(classes.Length)],
                    IconPath = $"UI/Heroes/hero_{i + 1:D2}",
                    Level = random.Next(1, 6), // Рівень 1-5
                    ManaCost = random.Next(1, 4), // Вартість 1-3 мани
                    Health = random.Next(80, 121),
                    Energy = random.Next(40, 81),
                    Damage = random.Next(10, 21),
                    Defense = random.Next(3, 11),
                    Speed = random.Next(3, 9)
                };

                heroes.Add(hero);
            }

            return heroes;
        }
    }
}
