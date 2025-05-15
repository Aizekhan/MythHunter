// Assets/_MythHunter/Code/Editor/Heroes/HeroArchetypeEditor.cs
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using MythHunter.Entities.Archetypes;

namespace MythHunter.Editor.Heroes
{
    [CustomEditor(typeof(HeroArchetypeSO))]
    public class HeroArchetypeEditor : UnityEditor.Editor
    {
        private bool showAdvancedSettings = false;

        public override void OnInspectorGUI()
        {
            HeroArchetypeSO heroArchetype = (HeroArchetypeSO)target;

            // Основна інформація
            EditorGUILayout.LabelField("Основна інформація про героя", EditorStyles.boldLabel);

            // Автоматичне генерування ID, якщо порожній
            if (string.IsNullOrEmpty(heroArchetype.ArchetypeId))
            {
                heroArchetype.ArchetypeId = "Hero_" + System.Guid.NewGuid().ToString().Substring(0, 8);
                EditorUtility.SetDirty(heroArchetype);
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Архетип ID", heroArchetype.ArchetypeId);
            EditorGUI.EndDisabledGroup();

            heroArchetype.HeroName = EditorGUILayout.TextField("Ім'я героя", heroArchetype.HeroName);
            heroArchetype.Description = EditorGUILayout.TextArea(heroArchetype.Description, GUILayout.Height(60));

            EditorGUILayout.Space();

            // Раса і клас
            EditorGUILayout.LabelField("Параметри героя", EditorStyles.boldLabel);
            heroArchetype.Race = (HeroRace)EditorGUILayout.EnumPopup("Раса", heroArchetype.Race);
            heroArchetype.Class = (HeroClass)EditorGUILayout.EnumPopup("Клас", heroArchetype.Class);

            EditorGUILayout.Space();

            // Основні характеристики
            EditorGUILayout.LabelField("Базові характеристики", EditorStyles.boldLabel);
            heroArchetype.BaseHealth = EditorGUILayout.FloatField("Здоров'я", heroArchetype.BaseHealth);
            heroArchetype.BaseAttack = EditorGUILayout.FloatField("Атака", heroArchetype.BaseAttack);
            heroArchetype.BaseDefense = EditorGUILayout.FloatField("Захист", heroArchetype.BaseDefense);
            heroArchetype.BaseMoveSpeed = EditorGUILayout.FloatField("Швидкість руху", heroArchetype.BaseMoveSpeed);
            heroArchetype.BaseMovementPoints = EditorGUILayout.FloatField("Очки руху", heroArchetype.BaseMovementPoints);

            EditorGUILayout.Space();

            // Бойові характеристики
            EditorGUILayout.LabelField("Бойові характеристики", EditorStyles.boldLabel);
            heroArchetype.CriticalChance = EditorGUILayout.Slider("Шанс критичного удару", heroArchetype.CriticalChance, 0f, 1f);
            heroArchetype.CriticalMultiplier = EditorGUILayout.FloatField("Множник критичного удару", heroArchetype.CriticalMultiplier);
            heroArchetype.DodgeChance = EditorGUILayout.Slider("Шанс ухилення", heroArchetype.DodgeChance, 0f, 1f);
            heroArchetype.BlockChance = EditorGUILayout.Slider("Шанс блокування", heroArchetype.BlockChance, 0f, 1f);
            heroArchetype.AttackSpeed = EditorGUILayout.FloatField("Швидкість атаки", heroArchetype.AttackSpeed);

            EditorGUILayout.Space();

            // Ресурси
            EditorGUILayout.LabelField("Ресурси", EditorStyles.boldLabel);
            heroArchetype.MaxRage = EditorGUILayout.FloatField("Максимальна лють", heroArchetype.MaxRage);
            heroArchetype.MaxConcentration = EditorGUILayout.FloatField("Максимальна концентрація", heroArchetype.MaxConcentration);

            EditorGUILayout.Space();

            // Здібності
            EditorGUILayout.LabelField("Спеціальні здібності", EditorStyles.boldLabel);
            heroArchetype.ActiveAbilityId = EditorGUILayout.TextField("Активна здібність", heroArchetype.ActiveAbilityId);

            EditorGUILayout.LabelField("Пасивні здібності");
            if (heroArchetype.PassiveAbilityIds == null)
                heroArchetype.PassiveAbilityIds = new System.Collections.Generic.List<string>();

            int passiveCount = EditorGUILayout.IntField("Кількість пасивних здібностей", heroArchetype.PassiveAbilityIds.Count);

            // Змінюємо розмір списку
            while (heroArchetype.PassiveAbilityIds.Count < passiveCount)
                heroArchetype.PassiveAbilityIds.Add("");
            while (heroArchetype.PassiveAbilityIds.Count > passiveCount)
                heroArchetype.PassiveAbilityIds.RemoveAt(heroArchetype.PassiveAbilityIds.Count - 1);

            for (int i = 0; i < heroArchetype.PassiveAbilityIds.Count; i++)
            {
                heroArchetype.PassiveAbilityIds[i] = EditorGUILayout.TextField($"Пасивна здібність {i + 1}", heroArchetype.PassiveAbilityIds[i]);
            }

            EditorGUILayout.Space();

            // Розділ стилів бою
            EditorGUILayout.LabelField("Стилі бою", EditorStyles.boldLabel);

            // Агресивний стиль
            EditorGUILayout.LabelField("Агресивний стиль", EditorStyles.boldLabel);
            heroArchetype.AggressiveAttackMod = EditorGUILayout.Slider("Модифікатор атаки", heroArchetype.AggressiveAttackMod, 0.5f, 2f);
            heroArchetype.AggressiveDefenseMod = EditorGUILayout.Slider("Модифікатор захисту", heroArchetype.AggressiveDefenseMod, 0.5f, 2f);
            heroArchetype.AggressiveDodgeMod = EditorGUILayout.Slider("Модифікатор ухилення", heroArchetype.AggressiveDodgeMod, 0.5f, 2f);
            heroArchetype.AggressiveBlockMod = EditorGUILayout.Slider("Модифікатор блокування", heroArchetype.AggressiveBlockMod, 0.5f, 2f);

            // Захисний стиль
            EditorGUILayout.LabelField("Захисний стиль", EditorStyles.boldLabel);
            heroArchetype.DefensiveAttackMod = EditorGUILayout.Slider("Модифікатор атаки", heroArchetype.DefensiveAttackMod, 0.5f, 2f);
            heroArchetype.DefensiveDefenseMod = EditorGUILayout.Slider("Модифікатор захисту", heroArchetype.DefensiveDefenseMod, 0.5f, 2f);
            heroArchetype.DefensiveDodgeMod = EditorGUILayout.Slider("Модифікатор ухилення", heroArchetype.DefensiveDodgeMod, 0.5f, 2f);
            heroArchetype.DefensiveBlockMod = EditorGUILayout.Slider("Модифікатор блокування", heroArchetype.DefensiveBlockMod, 0.5f, 2f);

            EditorGUILayout.Space();

            // Додаткові настройки
            showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Додаткові настройки");
            if (showAdvancedSettings)
            {
                EditorGUILayout.LabelField("Поле зору", EditorStyles.boldLabel);
                heroArchetype.VisionRadius = EditorGUILayout.FloatField("Радіус огляду", heroArchetype.VisionRadius);
                heroArchetype.VisionAngle = EditorGUILayout.Slider("Кут огляду", heroArchetype.VisionAngle, 1f, 360f);

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Команда", EditorStyles.boldLabel);
                heroArchetype.TeamId = EditorGUILayout.IntField("ID команди", heroArchetype.TeamId);
            }

            // Зберігаємо зміни
            if (GUI.changed)
            {
                EditorUtility.SetDirty(heroArchetype);
            }
        }
    }
}
#endif
