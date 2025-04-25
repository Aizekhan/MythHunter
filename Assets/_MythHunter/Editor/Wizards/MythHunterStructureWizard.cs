using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// ��������� ����� ��� ��������� ��������� ��������� ������� MythHunter.
/// </summary>
public class MythHunterStructureWizard : EditorWindow
{
    private bool createTestFolders = true;
    private bool createResourceFolders = true;
    private bool createEditorFolders = true;
    private bool createBaseFiles = true;

    private List<string> createdPaths = new List<string>();
    private bool showCreatedFolders = false;

    private Vector2 scrollPosition;

    private static readonly string ROOT_PATH = "Assets/_MythHunter";
    private static readonly string CODE_PATH = ROOT_PATH + "/Code";

    [MenuItem("MythHunter Tools/Project Structure Wizard")]
    public static void ShowWindow()
    {
        GetWindow<MythHunterStructureWizard>("MythHunter Structure Wizard");
    }

    private void OnGUI()
    {
        GUILayout.Label("��������� ��������� ������� MythHunter", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("��� ����� �������� ������� ��������� ����� ��� ������� MythHunter.", MessageType.Info);

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("����� ��������� �����:", EditorStyles.boldLabel);
        createTestFolders = EditorGUILayout.Toggle("�������� ����� ��� �����", createTestFolders);
        createResourceFolders = EditorGUILayout.Toggle("�������� ����� ��� �������", createResourceFolders);
        createEditorFolders = EditorGUILayout.Toggle("�������� ����� ��� ���������", createEditorFolders);
        createBaseFiles = EditorGUILayout.Toggle("�������� ����� �����", createBaseFiles);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("�������� ��������� �������", GUILayout.Height(30)))
        {
            createdPaths.Clear();
            CreateProjectStructure();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("����", "��������� ������� �������� ������!", "OK");
        }

        EditorGUILayout.Space(10);

        // �������� ������� �����
        if (createdPaths.Count > 0)
        {
            showCreatedFolders = EditorGUILayout.Foldout(showCreatedFolders, "������� ����� �� ����� (" + createdPaths.Count + ")");

            if (showCreatedFolders)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

                foreach (var path in createdPaths)
                {
                    EditorGUILayout.LabelField(path);
                }

                EditorGUILayout.EndScrollView();
            }
        }
    }

    private void CreateProjectStructure()
    {
        // ������ ����� ��������� ������ ����
        List<string> directories = new List<string>
        {
            // ����� ������� � ���
            ROOT_PATH,
            CODE_PATH,
            
            // Core
            $"{CODE_PATH}/Core",
            $"{CODE_PATH}/Core/DI",
            $"{CODE_PATH}/Core/Game",
            $"{CODE_PATH}/Core/StateMachine",
            $"{CODE_PATH}/Core/Config",
            $"{CODE_PATH}/Core/ECS",
            
            // Events
            $"{CODE_PATH}/Events",
            $"{CODE_PATH}/Events/Domain",
            $"{CODE_PATH}/Events/Debugging",
            
            // Components
            $"{CODE_PATH}/Components",
            $"{CODE_PATH}/Components/Core",
            $"{CODE_PATH}/Components/Character",
            $"{CODE_PATH}/Components/Combat",
            $"{CODE_PATH}/Components/Movement",
            $"{CODE_PATH}/Components/Tags",
            
            // Entities
            $"{CODE_PATH}/Entities",
            
            // Systems
            $"{CODE_PATH}/Systems",
            $"{CODE_PATH}/Systems/Core",
            $"{CODE_PATH}/Systems/Groups",
            $"{CODE_PATH}/Systems/Features",
            $"{CODE_PATH}/Systems/Phase",
            $"{CODE_PATH}/Systems/Combat",
            $"{CODE_PATH}/Systems/Movement",
            $"{CODE_PATH}/Systems/AI",
            
            // Authoring
            $"{CODE_PATH}/Authoring",
            
            // Data
            $"{CODE_PATH}/Data",
            $"{CODE_PATH}/Data/Interfaces",
            $"{CODE_PATH}/Data/ScriptableObjects",
            $"{CODE_PATH}/Data/StaticData",
            $"{CODE_PATH}/Data/Serialization",
            
            // Networking
            $"{CODE_PATH}/Networking",
            $"{CODE_PATH}/Networking/Core",
            $"{CODE_PATH}/Networking/Client",
            $"{CODE_PATH}/Networking/Server",
            $"{CODE_PATH}/Networking/Messages",
            $"{CODE_PATH}/Networking/Serialization",
            
            // Resources
            $"{CODE_PATH}/Resources",
            $"{CODE_PATH}/Resources/Core",
            $"{CODE_PATH}/Resources/Providers",
            $"{CODE_PATH}/Resources/Pool",
            $"{CODE_PATH}/Resources/SceneManagement",
            
            // Cloud
            $"{CODE_PATH}/Cloud",
            $"{CODE_PATH}/Cloud/Core",
            $"{CODE_PATH}/Cloud/Analytics",
            $"{CODE_PATH}/Cloud/AWS",
            $"{CODE_PATH}/Cloud/Local",
            
            // UI
            $"{CODE_PATH}/UI",
            $"{CODE_PATH}/UI/Core",
            $"{CODE_PATH}/UI/Views",
            $"{CODE_PATH}/UI/Presenters",
            $"{CODE_PATH}/UI/Models",
            
            // Other
            $"{CODE_PATH}/Replay",
            $"{CODE_PATH}/Debug",
            $"{CODE_PATH}/Utils",
            $"{CODE_PATH}/Utils/Logging",
            $"{CODE_PATH}/Utils/Extensions",
            $"{CODE_PATH}/Utils/Validation"
        };

        // Test folders
        if (createTestFolders)
        {
            directories.AddRange(new[]
            {
                $"{ROOT_PATH}/Tests",
                $"{ROOT_PATH}/Tests/Editor",
                $"{ROOT_PATH}/Tests/Editor/Systems",
                $"{ROOT_PATH}/Tests/Editor/Components",
                $"{ROOT_PATH}/Tests/Editor/Events",
                $"{ROOT_PATH}/Tests/Runtime",
                $"{ROOT_PATH}/Tests/Runtime/Integration",
                $"{ROOT_PATH}/Tests/Runtime/Performance"
            });
        }

        // Resource folders
        if (createResourceFolders)
        {
            directories.AddRange(new[]
            {
                $"{ROOT_PATH}/Resources",
                $"{ROOT_PATH}/Resources/Prefabs",
                $"{ROOT_PATH}/Resources/Prefabs/Characters",
                $"{ROOT_PATH}/Resources/Prefabs/Weapons",
                $"{ROOT_PATH}/Resources/Prefabs/UI",
                $"{ROOT_PATH}/Resources/Prefabs/Effects",
                $"{ROOT_PATH}/Resources/ScriptableObjects",
                $"{ROOT_PATH}/Resources/ScriptableObjects/Characters",
                $"{ROOT_PATH}/Resources/ScriptableObjects/Weapons",
                $"{ROOT_PATH}/Resources/ScriptableObjects/Items",
                $"{ROOT_PATH}/Resources/ScriptableObjects/Runes",
                $"{ROOT_PATH}/Resources/Data",
                $"{ROOT_PATH}/Resources/Data/Configs",
                $"{ROOT_PATH}/Resources/Data/Maps",
                $"{ROOT_PATH}/Resources/Data/Localizations",
                $"{ROOT_PATH}/Scenes"
            });
        }

        // Editor folders
        if (createEditorFolders)
        {
            directories.AddRange(new[]
            {
                $"{ROOT_PATH}/Editor",
                $"{ROOT_PATH}/Editor/Tools",
                $"{ROOT_PATH}/Editor/Inspectors",
                $"{ROOT_PATH}/Editor/Wizards"
            });
        }

        // Create all directories
        foreach (var dir in directories)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                createdPaths.Add(dir);
            }
        }

        // Create base files for core architecture if needed
        if (createBaseFiles)
        {
            CreateBaseFiles();
            CreateExtraInterfaces();
        }
    }

    private void CreateBaseFiles()
    {
        // Create a simple interface file to start with
        string iComponentFile = $"{CODE_PATH}/Core/ECS/IComponent.cs";
        string iComponentContent =
@"namespace MythHunter.Core.ECS
{
    /// <summary>
    /// ������� ��������� ��� ���������� ECS
    /// </summary>
    public interface IComponent
    {
        // ��������� ���������
    }
}";
        WriteFile(iComponentFile, iComponentContent);

        // Create a simple system interface
        string iSystemFile = $"{CODE_PATH}/Core/ECS/ISystem.cs";
        string iSystemContent =
@"namespace MythHunter.Core.ECS
{
    /// <summary>
    /// ������� ��������� ��� ������ ECS
    /// </summary>
    public interface ISystem
    {
        void Initialize();
        void Update(float deltaTime);
        void Dispose();
    }
}";
        WriteFile(iSystemFile, iSystemContent);

        // Create a basic event interface
        string iEventFile = $"{CODE_PATH}/Events/IEvent.cs";
        string iEventContent =
@"namespace MythHunter.Events
{
    /// <summary>
    /// ������� ��������� ��� ����
    /// </summary>
    public interface IEvent
    {
        string GetEventId();
    }
}";
        WriteFile(iEventFile, iEventContent);

        // Create a basic README file
        string readmeFile = $"{ROOT_PATH}/README.md";
        string readmeContent =
@"# MythHunter Project Structure

This is the base structure for the MythHunter project. 

## Structure Overview

- `/Code` - Contains all the code for the project
  - `/Core` - Core systems and interfaces
  - `/Components` - ECS components
  - `/Systems` - ECS systems
  - `/Events` - Event system
  - ...

## Getting Started

1. First, familiarize yourself with the project structure
2. Check the architecture documentation
3. Use the MythHunter wizards for generating new components and systems

## Development Guidelines

- Follow the ECS architecture pattern
- Use events for communication between systems
- Implement interfaces for all components and systems
";
        WriteFile(readmeFile, readmeContent);
    }
    private void CreateExtraInterfaces()
    {
        // ��������� ISerializable
        string iSerializablePath = $"{CODE_PATH}/Data/Serialization/ISerializable.cs";
        string iSerializableContent =
    @"namespace MythHunter.Data.Serialization
{
    /// <summary>
    /// ��������� ��� ���������� ��'����
    /// </summary>
    public interface ISerializable
    {
        byte[] Serialize();
        void Deserialize(byte[] data);
    }
}";
        WriteFile(iSerializablePath, iSerializableContent);

        // ��������� IView
        string iViewPath = $"{CODE_PATH}/UI/Core/IView.cs";
        string iViewContent =
    @"namespace MythHunter.UI.Core
{
    /// <summary>
    /// ��������� �������� UI View
    /// </summary>
    public interface IView
    {
        void Show();
        void Hide();
    }
}";
        WriteFile(iViewPath, iViewContent);

        // ��������� IPresenter
        string iPresenterPath = $"{CODE_PATH}/UI/Core/IPresenter.cs";
        string iPresenterContent =
    @"namespace MythHunter.UI.Core
{
    /// <summary>
    /// ��������� �������� Presenter ��� MVP
    /// </summary>
    public interface IPresenter
    {
        void Initialize();
        void Dispose();
    }
}";
        WriteFile(iPresenterPath, iPresenterContent);

        // ��������� IModel
        string iModelPath = $"{CODE_PATH}/UI/Core/IModel.cs";
        string iModelContent =
    @"namespace MythHunter.UI.Core
{
    /// <summary>
    /// ��������� ������ Model ��� MVP
    /// </summary>
    public interface IModel
    {
        void Reset();
    }
}";
        WriteFile(iModelPath, iModelContent);

        // ��������� IEventBus
        string iEventBusPath = $"{CODE_PATH}/Events/IEventBus.cs";
        string iEventBusContent =
    @"using System;

namespace MythHunter.Events
{
    /// <summary>
    /// ��������� ���� ����
    /// </summary>
    public interface IEventBus
    {
        void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent;
        void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent;
        void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent;
        void Clear();
    }
}";
        WriteFile(iEventBusPath, iEventBusContent);

        // ��������� IEntityManager
        string iEntityManagerPath = $"{CODE_PATH}/Core/ECS/IEntityManager.cs";
        string iEntityManagerContent =
    @"namespace MythHunter.Core.ECS
{
    /// <summary>
    /// ��������� ��������� ����
    /// </summary>
    public interface IEntityManager
    {
        int CreateEntity();
        void DestroyEntity(int entityId);
        void AddComponent<TComponent>(int entityId, TComponent component) where TComponent : IComponent;
        bool HasComponent<TComponent>(int entityId) where TComponent : IComponent;
        TComponent GetComponent<TComponent>(int entityId) where TComponent : IComponent;
    }
}";
        WriteFile(iEntityManagerPath, iEntityManagerContent);

        // ��������� ILogger
        string iLoggerPath = $"{CODE_PATH}/Utils/Logging/ILogger.cs";
        string iLoggerContent =
    @"namespace MythHunter.Utils.Logging
{
    /// <summary>
    /// ��������� ���������
    /// </summary>
    public interface ILogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}";
        WriteFile(iLoggerPath, iLoggerContent);
    }


    private void WriteFile(string path, string content)
    {
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, content);
            createdPaths.Add(path);
        }
    }
}