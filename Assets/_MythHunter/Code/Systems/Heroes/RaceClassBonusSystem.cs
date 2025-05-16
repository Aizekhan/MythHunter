// Assets/_MythHunter/Code/Systems/Heroes/RaceClassBonusSystem.cs
using System.Collections.Generic;
using MythHunter.Core.DI;
using MythHunter.Core.ECS;
using MythHunter.Components.Character;
using MythHunter.Entities.Heroes;
using MythHunter.Utils.Logging;
using MythHunter.Events;
using MythHunter.Events.Domain.Gameplay;

namespace MythHunter.Systems.Heroes
{
    /// <summary>
    /// Система для застосування бонусів від раси та класу героя
    /// </summary>
    public class RaceClassBonusSystem : SystemBase
    {
        private readonly IEntityManager _entityManager;

        // Словник расових бонусів
        private readonly Dictionary<HeroRace, RacialBonuses> _racialBonuses;

        // Словник класових бонусів
        private readonly Dictionary<HeroClass, ClassBonuses> _classBonuses;

        [Inject]
        public RaceClassBonusSystem(
            IEntityManager entityManager,
            IEventBus eventBus,
            IMythLogger logger)
            : base(logger, eventBus)
        {
            _entityManager = entityManager;

            // Ініціалізація расових бонусів
            _racialBonuses = new Dictionary<HeroRace, RacialBonuses>
            {
                { HeroRace.Human, new RacialBonuses
                    {
                        // Люди універсальні - бонус до досвіду та дипломатії
                        StatBonuses = new Dictionary<StatType, float>
                        {
                            { StatType.GoldPerTap, 2 }, // +2 до золота за клік
                            { StatType.CommonItemChance, 5 } // +5% до шансу звичайних предметів
                        },
                        PassiveAbilityId = "racial_human_adaptability"
                    }
                },
                { HeroRace.Dwarf, new RacialBonuses
                    {
                        // Дворфи витривалі - стійкіші, сильніші
                        StatBonuses = new Dictionary<StatType, float>
                        {
                            { StatType.ArmorResistance, 10 }, // +10 до захисту
                            { StatType.BlockChance, 5 } // +5% до шансу блоку
                        },
                        PassiveAbilityId = "racial_dwarf_resilience"
                    }
                },
                { HeroRace.Elf, new RacialBonuses
                    {
                        // Ельфи спритні - швидші, точніші
                        StatBonuses = new Dictionary<StatType, float>
                        {
                            { StatType.EvasionChance, 5 }, // +5% до шансу ухилення
                            { StatType.AccuracyChance, 10 } // +10% до точності
                        },
                        PassiveAbilityId = "racial_elf_agility"
                    }
                },
                { HeroRace.DarkElf, new RacialBonuses
                    {
                        // Темні ельфи - підступні та магічні
                        StatBonuses = new Dictionary<StatType, float>
                        {
                            { StatType.MagicPower, 10 }, // +10 до сили магії
                            { StatType.CritChance, 5 } // +5% до шансу критичного удару
                        },
                        PassiveAbilityId = "racial_darkelf_shadow_magic"
                    }
                },
                { HeroRace.Troll, new RacialBonuses
                    {
                        // Тролі - живучі та регенеруючі
                        StatBonuses = new Dictionary<StatType, float>
                        {
                            { StatType.HP, 20 }, // +20 до здоров'я
                            { StatType.HpRegen, 2 } // +2 до регенерації здоров'я
                        },
                        PassiveAbilityId = "racial_troll_regeneration"
                    }
                },
                { HeroRace.Goblin, new RacialBonuses
                    {
                        // Гобліни - спритні та жадібні
                        StatBonuses = new Dictionary<StatType, float>
                        {
                            { StatType.ThiefChance, 10 }, // +10% до шансу крадіжки
                            { StatType.GoldPerTap, 5 } // +5 до золота за клік
                        },
                        PassiveAbilityId = "racial_goblin_greed"
                    }
                }
            };

            // Ініціалізація класових бонусів
            _classBonuses = new Dictionary<HeroClass, ClassBonuses>
            {
                { HeroClass.Warrior, new ClassBonuses
                    {
                        // Воїни - сильні в ближньому бою
                        StatBonuses = new Dictionary<StatType, float>
                        {
                            { StatType.Damage, 10 }, // +10 до пошкодження
                            { StatType.BlockChance, 10 } // +10% до шансу блоку
                        },
                        PassiveAbilityId = "class_warrior_strength",
                        ActiveAbilityId = "class_warrior_battle_shout"
                    }
                },
                { HeroClass.Rogue, new ClassBonuses
                    {
                        // Розбійники - швидкі, з шансом критичної атаки
                        StatBonuses = new Dictionary<StatType, float>
                        {
                            { StatType.CritChance, 15 }, // +15% до шансу критичного удару
                            { StatType.EvasionChance, 10 } // +10% до шансу ухилення
                        },
                        PassiveAbilityId = "class_rogue_backstab",
                        ActiveAbilityId = "class_rogue_vanish"
                    }
                },
                { HeroClass.Ranger, new ClassBonuses
                    {
                        // Рейнджери - точні стрільці
                        StatBonuses = new Dictionary<StatType, float>
                        {
                            { StatType.AccuracyChance, 15 }, // +15% до точності
                            { StatType.CritPower, 10 } // +10% до сили критичного удару
                        },
                        PassiveAbilityId = "class_ranger_precision",
                        ActiveAbilityId = "class_ranger_aimed_shot"
                    }
                },
                { HeroClass.Mage, new ClassBonuses
                    {
                        // Маги - магічна сила
                        StatBonuses = new Dictionary<StatType, float>
                        {
                            { StatType.MagicPower, 20 }, // +20 до сили магії
                            { StatType.MagicChance, 10 } // +10% до шансу магії
                        },
                        PassiveAbilityId = "class_mage_arcane_intellect",
                        ActiveAbilityId = "class_mage_fireball"
                    }
                },
                { HeroClass.Support, new ClassBonuses
                    {
                        // Підтримка - допомога команді
                        StatBonuses = new Dictionary<StatType, float>
                        {
                            { StatType.HpRegen, 5 }, // +5 до регенерації здоров'я
                            { StatType.ReturnDamagePower, 15 } // +15% до сили повернення пошкодження
                        },
                        PassiveAbilityId = "class_support_blessing",
                        ActiveAbilityId = "class_support_heal"
                    }
                },
                { HeroClass.Tank, new ClassBonuses
                    {
                        // Танки - стійкість і захист
                        StatBonuses = new Dictionary<StatType, float>
                        {
                            { StatType.HP, 50 }, // +50 до здоров'я
                            { StatType.ArmorResistance, 20 } // +20 до захисту
                        },
                        PassiveAbilityId = "class_tank_resilience",
                        ActiveAbilityId = "class_tank_taunt"
                    }
                }
            };
        }

