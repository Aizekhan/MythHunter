// Шлях: Assets/_MythHunter/Code/Editor/Heroes/CreateHeroArchetypeWizard.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using MythHunter.Entities.Archetypes;
using MythHunter.Entities.Heroes;
using MythHunter.Components.Character;
using System.Collections.Generic;

namespace MythHunter.Editor.Heroes
{
    public class CreateHeroArchetypeWizard : ScriptableWizard
    {
        [Header("Основна інформація")]
        public string heroName = "Новий герой";
        public HeroRace race = HeroRace.Human;
        public HeroClass heroClass = HeroClass.Warrior;

        [Header("Шаблон на основі")]
        public HeroArchetypeSO baseTemplate;

        [MenuItem("MythHunter/Heroes/Create New Hero Archetype")]
        static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard<CreateHeroArchetypeWizard>("Створення архетипу героя", "Створити");
        }

        void OnWizardCreate()
        {
            // Створюємо новий архетип
            HeroArchetypeSO heroArchetype = ScriptableObject.CreateInstance<HeroArchetypeSO>();

            // Генеруємо унікальний ID
            heroArchetype.ArchetypeId = $"Hero_{race}_{heroClass}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            heroArchetype.HeroName = heroName;
            heroArchetype.Race = race;
            heroArchetype.Class = heroClass;

            // Встановлюємо стандартні або копіюємо з шаблону
            if (baseTemplate != null)
            {
                // Копіюємо опис
                heroArchetype.Description = baseTemplate.Description;

                // Копіюємо всі характеристики
                foreach (var statValue in baseTemplate.GetAllStats())
                {
                    heroArchetype.SetStat(statValue.Type, statValue.Value);
                }

                // Копіюємо навички
                heroArchetype._passiveSkills = baseTemplate._passiveSkills;
                heroArchetype._activeSkills = baseTemplate._activeSkills;

                // Копіюємо візуальні та інші налаштування
                heroArchetype.IconPath = baseTemplate.IconPath;
                heroArchetype.Skins = baseTemplate.Skins;
                heroArchetype.TeamId = baseTemplate.TeamId;

                // Копіюємо соціальні навички
                heroArchetype.Religion = baseTemplate.Religion;
                heroArchetype.Ideology = baseTemplate.Ideology;
                heroArchetype._professions = baseTemplate._professions;
            }
            else
            {
                // Встановлюємо базові значення залежно від класу та раси
                SetDefaultStats(heroArchetype);
            }

            // Створюємо директорію, якщо її ще немає
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");

            if (!AssetDatabase.IsValidFolder("Assets/Resources/ScriptableObjects"))
                AssetDatabase.CreateFolder("Assets/Resources", "ScriptableObjects");

            if (!AssetDatabase.IsValidFolder("Assets/Resources/ScriptableObjects/Heroes"))
                AssetDatabase.CreateFolder("Assets/Resources/ScriptableObjects", "Heroes");

            // Зберігаємо новий архетип
            string assetPath = $"Assets/Resources/ScriptableObjects/Heroes/{heroArchetype.ArchetypeId}.asset";
            AssetDatabase.CreateAsset(heroArchetype, assetPath);
            AssetDatabase.SaveAssets();

            // Відкриваємо для редагування
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = heroArchetype;
        }

        private void SetDefaultStats(HeroArchetypeSO heroArchetype)
        {
            // Опис за замовчуванням для класу
            string description = GetDefaultDescription(heroClass);
            heroArchetype.Description = description;

            // Встановлюємо основні характеристики
            SetDefaultStatsForClass(heroArchetype, heroClass);

            // Додаємо расові бонуси
            ApplyRacialBonuses(heroArchetype, race);

            // Стилі бою
            SetCombatStyles(heroArchetype);

            // Порожні списки по замовчуванню
            heroArchetype._passiveSkills = new System.Collections.Generic.List<HeroArchetypeSO.SkillValue>();
            heroArchetype._activeSkills = new System.Collections.Generic.List<HeroArchetypeSO.SkillValue>();

            // Додаємо стандартну активну здібність для класу
            AddDefaultActiveSkill(heroArchetype, heroClass);

            // Додаємо пасивну здібність для раси
            AddDefaultPassiveSkill(heroArchetype, race);

            // За замовчуванням команда 0
            heroArchetype.TeamId = 0;
        }

