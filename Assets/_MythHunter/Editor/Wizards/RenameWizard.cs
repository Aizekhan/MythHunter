using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

/// <summary>
/// Інструмент для масового перейменування класів, інтерфейсів та інших типів у проекті.
/// </summary>
public class RenameWizard : EditorWindow
{
    // Основні налаштування
    private string oldName = "IMythLogger";
    private string newName = "IMythLogger";
    private string searchPath = "Assets/_MythHunter";
    private bool includeInterfaces = true;
    private bool includeClasses = true;
    private bool includeMethodNames = false;
    private bool includeMemberNames = false;
    private bool includeInStringLiterals = false;
    private bool preserveCasing = true;
    private bool backupFiles = true;

    // Показ результатів
    private Vector2 scrollPosition;
    private bool showPreview = false;
    private Dictionary<string, RenamePreview> previewResults = new Dictionary<string, RenamePreview>();
    private List<string> filesToModify = new List<string>();
    private bool isProcessing = false;
    private int totalMatches = 0;
    private int filesWithMatches = 0;

    private string statusMessage = "";
    private MessageType statusMessageType = MessageType.None;

    // Конфігурація пошуку
    private string[] searchExtensions = { "*.cs", "*.asmdef", "*.asmref" };
    private string[] excludeFolders = { "Library", "Temp", "Logs", "obj" };

