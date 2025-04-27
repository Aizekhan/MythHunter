using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Linq;
using System.Net;
using UnityEngine.Networking;
using System.Collections;
using UnityEditor.Build;

/// <summary>
/// Візард для початкового налаштування проекту Unity перед створенням структури MythHunter.
/// </summary>
public class MythHunterSetupWizard : EditorWindow
{
    private Vector2 scrollPosition;

    // Режими роботи візарда
    private enum WizardMode
    {
        Form,           // Форма введення даних користувачем
        Setup,          // Налаштування проекту
        Complete        // Завершено
    }
    private WizardMode currentMode = WizardMode.Form;

    // Дані користувача
    private string companyName = "YourCompany";
    private string productName = "MythHunter";
    private string bundleIdentifier = "com.yourcompany.mythhunter";
    private string startSceneName = "Boot";

    // Цільова платформа
    public enum TargetPlatform
    {
        PC,
        Mobile,
        WebGL,
        Console
    }
    private TargetPlatform targetPlatform = TargetPlatform.PC;

    // Налаштування проекту
    private bool setupPlayerSettings = true;
    private bool setupQualitySettings = true;
    private bool setupProjectSettings = true;
    private bool setupGitIgnore = true;
    private bool installRequiredPackages = true;

    // Player Settings
    private bool setScriptingBackend = true;
    private ScriptingImplementation scriptingBackend = ScriptingImplementation.IL2CPP;
    private bool setApiCompatibility = true;
    private ApiCompatibilityLevel apiCompatibility = ApiCompatibilityLevel.NET_4_6;

    // Quality Settings
    private bool setupPCQuality = true;
    private bool setupMobileQuality = true;
    private bool setupWebGLQuality = true;
    private bool setupConsoleQuality = true;
    private int defaultQualityLevel = 3; // Medium

    // Project Settings
    private ColorSpace colorSpace = ColorSpace.Linear;
    private bool enableNewInputSystem = true;
    private bool enableAddressables = true;

    // Packages
    private readonly string[] essentialPackages = new string[]
    {
        "com.unity.textmeshpro",
        "com.unity.inputsystem",
        "com.unity.addressables",
        "com.unity.cinemachine",
        "com.unity.2d.sprite",
        "com.unity.ugui"
    };

    private readonly Dictionary<string, bool> optionalPackages = new Dictionary<string, bool>
    {
        { "com.unity.postprocessing", true },
        { "com.unity.render-pipelines.universal", true },
        { "com.unity.visualeffectgraph", false },
        { "com.unity.animation", true },
        { "com.unity.timeline", true },
        { "com.unity.probuilder", false }
    };

    // GitIgnore
    private bool useStandardGitIgnore = true;
    private bool addCustomGitIgnoreRules = true;
    private string customGitIgnoreRules = "# Custom rules\n/Assets/Temp/\n/Assets/Builds/\n*.log";

    // URL для пакетів
    private bool installUniTask = true;
    private string uniTaskGitUrl = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask";
    private bool validateUniTaskUrl = true;

    private bool installNuGetForUnity = false;
    private string nuGetForUnityUrl = "https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity";
    private bool validateNuGetUrl = true;

    // Стан
    private bool showAdvancedSettings = false;
    private bool isProcessing = false;
    private string currentStatus = "";
    private float progressBarValue = 0f;
    private List<string> setupLog = new List<string>();
    private AddRequest packageAddRequest;
    private ListRequest packageListRequest;
    private bool showSetupLog = false;

    // Результати перевірки URL
    private Dictionary<string, bool> urlValidationResults = new Dictionary<string, bool>();