        private string GetDefaultDescription(HeroClass heroClass)
        {
            switch (heroClass)
            {
                case HeroClass.Warrior:
                    return "Могутній воїн, що спеціалізується на ближньому бою";
                case HeroClass.Rogue:
                    return "Спритний розбійник, що спеціалізується на несподіваних атаках";
                case HeroClass.Ranger:
                    return "Майстерний лучник, що спеціалізується на дальніх атаках";
                case HeroClass.Mage:
                    return "Могутній заклинач з величезним магічним потенціалом";
                case HeroClass.Support:
                    return "Цілитель та підтримка, що допомагає союзникам у бою";
                case HeroClass.Tank:
                    return "Витривалий захисник, що здатен витримати багато ударів";
                default:
                    return "Герой з невідомими здібностями";
            }
        }

        private void SetDefaultStatsForClass(HeroArchetypeSO heroArchetype, HeroClass heroClass)
        {
            // Базові характеристики для всіх класів
            heroArchetype.SetStat(StatType.Level, 1);

            // Специфічні характеристики для класів
            switch (heroClass)
            {
                case HeroClass.Warrior:
                    heroArchetype.SetStat(StatType.HP, 120);
                    heroArchetype.SetStat(StatType.HpRegen, 5);
                    heroArchetype.SetStat(StatType.Stamina, 100);
                    heroArchetype.SetStat(StatType.Damage, 10);
                    heroArchetype.SetStat(StatType.ArmorResistance, 8);
                    heroArchetype.SetStat(StatType.CritChance, 20);
                    heroArchetype.SetStat(StatType.CritPower, 2);
                    heroArchetype.SetStat(StatType.BlockChance, 30);
                    heroArchetype.SetStat(StatType.EvasionChance, 10);
                    heroArchetype.SetStat(StatType.AccuracyChance, 80);
                    heroArchetype.SetStat(StatType.AttackSpeed, 1.0f);
                    heroArchetype.SetStat(StatType.VisionRadius, 5);
                    break;

                case HeroClass.Rogue:
                    heroArchetype.SetStat(StatType.HP, 90);
                    heroArchetype.SetStat(StatType.HpRegen, 3);
                    heroArchetype.SetStat(StatType.Stamina, 120);
                    heroArchetype.SetStat(StatType.Damage, 12);
                    heroArchetype.SetStat(StatType.ArmorResistance, 5);
                    heroArchetype.SetStat(StatType.CritChance, 40);
                    heroArchetype.SetStat(StatType.CritPower, 2.5f);
                    heroArchetype.SetStat(StatType.BlockChance, 10);
                    heroArchetype.SetStat(StatType.EvasionChance, 30);
                    heroArchetype.SetStat(StatType.AccuracyChance, 75);
                    heroArchetype.SetStat(StatType.AttackSpeed, 1.2f);
                    heroArchetype.SetStat(StatType.VisionRadius, 6);
                    break;

                case HeroClass.Ranger:
                    heroArchetype.SetStat(StatType.HP, 85);
                    heroArchetype.SetStat(StatType.HpRegen, 3);
                    heroArchetype.SetStat(StatType.Stamina, 110);
                    heroArchetype.SetStat(StatType.Damage, 11);
                    heroArchetype.SetStat(StatType.ArmorResistance, 4);
                    heroArchetype.SetStat(StatType.CritChance, 30);
                    heroArchetype.SetStat(StatType.CritPower, 2.2f);
                    heroArchetype.SetStat(StatType.BlockChance, 10);
                    heroArchetype.SetStat(StatType.EvasionChance, 20);
                    heroArchetype.SetStat(StatType.AccuracyChance, 90);
                    heroArchetype.SetStat(StatType.AttackSpeed, 1.5f);
                    heroArchetype.SetStat(StatType.VisionRadius, 7);
                    break;

                case HeroClass.Mage:
                    heroArchetype.SetStat(StatType.HP, 75);
                    heroArchetype.SetStat(StatType.HpRegen, 2);
                    heroArchetype.SetStat(StatType.Stamina, 90);
                    heroArchetype.SetStat(StatType.Damage, 15);
                    heroArchetype.SetStat(StatType.ArmorResistance, 3);
                    heroArchetype.SetStat(StatType.CritChance, 25);
                    heroArchetype.SetStat(StatType.CritPower, 3);
                    heroArchetype.SetStat(StatType.BlockChance, 5);
                    heroArchetype.SetStat(StatType.EvasionChance, 15);
                    heroArchetype.SetStat(StatType.AccuracyChance, 85);
                    heroArchetype.SetStat(StatType.AttackSpeed, 0.8f);
                    heroArchetype.SetStat(StatType.MagicChance, 90);
                    heroArchetype.SetStat(StatType.MagicPower, 20);
                    heroArchetype.SetStat(StatType.VisionRadius, 6);
                    break;

                case HeroClass.Support:
                    heroArchetype.SetStat(StatType.HP, 95);
                    heroArchetype.SetStat(StatType.HpRegen, 6);
                    heroArchetype.SetStat(StatType.Stamina, 100);
                    heroArchetype.SetStat(StatType.Damage, 7);
                    heroArchetype.SetStat(StatType.ArmorResistance, 6);
                    heroArchetype.SetStat(StatType.CritChance, 15);
                    heroArchetype.SetStat(StatType.CritPower, 1.8f);
                    heroArchetype.SetStat(StatType.BlockChance, 15);
                    heroArchetype.SetStat(StatType.EvasionChance, 15);
                    heroArchetype.SetStat(StatType.AccuracyChance, 80);
                    heroArchetype.SetStat(StatType.AttackSpeed, 1.0f);
                    heroArchetype.SetStat(StatType.MagicChance, 70);
                    heroArchetype.SetStat(StatType.MagicPower, 15);
                    heroArchetype.SetStat(StatType.VisionRadius, 5.5f);
                    break;

                case HeroClass.Tank:
                    heroArchetype.SetStat(StatType.HP, 150);
                    heroArchetype.SetStat(StatType.HpRegen, 7);
                    heroArchetype.SetStat(StatType.Stamina, 90);
                    heroArchetype.SetStat(StatType.Damage, 8);
                    heroArchetype.SetStat(StatType.ArmorResistance, 12);
                    heroArchetype.SetStat(StatType.CritChance, 10);
                    heroArchetype.SetStat(StatType.CritPower, 1.5f);
                    heroArchetype.SetStat(StatType.BlockChance, 40);
                    heroArchetype.SetStat(StatType.EvasionChance, 5);
                    heroArchetype.SetStat(StatType.AccuracyChance, 75);
                    heroArchetype.SetStat(StatType.AttackSpeed, 0.8f);
                    heroArchetype.SetStat(StatType.VisionRadius, 4.5f);
                    break;
            }
        }

