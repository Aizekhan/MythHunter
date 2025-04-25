using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class UnifiedMythHunterWizard : EditorWindow
{
    private enum WizardType { ECS, MVP_UI }
    private WizardType wizardType;

    // ECS fields
    private enum ECSType { Component, System, Event }
    private ECSType ecsType;
    private string ecsName = "MyFeature";
    private string ecsDomain = "Core";
    private bool includeNetworking = false;

    // MVP UI fields
    private string panelName = "MyPanel";
    private bool includeViewFinder = true;
    private bool includeTest = true;
    private bool createPrefab = true;

    private List<string> createdFiles = new();

    [MenuItem("MythHunter Tools/Wizard")]
    public static void ShowWindow() => GetWindow<UnifiedMythHunterWizard>("MythHunter Wizard");

    private void OnGUI()
    {
        wizardType = (WizardType)GUILayout.Toolbar((int)wizardType, new string[] { "ECS", "MVP UI" });

        EditorGUILayout.Space(10);

        switch (wizardType)
        {
            case WizardType.ECS:
                DrawECSWizard();
                break;
            case WizardType.MVP_UI:
                DrawMvpUiWizard();
                break;
        }
    }

    private void DrawECSWizard()
    {
        GUILayout.Label("🧙 ECS Wizard", EditorStyles.boldLabel);
        ecsType = (ECSType)EditorGUILayout.EnumPopup(new GUIContent("Тип ECS:", "Виберіть тип ECS-об'єкта для генерації."), ecsType);
        ecsName = EditorGUILayout.TextField("Назва:", ecsName);
        ecsDomain = EditorGUILayout.TextField("Домен:", ecsDomain);
        includeNetworking = EditorGUILayout.Toggle("Include Networking", includeNetworking);

        if (GUILayout.Button("Створити ECS"))
        {
            createdFiles.Clear();
            CreateEcsTemplates(ecsType, ecsName, ecsDomain, includeNetworking);
            AssetDatabase.Refresh();
            Debug.Log("✅ ECS Templates created:");
            foreach (var file in createdFiles)
                Debug.Log("  • " + file);
        }
    }

    private void DrawMvpUiWizard()
    {
        GUILayout.Label("📐 MVP UI Panel", EditorStyles.boldLabel);
        panelName = EditorGUILayout.TextField("Назва панелі:", panelName);
        includeViewFinder = EditorGUILayout.Toggle("Include ViewFinder", includeViewFinder);
        includeTest = EditorGUILayout.Toggle("Include Test", includeTest);
        createPrefab = EditorGUILayout.Toggle("Create Prefab", createPrefab);

        if (GUILayout.Button("Створити MVP Panel"))
        {
            createdFiles.Clear();
            CreateMvpPanel(panelName, includeViewFinder, includeTest, createPrefab);
            AutoRegisterInstaller(panelName + "Installer");
            AssetDatabase.Refresh();
            Debug.Log("✅ MVP Panel created:");
            foreach (var file in createdFiles)
                Debug.Log("  • " + file);
        }
    }

    private void CreateEcsTemplates(ECSType type, string name, string domain, bool network)
    {
        string root = "Assets/_MythHunter";

        try
        {
            switch (type)
            {
                case ECSType.Component:
                    WriteFile($"{root}/Components/{domain}/{name}Component.cs", $"namespace MythHunter.Components.{domain} {{ public struct {name}Component : IComponent {{ }} }}");
                    if (network)
                        WriteFile($"{root}/Networking/{domain}/{name}NetworkComponent.cs", $"namespace MythHunter.Networking.{domain} {{ public struct {name}NetworkComponent : INetworkSerializable {{ }} }}");
                    break;

                case ECSType.System:
                    WriteFile($"{root}/Systems/{domain}/{name}System.cs", $"namespace MythHunter.Systems.{domain} {{ public class {name}System : SystemBase {{ public override void Update(float deltaTime) {{ }} }} }}");
                    if (network)
                    {
                        WriteFile($"{root}/Networking/{domain}/{name}ClientSystem.cs", $"namespace MythHunter.Networking.{domain} {{ public class {name}ClientSystem : ClientSystemBase {{ }} }}");
                        WriteFile($"{root}/Networking/{domain}/{name}ServerSystem.cs", $"namespace MythHunter.Networking.{domain} {{ public class {name}ServerSystem : ServerSystemBase {{ }} }}");
                    }
                    break;

                case ECSType.Event:
                    WriteFile($"{root}/Events/{domain}/{name}Event.cs", $"namespace MythHunter.Events.{domain} {{ public struct {name}Event : IEvent {{ }} }}");
                    if (network)
                        WriteFile($"{root}/Networking/{domain}/{name}NetworkEvent.cs", $"namespace MythHunter.Networking.{domain} {{ public struct {name}NetworkEvent : INetworkEvent {{ }} }}");
                    break;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error creating ECS Templates: {ex.Message}");
        }
    }

    private void CreateMvpPanel(string panelName, bool viewFinder, bool test, bool prefab)
    {
        string basePath = "Assets/_MythHunter/Code/UI";
        string viewCode = $"using UnityEngine;\nusing UnityEngine.UI;\n\npublic class {panelName}View : MonoBehaviour, IView\n{{\n    [SerializeField] private Text label;\n    [SerializeField] private Button button;\n\n    public void SetText(string text) => label.text = text;\n    public void SetInteractable(bool state) => button.interactable = state;\n}}";

        string presenterCode = $"public class {panelName}Presenter : IPresenter\n{{\n    private readonly {panelName}View view;\n    private readonly {panelName}Model model;\n\n    public {panelName}Presenter({panelName}View view, {panelName}Model model)\n    {{\n        this.view = view;\n        this.model = model;\n    }}\n\n    public void Initialize()\n    {{\n        view.SetText(model.Text);\n        view.SetInteractable(model.IsEnabled);\n    }}\n}}";

        string modelCode = $"public class {panelName}Model : IModel\n{{\n    public string Text {{ get; set; }}\n    public bool IsEnabled {{ get; set; }}\n}}";

        string installerCode = $"using UnityEngine;\n\npublic class {panelName}Installer : MonoBehaviour\n{{\n    [SerializeField] private {panelName}View view;\n\n    private void Awake()\n    {{\n        var model = new {panelName}Model();\n        var presenter = new {panelName}Presenter(view, model);\n        presenter.Initialize();\n    }}\n}}";

        string viewFinderCode = $"using UnityEngine;\n\npublic static class {panelName}ViewFinder\n{{\n    public static {panelName}View FindView()\n    {{\n        return Object.FindObjectOfType<{panelName}View>();\n    }}\n}}";

        string testCode = $"using NUnit.Framework;\n\npublic class {panelName}PresenterTests\n{{\n    [Test]\n    public void Presenter_InitializesViewCorrectly()\n    {{\n        var view = new Mock<{panelName}View>();\n        var model = new {panelName}Model {{ Text = \"Test\", IsEnabled = true }};\n        var presenter = new {panelName}Presenter(view.Object, model);\n        presenter.Initialize();\n        Assert.IsTrue(true); // Replace with real assertions\n    }}\n}}";

        WriteFile($"{basePath}/Views/{panelName}View.cs", viewCode);
        WriteFile($"{basePath}/Presenters/{panelName}Presenter.cs", presenterCode);
        WriteFile($"{basePath}/Models/{panelName}Model.cs", modelCode);
        WriteFile($"{basePath}/Installers/{panelName}Installer.cs", installerCode);
        if (viewFinder) WriteFile($"{basePath}/ViewFinders/{panelName}ViewFinder.cs", viewFinderCode);
        if (test) WriteFile($"Assets/Tests/UI/{panelName}PresenterTests.cs", testCode);

        if (prefab)
        {
            Directory.CreateDirectory("Assets/Resources/Prefabs/UI");
            var go = new GameObject(panelName);
            go.AddComponent(System.Type.GetType(panelName + "View"));
            var prefabPath = $"Assets/Resources/Prefabs/UI/{panelName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            createdFiles.Add(prefabPath);
            Object.DestroyImmediate(go);
        }
    }

    private void AutoRegisterInstaller(string installerName)
    {
        string path = "Assets/_MythHunter/Core/InstallerRegistry.cs";
        string registryLine = $"container.Register<{installerName}>();";

        if (File.Exists(path))
        {
            string content = File.ReadAllText(path);
            if (!content.Contains(registryLine))
            {
                int insertPoint = content.LastIndexOf("}");
                if (insertPoint != -1)
                {
                    content = content.Insert(insertPoint, "    " + registryLine + "\n");
                    File.WriteAllText(path, content);
                    AssetDatabase.Refresh();
                    Debug.Log($"✅ Installer '{installerName}' зареєстровано в DI.");
                }
                else
                {
                    Debug.LogError("InstallerRegistry.cs has unexpected format.");
                }
            }
        }
        else
        {
            Debug.LogError("InstallerRegistry.cs file does not exist.");
        }
    }

    private void WriteFile(string path, string content)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
            createdFiles.Add(path);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"❌ Error writing file '{path}': {ex.Message}");
        }
    }
}
