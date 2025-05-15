// Assets/_MythHunter/Code/Entities/Archetypes/HeroArchetypeSO.cs
using UnityEngine;
using System.Collections.Generic;
using MythHunter.Components.Core;
using MythHunter.Components.Combat;
using MythHunter.Components.Movement;

namespace MythHunter.Entities.Archetypes
{
    [CreateAssetMenu(fileName = "HeroArchetype", menuName = "MythHunter/Heroes/Hero Archetype")]
    public class HeroArchetypeSO : ScriptableObject
    {
        [Header("Ідентифікація")]
        public string ArchetypeId;
        public string HeroName;
        public string Description;

        [Header("Расові характеристики")]
        public HeroRace Race;

        [Header("Класові характеристики")]
        public HeroClass Class;

        [Header("Базові характеристики")]
        public float BaseHealth;
        public float BaseAttack;
        public float BaseDefense;
        public float BaseMoveSpeed;
        public float BaseMovementPoints;

        [Header("Бойові характеристики")]
        public float CriticalChance;
        public float CriticalMultiplier;
        public float DodgeChance;
        public float BlockChance;
        public float AttackSpeed;

        [Header("Ресурси")]
        public float MaxRage;
        public float MaxConcentration;

        [Header("Спеціальні здібності")]
        public string ActiveAbilityId;
        public List<string> PassiveAbilityIds = new List<string>();

        [Header("Стилі бою")]
        [Range(0.5f, 2f)] public float AggressiveAttackMod = 1.5f;
        [Range(0.5f, 2f)] public float AggressiveDefenseMod = 0.7f;
        [Range(0.5f, 2f)] public float AggressiveDodgeMod = 0.7f;
        [Range(0.5f, 2f)] public float AggressiveBlockMod = 0.5f;

        [Range(0.5f, 2f)] public float DefensiveAttackMod = 0.7f;
        [Range(0.5f, 2f)] public float DefensiveDefenseMod = 1.5f;
        [Range(0.5f, 2f)] public float DefensiveDodgeMod = 1.3f;
        [Range(0.5f, 2f)] public float DefensiveBlockMod = 1.5f;

        [Header("Поле зору")]
        public float VisionRadius = 5f;
        public float VisionAngle = 120f;

        [Header("Команда")]
        public int TeamId = 0;

        public void RegisterWithArchetypeSystem(ArchetypeTemplateRegistry registry)
        {
            // Створення шаблону героя
            registry.RegisterArchetypeTemplate(ArchetypeId)
                .WithComponent(new NameComponent { Name = HeroName })
                .WithComponent(new DescriptionComponent { Description = Description })
                .WithComponent(new HealthComponent
                {
                    CurrentHealth = BaseHealth,
                    MaxHealth = BaseHealth,
                    RegenRate = 1f
                })
                .WithComponent(new CombatStatsComponent
                {
                    AttackPower = BaseAttack,
                    Defense = BaseDefense,
                    CriticalChance = CriticalChance,
                    CriticalMultiplier = CriticalMultiplier,
                    DodgeChance = DodgeChance,
                    BlockChance = BlockChance,
                    AttackSpeed = AttackSpeed,
                    Rage = 0,
                    MaxRage = MaxRage,
                    Concentration = MaxConcentration,
                    MaxConcentration = MaxConcentration,
                    IsInCombat = false
                })
                .WithComponent(new MovementComponent
                {
                    MoveSpeed = BaseMoveSpeed,
                    MovementPoints = BaseMovementPoints,
                    MaxMovementPoints = BaseMovementPoints,
                    IsMoving = false
                })
                .WithComponent(new VisibilityComponent
                {
                    VisionRadius = VisionRadius,
                    VisionAngle = VisionAngle,
                    IsVisible = true
                })
                .WithComponent(new TeamComponent
                {
                    TeamId = TeamId,
                    IsNeutral = false
                })
                .WithComponent(new CombatStyleComponent
                {
                    CurrentStance = CombatStance.Balanced,
                    DefaultStance = CombatStance.Balanced,
                    AggressiveAttackMod = AggressiveAttackMod,
                    AggressiveDefenseMod = AggressiveDefenseMod,
                    AggressiveDodgeMod = AggressiveDodgeMod,
                    AggressiveBlockMod = AggressiveBlockMod,
                    DefensiveAttackMod = DefensiveAttackMod,
                    DefensiveDefenseMod = DefensiveDefenseMod,
                    DefensiveDodgeMod = DefensiveDodgeMod,
                    DefensiveBlockMod = DefensiveBlockMod
                })
                .WithComponent(new CombatAbilityComponent
                {
                    ActiveAbilityId = ActiveAbilityId,
                    PassiveAbilityIds = PassiveAbilityIds.ToArray(),
                    IsActiveAbilityUsed = false
                });
        }
    }

    public enum HeroRace
    {
        Human,
        Dwarf,
        Elf,
        DarkElf,
        Troll,
        Goblin
    }

    public enum HeroClass
    {
        Warrior,
        Rogue,
        Ranger,
        Mage,
        Support,
        Tank
    }
}