        private void ApplyRacialBonuses(HeroArchetypeSO heroArchetype, HeroRace race)
        {
            switch (race)
            {
                case HeroRace.Human:
                    // Універсальність
                    heroArchetype.SetStat(StatType.GoldPerTap, heroArchetype.GetStat(StatType.GoldPerTap) + 2);
                    heroArchetype.SetStat(StatType.CommonItemChance, heroArchetype.GetStat(StatType.CommonItemChance) + 5);
                    break;

                case HeroRace.Dwarf:
                    // Витривалість
                    heroArchetype.SetStat(StatType.ArmorResistance, heroArchetype.GetStat(StatType.ArmorResistance) + 10);
                    heroArchetype.SetStat(StatType.BlockChance, heroArchetype.GetStat(StatType.BlockChance) + 5);
                    break;

                case HeroRace.Elf:
                    // Спритність
                    heroArchetype.SetStat(StatType.EvasionChance, heroArchetype.GetStat(StatType.EvasionChance) + 5);
                    heroArchetype.SetStat(StatType.AccuracyChance, heroArchetype.GetStat(StatType.AccuracyChance) + 10);
                    break;

                case HeroRace.DarkElf:
                    // Магія тіней
                    heroArchetype.SetStat(StatType.MagicPower, heroArchetype.GetStat(StatType.MagicPower) + 10);
                    heroArchetype.SetStat(StatType.CritChance, heroArchetype.GetStat(StatType.CritChance) + 5);
                    break;

                case HeroRace.Troll:
                    // Регенерація
                    heroArchetype.SetStat(StatType.HP, heroArchetype.GetStat(StatType.HP) + 20);
                    heroArchetype.SetStat(StatType.HpRegen, heroArchetype.GetStat(StatType.HpRegen) + 2);
                    break;

                case HeroRace.Goblin:
                    // Жадібність
                    heroArchetype.SetStat(StatType.ThiefChance, heroArchetype.GetStat(StatType.ThiefChance) + 10);
                    heroArchetype.SetStat(StatType.GoldPerTap, heroArchetype.GetStat(StatType.GoldPerTap) + 5);
                    break;
            }
        }

