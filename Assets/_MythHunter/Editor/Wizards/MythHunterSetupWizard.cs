using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using System.Linq;

/// <summary>
/// Візард для початкового налаштування проекту Unity перед створенням структури MythHunter.
/// </summary>
public class MythHunterSetupWizard : EditorWindow
{
    private Vector2 scrollPosition;

    // Налаштування проекту
    private bool setupPlayerSettings = true;
    private bool setupQualitySettings = true;
    private bool setupProjectSettings = true;
    private bool setupGitIgnore = true;
    private bool installRequiredPackages = true;

    // Player Settings
    private string companyName = "YourCompany";
    private string productName = "MythHunter";
    private string bundleIdentifier = "com.yourcompany.mythhunter";
    private bool setScriptingBackend = true;
    private ScriptingImplementation scriptingBackend = ScriptingImplementation.IL2CPP;
    private bool setApiCompatibility = true;
    private ApiCompatibilityLevel apiCompatibility = ApiCompatibilityLevel.NET_4_6;
    private bool optimizeForMobile = true;

    // Quality Settings
    private bool setupPCQuality = true;
    private bool setupMobileQuality = true;
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

    // UniTask
    private bool installUniTask = true;
    private string uniTaskGitUrl = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask";

    // Стан
    private bool showAdvancedSettings = false;
    private bool isProcessing = false;
    private string currentStatus = "";
    private float progressBarValue = 0f;
    private List<string> setupLog = new List<string>();
    private AddRequest packageAddRequest;
    private ListRequest packageListRequest;
    private bool showSetupLog = false;

    [MenuItem("MythHunter Tools/Project Setup Wizard")]
    public static void ShowWindow()
    {
        GetWindow<MythHunterSetupWizard>("MythHunter Setup Wizard");
    }

    private void OnGUI()
    {
        GUILayout.Label("MythHunter Project Setup Wizard", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Цей інструмент налаштує Unity для оптимальної роботи з проектом MythHunter.", MessageType.Info);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space(10);
        DrawSetupOptions();

        EditorGUILayout.Space(10);
        DrawPlayerSettings();

        EditorGUILayout.Space(10);
        DrawOtherSettings();

        EditorGUILayout.Space(10);
        DrawPackages();

        EditorGUILayout.Space(10);
        DrawGitSettings();

        EditorGUILayout.Space(10);
        DrawButtons();

        if (isProcessing)
        {
            EditorGUILayout.Space(5);
            DrawProgressBar();
        }

        DrawSetupLog();

        EditorGUILayout.EndScrollView();
    }

    private void DrawSetupOptions()
    {
        GUILayout.Label("Налаштування для застосування:", EditorStyles.boldLabel);
        setupPlayerSettings = EditorGUILayout.Toggle("Налаштувати Player Settings", setupPlayerSettings);
        setupQualitySettings = EditorGUILayout.Toggle("Налаштувати Quality Settings", setupQualitySettings);
        setupProjectSettings = EditorGUILayout.Toggle("Налаштувати Project Settings", setupProjectSettings);
        setupGitIgnore = EditorGUILayout.Toggle("Налаштувати GitIgnore", setupGitIgnore);
        installRequiredPackages = EditorGUILayout.Toggle("Встановити необхідні пакети", installRequiredPackages);
        installUniTask = EditorGUILayout.Toggle("Встановити UniTask", installUniTask);

        showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Розширені налаштування");
    }

    private void DrawPlayerSettings()
    {
        if (!showAdvancedSettings || !setupPlayerSettings)
            return;

        GUILayout.Label("Player Settings:", EditorStyles.boldLabel);

        EditorGUI.indentLevel++;
        companyName = EditorGUILayout.TextField("Company Name", companyName);
        productName = EditorGUILayout.TextField("Product Name", productName);
        bundleIdentifier = EditorGUILayout.TextField("Bundle Identifier", bundleIdentifier);

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

        optimizeForMobile = EditorGUILayout.Toggle("Optimize for Mobile", optimizeForMobile);
        EditorGUI.indentLevel--;
    }

    private void DrawOtherSettings()
    {
        if (!showAdvancedSettings)
            return;

        if (setupQualitySettings)
        {
            GUILayout.Label("Quality Settings:", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            setupPCQuality = EditorGUILayout.Toggle("Setup PC Quality", setupPCQuality);
            setupMobileQuality = EditorGUILayout.Toggle("Setup Mobile Quality", setupMobileQuality);
            defaultQualityLevel = EditorGUILayout.IntSlider("Default Quality Level", defaultQualityLevel, 0, 5);
            EditorGUI.indentLevel--;
        }

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
    }

    private void DrawPackages()
    {
        if (!showAdvancedSettings || !installRequiredPackages)
            return;

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

        if (installUniTask)
        {
            EditorGUILayout.Space(5);
            GUILayout.Label("UniTask Settings:", EditorStyles.miniBoldLabel);
            EditorGUI.indentLevel++;
            uniTaskGitUrl = EditorGUILayout.TextField("Git URL", uniTaskGitUrl);
            EditorGUI.indentLevel--;
        }

        EditorGUI.indentLevel--;
    }

    private void DrawGitSettings()
    {
        if (!showAdvancedSettings || !setupGitIgnore)
            return;

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

    private void DrawButtons()
    {
        EditorGUILayout.Space(10);

        EditorGUI.BeginDisabledGroup(isProcessing);

        if (GUILayout.Button("Застосувати налаштування", GUILayout.Height(40)))
        {
            ApplySettings();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Відновити налаштування за замовчуванням"))
        {
            ResetToDefaults();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button("Перейти до налаштування структури проекту"))
        {
            ShowProjectStructureWizard();
        }

        EditorGUI.EndDisabledGroup();
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
                foreach (var logEntry in setupLog)
                {
                    EditorGUILayout.LabelField(logEntry);
                }
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Очистити лог"))
                {
                    setupLog.Clear();
                }
            }
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
            (installUniTask ? 1 : 0)
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

        UpdateStatus("Налаштування завершено");
        AddLog("Всі налаштування застосовано успішно");

        progressBarValue = 1f;
        isProcessing = false;
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Успіх", "Налаштування проекту завершено успішно!", "OK");
    }

    private void SetupPlayerSettings()
    {
        try
        {
            PlayerSettings.companyName = companyName;
            PlayerSettings.productName = productName;

#if UNITY_ANDROID
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, bundleIdentifier);
#elif UNITY_IOS
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, bundleIdentifier);
#else
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, bundleIdentifier);
#endif

            if (setScriptingBackend)
            {
#if UNITY_ANDROID
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, scriptingBackend);
#elif UNITY_IOS
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, scriptingBackend);
#else
                PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, scriptingBackend);
#endif

