// Шлях: Assets/_MythHunter/Code/Editor/DependencyAnalyzer.cs
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using MythHunter.Core.DI;

namespace MythHunter.Editor
{
    /// <summary>
    /// Статичний аналізатор залежностей для використання в редакторі Unity
    /// </summary>
    public class DependencyAnalyzer : EditorWindow
    {
        private Dictionary<Type, HashSet<Type>> _dependencyGraph = new Dictionary<Type, HashSet<Type>>();
        private List<Type> _allTypes = new List<Type>();
        private List<DependencyCycle> _cycles = new List<DependencyCycle>();
        private Vector2 _scrollPosition;
        private bool _showGraph = true;
        private bool _showCycles = true;
        private bool _showMissingDependencies = true;
        private bool _showOnlyErrors = false;
        private bool _showMonoBehaviours = true;
        private string _searchFilter = "";

        /// <summary>
        /// Цикл залежностей
        /// </summary>
        private class DependencyCycle
        {
            public List<Type> Types { get; set; } = new List<Type>();

            public override string ToString()
            {
                return string.Join(" -> ", Types.Select(t => t.Name)) + " -> " + Types[0].Name;
            }
        }

        [MenuItem("MythHunter/Dependency Analyzer")]
        public static void ShowWindow()
        {
            GetWindow<DependencyAnalyzer>("Dependency Analyzer");
        }

        private void OnEnable()
        {
            AnalyzeDependencies();
        }

