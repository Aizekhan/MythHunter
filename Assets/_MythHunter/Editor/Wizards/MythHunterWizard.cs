using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
/// Універсальний інструмент для генерації коду відповідно до архітектури MythHunter.
/// Підтримує створення компонентів ECS, систем, подій, MVP UI елементів та ScriptableObjects.
/// </summary>
public class MythHunterWizard : EditorWindow
{
    #region Enums and Constants

    private enum WizardType { ECS, MVP_UI, ScriptableObjects }
    private enum ECSType { Component, System, Event, EntityFactory }
    private enum ComponentCategory { Core, Character, Combat, Movement, Tags }
    private enum SystemCategory { Core, Groups, Phase, Combat, Movement, AI }
    private enum Scope { Client, Server, Shared }

    private static readonly string ROOT_PATH = "Assets/_MythHunter";
    private static readonly string CODE_PATH = ROOT_PATH + "/Code";
    private static readonly Dictionary<string, string> NAMESPACE_MAP = new Dictionary<string, string>
    {
        { "Components", "MythHunter.Components" },
        { "Systems", "MythHunter.Systems" },
        { "Events", "MythHunter.Events" },
        { "Entities", "MythHunter.Entities" },
        { "UI", "MythHunter.UI" },
        { "Data", "MythHunter.Data" },
        { "Core", "MythHunter.Core" },
        { "Networking", "MythHunter.Networking" }
    };

    #endregion

    #region GUI Fields

    private WizardType wizardType;
    private Vector2 scrollPosition;
    private List<string> createdFiles = new List<string>();
    private bool showCreatedFiles = false;
    private string statusMessage = "";
    private MessageType statusMessageType = MessageType.None;

    // ECS fields
    private ECSType ecsType;
    private string ecsName = "MyFeature";
    private string ecsDomain = "Core";
    private ComponentCategory componentCategory;
    private SystemCategory systemCategory;
    private bool implementsInterface = true;
    private bool includeComment = true;
    private bool includeNetworking = false;
    private bool includeSerializable = true;
    private bool includeTest = true;
    private Scope scope = Scope.Shared;

    // MVP UI fields
    private string panelName = "MyPanel";
    private bool includeViewFinder = true;
    private bool includeUiTest = true;
    private bool createPrefab = true;
    private bool createInstaller = true;
    private bool createUIEvent = true;

    // ScriptableObject fields
    private string configName = "MyConfig";
    private bool includeEditor = true;
    private bool createAsset = true;

    #endregion

    [MenuItem("MythHunter Tools/Component Wizard")]
    public static void ShowWindow() => GetWindow<MythHunterWizard>("MythHunter Wizard");

    private void OnGUI()
    {
        DrawToolbar();

        EditorGUILayout.Space(10);

        // Status message
        if (!string.IsNullOrEmpty(statusMessage))
        {
            EditorGUILayout.HelpBox(statusMessage, statusMessageType);
            EditorGUILayout.Space(5);
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        switch (wizardType)
        {
            case WizardType.ECS:
                DrawECSWizard();
                break;
            case WizardType.MVP_UI:
                DrawMvpUiWizard();
                break;
            case WizardType.ScriptableObjects:
                DrawScriptableObjectWizard();
                break;
        }

        DrawCreatedFiles();

        EditorGUILayout.EndScrollView();
    }

    #region UI Drawing Methods

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        if (GUILayout.Button("ECS", EditorStyles.toolbarButton))
            wizardType = WizardType.ECS;

        if (GUILayout.Button("MVP UI", EditorStyles.toolbarButton))
            wizardType = WizardType.MVP_UI;

        if (GUILayout.Button("ScriptableObjects", EditorStyles.toolbarButton))
            wizardType = WizardType.ScriptableObjects;

        EditorGUILayout.EndHorizontal();
    }

    private void DrawECSWizard()
    {
        GUILayout.Label("🧙 ECS Wizard", EditorStyles.boldLabel);

        // Basic properties
        ecsType = (ECSType)EditorGUILayout.EnumPopup(new GUIContent("Тип ECS:", "Виберіть тип ECS-об'єкта для генерації."), ecsType);
        ecsName = EditorGUILayout.TextField("Назва:", ecsName);

        if (string.IsNullOrEmpty(ecsName) || !Regex.IsMatch(ecsName, @"^[a-zA-Z][a-zA-Z0-9_]*$"))
        {
            EditorGUILayout.HelpBox("Назва повинна починатися з літери та містити лише букви, цифри та '_'", MessageType.Warning);
        }

        // Category selection based on type
        if (ecsType == ECSType.Component)
        {
            componentCategory = (ComponentCategory)EditorGUILayout.EnumPopup("Категорія:", componentCategory);
            ecsDomain = componentCategory.ToString();
        }
        else if (ecsType == ECSType.System)
        {
            systemCategory = (SystemCategory)EditorGUILayout.EnumPopup("Категорія:", systemCategory);
            ecsDomain = systemCategory.ToString();
        }
        else
        {
            ecsDomain = EditorGUILayout.TextField("Домен:", ecsDomain);
        }

        // Options
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Опції", EditorStyles.boldLabel);

        implementsInterface = EditorGUILayout.Toggle("Implement Interface", implementsInterface);
        includeComment = EditorGUILayout.Toggle("Include XML Comments", includeComment);

        if (ecsType == ECSType.Component || ecsType == ECSType.Event)
        {
            includeSerializable = EditorGUILayout.Toggle("Implement ISerializable", includeSerializable);
        }

        includeTest = EditorGUILayout.Toggle("Create Test File", includeTest);

        if (ecsType == ECSType.System || ecsType == ECSType.Event)
        {
            includeNetworking = EditorGUILayout.Toggle("Include Networking", includeNetworking);

            if (includeNetworking)
            {
                scope = (Scope)EditorGUILayout.EnumPopup("Scope:", scope);
            }
        }

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Create ECS", GUILayout.Height(30)))
        {
            if (ValidateInput(ecsName))
            {
                createdFiles.Clear();
                CreateEcsTemplates(ecsType, ecsName, ecsDomain, scope);
                AssetDatabase.Refresh();
                SetStatusMessage("✅ ECS Templates created successfully!", MessageType.Info);
            }
        }
    }