    [MenuItem("MythHunter Tools/Project Setup Wizard")]
    public static void ShowWindow()
    {
        GetWindow<MythHunterSetupWizard>("MythHunter Setup Wizard");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        switch (currentMode)
        {
            case WizardMode.Form:
                DrawUserForm();
                break;
            case WizardMode.Setup:
                DrawSetupOptions();
                break;
            case WizardMode.Complete:
                DrawCompleteScreen();
                break;
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawUserForm()
    {
        GUILayout.Label("MythHunter Project Setup Wizard", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Заповніть базові параметри для вашого проекту. Ці дані будуть використані для налаштування Unity.", MessageType.Info);

        EditorGUILayout.Space(15);
        GUILayout.Label("Основні параметри проекту", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        companyName = EditorGUILayout.TextField("Назва компанії:", companyName);
        productName = EditorGUILayout.TextField("Назва продукту:", productName);
        bundleIdentifier = EditorGUILayout.TextField("Bundle Identifier:", bundleIdentifier);
        startSceneName = EditorGUILayout.TextField("Назва стартової сцени:", startSceneName);

        EditorGUILayout.Space(5);

        GUILayout.Label("Цільова платформа:", EditorStyles.boldLabel);
        targetPlatform = (TargetPlatform)EditorGUILayout.EnumPopup("Основна платформа:", targetPlatform);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);
        GUILayout.Label("Налаштування пакетів", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        // UniTask
        installUniTask = EditorGUILayout.BeginToggleGroup("Встановити UniTask", installUniTask);
        EditorGUI.indentLevel++;
        uniTaskGitUrl = EditorGUILayout.TextField("Git URL для UniTask:", uniTaskGitUrl);
        validateUniTaskUrl = EditorGUILayout.Toggle("Перевірити URL перед завантаженням", validateUniTaskUrl);

        // Показуємо результат валідації, якщо він є
        if (urlValidationResults.ContainsKey("UniTask"))
        {
            EditorGUILayout.HelpBox(
                urlValidationResults["UniTask"]
                    ? "URL доступний ✓"
                    : "URL недоступний! Перевірте правильність ссилки ✗",
                urlValidationResults["UniTask"] ? MessageType.Info : MessageType.Error);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndToggleGroup();

        // NuGetForUnity
        installNuGetForUnity = EditorGUILayout.BeginToggleGroup("Встановити NuGetForUnity", installNuGetForUnity);
        EditorGUI.indentLevel++;
        nuGetForUnityUrl = EditorGUILayout.TextField("Git URL для NuGetForUnity:", nuGetForUnityUrl);
        validateNuGetUrl = EditorGUILayout.Toggle("Перевірити URL перед завантаженням", validateNuGetUrl);

        // Показуємо результат валідації, якщо він є
        if (urlValidationResults.ContainsKey("NuGet"))
        {
            EditorGUILayout.HelpBox(
                urlValidationResults["NuGet"]
                    ? "URL доступний ✓"
                    : "URL недоступний! Перевірте правильність ссилки ✗",
                urlValidationResults["NuGet"] ? MessageType.Info : MessageType.Error);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.EndToggleGroup();

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);

        // Кнопки для навігації
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Валідувати URL", GUILayout.Height(30)))
        {
            ValidateUrls();
        }

        if (GUILayout.Button("Далі", GUILayout.Height(30)))
        {
            bool urlsValid = true;

            // Перевіряємо валідність URL, якщо потрібно
            if ((installUniTask && validateUniTaskUrl && urlValidationResults.ContainsKey("UniTask") && !urlValidationResults["UniTask"]) ||
                (installNuGetForUnity && validateNuGetUrl && urlValidationResults.ContainsKey("NuGet") && !urlValidationResults["NuGet"]))
            {
                urlsValid = false;
                EditorUtility.DisplayDialog("Помилка", "URL для пакетів недоступні. Перевірте правильність ссилок або відключіть перевірку.", "OK");
            }

            if (urlsValid)
            {
                currentMode = WizardMode.Setup;
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawSetupOptions()
    {
        GUILayout.Label("MythHunter Project Setup Wizard", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Налаштуйте параметри для вашого проекту на основі платформи: " + targetPlatform.ToString(), MessageType.Info);

        EditorGUILayout.Space(10);
        GUILayout.Label("Налаштування для застосування:", EditorStyles.boldLabel);
        setupPlayerSettings = EditorGUILayout.Toggle("Налаштувати Player Settings", setupPlayerSettings);
        setupQualitySettings = EditorGUILayout.Toggle("Налаштувати Quality Settings", setupQualitySettings);
        setupProjectSettings = EditorGUILayout.Toggle("Налаштувати Project Settings", setupProjectSettings);
        setupGitIgnore = EditorGUILayout.Toggle("Налаштувати GitIgnore", setupGitIgnore);
        installRequiredPackages = EditorGUILayout.Toggle("Встановити необхідні пакети", installRequiredPackages);

        showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Розширені налаштування");

        if (showAdvancedSettings)
        {
            DrawAdvancedSettings();
        }

        EditorGUILayout.Space(10);

        if (isProcessing)
        {
            DrawProgressBar();
        }
        else
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Назад", GUILayout.Height(30)))
            {
                currentMode = WizardMode.Form;
            }

            if (GUILayout.Button("Застосувати налаштування", GUILayout.Height(30)))
            {
                ApplySettings();
            }

            EditorGUILayout.EndHorizontal();
        }

        DrawSetupLog();
    }

    private void DrawAdvancedSettings()
    {
        EditorGUILayout.Space(10);

        // Player Settings
        if (setupPlayerSettings)
        {
            GUILayout.Label("Player Settings:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            setScriptingBackend = EditorGUILayout.Toggle("Set Scripting Backend", setScriptingBackend);
            if (setScriptingBackend)
            {
                EditorGUI.indentLevel++;
                scriptingBackend = (ScriptingImplementation)EditorGUILayout.EnumPopup("Scripting Backend", scriptingBackend);
                EditorGUI.indentLevel--;
            }

            setApiCompatibility = EditorGUILayout.Toggle("Set API Compatibility", setApiCompatibility);
            if (setApiCompatibility)
            {
                EditorGUI.indentLevel++;
                apiCompatibility = (ApiCompatibilityLevel)EditorGUILayout.EnumPopup("API Compatibility", apiCompatibility);
                EditorGUI.indentLevel--;
            }

            EditorGUI.indentLevel--;
        }

        // Quality Settings
        if (setupQualitySettings)
        {
            EditorGUILayout.Space(5);
            GUILayout.Label("Quality Settings:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            setupPCQuality = EditorGUILayout.Toggle("Setup PC Quality", setupPCQuality);
            setupMobileQuality = EditorGUILayout.Toggle("Setup Mobile Quality", setupMobileQuality);
            setupWebGLQuality = EditorGUILayout.Toggle("Setup WebGL Quality", setupWebGLQuality);
            setupConsoleQuality = EditorGUILayout.Toggle("Setup Console Quality", setupConsoleQuality);
            defaultQualityLevel = EditorGUILayout.IntSlider("Default Quality Level", defaultQualityLevel, 0, 5);

            EditorGUI.indentLevel--;
        }

        // Project Settings
        if (setupProjectSettings)
        {
            EditorGUILayout.Space(5);
            GUILayout.Label("Project Settings:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;

            colorSpace = (ColorSpace)EditorGUILayout.EnumPopup("Color Space", colorSpace);
            enableNewInputSystem = EditorGUILayout.Toggle("Enable New Input System", enableNewInputSystem);
            enableAddressables = EditorGUILayout.Toggle("Enable Addressables", enableAddressables);

            EditorGUI.indentLevel--;
        }

        // Packages
        if (installRequiredPackages)
        {
            EditorGUILayout.Space(5);
            GUILayout.Label("Packages:", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            GUILayout.Label("Essential Packages (завжди встановлюються):", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            foreach (var package in essentialPackages)
            {
                EditorGUILayout.LabelField(package);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(5);
            GUILayout.Label("Optional Packages:", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;

            Dictionary<string, bool> updatedOptionalPackages = new Dictionary<string, bool>();
            foreach (var package in optionalPackages)
            {
                bool isSelected = EditorGUILayout.Toggle(package.Key, package.Value);
                updatedOptionalPackages[package.Key] = isSelected;
            }
            optionalPackages.Clear();
            foreach (var package in updatedOptionalPackages)
            {
                optionalPackages[package.Key] = package.Value;
            }

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
        }

        // GitIgnore
        if (setupGitIgnore)
        {
            EditorGUILayout.Space(5);
            GUILayout.Label("Git Settings:", EditorStyles.boldLabel);

            EditorGUI.indentLevel++;
            useStandardGitIgnore = EditorGUILayout.Toggle("Use Standard Unity GitIgnore", useStandardGitIgnore);
            addCustomGitIgnoreRules = EditorGUILayout.Toggle("Add Custom GitIgnore Rules", addCustomGitIgnoreRules);

            if (addCustomGitIgnoreRules)
            {
                EditorGUILayout.LabelField("Custom Rules:");
                customGitIgnoreRules = EditorGUILayout.TextArea(customGitIgnoreRules, GUILayout.Height(100));
            }
            EditorGUI.indentLevel--;
        }
    }

    private void DrawCompleteScreen()
    {
        GUILayout.Label("MythHunter Project Setup Wizard", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox("Налаштування проекту успішно завершено!", MessageType.Info);

        EditorGUILayout.Space(20);

        GUILayout.Label("Підсумок:", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Компанія:", companyName);
        EditorGUILayout.LabelField("Продукт:", productName);
        EditorGUILayout.LabelField("Bundle Identifier:", bundleIdentifier);
        EditorGUILayout.LabelField("Цільова платформа:", targetPlatform.ToString());

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Встановлені пакети:");

        EditorGUI.indentLevel++;
        foreach (var package in essentialPackages)
        {
            EditorGUILayout.LabelField("✓ " + package);
        }

        foreach (var package in optionalPackages)
        {
            if (package.Value)
            {
                EditorGUILayout.LabelField("✓ " + package.Key);
            }
        }

        if (installUniTask)
        {
            EditorGUILayout.LabelField("✓ UniTask");
        }

        if (installNuGetForUnity)
        {
            EditorGUILayout.LabelField("✓ NuGetForUnity");
        }

        EditorGUI.indentLevel--;

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Перейти до налаштування структури проекту", GUILayout.Height(40)))
        {
            ShowProjectStructureWizard();
        }

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Закрити", GUILayout.Height(30)))
        {
            Close();
        }

        DrawSetupLog();
    }

    private void DrawProgressBar()
    {
        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField($"Налаштування проекту: {currentStatus}");
        Rect progressRect = EditorGUILayout.GetControlRect(false, 20);
        EditorGUI.ProgressBar(progressRect, progressBarValue, $"{Mathf.Round(progressBarValue * 100)}%");
    }

    private void DrawSetupLog()
    {
        if (setupLog.Count > 0)
        {
            EditorGUILayout.Space(10);
            showSetupLog = EditorGUILayout.Foldout(showSetupLog, $"Лог налаштувань ({setupLog.Count})");

            if (showSetupLog)
            {
                EditorGUILayout.BeginVertical("box");
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

                foreach (var logEntry in setupLog)
                {
                    EditorGUILayout.LabelField(logEntry);
                }

                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Очистити лог"))
                {
                    setupLog.Clear();
                }
            }
        }
    }

    private void ValidateUrls()
    {
        // Очищаємо попередні результати
        urlValidationResults.Clear();

        // Перевіряємо URL для UniTask
        if (installUniTask && validateUniTaskUrl)
        {
            CheckUrl(uniTaskGitUrl, "UniTask");
        }

        // Перевіряємо URL для NuGetForUnity
        if (installNuGetForUnity && validateNuGetUrl)
        {
            CheckUrl(nuGetForUnityUrl, "NuGet");
        }
    }

    private void CheckUrl(string url, string key)
    {
        // Відправляємо HEAD запит, щоб визначити, чи доступний URL
        WebRequest request = WebRequest.Create(url);
        request.Method = "HEAD";

        try
        {
            request.BeginGetResponse(ar =>
            {
                try
                {
                    WebResponse response = request.EndGetResponse(ar);
                    // URL доступний
                    urlValidationResults[key] = true;
                    AddLog($"URL {key} доступний");

                    // Закриваємо з'єднання
                    response.Close();
                }
                catch (WebException e)
                {
                    // URL недоступний
                    urlValidationResults[key] = false;
                    AddLog($"Помилка доступу до URL {key}: {e.Message}");
                }

                // Необхідно перемалювати вікно, оскільки запит асинхронний
                Repaint();
            }, null);
        }
        catch (Exception ex)
        {
            // Помилка при створенні запиту
            urlValidationResults[key] = false;
            AddLog($"Помилка запиту URL {key}: {ex.Message}");
            Repaint();
        }
    }

    private void ApplySettings()
    {
        isProcessing = true;
        progressBarValue = 0f;
        currentStatus = "Початок налаштування...";
        setupLog.Clear();

        EditorApplication.delayCall += () => StartSettingsProcess();
    }

    private void StartSettingsProcess()
    {
        // Запускаємо налаштування послідовно
        float progressStep = 1f / (
            (setupPlayerSettings ? 1 : 0) +
            (setupQualitySettings ? 1 : 0) +
            (setupProjectSettings ? 1 : 0) +
            (setupGitIgnore ? 1 : 0) +
            (installRequiredPackages ? 1 : 0) +
            (installUniTask ? 1 : 0) +
            (installNuGetForUnity ? 1 : 0)
        );

        float currentProgress = 0f;

        // Player Settings
        if (setupPlayerSettings)
        {
            UpdateStatus("Налаштування Player Settings...");
            SetupPlayerSettings();
            currentProgress += progressStep;
            progressBarValue = currentProgress;
        }

        // Quality Settings
        if (setupQualitySettings)
        {
            UpdateStatus("Налаштування Quality Settings...");
            SetupQualitySettings();
            currentProgress += progressStep;
            progressBarValue = currentProgress;
        }

        // Project Settings
        if (setupProjectSettings)
        {
            UpdateStatus("Налаштування Project Settings...");
            SetupProjectSettings();
            currentProgress += progressStep;
            progressBarValue = currentProgress;
        }

        // GitIgnore
        if (setupGitIgnore)
        {
            UpdateStatus("Налаштування GitIgnore...");
            SetupGitIgnore();
            currentProgress += progressStep;
            progressBarValue = currentProgress;
        }

        // Packages
        if (installRequiredPackages)
        {
            UpdateStatus("Встановлення пакетів...");
            SetupPackages();
            currentProgress += progressStep;
            progressBarValue = currentProgress;
        }

        // UniTask
        if (installUniTask)
        {
            UpdateStatus("Встановлення UniTask...");
            InstallUniTask();
            currentProgress += progressStep;
            progressBarValue = currentProgress;
        }

        // NuGetForUnity
        if (installNuGetForUnity)
        {
            UpdateStatus("Встановлення NuGetForUnity...");
            InstallNuGetForUnity();
            currentProgress += progressStep;
            progressBarValue = currentProgress;
        }

        UpdateStatus("Налаштування завершено");
        AddLog("Всі налаштування застосовано успішно");

        progressBarValue = 1f;
        isProcessing = false;
        AssetDatabase.Refresh();

        // Перехід до екрану завершення
        currentMode = WizardMode.Complete;

        EditorUtility.DisplayDialog("Успіх", "Налаштування проекту завершено успішно!", "OK");
    }

    private void SetupPlayerSettings()
    {
        try
        {
            PlayerSettings.companyName = companyName;
            PlayerSettings.productName = productName;

            // Налаштування для різних платформ
            switch (targetPlatform)
            {
                case TargetPlatform.PC:
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Standalone, bundleIdentifier);
                    break;

                case TargetPlatform.Mobile:
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, bundleIdentifier);
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, bundleIdentifier);
                    // Додаткові налаштування для мобільних
                    PlayerSettings.MTRendering = true;
                    PlayerSettings.SetMobileMTRendering(NamedBuildTarget.Android, true);
                    PlayerSettings.SetMobileMTRendering(NamedBuildTarget.Android, true);
                    PlayerSettings.SetMobileMTRendering(NamedBuildTarget.iOS, true);
                    AddLog("Оптимізовано настройки для мобільних платформ");
                    break;

                case TargetPlatform.WebGL:
                    PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.WebGL, bundleIdentifier);
                    // Специфічні налаштування WebGL
#if UNITY_2020_1_OR_NEWER
                    PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
                    PlayerSettings.WebGL.dataCaching = true;
                    PlayerSettings.WebGL.threadsSupport = false;
                    PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
                    PlayerSettings.WebGL.memorySize = 512;
#endif
                    AddLog("Оптимізовано настройки для WebGL");
                    break;

              
            }

            if (setScriptingBackend)
            {
                switch (targetPlatform)
                {
                    case TargetPlatform.PC:
                        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, scriptingBackend);
                        break;
                    case TargetPlatform.Mobile:
                        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, scriptingBackend);
                        PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, scriptingBackend);
                        break;
                    case TargetPlatform.WebGL:
                        // WebGL має тільки IL2CPP
                        PlayerSettings.SetScriptingBackend(NamedBuildTarget.WebGL, ScriptingImplementation.IL2CPP);
                        break;
                    
                }

                AddLog($"Встановлено Scripting Backend: {scriptingBackend}");
            }

            if (setApiCompatibility)
            {
                switch (targetPlatform)
                {
                    case TargetPlatform.PC:
                        PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Standalone, apiCompatibility);
                        break;
                    case TargetPlatform.Mobile:
                        PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.Android, apiCompatibility);
                        PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.iOS, apiCompatibility);
                        break;
                    case TargetPlatform.WebGL:
                        PlayerSettings.SetApiCompatibilityLevel(NamedBuildTarget.WebGL, apiCompatibility);
                        break;
                   
                }

                AddLog($"Встановлено API Compatibility Level: {apiCompatibility}");
            }

            AddLog("Налаштування Player Settings успішно застосовані");
        }
        catch (Exception ex)
        {
            AddLog($"Помилка в налаштуванні Player Settings: {ex.Message}");
        }
    }

    private void SetupQualitySettings()
    {
        try
        {
            QualitySettings.SetQualityLevel(defaultQualityLevel);
            AddLog($"Встановлено рівень якості за замовчуванням: {QualitySettings.names[defaultQualityLevel]}");

            if (setupPCQuality)
            {
                // Налаштування для PC
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                QualitySettings.antiAliasing = 4;
                QualitySettings.softParticles = true;
                QualitySettings.realtimeReflectionProbes = true;
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.shadowResolution = ShadowResolution.High;
                QualitySettings.shadowDistance = 150f;
                QualitySettings.lodBias = 1f;

                AddLog("Налаштовано якість для PC");
            }

            if (setupMobileQuality)
            {
                // Налаштування для мобільних
                int mobileQualityIndex = QualitySettings.names.ToList().IndexOf("Low");
                if (mobileQualityIndex != -1)
                {
                    // Встановлюємо специфічні налаштування для мобільної платформи
                    QualitySettings.SetQualityLevel(mobileQualityIndex);
                    QualitySettings.vSyncCount = 0; // FPS не обмежений vsync
                    QualitySettings.antiAliasing = 0; // Без Anti-aliasing для продуктивності
                    QualitySettings.softParticles = false;
                    QualitySettings.realtimeReflectionProbes = false;
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.shadowResolution = ShadowResolution.Low;
                    QualitySettings.shadowDistance = 50f;
                    QualitySettings.lodBias = 0.7f;

                    // Повертаємося до початкового рівня якості
                    QualitySettings.SetQualityLevel(defaultQualityLevel);
                }

                AddLog("Налаштовано якість для мобільних платформ");
            }

            if (setupWebGLQuality)
            {
                // Налаштування для WebGL
                int webGLQualityIndex = QualitySettings.names.ToList().IndexOf("Medium");
                if (webGLQualityIndex != -1)
                {
                    QualitySettings.SetQualityLevel(webGLQualityIndex);
                    QualitySettings.vSyncCount = 0;
                    QualitySettings.antiAliasing = 2;
                    QualitySettings.softParticles = false;
                    QualitySettings.realtimeReflectionProbes = false;
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.shadowResolution = ShadowResolution.Medium;
                    QualitySettings.shadowDistance = 80f;
                    QualitySettings.lodBias = 0.8f;

                    // Повертаємося до початкового рівня якості
                    QualitySettings.SetQualityLevel(defaultQualityLevel);
                }

                AddLog("Налаштовано якість для WebGL");
            }

           
        }
        catch (Exception ex)
        {
            AddLog($"Помилка в налаштуванні Quality Settings: {ex.Message}");
        }
    }

    private void SetupProjectSettings()
    {
        try
        {
            // Color Space
            PlayerSettings.colorSpace = colorSpace;
            AddLog($"Встановлено Color Space: {colorSpace}");

            // Physics and other project settings 
            Physics.simulationMode = SimulationMode.FixedUpdate;

            Physics.defaultContactOffset = 0.01f;
            Physics.sleepThreshold = 0.005f;

            // Lightmapping налаштування для цільової платформи
            switch (targetPlatform)
            {
                case TargetPlatform.PC:
                    LightmapSettings.lightmapsMode = LightmapsMode.CombinedDirectional;
                    break;
                case TargetPlatform.Mobile:
                    LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
                    break;
                case TargetPlatform.WebGL:
                    LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
                    break;
               
            }

            // Створення стартової сцени, якщо її ще не існує
            string scenesFolder = "Assets/_MythHunter/Scenes";
            if (!Directory.Exists(scenesFolder))
            {
                Directory.CreateDirectory(scenesFolder);
                AddLog($"Створено папку для сцен: {scenesFolder}");
            }

            string startScenePath = Path.Combine(scenesFolder, $"{startSceneName}.unity");
            if (!File.Exists(startScenePath))
            {
                AddLog($"Потрібно створити стартову сцену: {startScenePath}");
                // Примітка: фактичне створення сцени потребує додаткового коду
            }

            AddLog("Налаштування Project Settings успішно застосовані");
        }
        catch (Exception ex)
        {
            AddLog($"Помилка в налаштуванні Project Settings: {ex.Message}");
        }
    }

    private void SetupGitIgnore()
    {
        try
        {
            string gitIgnorePath = Path.Combine(Application.dataPath, "..", ".gitignore");

            if (useStandardGitIgnore)
            {
                string standardGitIgnore = GetStandardGitIgnore();

                // Додаємо специфічні правила в залежності від платформи
                switch (targetPlatform)
                {
                    case TargetPlatform.PC:
                        standardGitIgnore += "\n\n# PC Specific\n*.exe\n*.pdb\n";
                        break;
                    case TargetPlatform.Mobile:
                        standardGitIgnore += "\n\n# Mobile Specific\n*.apk\n*.aab\n*.ipa\n/[Kk]eystore/\n";
                        break;
                    case TargetPlatform.WebGL:
                        standardGitIgnore += "\n\n# WebGL Specific\n/WebGL/\n*.wasm\n";
                        break;
                   
                }

                if (addCustomGitIgnoreRules && !string.IsNullOrEmpty(customGitIgnoreRules))
                {
                    standardGitIgnore += "\n\n# Custom Rules\n" + customGitIgnoreRules;
                }

                File.WriteAllText(gitIgnorePath, standardGitIgnore);
                AddLog("Створено .gitignore файл з налаштуваннями для платформи " + targetPlatform.ToString());
            }
            else if (addCustomGitIgnoreRules && !string.IsNullOrEmpty(customGitIgnoreRules))
            {
                if (File.Exists(gitIgnorePath))
                {
                    string currentGitIgnore = File.ReadAllText(gitIgnorePath);

                    if (!currentGitIgnore.Contains("# Custom Rules"))
                    {
                        currentGitIgnore += "\n\n# Custom Rules\n" + customGitIgnoreRules;
                        File.WriteAllText(gitIgnorePath, currentGitIgnore);
                        AddLog("Додано користувацькі правила до існуючого .gitignore файлу");
                    }
                    else
                    {
                        AddLog("Користувацькі правила вже присутні в .gitignore файлі");
                    }
                }
                else
                {
                    File.WriteAllText(gitIgnorePath, "# Custom Rules\n" + customGitIgnoreRules);
                    AddLog("Створено новий .gitignore файл з користувацькими правилами");
                }
            }
        }
        catch (Exception ex)
        {
            AddLog($"Помилка в налаштуванні GitIgnore: {ex.Message}");
        }
    }

    private void SetupPackages()
    {
        try
        {
            // Отримуємо список встановлених пакетів
            packageListRequest = Client.List();
            EditorApplication.update += OnPackageListComplete;
            AddLog("Запит списку встановлених пакетів...");
        }
        catch (Exception ex)
        {
            AddLog($"Помилка при встановленні пакетів: {ex.Message}");
            isProcessing = false;
        }
    }

    private void OnPackageListComplete()
    {
        if (packageListRequest == null || !packageListRequest.IsCompleted)
            return;

        EditorApplication.update -= OnPackageListComplete;

        if (packageListRequest.Status == StatusCode.Success)
        {
            AddLog("Отримано список встановлених пакетів");

            var installedPackages = packageListRequest.Result.Select(p => p.name).ToList();
            List<string> packagesToInstall = new List<string>();

            // Додаємо обов'язкові пакети, яких ще немає
            foreach (var package in essentialPackages)
            {
                if (!installedPackages.Contains(package))
                {
                    packagesToInstall.Add(package);
                }
            }

            // Додаємо вибрані опціональні пакети
            foreach (var package in optionalPackages)
            {
                if (package.Value && !installedPackages.Contains(package.Key))
                {
                    packagesToInstall.Add(package.Key);
                }
            }

            // Додаємо платформозалежні пакети
            switch (targetPlatform)
            {
                case TargetPlatform.PC:
                    if (!installedPackages.Contains("com.unity.probuilder") && !packagesToInstall.Contains("com.unity.probuilder"))
                        packagesToInstall.Add("com.unity.probuilder");
                    break;

                case TargetPlatform.Mobile:
                    if (!installedPackages.Contains("com.unity.mobile.android-logcat") && !packagesToInstall.Contains("com.unity.mobile.android-logcat"))
                        packagesToInstall.Add("com.unity.mobile.android-logcat");
                    break;

                case TargetPlatform.WebGL:
                    // Пакети специфічні для WebGL
                    break;

               
            }

            if (packagesToInstall.Count > 0)
            {
                StartInstallingPackages(packagesToInstall);
            }
            else
            {
                AddLog("Усі необхідні пакети вже встановлені");
            }
        }
        else
        {
            AddLog($"Помилка при отриманні списку пакетів: {packageListRequest.Error.message}");
        }
    }

    private void StartInstallingPackages(List<string> packagesToInstall)
    {
        AddLog($"Необхідно встановити {packagesToInstall.Count} пакетів");
        StartInstallPackage(packagesToInstall, 0);
    }

    private void StartInstallPackage(List<string> packages, int index)
    {
        if (index >= packages.Count)
        {
            AddLog("Всі пакети успішно встановлені");
            return;
        }

        string packageName = packages[index];
        AddLog($"Встановлення пакету {packageName}...");

        packageAddRequest = Client.Add(packageName);
        EditorApplication.update += () => OnPackageInstallComplete(packages, index);
    }

    private void OnPackageInstallComplete(List<string> packages, int currentIndex)
    {
        if (packageAddRequest == null || !packageAddRequest.IsCompleted)
            return;

        EditorApplication.update -= () => OnPackageInstallComplete(packages, currentIndex);

        if (packageAddRequest.Status == StatusCode.Success)
        {
            AddLog($"Пакет {packages[currentIndex]} успішно встановлено");
        }
        else
        {
            AddLog($"Помилка при встановленні пакету {packages[currentIndex]}: {packageAddRequest.Error.message}");
        }

        // Встановлюємо наступний пакет
        StartInstallPackage(packages, currentIndex + 1);
    }

    private void InstallUniTask()
    {
        try
        {
            if (string.IsNullOrEmpty(uniTaskGitUrl))
            {
                AddLog("URL для UniTask не вказано");
                return;
            }

            // Перевірка URL перед встановленням, якщо потрібно
            if (validateUniTaskUrl && (!urlValidationResults.ContainsKey("UniTask") || !urlValidationResults["UniTask"]))
            {
                AddLog("URL для UniTask недоступний або не валідований. Пропускаємо встановлення.");
                return;
            }

            AddLog("Встановлення UniTask з Git...");

            packageAddRequest = Client.Add(uniTaskGitUrl);
            EditorApplication.update += OnUniTaskInstallComplete;
        }
        catch (Exception ex)
        {
            AddLog($"Помилка при встановленні UniTask: {ex.Message}");
        }
    }

    private void OnUniTaskInstallComplete()
    {
        if (packageAddRequest == null || !packageAddRequest.IsCompleted)
            return;

        EditorApplication.update -= OnUniTaskInstallComplete;

        if (packageAddRequest.Status == StatusCode.Success)
        {
            AddLog("UniTask успішно встановлено");
        }
        else
        {
            AddLog($"Помилка при встановленні UniTask: {packageAddRequest.Error.message}");
        }
    }

    private void InstallNuGetForUnity()
    {
        try
        {
            if (string.IsNullOrEmpty(nuGetForUnityUrl))
            {
                AddLog("URL для NuGetForUnity не вказано");
                return;
            }

            // Перевірка URL перед встановленням, якщо потрібно
            if (validateNuGetUrl && (!urlValidationResults.ContainsKey("NuGet") || !urlValidationResults["NuGet"]))
            {
                AddLog("URL для NuGetForUnity недоступний або не валідований. Пропускаємо встановлення.");
                return;
            }

            AddLog("Встановлення NuGetForUnity з Git...");

            packageAddRequest = Client.Add(nuGetForUnityUrl);
            EditorApplication.update += OnNuGetForUnityInstallComplete;
        }
        catch (Exception ex)
        {
            AddLog($"Помилка при встановленні NuGetForUnity: {ex.Message}");
        }
    }

    private void OnNuGetForUnityInstallComplete()
    {
        if (packageAddRequest == null || !packageAddRequest.IsCompleted)
            return;

        EditorApplication.update -= OnNuGetForUnityInstallComplete;

        if (packageAddRequest.Status == StatusCode.Success)
        {
            AddLog("NuGetForUnity успішно встановлено");
        }
        else
        {
            AddLog($"Помилка при встановленні NuGetForUnity: {packageAddRequest.Error.message}");
        }
    }

    private void ShowProjectStructureWizard()
    {
        MythHunterStructureWizard.ShowWindow();
    }

    private void UpdateStatus(string status)
    {
        currentStatus = status;
        AddLog(status);
        Repaint();
    }

    private void AddLog(string message)
    {
        setupLog.Add($"[{DateTime.Now.ToString("HH:mm:ss")}] {message}");
        Debug.Log($"[MythHunterSetup] {message}");
        Repaint();
    }

    private string GetStandardGitIgnore()
    {
        return @"# This .gitignore file should be placed at the root of your Unity project directory
#
# Get latest from https://github.com/github/gitignore/blob/main/Unity.gitignore
#
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Uu]ser[Ss]ettings/

# MemoryCaptures can get excessive in size.
# They also could contain extremely sensitive data
/[Mm]emoryCaptures/

# Recordings can get excessive in size
/[Rr]ecordings/

# Uncomment this line if you wish to ignore the asset store tools plugin
# /[Aa]ssets/AssetStoreTools*

# Autogenerated Jetbrains Rider plugin
/[Aa]ssets/Plugins/Editor/JetBrains*

# Visual Studio cache directory
.vs/

# Gradle cache directory
.gradle/

# Autogenerated VS/MD/Consulo solution and project files
ExportedObj/
.consulo/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db

# Unity3D generated meta files
*.pidb.meta
*.pdb.meta
*.mdb.meta

# Unity3D generated file on crash reports
sysinfo.txt

# Builds
*.apk
*.aab
*.unitypackage
*.app

# Crashlytics generated file
crashlytics-build.properties

# Packed Addressables
/[Aa]ssets/[Aa]ddressable[Aa]ssets[Dd]ata/*/*.bin*

# Temporary auto-generated Android Assets
/[Aa]ssets/[Ss]treamingAssets/aa.meta
/[Aa]ssets/[Ss]treamingAssets/aa/*

# MythHunter specific
/[Aa]ssets/_MythHunter/[Tt]emp/
/[Aa]ssets/_MythHunter/[Bb]uilds/
*.log";
    }
}