        public override void Initialize()
        {
            base.Initialize();
            SubscribeToEvents();
        }

        protected override void OnSubscribeToEvents()
        {
            Subscribe<HeroCreatedEvent>(OnHeroCreated);
        }

        protected override void OnUnsubscribeFromEvents()
        {
            Unsubscribe<HeroCreatedEvent>(OnHeroCreated);
        }

        private void OnHeroCreated(HeroCreatedEvent evt)
        {
            ApplyRacialBonuses(evt.EntityId, evt.Race);
            ApplyClassBonuses(evt.EntityId, evt.Class);

            _logger.LogInfo($"Applied racial and class bonuses to hero {evt.HeroName} (ID: {evt.EntityId})", "RaceClassBonus");
        }

        /// <summary>
        /// Застосовує расові бонуси до героя
        /// </summary>
        private void ApplyRacialBonuses(int entityId, HeroRace race)
        {
            if (!_racialBonuses.TryGetValue(race, out var bonuses))
                return;

            // Додаємо бонуси до характеристик
            if (_entityManager.HasComponent<StatsComponent>(entityId))
            {
                var statsComponent = _entityManager.GetComponent<StatsComponent>(entityId);

                foreach (var bonus in bonuses.StatBonuses)
                {
                    // Додаємо бонус до існуючої характеристики
                    float currentValue = statsComponent.GetStat(bonus.Key);
                    statsComponent.SetStat(bonus.Key, currentValue + bonus.Value);
                }

                _entityManager.AddComponent(entityId, statsComponent);
            }

            // Додаємо пасивну здібність
            if (!string.IsNullOrEmpty(bonuses.PassiveAbilityId) &&
                _entityManager.HasComponent<SkillsComponent>(entityId))
            {
                var skillsComponent = _entityManager.GetComponent<SkillsComponent>(entityId);

                // Перевіряємо, чи вже є ця здібність
                bool hasAbility = false;
                if (skillsComponent.PassiveSkills != null)
                {
                    foreach (var skill in skillsComponent.PassiveSkills)
                    {
                        if (skill.Id == bonuses.PassiveAbilityId)
                        {
                            hasAbility = true;
                            break;
                        }
                    }
                }

                // Додаємо нову здібність, якщо вона відсутня
                if (!hasAbility)
                {
                    if (skillsComponent.PassiveSkills == null)
                        skillsComponent.PassiveSkills = new List<Skill>();

                    skillsComponent.PassiveSkills.Add(new Skill
                    {
                        Id = bonuses.PassiveAbilityId,
                        Level = 1
                    });

                    _entityManager.AddComponent(entityId, skillsComponent);
                }
            }
        }

