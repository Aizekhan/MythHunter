using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class LazyMonoBehaviourAnalyzer : EditorWindow
{
    private List<Type> _potentialLazyTypes = new List<Type>();
    private List<Type> _notRecommendedTypes = new List<Type>();
    private Vector2 _scrollPosition;
    private bool _showRecommended = true;
    private bool _showNotRecommended = true;
    private string _searchFilter = "";

    [MenuItem("MythHunter/Tools/Lazy MonoBehaviour Analyzer")]
    public static void ShowWindow()
    {
        var window = GetWindow<LazyMonoBehaviourAnalyzer>("Lazy MB Analyzer");
        window.AnalyzeTypes();
    }

    private void AnalyzeTypes()
    {
        _potentialLazyTypes.Clear();
        _notRecommendedTypes.Clear();

        // Отримуємо всі типи з усіх завантажених збірок
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            // Пропускаємо системні збірки та збірки Unity
            if (assembly.FullName.StartsWith("System") ||
                assembly.FullName.StartsWith("Unity") ||
                assembly.FullName.StartsWith("mscorlib"))
                continue;

            try
            {
                var types = assembly.GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(MonoBehaviour)) &&
                           !t.IsSubclassOf(typeof(MythHunter.Core.MonoBehaviours.LazyMonoBehaviour)))
                    .ToList();

                foreach (var type in types)
                {
                    if (ShouldBeLazy(type))
                        _potentialLazyTypes.Add(type);
                    else
                        _notRecommendedTypes.Add(type);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error analyzing assembly {assembly.FullName}: {ex.Message}");
            }
        }

        Debug.Log($"Analysis complete. Found {_potentialLazyTypes.Count} potential LazyMonoBehaviour candidates and {_notRecommendedTypes.Count} not recommended.");
    }

    private bool ShouldBeLazy(Type type)
    {
        // Перевіряємо наявність полів з атрибутом [Inject]
        var injectFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Any(f => f.GetCustomAttributes(true).Any(a => a.GetType().Name == "InjectAttribute"));

        if (injectFields)
            return true;

        // Перевіряємо, чи є метод Awake і чи використовує він GameBootstrapper або DI
        var awakeMethods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .Where(m => m.Name == "Awake" || m.Name == "Start");

        // Перевіряємо наявність інших ознак DI-залежностей
        var suspiciousMethodCalls = new[] { "Resolve", "GetService", "Container" };

        bool usesDI = false;
        foreach (var method in awakeMethods)
        {
            if (method.GetMethodBody() != null)
            {
                // Це груба перевірка, яка може давати хибні спрацювання,
                // але достатньо точна для першого відбору
                var methodText = method.ToString();
                if (suspiciousMethodCalls.Any(call => methodText.Contains(call)))
                {
                    usesDI = true;
                    break;
                }
            }
        }

        return usesDI;
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Analyze MonoBehaviours", GUILayout.Height(30)))
        {
            AnalyzeTypes();
        }
        EditorGUILayout.EndHorizontal();

        _searchFilter = EditorGUILayout.TextField("Filter:", _searchFilter);

        EditorGUILayout.Space();
        _showRecommended = EditorGUILayout.Foldout(_showRecommended, $"Recommended for LazyMonoBehaviour ({_potentialLazyTypes.Count})");

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        if (_showRecommended)
        {
            DrawTypeList(_potentialLazyTypes, true);
        }

        EditorGUILayout.Space();
        _showNotRecommended = EditorGUILayout.Foldout(_showNotRecommended, $"Not Recommended for LazyMonoBehaviour ({_notRecommendedTypes.Count})");

        if (_showNotRecommended)
        {
            DrawTypeList(_notRecommendedTypes, false);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawTypeList(List<Type> types, bool recommended)
    {
        EditorGUI.indentLevel++;

        foreach (var type in types)
        {
            // Фільтрація за пошуком
            if (!string.IsNullOrEmpty(_searchFilter) &&
                !type.FullName.ToLower().Contains(_searchFilter.ToLower()))
                continue;

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(type.FullName);

            if (GUILayout.Button("Select", GUILayout.Width(60)))
            {
                // Знаходимо файл скрипту
                string[] guids = AssetDatabase.FindAssets($"t:Script {type.Name}");
                if (guids.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                    var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                    if (script != null)
                    {
                        Selection.activeObject = script;
                        EditorGUIUtility.PingObject(script);
                    }
                }
            }

            if (GUILayout.Button("Convert", GUILayout.Width(60)))
            {
                ConvertToLazyMonoBehaviour(type);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUI.indentLevel--;
    }

    private void ConvertToLazyMonoBehaviour(Type type)
    {
        // Знаходимо файл скрипту
        string[] guids = AssetDatabase.FindAssets($"t:Script {type.Name}");
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);

            try
            {
                // Читаємо вміст файлу
                string content = System.IO.File.ReadAllText(path);

                // Замінюємо наслідування
                string original = $": MonoBehaviour";
                string replacement = $": MythHunter.Core.MonoBehaviours.LazyMonoBehaviour";

                if (content.Contains(original))
                {
                    content = content.Replace(original, replacement);

                    // Перевіряємо, чи потрібно додати using
                    if (!content.Contains("using MythHunter.Core.MonoBehaviours;"))
                    {
                        int lastUsingIndex = content.LastIndexOf("using ");
                        int insertionPoint = content.IndexOf(";", lastUsingIndex) + 1;

                        content = content.Insert(insertionPoint,
                            Environment.NewLine + "using MythHunter.Core.MonoBehaviours;");
                    }

                    // Зберігаємо змінений вміст
                    System.IO.File.WriteAllText(path, content);
                    AssetDatabase.Refresh();

                    Debug.Log($"Successfully converted {type.Name} to LazyMonoBehaviour");
                }
                else
                {
                    Debug.LogWarning($"Could not find class declaration pattern for {type.Name}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error converting {type.Name}: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Could not find script file for {type.Name}");
        }
    }
}