    private void DrawMvpUiWizard()
    {
        GUILayout.Label("📐 MVP UI Panel", EditorStyles.boldLabel);

        panelName = EditorGUILayout.TextField("Назва панелі:", panelName);

        if (string.IsNullOrEmpty(panelName) || !Regex.IsMatch(panelName, @"^[a-zA-Z][a-zA-Z0-9_]*$"))
        {
            EditorGUILayout.HelpBox("Назва повинна починатися з літери та містити лише букви, цифри та '_'", MessageType.Warning);
        }

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Опції", EditorStyles.boldLabel);

        includeViewFinder = EditorGUILayout.Toggle("Include ViewFinder", includeViewFinder);
        includeUiTest = EditorGUILayout.Toggle("Include Test", includeUiTest);
        createPrefab = EditorGUILayout.Toggle("Create Prefab", createPrefab);
        createInstaller = EditorGUILayout.Toggle("Create Installer", createInstaller);
        createUIEvent = EditorGUILayout.Toggle("Create UI Events", createUIEvent);

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Create MVP Panel", GUILayout.Height(30)))
        {
            if (ValidateInput(panelName))
            {
                createdFiles.Clear();
                CreateMvpPanel(panelName, includeViewFinder, includeUiTest, createPrefab, createInstaller, createUIEvent);

                if (createInstaller)
                    AutoRegisterInstaller(panelName + "Installer");

                AssetDatabase.Refresh();
                SetStatusMessage("✅ MVP Panel created successfully!", MessageType.Info);
            }
        }
    }

    private void DrawScriptableObjectWizard()
    {
        GUILayout.Label("⚙️ ScriptableObject Generator", EditorStyles.boldLabel);

        configName = EditorGUILayout.TextField("Config Name:", configName);

        if (string.IsNullOrEmpty(configName) || !Regex.IsMatch(configName, @"^[a-zA-Z][a-zA-Z0-9_]*$"))
        {
            EditorGUILayout.HelpBox("Назва повинна починатися з літери та містити лише букви, цифри та '_'", MessageType.Warning);
        }

        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("Опції", EditorStyles.boldLabel);

        includeEditor = EditorGUILayout.Toggle("Create Custom Editor", includeEditor);
        createAsset = EditorGUILayout.Toggle("Create Default Asset", createAsset);

        EditorGUILayout.EndVertical();

        if (GUILayout.Button("Create ScriptableObject", GUILayout.Height(30)))
        {
            if (ValidateInput(configName))
            {
                createdFiles.Clear();
                CreateScriptableObject(configName, includeEditor, createAsset);
                AssetDatabase.Refresh();
                SetStatusMessage("✅ ScriptableObject created successfully!", MessageType.Info);
            }
        }
    }

    private void DrawCreatedFiles()
    {
        if (createdFiles.Count > 0)
        {
            EditorGUILayout.Space(10);

            showCreatedFiles = EditorGUILayout.Foldout(showCreatedFiles, $"Created Files ({createdFiles.Count})");

            if (showCreatedFiles)
            {
                EditorGUILayout.BeginVertical("box");

                foreach (var file in createdFiles)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(file);

                    if (GUILayout.Button("Open", GUILayout.Width(60)))
                    {
                        AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(file));
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }
        }
    }

    #endregion

    #region ECS Generation

    private void CreateEcsTemplates(ECSType type, string name, string domain, Scope scope)
    {
        try
        {
            switch (type)
            {
                case ECSType.Component:
                    CreateComponent(name, domain);
                    break;

                case ECSType.System:
                    CreateSystem(name, domain, scope);
                    break;

                case ECSType.Event:
                    CreateEvent(name, domain, scope);
                    break;

                case ECSType.EntityFactory:
                    CreateEntityFactory(name, domain);
                    break;
            }
        }
        catch (System.Exception ex)
        {
            SetStatusMessage($"❌ Error creating ECS Templates: {ex.Message}", MessageType.Error);
        }
    }

    private void CreateComponent(string name, string domain)
    {
        string componentName = name + "Component";
        string componentPath = $"{CODE_PATH}/Components/{domain}/{componentName}.cs";
        string namespaceName = $"{NAMESPACE_MAP["Components"]}.{domain}";

        string comment = includeComment ? $"/// <summary>\n/// {name} component\n/// </summary>" : "";
        List<string> interfaces = new List<string> { "IComponent" };

        if (includeSerializable)
            interfaces.Add("ISerializable");

        List<string> body = new List<string>();

        if (includeSerializable)
        {
            body.AddRange(new List<string> {
                "public byte[] Serialize()",
                "{",
                "    using (MemoryStream stream = new MemoryStream())",
                "    using (BinaryWriter writer = new BinaryWriter(stream))",
                "    {",
                "        // Write component data",
                "        return stream.ToArray();",
                "    }",
                "}",
                "",
                "public void Deserialize(byte[] data)",
                "{",
                "    using (MemoryStream stream = new MemoryStream(data))",
                "    using (BinaryReader reader = new BinaryReader(stream))",
                "    {",
                "        // Read component data",
                "    }",
                "}"
            });
        }

        string imports = "using MythHunter.Core.ECS;\n";

        if (includeSerializable)
            imports += "using System.IO;\nusing MythHunter.Data.Serialization;\n";

        string content = GetTemplateForStruct(componentName, namespaceName, comment, interfaces, body, imports);
        WriteFile(componentPath, content);

        // Create test if needed
        if (includeTest)
        {
            string testPath = $"{ROOT_PATH}/Tests/Editor/Components/{componentName}Tests.cs";
            string testContent = GetTemplateForComponentTest(componentName, namespaceName);
            WriteFile(testPath, testContent);
        }
    }

