// Assets/_MythHunter/Code/Entities/Heroes/HeroArchetypeRegistry.cs
using System.Collections.Generic;
using System;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Entities.Archetypes
{
    // Утиліта для завантаження всіх архетипів героїв зі ScriptableObjects
    public class HeroArchetypeRegistry : IHeroArchetypeRegistry
    {
        private readonly IArchetypeTemplateRegistry _templateRegistry;
        private readonly IMythLogger _logger;
        private readonly Dictionary<string, HeroArchetypeSO> _heroArchetypes = new Dictionary<string, HeroArchetypeSO>();

        
        [Inject]
        public HeroArchetypeRegistry(IArchetypeTemplateRegistry templateRegistry, IMythLogger logger)
        {
            _templateRegistry = templateRegistry;
            _logger = logger;

            LoadAllHeroArchetypes();
        }

        // Assets/_MythHunter/Code/Entities/Heroes/HeroArchetypeRegistry.cs
        private void LoadAllHeroArchetypes()
        {
            // Завантаження архетипів
            HeroArchetypeSO[] heroArchetypes = UnityEngine.Resources.LoadAll<HeroArchetypeSO>("ScriptableObjects/Heroes");

            foreach (var archetype in heroArchetypes)
            {
                if (string.IsNullOrEmpty(archetype.ArchetypeId))
                {
                    _logger.LogWarning($"Hero archetype {archetype.name} has empty ArchetypeId. Skipping.", "HeroArchetype");
                    continue;
                }

                // Перевірка на дублікати
                if (_heroArchetypes.ContainsKey(archetype.ArchetypeId))
                {
                    _logger.LogError($"Duplicate hero archetype ID found: {archetype.ArchetypeId}. This will cause conflicts.", "HeroArchetype");
                    continue; // Пропускаємо дублікати
                }

                // Зберігаємо тільки посилання на архетип
                _heroArchetypes[archetype.ArchetypeId] = archetype;

                // Реєструємо архетип в системі
                RegisterArchetypeInSystem(archetype);

                _logger.LogInfo($"Registered hero archetype: {archetype.ArchetypeId} - {archetype.HeroName}", "HeroArchetype");
            }

            _logger.LogInfo($"Loaded {_heroArchetypes.Count} hero archetypes", "HeroArchetype");
        }
        private void RegisterArchetypeInSystem(HeroArchetypeSO archetype)
        {
            if (archetype == null || string.IsNullOrEmpty(archetype.ArchetypeId))
            {
                _logger.LogError("Спроба зареєструвати null або некоректний архетип героя", "HeroArchetype");
                return;
            }

            try
            {
                // Створення компонента ідентифікації героя
                var identityComponent = new Components.Character.HeroIdentityComponent
                {
                    HeroID = archetype.ArchetypeId,
                    Name = archetype.HeroName,
                    Race = archetype.Race.ToString(),
                    Image = archetype.IconPath,
                    Skins = archetype.Skins != null ? new List<string>(archetype.Skins) : new List<string>()
                };

                // Створення компонента соціальних навичок
                var socialComponent = new Components.Character.SocialSkillsComponent
                {
                    Religion = archetype.Religion,
                    Ideology = archetype.Ideology,
                    Class = archetype.Class.ToString(),
                    Professions = archetype.GetProfessions()
                };

                // Створення компонента характеристик
                var statsComponent = new Components.Character.StatsComponent();
                statsComponent.Values = new Dictionary<Components.Character.StatType, float>();

                // Заповнення значень характеристик
                foreach (var stat in archetype.GetAllStats())
                {
                    statsComponent.SetStat(stat.Type, stat.Value);
                }

                // Якщо характеристики не визначені, встановлюємо стандартні
                if (statsComponent.Values.Count == 0)
                {
                    statsComponent.SetStat(Components.Character.StatType.Level, 1);
                    statsComponent.SetStat(Components.Character.StatType.HP, 100);
                    statsComponent.SetStat(Components.Character.StatType.Stamina, 100);
                    statsComponent.SetStat(Components.Character.StatType.Damage, 10);
                    statsComponent.SetStat(Components.Character.StatType.ArmorResistance, 10);
                    statsComponent.SetStat(Components.Character.StatType.ManaCost, archetype.ManaCost);
                }

                // Створення компонента навичок
                var skillsComponent = new Components.Character.SkillsComponent
                {
                    PassiveSkills = ConvertSkillValues(archetype.GetPassiveSkills()),
                    ActiveSkills = ConvertSkillValues(archetype.GetActiveSkills())
                };

                // Створення компонента інвентаря з базовими значеннями
                var inventoryComponent = new Components.Character.InventoryComponent
                {
                    Items = new List<string>(),
                    EquippedItems = new Components.Character.EquippedItems
                    {
                        Armor = new Components.Character.Armor(),
                        Weapons = new Components.Character.Weapons()
                    },
                    Bag = new Components.Character.Bag
                    {
                        Slots = (int)statsComponent.GetStat(Components.Character.StatType.BagSlots, 2),
                        Elixirs = new List<string>()
                    }
                };

                // Реєструємо шаблон героя з усіма компонентами
                _templateRegistry.RegisterArchetypeTemplate(archetype.ArchetypeId)
                    .WithComponent(identityComponent)
                    .WithComponent(socialComponent)
                    .WithComponent(statsComponent)
                    .WithComponent(skillsComponent)
                    .WithComponent(inventoryComponent)
                    .WithComponent(new Components.Combat.TeamComponent
                    {
                        TeamId = archetype.TeamId,
                        IsNeutral = false
                    })
                    .WithComponent(new Components.Movement.VisibilityComponent
                    {
                        VisionRadius = statsComponent.GetStat(Components.Character.StatType.VisionRadius, 5),
                        VisionAngle = statsComponent.GetStat(Components.Character.StatType.VisionAngle, 120),
                        IsVisible = true,
                        LookDirection = Vector3.forward
                    })
                    .WithComponent(new Components.Combat.CombatStyleComponent
                    {
                        CurrentStance = Components.Combat.CombatStance.Balanced,
                        DefaultStance = Components.Combat.CombatStance.Balanced,
                        UseAutoStanceChange = false,
                        AggressiveAttackMod = archetype.AggressiveAttackMod,
                        AggressiveDefenseMod = archetype.AggressiveDefenseMod,
                        AggressiveDodgeMod = archetype.AggressiveDodgeMod,
                        AggressiveBlockMod = archetype.AggressiveBlockMod,
                        DefensiveAttackMod = archetype.DefensiveAttackMod,
                        DefensiveDefenseMod = archetype.DefensiveDefenseMod,
                        DefensiveDodgeMod = archetype.DefensiveDodgeMod,
                        DefensiveBlockMod = archetype.DefensiveBlockMod,
                        LastStanceChangeTime = 0
                    })
                    .WithComponent(new Components.Combat.HealthComponent
                    {
                        CurrentHealth = statsComponent.GetStat(Components.Character.StatType.HP, 100),
                        MaxHealth = statsComponent.GetStat(Components.Character.StatType.HP, 100),
                        RegenRate = statsComponent.GetStat(Components.Character.StatType.HpRegen, 1),
                        IsInvulnerable = false,
                        IsDead = false,
                        LastDamageTime = 0,
                        LastRegenTime = 0
                    })
                    .WithComponent(new Components.Movement.MovementComponent
                    {
                        MoveSpeed = 5f, // Базова швидкість руху
                        RotationSpeed = 180f, // Швидкість повороту в градусах за секунду
                        Direction = Vector3.forward,
                        IsMoving = false,
                        MovementPoints = statsComponent.GetStat(Components.Character.StatType.Stamina, 100),
                        MaxMovementPoints = statsComponent.GetStat(Components.Character.StatType.Stamina, 100)
                    })
                    .Build();

                _logger.LogInfo($"Успішно зареєстровано архетип героя: {archetype.ArchetypeId} - {archetype.HeroName}", "HeroArchetype");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Помилка при реєстрації архетипу героя {archetype.ArchetypeId}: {ex.Message}", "HeroArchetype", ex);
            }
        }

        // Допоміжний метод для конвертації навичок
        private List<Components.Character.Skill> ConvertSkillValues(HeroArchetypeSO.SkillValue[] skillValues)
        {
            var result = new List<Components.Character.Skill>();

            if (skillValues != null)
            {
                foreach (var skillValue in skillValues)
                {
                    result.Add(new Components.Character.Skill
                    {
                        Id = skillValue.Id,
                        Level = skillValue.Level
                    });
                }
            }

            return result;
        }
        public IReadOnlyDictionary<string, HeroArchetypeSO> GetAllHeroArchetypes()
        {
            return _heroArchetypes;
        }

        public HeroArchetypeSO GetHeroArchetype(string archetypeId)
        {
            if (_heroArchetypes.TryGetValue(archetypeId, out var archetype))
            {
                return archetype;
            }

            _logger.LogWarning($"Hero archetype not found: {archetypeId}", "HeroArchetype");
            return null;
        }

    }

    public interface IHeroArchetypeRegistry
    {
        IReadOnlyDictionary<string, HeroArchetypeSO> GetAllHeroArchetypes();
        HeroArchetypeSO GetHeroArchetype(string archetypeId);
    }
}
