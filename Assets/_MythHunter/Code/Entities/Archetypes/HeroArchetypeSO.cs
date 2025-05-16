// Assets/_MythHunter/Code/Entities/Archetypes/HeroArchetypeSO.cs

using UnityEngine;
using System;
using System.Collections.Generic;
using MythHunter.Components.Core;
using MythHunter.Components.Combat;
using MythHunter.Components.Movement;
using MythHunter.Components.Character;
using MythHunter.Entities.Heroes;
namespace MythHunter.Entities.Archetypes
{
    [CreateAssetMenu(fileName = "HeroArchetype", menuName = "MythHunter/Heroes/Hero Archetype")]
    public class HeroArchetypeSO : ScriptableObject
    {
        [Header("UI")]
        [SerializeField] public string IconPath; // Шлях до іконки героя

        [Header("Ідентифікація")]
        public string ArchetypeId;
        public string HeroName;
        public string Description;

        [Header("Расові та класові характеристики")]
        public HeroRace Race;
        public HeroClass Class;

        [Header("Соціальні навички")]
        public string Religion;
        public string Ideology;
        [SerializeField] private string[] _professions = new string[3];

        [Header("Характеристики")]
        [SerializeField] private StatValue[] _stats = new StatValue[0];

        // Клас для представлення характеристик в інспекторі
        [Serializable]
        public class StatValue
        {
            public StatType Type;
            public float Value;
        }

        [Header("Зображення та скіни")]
        public string[] Skins = new string[0];

        [Header("Навички")]
        [SerializeField] private SkillValue[] _passiveSkills = new SkillValue[0];
        [SerializeField] private SkillValue[] _activeSkills = new SkillValue[0];

        // Клас для представлення навичок в інспекторі
        [Serializable]
        public class SkillValue
        {
            public string Id;
            public int Level;
        }

        [Header("Команда")]
        public int TeamId = 0;

        public void RegisterWithArchetypeSystem(ArchetypeTemplateRegistry registry)
        {
            // Створюємо компонент ідентифікації
            var identityComponent = new HeroIdentityComponent
            {
                HeroID = ArchetypeId,
                Name = HeroName,
                Race = Race.ToString(),
                Image = IconPath,
                Skins = new List<string>(Skins)
            };

            // Створюємо компонент соціальних навичок
            var socialComponent = new SocialSkillsComponent
            {
                Religion = Religion,
                Ideology = Ideology,
                Class = Class.ToString(),
                Professions = _professions
            };

            // Створюємо компонент характеристик
            var statsComponent = new StatsComponent();
            statsComponent.Values = new Dictionary<StatType, float>();

            // Заповнюємо значення характеристик
            foreach (var stat in _stats)
            {
                statsComponent.Values[stat.Type] = stat.Value;
            }

            // Якщо характеристики не визначені, встановлюємо стандартні
            if (_stats == null || _stats.Length == 0)
            {
                statsComponent.SetStat(StatType.Level, 1);
                statsComponent.SetStat(StatType.HP, 100);
                statsComponent.SetStat(StatType.Stamina, 100);
                statsComponent.SetStat(StatType.Damage, 10);
                statsComponent.SetStat(StatType.ArmorResistance, 10);
            }

            // Створюємо компонент навичок
            var skillsComponent = new SkillsComponent
            {
                PassiveSkills = ConvertSkillValues(_passiveSkills),
                ActiveSkills = ConvertSkillValues(_activeSkills)
            };

            // Створюємо компонент інвентаря з базовими значеннями
            var inventoryComponent = new InventoryComponent
            {
                Items = new List<string>(),
                EquippedItems = new EquippedItems(),
                Bag = new Bag { Slots = (int)statsComponent.GetStat(StatType.BagSlots, 2), Elixirs = new List<string>() }
            };

            // Реєструємо шаблон героя з усіма компонентами
            registry.RegisterArchetypeTemplate(ArchetypeId)
                .WithComponent(identityComponent)
                .WithComponent(socialComponent)
                .WithComponent(statsComponent)
                .WithComponent(skillsComponent)
                .WithComponent(inventoryComponent)
                .WithComponent(new TeamComponent
                {
                    TeamId = TeamId,
                    IsNeutral = false
                })
                .WithComponent(new VisibilityComponent
                {
                    VisionRadius = 5, // Значення за замовчуванням
                    VisionAngle = 120,
                    IsVisible = true
                });
        }

        private List<Skill> ConvertSkillValues(SkillValue[] skillValues)
        {
            var result = new List<Skill>();

            if (skillValues != null)
            {
                foreach (var skillValue in skillValues)
                {
                    result.Add(new Skill
                    {
                        Id = skillValue.Id,
                        Level = skillValue.Level
                    });
                }
            }

            return result;
        }

        // Допоміжні методи для редактора
        public void SetStat(StatType type, float value)
        {
            if (_stats == null)
                _stats = new StatValue[0];

            // Пошук існуючої характеристики
            for (int i = 0; i < _stats.Length; i++)
            {
                if (_stats[i].Type == type)
                {
                    _stats[i].Value = value;
                    return;
                }
            }

            // Додавання нової характеристики
            Array.Resize(ref _stats, _stats.Length + 1);
            _stats[_stats.Length - 1] = new StatValue { Type = type, Value = value };
        }

        public float GetStat(StatType type, float defaultValue = 0f)
        {
            if (_stats == null)
                return defaultValue;

            foreach (var stat in _stats)
            {
                if (stat.Type == type)
                    return stat.Value;
            }

            return defaultValue;
        }

        public IEnumerable<StatValue> GetAllStats()
        {
            return _stats ?? new StatValue[0];
        }
    }
}