    private void CreateSystem(string name, string domain, Scope scope)
    {
        string systemName = name + "System";
        string namespaceName = $"{NAMESPACE_MAP["Systems"]}.{domain}";

        string comment = includeComment ?
            $"/// <summary>\n/// System for handling {name} functionality\n/// </summary>" : "";

        List<string> interfaces = new List<string> { "ISystem" };

        // Це вообще видалити мабуть треба, бо це дублює залежність від IEventSubscriber,IEventSubscriber - 2 рази в створюваних файлах
        //if (implementsInterface)
        //  interfaces.Add("IEventSubscriber");

        List<string> body = new List<string>
        {
            "private readonly IEventBus _eventBus;",
            "private readonly ILogger _logger;",
            "",
            "[Inject]",
            $"public {systemName}(IEventBus eventBus, ILogger logger)",
            "{",
            "    _eventBus = eventBus;",
            "    _logger = logger;",
            "}",
            "",
            "public override void Initialize()",
            "{",
            "    SubscribeToEvents();",
            $"    _logger.LogInfo(\"{systemName} initialized\");",
            "}",
            "",
            "public void SubscribeToEvents()",
            "{",
            "    // Subscribe to events here",
            "    // _eventBus.Subscribe<SomeEvent>(OnSomeEvent);",
            "}",
            "",
            "public void UnsubscribeFromEvents()",
            "{",
            "    // Unsubscribe from events here",
            "    // _eventBus.Unsubscribe<SomeEvent>(OnSomeEvent);",
            "}",
            "",
            "public override void Update(float deltaTime)",
            "{",
            "    // System logic here",
            "}",
            "",
            "public override void Dispose()",
            "{",
            "    UnsubscribeFromEvents();",
            $"    _logger.LogInfo(\"{systemName} disposed\");",
            "}"
        };

        string imports = "using MythHunter.Core.ECS;\nusing MythHunter.Events;\nusing MythHunter.Core.DI;\nusing MythHunter.Utils.Logging;\n";

        if (implementsInterface)
            imports += "using MythHunter.Systems.Core;\n";

        // Main system
        string systemPath = $"{CODE_PATH}/Systems/{domain}/{systemName}.cs";
        string content = GetTemplateForClass(systemName, namespaceName, comment, interfaces, body, imports);

        if (implementsInterface)
            content = content.Replace("public class", "public class").Replace(" : ISystem", " : SystemBase, IEventSubscriber");
        else
            content = content.Replace("public class", "public class").Replace(" : ISystem", " : SystemBase");

        WriteFile(systemPath, content);

        // Create networking variants if needed
        if (includeNetworking)
        {
            if (scope == Scope.Shared || scope == Scope.Client)
            {
                string clientSystemPath = $"{CODE_PATH}/Networking/{domain}/{name}ClientSystem.cs";
                string clientContent = GetTemplateForNetworkSystem(name, domain, true);
                WriteFile(clientSystemPath, clientContent);
            }

            if (scope == Scope.Shared || scope == Scope.Server)
            {
                string serverSystemPath = $"{CODE_PATH}/Networking/{domain}/{name}ServerSystem.cs";
                string serverContent = GetTemplateForNetworkSystem(name, domain, false);
                WriteFile(serverSystemPath, serverContent);
            }
        }

        // Create test if needed
        if (includeTest)
        {
            string testPath = $"{ROOT_PATH}/Tests/Editor/Systems/{systemName}Tests.cs";
            string testContent = GetTemplateForSystemTest(systemName, namespaceName);
            WriteFile(testPath, testContent);
        }
    }

    private void CreateEvent(string name, string domain, Scope scope)
    {
        string eventName = name + "Event";
        string eventPath = $"{CODE_PATH}/Events/Domain/{eventName}.cs";
        string namespaceName = $"{NAMESPACE_MAP["Events"]}.Domain";

        string comment = includeComment ?
            $"/// <summary>\n/// Event raised when {name}\n/// </summary>" : "";

        List<string> interfaces = new List<string> { "IEvent" };

        if (includeSerializable)
            interfaces.Add("ISerializable");

        // Basic properties for the event
        List<string> body = new List<string>
        {
            "// TODO: Add event properties here",
            "",
            "public string GetEventId() => $\"{GetType().Name}_{System.Guid.NewGuid()}\";"
        };

        if (includeSerializable)
        {
            body.AddRange(new List<string> {
                "",
                "public byte[] Serialize()",
                "{",
                "    using (System.IO.MemoryStream stream = new System.IO.MemoryStream())",
                "    using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream))",
                "    {",
                "        // Write event data",
                "        return stream.ToArray();",
                "    }",
                "}",
                "",
                "public void Deserialize(byte[] data)",
                "{",
                "    using (System.IO.MemoryStream stream = new System.IO.MemoryStream(data))",
                "    using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))",
                "    {",
                "        // Read event data",
                "    }",
                "}"
            });
        }

        string imports = "using System;\nusing MythHunter.Events;\n";

        if (includeSerializable)
            imports += "using MythHunter.Data.Serialization;\n";

        string content = GetTemplateForStruct(eventName, namespaceName, comment, interfaces, body, imports);
        WriteFile(eventPath, content);

        // Create networking variants if needed
        if (includeNetworking)
        {
            string networkEventPath = $"{CODE_PATH}/Networking/Messages/{name}Message.cs";
            string networkNamespace = $"{NAMESPACE_MAP["Networking"]}.Messages";
            string networkComment = includeComment ?
                $"/// <summary>\n/// Network message for {name} events\n/// </summary>" : "";

            List<string> networkInterfaces = new List<string> { "INetworkMessage" };

            List<string> networkBody = new List<string>
            {
                $"public {name}MessageType Type {{ get; set; }}",
                "",
                "// Add message properties here",
                "",
                "public string GetMessageId() => $\"{GetType().Name}_{Type}_{System.Guid.NewGuid()}\";",
                "",
                "public byte[] Serialize()",
                "{",
                "    using (System.IO.MemoryStream stream = new System.IO.MemoryStream())",
                "    using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(stream))",
                "    {",
                "        writer.Write((byte)Type);",
                "        // Write message data",
                "        return stream.ToArray();",
                "    }",
                "}",
                "",
                "public void Deserialize(byte[] data)",
                "{",
                "    using (System.IO.MemoryStream stream = new System.IO.MemoryStream(data))",
                "    using (System.IO.BinaryReader reader = new System.IO.BinaryReader(stream))",
                "    {",
                "        Type = ({name}MessageType)reader.ReadByte();",
                "        // Read message data",
                "    }",
                "}"
            };

            string networkImports = "using System;\nusing MythHunter.Networking.Serialization;\n";

            // First create the enum for message types
            string enumPath = $"{CODE_PATH}/Networking/Messages/{name}MessageType.cs";
            string enumContent = $"namespace {networkNamespace}\n{{\n    public enum {name}MessageType\n    {{\n        Default = 0,\n        // Add message types here\n    }}\n}}";

            WriteFile(enumPath, enumContent);

            // Then create the message class
            string messageContent = GetTemplateForStruct($"{name}Message", networkNamespace, networkComment, networkInterfaces, networkBody, networkImports);
            WriteFile(networkEventPath, messageContent);
        }

        // Create test if needed
        if (includeTest)
        {
            string testPath = $"{ROOT_PATH}/Tests/Editor/Events/{eventName}Tests.cs";
            string testContent = GetTemplateForEventTest(eventName, namespaceName);
            WriteFile(testPath, testContent);
        }
    }

