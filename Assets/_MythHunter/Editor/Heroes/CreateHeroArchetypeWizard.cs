// Assets/_MythHunter/Code/Editor/Heroes/CreateHeroArchetypeWizard.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using MythHunter.Entities.Archetypes;

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
                // Копіюємо значення з шаблону
                heroArchetype.Description = baseTemplate.Description;
                heroArchetype.BaseHealth = baseTemplate.BaseHealth;
                heroArchetype.BaseAttack = baseTemplate.BaseAttack;
                heroArchetype.BaseDefense = baseTemplate.BaseDefense;
                heroArchetype.BaseMoveSpeed = baseTemplate.BaseMoveSpeed;
                heroArchetype.BaseMovementPoints = baseTemplate.BaseMovementPoints;
                heroArchetype.CriticalChance = baseTemplate.CriticalChance;
                heroArchetype.CriticalMultiplier = baseTemplate.CriticalMultiplier;
                heroArchetype.DodgeChance = baseTemplate.DodgeChance;
                heroArchetype.BlockChance = baseTemplate.BlockChance;
                heroArchetype.AttackSpeed = baseTemplate.AttackSpeed;
                heroArchetype.MaxRage = baseTemplate.MaxRage;
                heroArchetype.MaxConcentration = baseTemplate.MaxConcentration;
                heroArchetype.ActiveAbilityId = baseTemplate.ActiveAbilityId;
                heroArchetype.PassiveAbilityIds = new System.Collections.Generic.List<string>(baseTemplate.PassiveAbilityIds);
                heroArchetype.AggressiveAttackMod = baseTemplate.AggressiveAttackMod;
                heroArchetype.AggressiveDefenseMod = baseTemplate.AggressiveDefenseMod;
                heroArchetype.AggressiveDodgeMod = baseTemplate.AggressiveDodgeMod;
                heroArchetype.AggressiveBlockMod = baseTemplate.AggressiveBlockMod;
                heroArchetype.DefensiveAttackMod = baseTemplate.DefensiveAttackMod;
                heroArchetype.DefensiveDefenseMod = baseTemplate.DefensiveDefenseMod;
                heroArchetype.DefensiveDodgeMod = baseTemplate.DefensiveDodgeMod;
                heroArchetype.DefensiveBlockMod = baseTemplate.DefensiveBlockMod;
                heroArchetype.VisionRadius = baseTemplate.VisionRadius;
                heroArchetype.VisionAngle = baseTemplate.VisionAngle;
                heroArchetype.TeamId = baseTemplate.TeamId;
            }
            // Assets/_MythHunter/Code/Editor/Heroes/CreateHeroArchetypeWizard.cs (продовження)
            else
            {
                // Стандартні значення залежно від класу
                switch (heroClass)
                {
                    case HeroClass.Warrior:
                        heroArchetype.Description = "Могутній воїн, що спеціалізується на ближньому бою";
                        heroArchetype.BaseHealth = 120f;
                        heroArchetype.BaseAttack = 10f;
                        heroArchetype.BaseDefense = 8f;
                        heroArchetype.BaseMoveSpeed = 4f;
                        heroArchetype.BaseMovementPoints = 100f;
                        heroArchetype.CriticalChance = 0.2f;
                        heroArchetype.CriticalMultiplier = 2f;
                        heroArchetype.DodgeChance = 0.1f;
                        heroArchetype.BlockChance = 0.3f;
                        heroArchetype.AttackSpeed = 1.0f;
                        heroArchetype.MaxRage = 100f;
                        heroArchetype.MaxConcentration = 100f;
                        heroArchetype.ActiveAbilityId = "shield_block";
                        heroArchetype.VisionRadius = 5f;
                        heroArchetype.VisionAngle = 120f;
                        break;

                    case HeroClass.Rogue:
                        heroArchetype.Description = "Спритний розбійник, що спеціалізується на несподіваних атаках";
                        heroArchetype.BaseHealth = 90f;
                        heroArchetype.BaseAttack = 12f;
                        heroArchetype.BaseDefense = 5f;
                        heroArchetype.BaseMoveSpeed = 5f;
                        heroArchetype.BaseMovementPoints = 120f;
                        heroArchetype.CriticalChance = 0.4f;
                        heroArchetype.CriticalMultiplier = 2.5f;
                        heroArchetype.DodgeChance = 0.3f;
                        heroArchetype.BlockChance = 0.1f;
                        heroArchetype.AttackSpeed = 1.2f;
                        heroArchetype.MaxRage = 80f;
                        heroArchetype.MaxConcentration = 120f;
                        heroArchetype.ActiveAbilityId = "backstab";
                        heroArchetype.VisionRadius = 6f;
                        heroArchetype.VisionAngle = 100f;
                        break;

                    case HeroClass.Ranger:
                        heroArchetype.Description = "Майстерний лучник, що спеціалізується на дальніх атаках";
                        heroArchetype.BaseHealth = 85f;
                        heroArchetype.BaseAttack = 11f;
                        heroArchetype.BaseDefense = 4f;
                        heroArchetype.BaseMoveSpeed = 4.5f;
                        heroArchetype.BaseMovementPoints = 110f;
                        heroArchetype.CriticalChance = 0.3f;
                        heroArchetype.CriticalMultiplier = 2.2f;
                        heroArchetype.DodgeChance = 0.2f;
                        heroArchetype.BlockChance = 0.1f;
                        heroArchetype.AttackSpeed = 1.5f;
                        heroArchetype.MaxRage = 90f;
                        heroArchetype.MaxConcentration = 110f;
                        heroArchetype.ActiveAbilityId = "aimed_shot";
                        heroArchetype.VisionRadius = 7f;
                        heroArchetype.VisionAngle = 140f;
                        break;

                    case HeroClass.Mage:
                        heroArchetype.Description = "Могутній заклинач з величезним магічним потенціалом";
                        heroArchetype.BaseHealth = 75f;
                        heroArchetype.BaseAttack = 15f;
                        heroArchetype.BaseDefense = 3f;
                        heroArchetype.BaseMoveSpeed = 3.5f;
                        heroArchetype.BaseMovementPoints = 90f;
                        heroArchetype.CriticalChance = 0.25f;
                        heroArchetype.CriticalMultiplier = 3f;
                        heroArchetype.DodgeChance = 0.15f;
                        heroArchetype.BlockChance = 0.05f;
                        heroArchetype.AttackSpeed = 0.8f;
                        heroArchetype.MaxRage = 70f;
                        heroArchetype.MaxConcentration = 150f;
                        heroArchetype.ActiveAbilityId = "teleport";
                        heroArchetype.VisionRadius = 6f;
                        heroArchetype.VisionAngle = 130f;
                        break;

                    case HeroClass.Support:
                        heroArchetype.Description = "Цілитель та підтримка, що допомагає союзникам у бою";
                        heroArchetype.BaseHealth = 95f;
                        heroArchetype.BaseAttack = 7f;
                        heroArchetype.BaseDefense = 6f;
                        heroArchetype.BaseMoveSpeed = 4f;
                        heroArchetype.BaseMovementPoints = 100f;
                        heroArchetype.CriticalChance = 0.15f;
                        heroArchetype.CriticalMultiplier = 1.8f;
                        heroArchetype.DodgeChance = 0.15f;
                        heroArchetype.BlockChance = 0.15f;
                        heroArchetype.AttackSpeed = 1.0f;
                        heroArchetype.MaxRage = 80f;
                        heroArchetype.MaxConcentration = 130f;
                        heroArchetype.ActiveAbilityId = "healing_wave";
                        heroArchetype.VisionRadius = 5.5f;
                        heroArchetype.VisionAngle = 135f;
                        break;

                    case HeroClass.Tank:
                        heroArchetype.Description = "Витривалий захисник, що здатен витримати багато ударів";
                        heroArchetype.BaseHealth = 150f;
                        heroArchetype.BaseAttack = 8f;
                        heroArchetype.BaseDefense = 12f;
                        heroArchetype.BaseMoveSpeed = 3.5f;
                        heroArchetype.BaseMovementPoints = 90f;
                        heroArchetype.CriticalChance = 0.1f;
                        heroArchetype.CriticalMultiplier = 1.5f;
                        heroArchetype.DodgeChance = 0.05f;
                        heroArchetype.BlockChance = 0.4f;
                        heroArchetype.AttackSpeed = 0.8f;
                        heroArchetype.MaxRage = 120f;
                        heroArchetype.MaxConcentration = 150f;
                        heroArchetype.ActiveAbilityId = "taunt";
                        heroArchetype.VisionRadius = 4.5f;
                        heroArchetype.VisionAngle = 110f;
                        break;
                }

                // Модифікатори для стилів бою
                heroArchetype.AggressiveAttackMod = 1.5f;
                heroArchetype.AggressiveDefenseMod = 0.7f;
                heroArchetype.AggressiveDodgeMod = 0.7f;
                heroArchetype.AggressiveBlockMod = 0.5f;

                heroArchetype.DefensiveAttackMod = 0.7f;
                heroArchetype.DefensiveDefenseMod = 1.5f;
                heroArchetype.DefensiveDodgeMod = 1.3f;
                heroArchetype.DefensiveBlockMod = 1.5f;

                // Пасивні здібності залежно від раси
                heroArchetype.PassiveAbilityIds = new System.Collections.Generic.List<string>();
                switch (race)
                {
                    case HeroRace.Human:
                        heroArchetype.PassiveAbilityIds.Add("versatility");
                        break;
                    case HeroRace.Dwarf:
                        heroArchetype.PassiveAbilityIds.Add("stone_resilience");
                        break;
                    case HeroRace.Elf:
                        heroArchetype.PassiveAbilityIds.Add("nature_affinity");
                        break;
                    case HeroRace.DarkElf:
                        heroArchetype.PassiveAbilityIds.Add("shadow_stealth");
                        break;
                    case HeroRace.Troll:
                        heroArchetype.PassiveAbilityIds.Add("regeneration");
                        break;
                    case HeroRace.Goblin:
                        heroArchetype.PassiveAbilityIds.Add("cunning_tricks");
                        break;
                }

                // За замовчуванням команда 0
                heroArchetype.TeamId = 0;
            }

            // Створюємо директорію, якщо її ще немає
            if (!AssetDatabase.IsValidFolder("Assets/Resources/ScriptableObjects"))
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
    }
}
#endif
