// Шлях: Assets/_MythHunter/Code/Editor/Heroes/CreateHeroArchetypeWizard.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using MythHunter.Entities.Archetypes;
using MythHunter.Components.Character;
using MythHunter.Entities.Heroes;

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
            DisplayWizard<CreateHeroArchetypeWizard>("Створення архетипу героя", "Створити");
        }

        private void OnWizardCreate()
        {
            var heroArchetype = ScriptableObject.CreateInstance<HeroArchetypeSO>();

            heroArchetype.ArchetypeId = $"Hero_{race}_{heroClass}_{System.Guid.NewGuid().ToString().Substring(0, 8)}";
            heroArchetype.HeroName = heroName;
            heroArchetype.Race = race;
            heroArchetype.Class = heroClass;

            if (baseTemplate != null)
            {
                heroArchetype.Description = baseTemplate.Description;
                heroArchetype.IconPath = baseTemplate.IconPath;
                heroArchetype.Skins = baseTemplate.Skins;
                heroArchetype.TeamId = baseTemplate.TeamId;
                heroArchetype.Religion = baseTemplate.Religion;
                heroArchetype.Ideology = baseTemplate.Ideology;
                heroArchetype.SetProfessions(baseTemplate.GetProfessions());

                foreach (var stat in baseTemplate.GetAllStats())
                {
                    heroArchetype.SetStat(stat.Type, stat.Value);
                }

                foreach (var skill in baseTemplate.GetActiveSkills())
                {
                    heroArchetype.AddActiveSkill(skill.Id, skill.Level);
                }

                foreach (var skill in baseTemplate.GetPassiveSkills())
                {
                    heroArchetype.AddPassiveSkill(skill.Id, skill.Level);
                }
            }
            else
            {
                SetDefaultStats(heroArchetype);
            }

            string path = "Assets/Resources/ScriptableObjects/Heroes";
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/ScriptableObjects"))
                AssetDatabase.CreateFolder("Assets/Resources", "ScriptableObjects");
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder("Assets/Resources/ScriptableObjects", "Heroes");

            string assetPath = $"{path}/{heroArchetype.ArchetypeId}.asset";
            AssetDatabase.CreateAsset(heroArchetype, assetPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = heroArchetype;
        }

        private void SetDefaultStats(HeroArchetypeSO hero)
        {
            hero.Description = GetDescription(heroClass);
            hero.TeamId = 0;

            hero.SetStat(StatType.Level, 1);
            hero.SetStat(StatType.HP, 100);
            hero.SetStat(StatType.Stamina, 100);
            hero.SetStat(StatType.Damage, 10);
            hero.SetStat(StatType.ArmorResistance, 10);
            hero.SetStat(StatType.CritChance, 20);
            hero.SetStat(StatType.CritPower, 2);
            hero.SetStat(StatType.BlockChance, 10);
            hero.SetStat(StatType.AttackSpeed, 1f);
            hero.SetStat(StatType.VisionRadius, 5);

            hero.AddActiveSkill(GetDefaultActiveSkill(heroClass));
            hero.AddPassiveSkill(GetDefaultPassiveSkill(race));
        }

        private string GetDescription(HeroClass heroClass)
        {
            return heroClass switch
            {
                HeroClass.Warrior => "Могутній воїн ближнього бою",
                HeroClass.Rogue => "Спритний розбійник",
                HeroClass.Ranger => "Лучник на дистанції",
                HeroClass.Mage => "Маг із потужною атакою",
                HeroClass.Support => "Підтримка команди",
                HeroClass.Tank => "Захисник із високим HP",
                _ => "Герой без опису"
            };
        }

        private string GetDefaultActiveSkill(HeroClass heroClass)
        {
            return heroClass switch
            {
                HeroClass.Warrior => "shield_block",
                HeroClass.Rogue => "backstab",
                HeroClass.Ranger => "aimed_shot",
                HeroClass.Mage => "teleport",
                HeroClass.Support => "healing_wave",
                HeroClass.Tank => "taunt",
                _ => ""
            };
        }

        private string GetDefaultPassiveSkill(HeroRace race)
        {
            return race switch
            {
                HeroRace.Human => "versatility",
                HeroRace.Dwarf => "stone_resilience",
                HeroRace.Elf => "nature_affinity",
                HeroRace.DarkElf => "shadow_stealth",
                HeroRace.Troll => "regeneration",
                HeroRace.Goblin => "cunning_tricks",
                _ => ""
            };
        }
    }
}
#endif