    private void CreateEntityFactory(string name, string domain)
    {
        string factoryName = name + "Factory";
        string factoryPath = $"{CODE_PATH}/Entities/{factoryName}.cs";
        string namespaceName = $"{NAMESPACE_MAP["Entities"]}";

        string comment = includeComment ?
            $"/// <summary>\n/// Factory for creating {name} entities\n/// </summary>" : "";

        List<string> interfaces = new List<string> { "IEntityFactory" };

        List<string> body = new List<string>
        {
            "private readonly IEntityManager _entityManager;",
            "private readonly ILogger _logger;",
            "",
            "[Inject]",
            $"public {factoryName}(IEntityManager entityManager, ILogger logger)",
            "{",
            "    _entityManager = entityManager;",
            "    _logger = logger;",
            "}",
            "",
            $"public int Create{name}()",
            "{",
            "    int entityId = _entityManager.CreateEntity();",
            "",
            "    // Add components to the entity",
            "    // _entityManager.AddComponent(entityId, new SomeComponent());",
            "",
            $"    _logger.LogInfo($\"Created {name} entity with ID {{entityId}}\");",
            "",
            "    return entityId;",
            "}"
        };

        string imports = "using MythHunter.Core.ECS;\nusing MythHunter.Core.DI;\nusing MythHunter.Utils.Logging;\n";

        string content = GetTemplateForClass(factoryName, namespaceName, comment, interfaces, body, imports);
        WriteFile(factoryPath, content);

        // Create test if needed
        if (includeTest)
        {
            string testPath = $"{ROOT_PATH}/Tests/Editor/Entities/{factoryName}Tests.cs";
            string testContent = GetTemplateForFactoryTest(factoryName, namespaceName);
            WriteFile(testPath, testContent);
        }
    }

    #endregion

    #region MVP UI Generation

    private void CreateMvpPanel(string name, bool viewFinder, bool includeTest, bool prefab, bool installer, bool includeEvent)
    {
        string basePath = $"{CODE_PATH}/UI";

        // Create View
        string viewCode = GetTemplateForMvpView(name);
        WriteFile($"{basePath}/Views/{name}View.cs", viewCode);

        // Create Presenter
        string presenterCode = GetTemplateForMvpPresenter(name);
        WriteFile($"{basePath}/Presenters/{name}Presenter.cs", presenterCode);

        // Create Model
        string modelCode = GetTemplateForMvpModel(name);
        WriteFile($"{basePath}/Models/{name}Model.cs", modelCode);

        // Create Installer if needed
        if (installer)
        {
            string installerCode = GetTemplateForMvpInstaller(name);
            WriteFile($"{basePath}/Core/{name}Installer.cs", installerCode);
        }

        // Create ViewFinder if needed
        if (viewFinder)
        {
            string viewFinderCode = GetTemplateForMvpViewFinder(name);
            WriteFile($"{basePath}/Core/{name}ViewFinder.cs", viewFinderCode);
        }

        // Create Events if needed
        if (includeEvent)
        {
            string eventCode = GetTemplateForMvpEvent(name);
            WriteFile($"{CODE_PATH}/Events/Domain/UI/{name}Events.cs", eventCode);
        }

        // Create Test if needed
        if (includeTest)
        {
            string testCode = GetTemplateForMvpTest(name);
            WriteFile($"{ROOT_PATH}/Tests/Editor/UI/{name}PresenterTests.cs", testCode);
        }

        // Create Prefab if needed
        if (prefab)
        {
            CreateUIPrefab(name);
        }
    }

    private void CreateUIPrefab(string name)
    {
        try
        {
            string prefabDir = "Assets/Resources/Prefabs/UI";
            Directory.CreateDirectory(prefabDir);

            GameObject go = new GameObject(name);
            go.AddComponent<Canvas>();
            go.AddComponent<UnityEngine.UI.CanvasScaler>();
            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // Create the View component dynamically (reflection needed)
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEngine.MonoBehaviour));
            var viewType = assembly.GetTypes().FirstOrDefault(t => t.Name == $"{name}View");

            if (viewType != null)
                go.AddComponent(viewType);
            else
                EditorUtility.DisplayDialog("Warning", $"Could not find {name}View type. The prefab will be created without the View component.", "OK");

            string prefabPath = $"{prefabDir}/{name}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            createdFiles.Add(prefabPath);

