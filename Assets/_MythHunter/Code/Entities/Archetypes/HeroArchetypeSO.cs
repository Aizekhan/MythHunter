// Assets/_MythHunter/Code/Entities/Archetypes/HeroArchetypeSO.cs

using UnityEngine;
using System;
using System.Collections.Generic;
using MythHunter.Components.Core;
using MythHunter.Components.Combat;
using MythHunter.Components.Movement;
using MythHunter.Components.Character;
using MythHunter.Entities.Heroes;
using MythHunter.Core.ECS;
namespace MythHunter.Entities.Archetypes
{
    [CreateAssetMenu(fileName = "HeroArchetype", menuName = "MythHunter/Heroes/Hero Archetype")]
    public class HeroArchetypeSO : ScriptableObject
    {
        [Header("UI")]
        [SerializeField] public string IconPath; // Шлях до іконки героя
        [SerializeField] public int ManaCost = 1;
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

        [Header("Бойові стилі (модифікатори)")]
        public float AggressiveAttackMod = 1.5f;
        public float AggressiveDefenseMod = 0.7f;
        public float AggressiveDodgeMod = 0.7f;
        public float AggressiveBlockMod = 0.5f;
        public float DefensiveAttackMod = 0.7f;
        public float DefensiveDefenseMod = 1.5f;
        public float DefensiveDodgeMod = 1.3f;
        public float DefensiveBlockMod = 1.5f;

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


        public void AddActiveSkill(string id, int level = 1)
        {
            var skill = new SkillValue { Id = id, Level = level };

            var list = new List<SkillValue>(_activeSkills ?? Array.Empty<SkillValue>());
            list.Add(skill);
            _activeSkills = list.ToArray();
        }

        public void AddPassiveSkill(string id, int level = 1)
        {
            var skill = new SkillValue { Id = id, Level = level };

            var list = new List<SkillValue>(_passiveSkills ?? Array.Empty<SkillValue>());
            list.Add(skill);
            _passiveSkills = list.ToArray();
        }
        public void SetProfessions(string[] professions)
        {
            _professions = professions != null ? (string[])professions.Clone() : new string[0];
        }
        public SkillValue[] GetActiveSkills() => _activeSkills ?? Array.Empty<SkillValue>();
        public SkillValue[] GetPassiveSkills() => _passiveSkills ?? Array.Empty<SkillValue>();
        public string[] GetProfessions() => _professions ?? new string[0];
    }
}