                AddLog($"Встановлено Scripting Backend: {scriptingBackend}");
            }

            if (setApiCompatibility)
            {
#if UNITY_ANDROID
                PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, apiCompatibility);
#elif UNITY_IOS
                PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.iOS, apiCompatibility);
#else
                PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, apiCompatibility);
#endif

                AddLog($"Встановлено API Compatibility Level: {apiCompatibility}");
            }

            if (optimizeForMobile)
            {
#if UNITY_ANDROID || UNITY_IOS
                // Оптимізація для мобільних платформ
                PlayerSettings.MTRendering = true;
                PlayerSettings.mobileMTRendering = true;
                PlayerSettings.SetMobileMTRendering(BuildTargetGroup.Android, true);
                PlayerSettings.SetMobileMTRendering(BuildTargetGroup.iOS, true);
                AddLog("Встановлено оптимізації для мобільних платформ");
#endif
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
            if (setupPCQuality)
            {
                // Налаштування для PC
                QualitySettings.SetQualityLevel(defaultQualityLevel);

                // High Quality
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.ForceEnable;
                QualitySettings.antiAliasing = 4;
                QualitySettings.softParticles = true;
                QualitySettings.realtimeReflectionProbes = true;

                AddLog("Налаштовано якість для PC");
            }

            if (setupMobileQuality)
            {
                // Специфічні налаштування для мобільних
#if UNITY_ANDROID || UNITY_IOS
                QualitySettings.vSyncCount = 0; // FPS не обмежений vsync
                QualitySettings.antiAliasing = 0; // Без Anti-aliasing для продуктивності
                QualitySettings.softParticles = false;
                QualitySettings.realtimeReflectionProbes = false;
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.shadowResolution = ShadowResolution.Low;
                QualitySettings.shadowDistance = 50f;
                QualitySettings.lodBias = 0.7f;
                
                AddLog("Налаштовано якість для мобільних платформ");
#endif
            }

            AddLog($"Встановлено рівень якості за замовчуванням: {QualitySettings.names[defaultQualityLevel]}");
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

            // New Input System налаштовується через Package Manager автоматично

            // Physics and other project settings can be set here
            Physics.autoSimulation = true;
            Physics.defaultContactOffset = 0.01f;
            Physics.sleepThreshold = 0.005f;

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

                if (addCustomGitIgnoreRules && !string.IsNullOrEmpty(customGitIgnoreRules))
                {
                    standardGitIgnore += "\n\n# Custom Rules\n" + customGitIgnoreRules;
                }

                File.WriteAllText(gitIgnorePath, standardGitIgnore);
                AddLog("Створено стандартний .gitignore файл для Unity");
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
            // Спочатку отримаємо список встановлених пакетів
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

    private void ResetToDefaults()
    {
        // Reset all settings to their defaults
        setupPlayerSettings = true;
        setupQualitySettings = true;
        setupProjectSettings = true;
        setupGitIgnore = true;
        installRequiredPackages = true;
        installUniTask = true;

        companyName = "YourCompany";
        productName = "MythHunter";
        bundleIdentifier = "com.yourcompany.mythhunter";
        setScriptingBackend = true;
        scriptingBackend = ScriptingImplementation.IL2CPP;
        setApiCompatibility = true;
        apiCompatibility = ApiCompatibilityLevel.NET_4_6;
        optimizeForMobile = true;

        setupPCQuality = true;
        setupMobileQuality = true;
        defaultQualityLevel = 3;

        colorSpace = ColorSpace.Linear;
        enableNewInputSystem = true;
        enableAddressables = true;

        useStandardGitIgnore = true;
        addCustomGitIgnoreRules = true;
        customGitIgnoreRules = "# Custom rules\n/Assets/Temp/\n/Assets/Builds/\n*.log";

        uniTaskGitUrl = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask";

        // Reset optional packages
        foreach (var key in optionalPackages.Keys.ToList())
        {
            if (key == "com.unity.postprocessing" || key == "com.unity.render-pipelines.universal" || key == "com.unity.animation" || key == "com.unity.timeline")
                optionalPackages[key] = true;
            else
                optionalPackages[key] = false;
        }

        AddLog("Налаштування скинуто до значень за замовчуванням");
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