            Object.DestroyImmediate(go);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error creating prefab: {ex.Message}");
        }
    }

    #endregion

    #region ScriptableObject Generation

    private void CreateScriptableObject(string name, bool includeEditor, bool createAsset)
    {
        string configName = name + (name.EndsWith("Config") ? "" : "Config");
        string configPath = $"{CODE_PATH}/Data/ScriptableObjects/{configName}.cs";
        string namespaceName = $"{NAMESPACE_MAP["Data"]}.ScriptableObjects";

        string comment = $"/// <summary>\n/// Configuration data for {name}\n/// </summary>";

        List<string> body = new List<string>
        {
            "[SerializeField]",
            "private string configName = \"Default\";",
            "",
            "[SerializeField]",
            "private int someValue = 0;",
            "",
            "// Add your configuration properties here",
            "",
            "public string ConfigName => configName;",
            "public int SomeValue => someValue;"
        };

        string imports = "using UnityEngine;\n";

        string content = $"{imports}\nnamespace {namespaceName}\n{{\n    {comment}\n    [CreateAssetMenu(fileName = \"{configName}\", menuName = \"MythHunter/Configs/{configName}\")]\n    public class {configName} : ScriptableObject\n    {{\n        {string.Join("\n        ", body)}\n    }}\n}}";

        WriteFile(configPath, content);

        // Create custom editor if needed
        if (includeEditor)
        {
            string editorPath = $"{ROOT_PATH}/Editor/Inspectors/{configName}Inspector.cs";
            string editorContent = GetTemplateForScriptableObjectEditor(configName, namespaceName);
            WriteFile(editorPath, editorContent);
        }

        // Create asset if needed
        if (createAsset)
        {
            string assetDirectory = "Assets/Resources/ScriptableObjects/Configs";
            Directory.CreateDirectory(assetDirectory);

            // We need to wait for the C# script to be compiled before creating the asset
            EditorApplication.delayCall += () =>
            {
                try
                {
                    var asset = ScriptableObject.CreateInstance(namespaceName + "." + configName);
                    if (asset != null)
                    {
                        string assetPath = $"{assetDirectory}/Default{configName}.asset";
                        AssetDatabase.CreateAsset(asset, assetPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        createdFiles.Add(assetPath);
                    }
                    else
                    {
                        Debug.LogError($"Failed to create instance of {configName}. Make sure the code is compiled first.");
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Error creating ScriptableObject asset: {ex.Message}");
                }
            };
        }
    }

    #endregion

    #region Template Methods

    private string GetTemplateForMvpView(string name)
    {
        return $@"using UnityEngine;
using UnityEngine.UI;
using MythHunter.UI.Core;

namespace MythHunter.UI.Views
{{
    /// <summary>
    /// View for {name} panel
    /// </summary>
    public class {name}View : MonoBehaviour, IView
    {{
        [Header(""UI References"")]
        [SerializeField] private Text titleText;
        [SerializeField] private Button actionButton;
        [SerializeField] private Image backgroundImage;

        // UI callbacks
        public event System.Action OnActionButtonClicked;

        private void Awake()
        {{
            if (actionButton != null)
                actionButton.onClick.AddListener(HandleActionButtonClick);
        }}

        private void OnDestroy()
        {{
            if (actionButton != null)
                actionButton.onClick.RemoveListener(HandleActionButtonClick);
        }}

        private void HandleActionButtonClick()
        {{
            OnActionButtonClicked?.Invoke();
        }}

        // Public interface
        public void SetTitle(string title)
        {{
            if (titleText != null)
                titleText.text = title;
        }}

        public void SetButtonInteractable(bool isInteractable)
        {{
            if (actionButton != null)
                actionButton.interactable = isInteractable;
        }}

        public void SetBackgroundColor(Color color)
        {{
            if (backgroundImage != null)
                backgroundImage.color = color;
        }}

        public void Show()
        {{
            gameObject.SetActive(true);
        }}

        public void Hide()
        {{
            gameObject.SetActive(false);
        }}
    }}
}}";
    }

    private string GetTemplateForMvpPresenter(string name)
    {
        return $@"using MythHunter.Events;
using MythHunter.UI.Core;
using MythHunter.UI.Views;
using MythHunter.UI.Models;
using MythHunter.Core.DI;

namespace MythHunter.UI.Presenters
{{
    /// <summary>
    /// Presenter for {name} panel
    /// </summary>
    public class {name}Presenter : IPresenter, IEventSubscriber
    {{
        private readonly {name}View _view;
        private readonly {name}Model _model;
        private readonly IEventBus _eventBus;

        [Inject]
        public {name}Presenter({name}View view, {name}Model model, IEventBus eventBus)
        {{
            _view = view;
            _model = model;
            _eventBus = eventBus;
        }}

        public void Initialize()
        {{
            // Subscribe to view events
            _view.OnActionButtonClicked += HandleActionButtonClick;

            // Subscribe to system events
            SubscribeToEvents();

            // Update view with initial model data
            UpdateView();
        }}

        public void Dispose()
        {{
            // Unsubscribe from view events
            _view.OnActionButtonClicked -= HandleActionButtonClick;

            // Unsubscribe from system events
            UnsubscribeFromEvents();
        }}

        private void UpdateView()
        {{
            _view.SetTitle(_model.Title);
            _view.SetButtonInteractable(_model.IsButtonEnabled);
        }}

        private void HandleActionButtonClick()
        {{
            // Handle button click
            _model.ButtonClickCount++;
            UpdateView();

            // Publish event if needed
            // _eventBus.Publish(new SomeEvent());
        }}

        public void SubscribeToEvents()
        {{
            // Subscribe to events
            // _eventBus.Subscribe<SomeEvent>(OnSomeEvent);
        }}

        public void UnsubscribeFromEvents()
        {{
            // Unsubscribe from events
            // _eventBus.Unsubscribe<SomeEvent>(OnSomeEvent);
        }}

        // Event handlers
        // private void OnSomeEvent(SomeEvent evt) {{ }}
    }}
}}";
    }

    private string GetTemplateForMvpModel(string name)
    {
        return $@"using MythHunter.UI.Core;

namespace MythHunter.UI.Models
{{
    /// <summary>
    /// Model for {name} panel
    /// </summary>
    public class {name}Model : IModel
    {{
        // Properties
        public string Title {{ get; set; }} = ""Default Title"";
        public bool IsButtonEnabled {{ get; set; }} = true;
        public int ButtonClickCount {{ get; set; }} = 0;

        // Methods
        public void Reset()
        {{
            Title = ""Default Title"";
            IsButtonEnabled = true;
            ButtonClickCount = 0;
        }}
    }}
}}";
    }

    private string GetTemplateForMvpInstaller(string name)
    {
        return $@"using UnityEngine;
using MythHunter.UI.Views;
using MythHunter.UI.Models;
using MythHunter.UI.Presenters;
using MythHunter.Events;
using MythHunter.Core.DI;

namespace MythHunter.UI.Core
{{
    /// <summary>
    /// Installer for {name} MVP components
    /// </summary>
    public class {name}Installer : MonoBehaviour
    {{
        [SerializeField] private {name}View view;

        private {name}Presenter _presenter;
        private {name}Model _model;
        private IEventBus _eventBus;

        [Inject]
        public void Construct(IEventBus eventBus)
        {{
            _eventBus = eventBus;
        }}

        private void Awake()
        {{
            if (view == null)
                view = GetComponent<{name}View>();

            _model = new {name}Model();
            _presenter = new {name}Presenter(view, _model, _eventBus);
            _presenter.Initialize();
        }}

        private void OnDestroy()
        {{
            if (_presenter != null)
                _presenter.Dispose();
        }}
    }}
}}";
    }

    private string GetTemplateForMvpViewFinder(string name)
    {
        return $@"using UnityEngine;
using MythHunter.UI.Views;

namespace MythHunter.UI.Core
{{
    /// <summary>
    /// Utility for finding {name} view in scene
    /// </summary>
    public static class {name}ViewFinder
    {{
        public static {name}View FindView()
        {{
            {name}View view = Object.FindObjectOfType<{name}View>();
            
            if (view == null)
                Debug.LogWarning($""{name}View not found in scene."");
                
            return view;
        }}
        
        public static {name}View FindViewWithTag(string tag = ""UI"")
        {{
            GameObject go = GameObject.FindWithTag(tag);
            if (go == null)
                return null;
                
            {name}View view = go.GetComponentInChildren<{name}View>(true);
            
            if (view == null)
                Debug.LogWarning($""{name}View not found in GameObject with tag {{tag}}."");
                
            return view;
        }}
    }}
}}";
    }

    private string GetTemplateForMvpEvent(string name)
    {
        return $@"using System;
using MythHunter.Events;

namespace MythHunter.Events.Domain.UI
{{
    /// <summary>
    /// Event raised when {name} is opened
    /// </summary>
    public struct {name}OpenedEvent : IEvent
    {{
        public DateTime Timestamp;
        
        public string GetEventId() => $""{{GetType().Name}}_{{Guid.NewGuid()}}"";
    }}
    
    /// <summary>
    /// Event raised when {name} is closed
    /// </summary>
    public struct {name}ClosedEvent : IEvent
    {{
        public DateTime Timestamp;
        public bool WasCancelled;
        
        public string GetEventId() => $""{{GetType().Name}}_{{Guid.NewGuid()}}"";
    }}
    
    /// <summary>
    /// Event raised when action is performed in {name}
    /// </summary>
    public struct {name}ActionEvent : IEvent
    {{
        public string ActionType;
        public DateTime Timestamp;
        
        public string GetEventId() => $""{{GetType().Name}}_{{Guid.NewGuid()}}"";
    }}
}}";
    }

    private string GetTemplateForMvpTest(string name)
    {
        return $@"using NUnit.Framework;
using System;
using MythHunter.UI.Views;
using MythHunter.UI.Models;
using MythHunter.UI.Presenters;
using MythHunter.Events;
using NSubstitute;

public class {name}PresenterTests
{{
    private {name}View _mockView;
    private {name}Model _model;
    private IEventBus _mockEventBus;
    private {name}Presenter _presenter;

    [SetUp]
    public void SetUp()
    {{
        // Create mocks
        _mockView = Substitute.For<{name}View>();
        _mockEventBus = Substitute.For<IEventBus>();
        
        // Create real model
        _model = new {name}Model();
        
        // Create presenter
        _presenter = new {name}Presenter(_mockView, _model, _mockEventBus);
    }}

    [Test]
    public void Initialize_UpdatesViewWithModelData()
    {{
        // Arrange
        _model.Title = ""Test Title"";
        _model.IsButtonEnabled = true;

        // Act
        _presenter.Initialize();

        // Assert
        _mockView.Received().SetTitle(""Test Title"");
        _mockView.Received().SetButtonInteractable(true);
    }}

    [Test]
    public void ActionButtonClick_UpdatesModelAndView()
    {{
        // Arrange
        _presenter.Initialize();
        int initialClickCount = _model.ButtonClickCount;

        // Act
        // Simulate button click
        var actionClickedHandler = _mockView.OnActionButtonClicked += Raise.Event<Action>();

        // Assert
        Assert.AreEqual(initialClickCount + 1, _model.ButtonClickCount);
        // Verify view was updated
        _mockView.Received(2).SetTitle(Arg.Any<string>());
        _mockView.Received(2).SetButtonInteractable(Arg.Any<bool>());
    }}

    [Test]
    public void Dispose_UnsubscribesFromEvents()
    {{
        // Arrange
        _presenter.Initialize();

        // Act
        _presenter.Dispose();

        // Assert
        // Verify that view events were unsubscribed
        // This is a bit tricky to test without exposing internal state
        // Typically, you'd verify that subsequent view events don't trigger updates
        
        // Alternatively, verify event bus unsubscriptions
        // _mockEventBus.Received().Unsubscribe(Arg.Any<Action<SomeEvent>>());
        
        Assert.Pass();
    }}
}}";
    }

    private string GetTemplateForScriptableObjectEditor(string configName, string namespaceName)
    {
        return $@"using UnityEditor;
using UnityEngine;
using {namespaceName};

namespace MythHunter.Editor.Inspectors
{{
    [CustomEditor(typeof({configName}))]
    public class {configName}Inspector : UnityEditor.Editor
    {{
        private SerializedProperty configNameProperty;
        private SerializedProperty someValueProperty;
        
        private void OnEnable()
        {{
            configNameProperty = serializedObject.FindProperty(""configName"");
            someValueProperty = serializedObject.FindProperty(""someValue"");
        }}

        public override void OnInspectorGUI()
        {{
            serializedObject.Update();
            
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField(""{configName} Settings"", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(configNameProperty, new GUIContent(""Config Name"", ""Unique name for this configuration""));
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField(""Parameters"", EditorStyles.boldLabel);
            
            EditorGUILayout.PropertyField(someValueProperty, new GUIContent(""Some Value"", ""Description for the value""));
            
            // Add other properties here
            
            EditorGUILayout.Space(10);
            if (GUILayout.Button(""Reset to Defaults"", GUILayout.Height(30)))
            {{
                ResetToDefaults();
            }}
            
            serializedObject.ApplyModifiedProperties();
        }}
        
        private void ResetToDefaults()
        {{
            if (EditorUtility.DisplayDialog(""Reset to Defaults"", 
                ""Are you sure you want to reset all values to defaults?"", ""Yes"", ""No""))
            {{
                configNameProperty.stringValue = ""Default"";
                someValueProperty.intValue = 0;
                serializedObject.ApplyModifiedProperties();
            }}
        }}
    }}
}}";
    }

    private string GetTemplateForComponentTest(string componentName, string namespaceName)
    {
        return $@"using NUnit.Framework;
using System.IO;
using {namespaceName};
using MythHunter.Data.Serialization;

public class {componentName}Tests
{{
    [Test]
    public void {componentName}_DefaultValues_AreCorrect()
    {{
        // Arrange
        var component = new {componentName}();

        // Assert
        // TODO: Assert default values are correct
        Assert.Pass(""Default values are correct"");
    }}

    [Test]
    public void {componentName}_Serialization_DeserializesCorrectly()
    {{
        // Arrange
        var original = new {componentName}();
        // TODO: Set properties on original component

        // Act
        byte[] serialized = original.Serialize();
        var deserialized = new {componentName}();
        deserialized.Deserialize(serialized);

        // Assert
        // TODO: Assert deserialized values match original
        Assert.Pass(""Serialization works correctly"");
    }}
}}";
    }

    private string GetTemplateForSystemTest(string systemName, string namespaceName)
    {
        return $@"using NUnit.Framework;
using System;
using {namespaceName};
using MythHunter.Events;
using MythHunter.Utils.Logging;
using NSubstitute;

public class {systemName}Tests
{{
    private IEventBus _eventBus;
    private ILogger _logger;
    private {systemName} _system;

    [SetUp]
    public void SetUp()
    {{
        // Create mocks
        _eventBus = Substitute.For<IEventBus>();
        _logger = Substitute.For<ILogger>();

        // Create system with mocks
        _system = new {systemName}(_eventBus, _logger);
    }}

    [Test]
    public void Initialize_SubscribesToEvents()
    {{
        // Act
        _system.Initialize();

        // Assert
        // Verify that subscriptions happened
        // Example: _eventBus.Received().Subscribe(Arg.Any<Action<SomeEvent>>());
        Assert.Pass();
    }}

    [Test]
    public void Update_ExecutesLogic()
    {{
        // Arrange
        _system.Initialize();

        // Act
        _system.Update(0.1f);

        // Assert
        // TODO: Assert expected behavior
        Assert.Pass();
    }}

    [Test]
    public void Dispose_UnsubscribesFromEvents()
    {{
        // Arrange
        _system.Initialize();

        // Act
        _system.Dispose();

        // Assert
        // Verify that unsubscriptions happened
        // Example: _eventBus.Received().Unsubscribe(Arg.Any<Action<SomeEvent>>());
        Assert.Pass();
    }}
}}";
    }

    private string GetTemplateForEventTest(string eventName, string namespaceName)
    {
        return $@"using NUnit.Framework;
using System.IO;
using {namespaceName};

public class {eventName}Tests
{{
    [Test]
    public void {eventName}_GetEventId_ReturnsUniqueId()
    {{
        // Arrange
        var event1 = new {eventName}();
        var event2 = new {eventName}();

        // Act
        string id1 = event1.GetEventId();
        string id2 = event2.GetEventId();

        // Assert
        Assert.IsNotNull(id1);
        Assert.IsNotNull(id2);
        Assert.AreNotEqual(id1, id2, ""Event IDs should be unique"");
    }}

    [Test]
    public void {eventName}_Serialization_DeserializesCorrectly()
    {{
        // Arrange
        var original = new {eventName}();
        // TODO: Set properties on original event

        // Act
        byte[] serialized = original.Serialize();
        var deserialized = new {eventName}();
        deserialized.Deserialize(serialized);

        // Assert
        // TODO: Assert deserialized values match original
        Assert.Pass(""Serialization works correctly"");
    }}
}}";
    }

    private string GetTemplateForFactoryTest(string factoryName, string namespaceName)
    {
        string entityName = factoryName.EndsWith("Factory")
            ? factoryName.Substring(0, factoryName.Length - "Factory".Length)
            : factoryName;

        return $@"using NUnit.Framework;
using {namespaceName};
using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;
using NSubstitute;

public class {factoryName}Tests
{{
    private IEntityManager _entityManager;
    private ILogger _logger;
    private {factoryName} _factory;

    [SetUp]
    public void SetUp()
    {{
        _entityManager = Substitute.For<IEntityManager>();
        _logger = Substitute.For<ILogger>();
        _entityManager.CreateEntity().Returns(1);
        _factory = new {factoryName}(_entityManager, _logger);
    }}

    [Test]
    public void Create_ReturnsEntityId()
    {{
        int entityId = _factory.Create{entityName}();
        Assert.AreEqual(1, entityId);
        _entityManager.Received(1).CreateEntity();
    }}

    [Test]
    public void Create_AddsRequiredComponents()
    {{
        int entityId = _factory.Create{entityName}();

        // TODO: Verify that required components were added
        // Example:
        // _entityManager.Received().AddComponent(entityId, Arg.Any<SomeComponent>());

        Assert.Pass();
    }}
}}";
    }

    private string GetTemplateForNetworkSystem(string name, string domain, bool isClient)
{
    string type = isClient ? "Client" : "Server";
    string systemName = $"{name}{type}System";
    string namespaceName = $"{NAMESPACE_MAP["Networking"]}.{domain}";

    string baseType = isClient ? "ClientSystemBase" : "ServerSystemBase";
    string networkType = isClient ? "NetworkClient" : "NetworkServer";

    string body = $@"using MythHunter.Core.ECS;
using MythHunter.Events;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using MythHunter.Networking.Core;

namespace {namespaceName}
{{
    /// <summary>
    /// {type} system for handling {name} functionality
    /// </summary>
    public class {systemName} : {baseType}, IEventSubscriber
    {{
        private readonly IEventBus _eventBus;
        private readonly ILogger _logger;
        private readonly I{networkType} _network;

        [Inject]
        public {systemName}(IEventBus eventBus, ILogger logger, I{networkType} network)
        {{
            _eventBus = eventBus;
            _logger = logger;
            _network = network;
        }}

        public override void Initialize()
        {{
            if (!_network.IsActive)
            {{
                _logger.LogWarning(""{systemName} initialized but {networkType} is not active"");
                return;
            }}

            SubscribeToEvents();
            _logger.LogInfo(""{systemName} initialized"");
        }}

        public void SubscribeToEvents()
        {{
            // Subscribe to events here
            // Example:
            // _eventBus.Subscribe<SomeEvent>(OnSomeEvent);
        }}

        public void UnsubscribeFromEvents()
        {{
            // Unsubscribe from events here
            // Example:
            // _eventBus.Unsubscribe<SomeEvent>(OnSomeEvent);
        }}

        public override void Update(float deltaTime)
        {{
            // {type} logic here
        }}

        public override void Dispose()
        {{
            UnsubscribeFromEvents();
            _logger.LogInfo(""{systemName} disposed"");
        }}

        // Add handler methods here
    }}
}}";

    return body;
}

private string GetTemplateForInterface(string name, string namespaceName, string comment, List<string> body, string imports = "")
{
    StringBuilder sb = new StringBuilder();

    if (!string.IsNullOrEmpty(imports))
        sb.AppendLine(imports);

    sb.AppendLine($"namespace {namespaceName}");
    sb.AppendLine("{");

    if (!string.IsNullOrEmpty(comment))
        sb.AppendLine($"    {comment}");

    sb.AppendLine($"    public interface {name}");
    sb.AppendLine("    {");

    foreach (var line in body)
        sb.AppendLine($"        {line}");

    sb.AppendLine("    }");
    sb.AppendLine("}");

    return sb.ToString();
}

private string GetTemplateForClass(string name, string namespaceName, string comment, List<string> interfaces, List<string> body, string imports = "")
{
    StringBuilder sb = new StringBuilder();

    if (!string.IsNullOrEmpty(imports))
        sb.AppendLine(imports);

    sb.AppendLine($"namespace {namespaceName}");
    sb.AppendLine("{");

    if (!string.IsNullOrEmpty(comment))
        sb.AppendLine($"    {comment}");

    string interfaceList = interfaces.Count > 0 ? $" : {string.Join(", ", interfaces)}" : "";
    sb.AppendLine($"    public class {name}{interfaceList}");
    sb.AppendLine("    {");

    foreach (var line in body)
        sb.AppendLine($"        {line}");

    sb.AppendLine("    }");
    sb.AppendLine("}");

    return sb.ToString();
}

private string GetTemplateForStruct(string name, string namespaceName, string comment, List<string> interfaces, List<string> body, string imports = "")
{
    StringBuilder sb = new StringBuilder();

    if (!string.IsNullOrEmpty(imports))
        sb.AppendLine(imports);

    sb.AppendLine($"namespace {namespaceName}");
    sb.AppendLine("{");

    if (!string.IsNullOrEmpty(comment))
        sb.AppendLine($"    {comment}");

    string interfaceList = interfaces.Count > 0 ? $" : {string.Join(", ", interfaces)}" : "";
    sb.AppendLine($"    public struct {name}{interfaceList}");
    sb.AppendLine("    {");

    foreach (var line in body)
        sb.AppendLine($"        {line}");

    sb.AppendLine("    }");
    sb.AppendLine("}");

    return sb.ToString();
}

#endregion

#region Helper Methods

private void WriteFile(string path, string content)
{
    try
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path));

        // Check if file exists and differ in content
        bool shouldWrite = true;
        if (File.Exists(path))
        {
            string existingContent = File.ReadAllText(path);
            if (existingContent == content)
                shouldWrite = false;
            else
                path = GetUniqueFilePath(path);
        }

        if (shouldWrite)
        {
            File.WriteAllText(path, content);
            createdFiles.Add(path);
        }
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"❌ Error writing file '{path}': {ex.Message}");
    }
}

