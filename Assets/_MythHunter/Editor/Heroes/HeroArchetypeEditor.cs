// Шлях: Assets/_MythHunter/Code/Editor/HeroArchetypeEditor.cs

using UnityEngine;
using UnityEditor;
using MythHunter.Entities.Archetypes;
using MythHunter.Components.Character;
using MythHunter.Data;
using System.Linq;

namespace MythHunter.Editor
{
    [CustomEditor(typeof(HeroArchetypeSO))]
    public class HeroArchetypeEditor : UnityEditor.Editor
    {
        private bool _showBasicStats = true;
        private bool _showCombatStats = true;
        private bool _showMagicStats = true;
        private bool _showStatusStats = true;
        private bool _showEconomyStats = true;
        private bool _showInventoryStats = true;
        private bool _showPassiveSkills = true;
        private bool _showActiveSkills = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            HeroArchetypeSO heroArchetype = (HeroArchetypeSO)target;

            // Відображення базових полів
            EditorGUILayout.LabelField("UI", EditorStyles.boldLabel);

            // Icon Path з вибором зображення
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("IconPath"));

            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFilePanel("Select Hero Icon", "Assets/Resources", "png,jpg,jpeg");
                if (!string.IsNullOrEmpty(path))
                {
                    // Перетворення шляху в формат ресурсів Unity
                    string resourcePath = path.Replace(Application.dataPath + "/Resources/", "");
                    resourcePath = resourcePath.Replace(".png", "").Replace(".jpg", "").Replace(".jpeg", "");
                    serializedObject.FindProperty("IconPath").stringValue = resourcePath;
                }
            }
            EditorGUILayout.EndHorizontal();

            // Попередній перегляд іконки
            if (!string.IsNullOrEmpty(heroArchetype.IconPath))
            {
                Sprite icon = UnityEngine.Resources.Load<Sprite>(heroArchetype.IconPath);
                if (icon != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    Rect previewRect = GUILayoutUtility.GetRect(64, 64);
                    GUI.DrawTexture(previewRect, icon.texture, ScaleMode.ScaleToFit);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.HelpBox($"Icon not found at path: {heroArchetype.IconPath}", MessageType.Warning);
                }
            }

            // Відображення ідентифікаційних полів
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ідентифікація", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ArchetypeId"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("HeroName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Description"));

            // Відображення расових та класових характеристик
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Раса та клас", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Race"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Class"));

            // Відображення соціальних навичок
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Соціальні навички", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Religion"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Ideology"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_professions"));

            // Відображення характеристик по категоріях
            EditorGUILayout.Space();
            DrawStatCategory(heroArchetype, StatCategory.Basic, ref _showBasicStats, "Базові характеристики");
            DrawStatCategory(heroArchetype, StatCategory.Combat, ref _showCombatStats, "Бойові характеристики");
            DrawStatCategory(heroArchetype, StatCategory.Magic, ref _showMagicStats, "Магічні характеристики");
            DrawStatCategory(heroArchetype, StatCategory.Status, ref _showStatusStats, "Статусні ефекти");
            DrawStatCategory(heroArchetype, StatCategory.Economy, ref _showEconomyStats, "Економічні характеристики");
            DrawStatCategory(heroArchetype, StatCategory.Inventory, ref _showInventoryStats, "Інвентар");

            // Відображення навичок
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Навички", EditorStyles.boldLabel);

            _showPassiveSkills = EditorGUILayout.Foldout(_showPassiveSkills, "Пасивні навички", true);
            if (_showPassiveSkills)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_passiveSkills"), true);
            }

            _showActiveSkills = EditorGUILayout.Foldout(_showActiveSkills, "Активні навички", true);
            if (_showActiveSkills)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("_activeSkills"), true);
            }

            // Відображення скінів
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Скіни", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Skins"), true);

            // Відображення команди
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Команда", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("TeamId"));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawStatCategory(HeroArchetypeSO heroArchetype, StatCategory category, ref bool showCategory, string categoryName)
        {
            showCategory = EditorGUILayout.Foldout(showCategory, categoryName, true);

            if (showCategory)
            {
                EditorGUI.indentLevel++;

                var definitions = StatRegistry.GetDefinitionsByCategory(category).ToList();
                foreach (var def in definitions)
                {
                    float currentValue = heroArchetype.GetStat(def.Type);
                    float newValue = EditorGUILayout.Slider(def.Name, currentValue, def.MinValue, def.MaxValue);

                    if (newValue != currentValue)
                    {
                        heroArchetype.SetStat(def.Type, newValue);
                        EditorUtility.SetDirty(heroArchetype);
                    }

                    // Додаємо підказку з описом
                    if (!string.IsNullOrEmpty(def.Description))
                    {
                        Rect lastRect = GUILayoutUtility.GetLastRect();
                        GUI.Label(lastRect, new GUIContent("", def.Description));
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Space(EditorGUI.indentLevel * 16);
                        EditorGUILayout.LabelField(def.Description, EditorStyles.miniLabel);
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUI.indentLevel--;
            }
        }
    }
}