    [MenuItem("MythHunter Tools/Rename Wizard")]
    public static void ShowWindow()
    {
        GetWindow<RenameWizard>("Rename Wizard");
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("🔄 Rename Wizard", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Цей інструмент дозволяє масово перейменовувати типи у всьому проекті.", MessageType.Info);

        EditorGUILayout.Space(10);

        // Основні параметри
        EditorGUILayout.LabelField("Параметри пошуку та заміни", EditorStyles.boldLabel);

        oldName = EditorGUILayout.TextField("Стара назва:", oldName);
        newName = EditorGUILayout.TextField("Нова назва:", newName);

        EditorGUILayout.BeginHorizontal();
        searchPath = EditorGUILayout.TextField("Шлях для пошуку:", searchPath);
        if (GUILayout.Button("...", GUILayout.Width(30)))
        {
            string path = EditorUtility.OpenFolderPanel("Виберіть папку для пошуку", searchPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                // Перетворення абсолютного шляху на відносний для проекту
                if (path.StartsWith(Application.dataPath))
                {
                    searchPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    searchPath = path;
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(5);

        // Опції пошуку
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Опції пошуку", EditorStyles.boldLabel);

        includeInterfaces = EditorGUILayout.Toggle("Шукати в інтерфейсах", includeInterfaces);
        includeClasses = EditorGUILayout.Toggle("Шукати в класах", includeClasses);
        includeMethodNames = EditorGUILayout.Toggle("Шукати в назвах методів", includeMethodNames);
        includeMemberNames = EditorGUILayout.Toggle("Шукати в назвах полів/властивостей", includeMemberNames);
        includeInStringLiterals = EditorGUILayout.Toggle("Шукати в рядкових літералах", includeInStringLiterals);
        preserveCasing = EditorGUILayout.Toggle("Зберігати регістр при заміні", preserveCasing);
        backupFiles = EditorGUILayout.Toggle("Створювати резервні копії", backupFiles);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(10);

        // Показ статусу
        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.HelpBox(statusMessage, statusMessageType);
        }

        EditorGUILayout.BeginHorizontal();

        GUI.enabled = !isProcessing && !string.IsNullOrEmpty(oldName) && !string.IsNullOrEmpty(newName);

        if (GUILayout.Button("Попередній перегляд", GUILayout.Height(30)))
        {
            PreviewReplace();
        }

        if (GUILayout.Button("Виконати заміну", GUILayout.Height(30)))
        {
            PerformReplacement();
        }

        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // Показ результатів попереднього перегляду
        if (showPreview && previewResults.Count > 0)
        {
            EditorGUILayout.LabelField($"Результати пошуку (знайдено {totalMatches} співпадінь у {filesWithMatches} файлах):", EditorStyles.boldLabel);

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));

            foreach (var filePath in filesToModify)
            {
                var preview = previewResults[filePath];

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(Path.GetFileName(filePath), EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"{preview.MatchCount} співпадінь", GUILayout.Width(100));

                if (GUILayout.Button("Відкрити", GUILayout.Width(80)))
                {
                    AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(filePath));
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField(filePath, EditorStyles.miniLabel);

                if (preview.MatchLines.Count > 0)
                {
                    foreach (var matchLine in preview.MatchLines.Take(5))
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"Рядок {matchLine.LineNumber}:", GUILayout.Width(80));
                        EditorGUILayout.TextField(matchLine.LineText);
                        EditorGUILayout.EndHorizontal();
                    }

                    if (preview.MatchLines.Count > 5)
                    {
                        EditorGUILayout.LabelField($"... ще {preview.MatchLines.Count - 5} співпадінь", EditorStyles.miniLabel);
                    }
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();
        }
    }

    private void PreviewReplace()
    {
        if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
        {
            SetStatusMessage("Необхідно вказати стару та нову назви", MessageType.Error);
            return;
        }

        isProcessing = true;
        try
        {
            previewResults.Clear();
            filesToModify.Clear();

            SetStatusMessage("Виконується пошук файлів...", MessageType.Info);
            List<string> files = FindFiles(searchPath, searchExtensions, excludeFolders);

            SetStatusMessage($"Аналізується {files.Count} файлів...", MessageType.Info);
            Regex searchPattern = BuildSearchPattern();

            totalMatches = 0;
            filesWithMatches = 0;

            foreach (var filePath in files)
            {
                RenamePreview preview = AnalyzeFile(filePath, searchPattern);

                if (preview.MatchCount > 0)
                {
                    previewResults[filePath] = preview;
                    filesToModify.Add(filePath);
                    totalMatches += preview.MatchCount;
                    filesWithMatches++;
                }

                // Дозволяє редактору оновитись під час довгої операції
                if (filesWithMatches % 50 == 0)
                {
                    EditorUtility.DisplayProgressBar("Пошук співпадінь",
                        $"Проаналізовано файлів: {files.IndexOf(filePath) + 1}/{files.Count}",
                        (float)(files.IndexOf(filePath) + 1) / files.Count);
                }
            }

            EditorUtility.ClearProgressBar();

            showPreview = true;

            if (filesWithMatches > 0)
            {
                SetStatusMessage($"Знайдено {totalMatches} співпадінь у {filesWithMatches} файлах.", MessageType.Info);
            }
            else
            {
                SetStatusMessage("Співпадінь не знайдено.", MessageType.Warning);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Помилка при попередньому перегляді: {ex.Message}");
            SetStatusMessage($"Помилка: {ex.Message}", MessageType.Error);
        }
        finally
        {
            isProcessing = false;
            EditorUtility.ClearProgressBar();
        }
    }

    private void PerformReplacement()
    {
        if (!showPreview || filesToModify.Count == 0)
        {
            PreviewReplace();
            if (filesToModify.Count == 0)
            {
                return;
            }
        }

        if (!EditorUtility.DisplayDialog("Підтвердження заміни",
            $"Ви збираєтесь замінити '{oldName}' на '{newName}' у {filesToModify.Count} файлах. Продовжити?",
            "Так", "Скасувати"))
        {
            return;
        }

        isProcessing = true;
        int modifiedFiles = 0;

        try
        {
            foreach (var filePath in filesToModify)
            {
                EditorUtility.DisplayProgressBar("Заміна тексту",
                    $"Обробка файлу {modifiedFiles + 1}/{filesToModify.Count}: {Path.GetFileName(filePath)}",
                    (float)(modifiedFiles + 1) / filesToModify.Count);

                ReplaceInFile(filePath);
                modifiedFiles++;
            }

            AssetDatabase.Refresh();

            SetStatusMessage($"Успішно модифіковано {modifiedFiles} файлів.", MessageType.Info);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Помилка при заміні: {ex.Message}");
            SetStatusMessage($"Помилка: {ex.Message}", MessageType.Error);
        }
        finally
        {
            isProcessing = false;
            EditorUtility.ClearProgressBar();
        }
    }

    private List<string> FindFiles(string rootPath, string[] extensions, string[] excludeFolders)
    {
        List<string> files = new List<string>();

        if (!Directory.Exists(rootPath))
        {
            Debug.LogWarning($"Шлях не існує: {rootPath}");
            return files;
        }

        foreach (string extension in extensions)
        {
            string[] foundFiles = Directory.GetFiles(rootPath, extension, SearchOption.AllDirectories);

            foreach (string file in foundFiles)
            {
                string relativePath = file.Replace("\\", "/");
                bool shouldExclude = false;

                foreach (string excludeFolder in excludeFolders)
                {
                    if (relativePath.Contains($"/{excludeFolder}/") || relativePath.StartsWith($"{excludeFolder}/"))
                    {
                        shouldExclude = true;
                        break;
                    }
                }

                if (!shouldExclude)
                {
                    files.Add(relativePath);
                }
            }
        }

        return files;
    }

    private Regex BuildSearchPattern()
    {
        List<string> patterns = new List<string>();

        // Пошук типів (інтерфейсів і класів)
        if (includeInterfaces || includeClasses)
        {
            // Пошук в декларації типів (class MyClass, interface IMyInterface)
            patterns.Add($@"(class|interface|struct|enum)\s+({oldName})");

            // Пошук у використанні типів (MyClass variable, List<IMyInterface>)
            patterns.Add($@"(\b)({oldName})(\b|[<,>\[\]\s\(\)\{{}}])");

            // Пошук у спадкуванні та реалізації інтерфейсів (: BaseClass, IMyInterface)
            patterns.Add($@":\s*([\w\s,<>]*\s)?({oldName})(\s|,|<|>|\{{)");
            patterns.Add($@",\s*({oldName})(\s|,|<|>|\{{)");
        }

        // Пошук назв методів
        if (includeMethodNames)
        {
            patterns.Add($@"(\b)({oldName})(\s*\()");
        }

        // Пошук назв полів і властивостей
        if (includeMemberNames)
        {
            patterns.Add($@"(\s|\t|;|=|\()({oldName})(\s|;|=|,|\)|<|>|\[)");
        }

        // Пошук у рядкових літералах
        if (includeInStringLiterals)
        {
            patterns.Add($@"(\""[^""]*?){oldName}([^""]*\"")");
        }

        // Об'єднання всіх шаблонів з OR
        string finalPattern = string.Join("|", patterns);

        return new Regex(finalPattern, RegexOptions.Multiline);
    }

    private RenamePreview AnalyzeFile(string filePath, Regex searchPattern)
    {
        RenamePreview preview = new RenamePreview();
        string content = File.ReadAllText(filePath);

        // Пошук всіх співпадінь у файлі
        MatchCollection matches = searchPattern.Matches(content);

        if (matches.Count > 0)
        {
            preview.MatchCount = matches.Count;

            // Отримання рядків з співпадіннями
            string[] lines = content.Split('\n');

            foreach (Match match in matches)
            {
                // Визначення номера рядка для співпадіння
                int lineNumber = 1;
                int charPosition = 0;

                for (int i = 0; i < lines.Length; i++)
                {
                    int lineLength = lines[i].Length + 1; // +1 для символу нового рядка

                    if (charPosition + lineLength > match.Index)
                    {
                        lineNumber = i + 1;
                        string line = lines[i];

                        // Додавання інформації про співпадіння
                        preview.MatchLines.Add(new MatchLine
                        {
                            LineNumber = lineNumber,
                            LineText = line.TrimEnd()
                        });

                        break;
                    }

                    charPosition += lineLength;
                }
            }
        }

        return preview;
    }

    private void ReplaceInFile(string filePath)
    {
        try
        {
            // Створення резервної копії файлу, якщо потрібно
            if (backupFiles)
            {
                string backupPath = filePath + ".bak";
                File.Copy(filePath, backupPath, true);
            }

            string content = File.ReadAllText(filePath);

            if (preserveCasing)
            {
                // Зберігати регістр при заміні (IMythLogger -> IMythLogger, iLogger -> iMythLogger)
                content = PreserveCasingReplace(content, oldName, newName);
            }
            else
            {
                // Пряма заміна
                content = ReplaceWithRegex(content, oldName, newName);
            }

            File.WriteAllText(filePath, content);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Помилка при заміні у файлі {filePath}: {ex.Message}");
            throw;
        }
    }

    private string PreserveCasingReplace(string content, string oldName, string newName)
    {
        // Пошук всіх варіантів написання oldName
        List<string> variants = new List<string> {
            oldName,                            // Як є
            oldName.ToLower(),                  // Нижній регістр
            oldName.ToUpper(),                  // Верхній регістр
            char.ToLower(oldName[0]) + oldName.Substring(1), // camelCase
            char.ToUpper(oldName[0]) + oldName.Substring(1)  // PascalCase
        };

        foreach (string variant in variants.Distinct())
        {
            // Перетворення нової назви за тим самим принципом
            string newVariant = newName;

            if (variant == oldName.ToLower())
            {
                newVariant = newName.ToLower();
            }
            else if (variant == oldName.ToUpper())
            {
                newVariant = newName.ToUpper();
            }
            else if (char.IsLower(variant[0]))
            {
                newVariant = char.ToLower(newName[0]) + newName.Substring(1);
            }
            else if (char.IsUpper(variant[0]))
            {
                newVariant = char.ToUpper(newName[0]) + newName.Substring(1);
            }

            content = ReplaceWithRegex(content, variant, newVariant);
        }

        return content;
    }

    private string ReplaceWithRegex(string content, string oldText, string newText)
    {
        // Розумна заміна з урахуванням контексту
        Regex regex = new Regex($@"(\b|_){Regex.Escape(oldText)}(\b|[<,>\[\]\s\(\)\{{}}_])");

        content = regex.Replace(content, m => {
            string prefix = m.Groups[1].Value;
            string suffix = m.Groups[2].Value;
            return prefix + newText + suffix;
        });

        return content;
    }

    private void SetStatusMessage(string message, MessageType type)
    {
        statusMessage = message;
        statusMessageType = type;
        Repaint();
    }
}

/// <summary>
/// Клас для зберігання інформації про попередній перегляд зміни
/// </summary>
public class RenamePreview
{
    public int MatchCount
    {
        get; set;
    }
    public List<MatchLine> MatchLines { get; set; } = new List<MatchLine>();
}

/// <summary>
/// Інформація про рядок з співпадінням
/// </summary>
public class MatchLine
{
    public int LineNumber
    {
        get; set;
    }
    public string LineText
    {
        get; set;
    }
}