private string GetUniqueFilePath(string path)
{
    string directory = Path.GetDirectoryName(path);
    string fileName = Path.GetFileNameWithoutExtension(path);
    string extension = Path.GetExtension(path);

    int counter = 1;
    string newPath = path;

    while (File.Exists(newPath))
    {
        newPath = Path.Combine(directory, $"{fileName}_{counter}{extension}");
        counter++;
    }

    return newPath;
}

private bool ValidateInput(string name)
{
    if (string.IsNullOrEmpty(name))
    {
        SetStatusMessage("❌ Name cannot be empty", MessageType.Error);
        return false;
    }

    if (!Regex.IsMatch(name, @"^[a-zA-Z][a-zA-Z0-9_]*$"))
    {
        SetStatusMessage("❌ Name must start with a letter and contain only letters, numbers, and underscores", MessageType.Error);
        return false;
    }

    return true;
}

private void SetStatusMessage(string message, MessageType type)
{
    statusMessage = message;
    statusMessageType = type;
}

private void AutoRegisterInstaller(string installerName)
{
    string path = $"{CODE_PATH}/Core/InstallerRegistry.cs";
    string registryLine = $"        container.Register<{installerName}>();";

    if (File.Exists(path))
    {
        string content = File.ReadAllText(path);
        if (!content.Contains(installerName))
        {
            int insertPoint = content.LastIndexOf("}");
            if (insertPoint != -1)
            {
                content = content.Insert(insertPoint, registryLine + "\n");
                File.WriteAllText(path, content);
                AssetDatabase.Refresh();
                Debug.Log($"✅ Installer '{installerName}' registered in DI.");
            }
            else
            {
                Debug.LogError("InstallerRegistry.cs has unexpected format.");
            }
        }
    }
    else
    {
        // Create installer registry if it doesn't exist
        string registryContent = $"using MythHunter.Core.DI;\n\nnamespace MythHunter.Core\n{{\n    public static class InstallerRegistry\n    {{\n        public static void RegisterInstallers(IDIContainer container)\n        {{\n{registryLine}\n        }}\n    }}\n}}";
        WriteFile(path, registryContent);
    }
}

    #endregion
}