        /// <summary>
        /// Застосовує класові бонуси до героя
        /// </summary>
        private void ApplyClassBonuses(int entityId, HeroClass heroClass)
        {
            if (!_classBonuses.TryGetValue(heroClass, out var bonuses))
                return;

            // Додаємо бонуси до характеристик
            if (_entityManager.HasComponent<StatsComponent>(entityId))
            {
                var statsComponent = _entityManager.GetComponent<StatsComponent>(entityId);

                foreach (var bonus in bonuses.StatBonuses)
                {
                    // Додаємо бонус до існуючої характеристики
                    float currentValue = statsComponent.GetStat(bonus.Key);
                    statsComponent.SetStat(bonus.Key, currentValue + bonus.Value);
                }

                _entityManager.AddComponent(entityId, statsComponent);
            }

            // Додаємо пасивну та активну здібності
            if (_entityManager.HasComponent<SkillsComponent>(entityId))
            {
                var skillsComponent = _entityManager.GetComponent<SkillsComponent>(entityId);

                // Додаємо пасивну здібність
                if (!string.IsNullOrEmpty(bonuses.PassiveAbilityId))
                {
                    bool hasPassive = false;
                    if (skillsComponent.PassiveSkills != null)
                    {
                        foreach (var skill in skillsComponent.PassiveSkills)
                        {
                            if (skill.Id == bonuses.PassiveAbilityId)
                            {
                                hasPassive = true;
                                break;
                            }
                        }
                    }

                    if (!hasPassive)
                    {
                        if (skillsComponent.PassiveSkills == null)
                            skillsComponent.PassiveSkills = new List<Skill>();

                        skillsComponent.PassiveSkills.Add(new Skill
                        {
                            Id = bonuses.PassiveAbilityId,
                            Level = 1
                        });
                    }
                }

                // Додаємо активну здібність
                if (!string.IsNullOrEmpty(bonuses.ActiveAbilityId))
                {
                    bool hasActive = false;
                    if (skillsComponent.ActiveSkills != null)
                    {
                        foreach (var skill in skillsComponent.ActiveSkills)
                        {
                            if (skill.Id == bonuses.ActiveAbilityId)
                            {
                                hasActive = true;
                                break;
                            }
                        }
                    }

                    if (!hasActive)
                    {
                        if (skillsComponent.ActiveSkills == null)
                            skillsComponent.ActiveSkills = new List<Skill>();

                        skillsComponent.ActiveSkills.Add(new Skill
                        {
                            Id = bonuses.ActiveAbilityId,
                            Level = 1
                        });
                    }
                }

                _entityManager.AddComponent(entityId, skillsComponent);
            }
        }

        /// <summary>
        /// Отримує інформацію про расові бонуси
        /// </summary>
        public RacialBonuses GetRacialBonuses(HeroRace race)
        {
            return _racialBonuses.TryGetValue(race, out var bonuses) ? bonuses : null;
        }

        /// <summary>
        /// Отримує інформацію про класові бонуси
        /// </summary>
        public ClassBonuses GetClassBonuses(HeroClass heroClass)
        {
            return _classBonuses.TryGetValue(heroClass, out var bonuses) ? bonuses : null;
        }
    }

    /// <summary>
    /// Бонуси від раси героя
    /// </summary>
    public class RacialBonuses
    {
        /// <summary>
        /// Бонуси до характеристик
        /// </summary>
        public Dictionary<StatType, float> StatBonuses { get; set; } = new Dictionary<StatType, float>();

        /// <summary>
        /// Ідентифікатор пасивної здібності раси
        /// </summary>
        public string PassiveAbilityId
        {
            get; set;
        }
    }

    /// <summary>
    /// Бонуси від класу героя
    /// </summary>
    public class ClassBonuses
    {
        /// <summary>
        /// Бонуси до характеристик
        /// </summary>
        public Dictionary<StatType, float> StatBonuses { get; set; } = new Dictionary<StatType, float>();

        /// <summary>
        /// Ідентифікатор пасивної здібності класу
        /// </summary>
        public string PassiveAbilityId
        {
            get; set;
        }

        /// <summary>
        /// Ідентифікатор активної здібності класу
        /// </summary>
        public string ActiveAbilityId
        {
            get; set;
        }
    }
}