        private void OnGUI()
        {
            GUILayout.Label("MythHunter Dependency Analyzer", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh Analysis", GUILayout.Width(150)))
            {
                AnalyzeDependencies();
            }

            if (GUILayout.Button("Export Report", GUILayout.Width(150)))
            {
                ExportDependencyReport();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Фільтри
            _searchFilter = EditorGUILayout.TextField("Filter:", _searchFilter);

            EditorGUILayout.BeginHorizontal();
            _showGraph = EditorGUILayout.Toggle("Show Graph", _showGraph);
            _showCycles = EditorGUILayout.Toggle("Show Cycles", _showCycles);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            _showMissingDependencies = EditorGUILayout.Toggle("Show Missing", _showMissingDependencies);
            _showOnlyErrors = EditorGUILayout.Toggle("Only Errors", _showOnlyErrors);
            _showMonoBehaviours = EditorGUILayout.Toggle("Show MonoBehaviours", _showMonoBehaviours);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Виведення циклів залежностей
            if (_showCycles && _cycles.Count > 0)
            {
                EditorGUILayout.LabelField("Dependency Cycles", EditorStyles.boldLabel);
                foreach (var cycle in _cycles)
                {
                    EditorGUILayout.HelpBox(cycle.ToString(), MessageType.Warning);
                }
                EditorGUILayout.Space();
            }

            // Виведення графу залежностей
            if (_showGraph)
            {
                EditorGUILayout.LabelField("Dependency Graph", EditorStyles.boldLabel);

                foreach (var typeEntry in _dependencyGraph)
                {
                    var type = typeEntry.Key;
                    var dependencies = typeEntry.Value;

                    // Фільтрація
                    if (!string.IsNullOrEmpty(_searchFilter) &&
                        !type.Name.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase) &&
                        !dependencies.Any(d => d.Name.Contains(_searchFilter, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    // Пропускаємо MonoBehaviour, якщо потрібно
                    if (!_showMonoBehaviours && typeof(MonoBehaviour).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    // Пропускаємо, якщо показуємо тільки помилки і немає помилок
                    bool hasMissingDependency = dependencies.Any(d =>
                        !_dependencyGraph.ContainsKey(d) && !d.IsInterface && !d.IsAbstract);

                    if (_showOnlyErrors && !hasMissingDependency)
                    {
                        continue;
                    }

                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    EditorGUILayout.LabelField(type.Name, EditorStyles.boldLabel);

                    if (dependencies.Count > 0)
                    {
                        EditorGUI.indentLevel++;
                        foreach (var dependency in dependencies)
                        {
                            bool isRegistered = _dependencyGraph.ContainsKey(dependency) ||
                                dependency.IsInterface || dependency.IsAbstract;

                            // Пропускаємо зареєстровані залежності, якщо показуємо тільки відсутні
                            if (_showMissingDependencies && isRegistered)
                            {
                                continue;
                            }

                            // Виділення проблемних залежностей
                            if (!isRegistered)
                            {
                                GUIStyle errorStyle = new GUIStyle(EditorStyles.label);
                                errorStyle.normal.textColor = Color.red;
                                EditorGUILayout.LabelField($"→ {dependency.Name} (Missing!)", errorStyle);
                            }
                            else
                            {
                                EditorGUILayout.LabelField($"→ {dependency.Name}");
                            }
                        }
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No dependencies");
                    }
                    EditorGUILayout.EndVertical();
                }
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Аналізує залежності в проекті
        /// </summary>
        private void AnalyzeDependencies()
        {
            _dependencyGraph.Clear();
            _cycles.Clear();

            // Знаходимо всі класи з атрибутом [Inject]
            _allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name.Contains("MythHunter"))
                .SelectMany(a =>
                {
                    try
                    {
                        return a.GetTypes();
                    }
                    catch { return Type.EmptyTypes; }
                })
                .Where(t => t.IsClass && !t.IsAbstract)
                .ToList();

            // Будуємо граф залежностей
            foreach (var type in _allTypes)
            {
                var dependencies = new HashSet<Type>();

                // Перевіряємо конструктори
                var constructor = type.GetConstructors()
                    .FirstOrDefault(c => c.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0);

                if (constructor == null && type.GetConstructors().Length > 0)
                {
                    constructor = type.GetConstructors()[0];
                }

                if (constructor != null)
                {
                    foreach (var param in constructor.GetParameters())
                    {
                        dependencies.Add(param.ParameterType);
                    }
                }

                // Перевіряємо поля
                foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(f => f.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0))
                {
                    dependencies.Add(field.FieldType);
                }

                // Перевіряємо властивості
                foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(p => p.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0 && p.CanWrite))
                {
                    dependencies.Add(property.PropertyType);
                }

                // Перевіряємо методи
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0))
                {
                    foreach (var param in method.GetParameters())
                    {
                        dependencies.Add(param.ParameterType);
                    }
                }

                // Додаємо тип до графу
                _dependencyGraph[type] = dependencies;
            }

            // Знаходимо цикли залежностей
            FindCycles();
        }

        /// <summary>
        /// Знаходить цикли залежностей у графі
        /// </summary>
        private void FindCycles()
        {
            var visited = new HashSet<Type>();
            var recursionStack = new HashSet<Type>();
            var currentPath = new List<Type>();

            foreach (var type in _dependencyGraph.Keys)
            {
                if (!visited.Contains(type))
                {
                    DetectCycle(type, visited, recursionStack, currentPath);
                }
            }
        }

        /// <summary>
        /// Рекурсивно шукає цикли залежностей
        /// </summary>
        private bool DetectCycle(Type current, HashSet<Type> visited, HashSet<Type> recursionStack, List<Type> currentPath)
        {
            visited.Add(current);
            recursionStack.Add(current);
            currentPath.Add(current);

            if (_dependencyGraph.TryGetValue(current, out var dependencies))
            {
                foreach (var dependency in dependencies)
                {
                    // Пропускаємо інтерфейси та абстрактні класи
                    if (dependency.IsInterface || dependency.IsAbstract)
                        continue;

                    // Якщо залежності немає в графі, пропускаємо
                    if (!_dependencyGraph.ContainsKey(dependency))
                        continue;

                    // Якщо залежність ще не відвідана
                    if (!visited.Contains(dependency))
                    {
                        if (DetectCycle(dependency, visited, recursionStack, currentPath))
                            return true;
                    }
                    // Якщо залежність вже в стеку рекурсії, то знайдено цикл
                    else if (recursionStack.Contains(dependency))
                    {
                        // Знаходимо початок циклу
                        int startIndex = currentPath.IndexOf(dependency);

                        if (startIndex >= 0)
                        {
                            var cycle = new DependencyCycle();
                            cycle.Types.AddRange(currentPath.Skip(startIndex));
                            _cycles.Add(cycle);
                        }

                        return true;
                    }
                }
            }

            // Видаляємо з поточного шляху та стеку рекурсії
            currentPath.RemoveAt(currentPath.Count - 1);
            recursionStack.Remove(current);

            return false;
        }

        /// <summary>
        /// Експортує звіт про залежності
        /// </summary>
        private void ExportDependencyReport()
        {
            var filePath = EditorUtility.SaveFilePanel(
                "Save Dependency Report",
                "",
                "MythHunter_Dependency_Report.txt",
                "txt");

            if (string.IsNullOrEmpty(filePath))
                return;

            using (var writer = new System.IO.StreamWriter(filePath))
            {
                writer.WriteLine("MythHunter Dependency Analysis Report");
                writer.WriteLine("Generated: " + DateTime.Now.ToString());
                writer.WriteLine("==================================");
                writer.WriteLine();

                // Виведення циклів залежностей
                if (_cycles.Count > 0)
                {
                    writer.WriteLine("DEPENDENCY CYCLES DETECTED");
                    writer.WriteLine("==================================");
                    foreach (var cycle in _cycles)
                    {
                        writer.WriteLine(cycle.ToString());
                    }
                    writer.WriteLine();
                }

                // Виведення відсутніх залежностей
                writer.WriteLine("MISSING DEPENDENCIES");
                writer.WriteLine("==================================");
                bool hasMissing = false;
                foreach (var typeEntry in _dependencyGraph)
                {
                    var type = typeEntry.Key;
                    var dependencies = typeEntry.Value;

                    var missingDependencies = dependencies
                        .Where(d => !_dependencyGraph.ContainsKey(d) && !d.IsInterface && !d.IsAbstract)
                        .ToList();

                    if (missingDependencies.Count > 0)
                    {
                        writer.WriteLine($"{type.FullName} depends on:");
                        foreach (var missing in missingDependencies)
                        {
                            writer.WriteLine($"  - {missing.FullName} (NOT REGISTERED)");
                        }
                        writer.WriteLine();
                        hasMissing = true;
                    }
                }

                if (!hasMissing)
                {
                    writer.WriteLine("No missing dependencies found.");
                    writer.WriteLine();
                }

                // Виведення повного графу залежностей
                writer.WriteLine("FULL DEPENDENCY GRAPH");
                writer.WriteLine("==================================");
                foreach (var typeEntry in _dependencyGraph)
                {
                    var type = typeEntry.Key;
                    var dependencies = typeEntry.Value;

                    writer.WriteLine($"{type.FullName} depends on:");
                    if (dependencies.Count > 0)
                    {
                        foreach (var dependency in dependencies)
                        {
                            bool isRegistered = _dependencyGraph.ContainsKey(dependency) ||
                                dependency.IsInterface || dependency.IsAbstract;

                            writer.WriteLine($"  - {dependency.FullName}" +
                                (isRegistered ? "" : " (NOT REGISTERED)"));
                        }
                    }
                    else
                    {
                        writer.WriteLine("  No dependencies");
                    }
                    writer.WriteLine();
                }
            }

            EditorUtility.DisplayDialog("Export Complete",
                "Dependency report has been saved to: " + filePath, "OK");
        }
    }
}
#endif