        private void SetCombatStyles(HeroArchetypeSO heroArchetype)
        {
            // Модифікатори для стилів бою
            heroArchetype.AggressiveAttackMod = 1.5f;
            heroArchetype.AggressiveDefenseMod = 0.7f;
            heroArchetype.AggressiveDodgeMod = 0.7f;
            heroArchetype.AggressiveBlockMod = 0.5f;

            heroArchetype.DefensiveAttackMod = 0.7f;
            heroArchetype.DefensiveDefenseMod = 1.5f;
            heroArchetype.DefensiveDodgeMod = 1.3f;
            heroArchetype.DefensiveBlockMod = 1.5f;
        }

        private void AddDefaultActiveSkill(HeroArchetypeSO heroArchetype, HeroClass heroClass)
        {
            string skillId = "";

            switch (heroClass)
            {
                case HeroClass.Warrior:
                    skillId = "shield_block";
                    break;
                case HeroClass.Rogue:
                    skillId = "backstab";
                    break;
                case HeroClass.Ranger:
                    skillId = "aimed_shot";
                    break;
                case HeroClass.Mage:
                    skillId = "teleport";
                    break;
                case HeroClass.Support:
                    skillId = "healing_wave";
                    break;
                case HeroClass.Tank:
                    skillId = "taunt";
                    break;
            }
            if (!string.IsNullOrEmpty(skillId))
            {
                var skillValue = new HeroArchetypeSO.SkillValue
                {
                    Id = skillId,
                    Level = 1
                };

                if (heroArchetype._activeSkills == null)
                    heroArchetype._activeSkills = new System.Collections.Generic.List<HeroArchetypeSO.SkillValue>();

                heroArchetype._activeSkills.Add(skillValue);
            }
        }
        private void AddDefaultPassiveSkill(HeroArchetypeSO heroArchetype, HeroRace race)
        {
            string skillId = "";

            switch (race)
            {
                case HeroRace.Human:
                    skillId = "versatility";
                    break;
                case HeroRace.Dwarf:
                    skillId = "stone_resilience";
                    break;
                case HeroRace.Elf:
                    skillId = "nature_affinity";
                    break;
                case HeroRace.DarkElf:
                    skillId = "shadow_stealth";
                    break;
                case HeroRace.Troll:
                    skillId = "regeneration";
                    break;
                case HeroRace.Goblin:
                    skillId = "cunning_tricks";
                    break;
            }

            if (!string.IsNullOrEmpty(skillId))
            {
                var skillValue = new HeroArchetypeSO.SkillValue
                {
                    Id = skillId,
                    Level = 1
                };

                if (heroArchetype._passiveSkills == null)
                    heroArchetype._passiveSkills = new System.Collections.Generic.List<HeroArchetypeSO.SkillValue>();

                heroArchetype._passiveSkills.Add(skillValue);
            }
        }
    }
}
#endif
