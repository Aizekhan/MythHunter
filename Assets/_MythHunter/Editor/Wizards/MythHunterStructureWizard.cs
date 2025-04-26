using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// Спрощений візард для створення початкової структури проекту MythHunter.
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
        GUILayout.Label("Створення структури проекту MythHunter", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Цей візард створить основну структуру папок для проекту MythHunter.", MessageType.Info);

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Опції створення папок:", EditorStyles.boldLabel);
        createTestFolders = EditorGUILayout.Toggle("Створити папки для тестів", createTestFolders);
        createResourceFolders = EditorGUILayout.Toggle("Створити папки для ресурсів", createResourceFolders);
        createEditorFolders = EditorGUILayout.Toggle("Створити папки для редактора", createEditorFolders);
        createBaseFiles = EditorGUILayout.Toggle("Створити базові файли", createBaseFiles);

        EditorGUILayout.Space(10);

        if (GUILayout.Button("Створити структуру проекту", GUILayout.Height(30)))
        {
            createdPaths.Clear();
            CreateProjectStructure();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Успіх", "Структура проекту створена успішно!", "OK");
        }

        EditorGUILayout.Space(10);

        // Показати створені папки
        if (createdPaths.Count > 0)
        {
            showCreatedFolders = EditorGUILayout.Foldout(showCreatedFolders, "Створені папки та файли (" + createdPaths.Count + ")");

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
        // Основні папки структури кодової бази
        List<string> directories = new List<string>
        {
            // Корінь проекту і код
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
            CreateCoreImplementations();
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
    /// Базовий інтерфейс для компонентів ECS
    /// </summary>
    public interface IComponent
    {
        // Маркерний інтерфейс
    }
}";
        WriteFile(iComponentFile, iComponentContent);

        // Create a simple system interface
        string iSystemFile = $"{CODE_PATH}/Core/ECS/ISystem.cs";
        string iSystemContent =
@"namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Базовий інтерфейс для систем ECS
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
    /// Базовий інтерфейс для подій
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
        string iDIContainerPath = $"{CODE_PATH}/Core/DI/IDIContainer.cs";
        string iDIContainerContent =
        @"namespace MythHunter.Core.DI
{
    /// <summary>
    /// Інтерфейс для DI контейнера
    /// </summary>
    public interface IDIContainer
    {
        void Register<TService, TImplementation>() where TImplementation : TService, new();
        void RegisterSingleton<TService, TImplementation>() where TImplementation : TService, new();
        void RegisterInstance<TService>(TService instance);
        TService Resolve<TService>();
    }
}";
        WriteFile(iDIContainerPath, iDIContainerContent);
        // Створення ISerializable
        string iSerializablePath = $"{CODE_PATH}/Data/Serialization/ISerializable.cs";
        string iSerializableContent =
    @"namespace MythHunter.Data.Serialization
{
    /// <summary>
    /// Інтерфейс для серіалізації об'єктів
    /// </summary>
    public interface ISerializable
    {
        byte[] Serialize();
        void Deserialize(byte[] data);
    }
}";
        WriteFile(iSerializablePath, iSerializableContent);

        // Створення IView
        string iViewPath = $"{CODE_PATH}/UI/Core/IView.cs";
        string iViewContent =
    @"namespace MythHunter.UI.Core
{
    /// <summary>
    /// Інтерфейс базового UI View
    /// </summary>
    public interface IView
    {
        void Show();
        void Hide();
    }
}";
        WriteFile(iViewPath, iViewContent);

        // Створення IPresenter
        string iPresenterPath = $"{CODE_PATH}/UI/Core/IPresenter.cs";
        string iPresenterContent =
    @"namespace MythHunter.UI.Core
{
    /// <summary>
    /// Інтерфейс базового Presenter для MVP
    /// </summary>
    public interface IPresenter
    {
        void Initialize();
        void Dispose();
    }
}";
        WriteFile(iPresenterPath, iPresenterContent);

        // Створення IModel
        string iModelPath = $"{CODE_PATH}/UI/Core/IModel.cs";
        string iModelContent =
    @"namespace MythHunter.UI.Core
{
    /// <summary>
    /// Інтерфейс базової Model для MVP
    /// </summary>
    public interface IModel
    {
        void Reset();
    }
}";
        WriteFile(iModelPath, iModelContent);

        // Створення IEventBus
        string iEventBusPath = $"{CODE_PATH}/Events/IEventBus.cs";
        string iEventBusContent =
    @"using System;

namespace MythHunter.Events
{
    /// <summary>
    /// Інтерфейс шини подій
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

        // Створення IEntityManager
        string iEntityManagerPath = $"{CODE_PATH}/Core/ECS/IEntityManager.cs";
        string iEntityManagerContent =
    @"namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Інтерфейс менеджера ентіті
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

        // Створення ILogger
        string iLoggerPath = $"{CODE_PATH}/Utils/Logging/ILogger.cs";
        string iLoggerContent =
    @"namespace MythHunter.Utils.Logging
{
    /// <summary>
    /// Інтерфейс логування
    /// </summary>
    public interface ILogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}";
        WriteFile(iLoggerPath, iLoggerContent);

        // Створення IEventSubscriber
        string iEventSubscriberPath = $"{CODE_PATH}/Events/IEventSubscriber.cs";
        string iEventSubscriberContent =
        @"namespace MythHunter.Events
{
    /// <summary>
    /// Інтерфейс для підписників на події
    /// </summary>
    public interface IEventSubscriber
    {
    }
}";
        WriteFile(iEventSubscriberPath, iEventSubscriberContent);

    }

    private void CreateCoreImplementations()
    {
        // DIContainer.cs
        string diContainerPath = $"{CODE_PATH}/Core/DI/DIContainer.cs";
        string diContainerContent =
    @"using System;
using System.Collections.Generic;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Проста реалізація DI контейнера
    /// </summary>
    public class DIContainer : IDIContainer
    {
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();

        public void Register<TService, TImplementation>() where TImplementation : TService, new()
        {
            _instances[typeof(TService)] = new TImplementation();
        }

        public void RegisterSingleton<TService, TImplementation>() where TImplementation : TService, new()
        {
            _instances[typeof(TService)] = new TImplementation();
        }

        public void RegisterInstance<TService>(TService instance)
        {
            _instances[typeof(TService)] = instance;
        }

        public TService Resolve<TService>()
        {
            return (TService)_instances[typeof(TService)];
        }
    }
}";
        WriteFile(diContainerPath, diContainerContent);

        // EventBus.cs
        string eventBusPath = $"{CODE_PATH}/Events/EventBus.cs";
        string eventBusContent =
    @"using System;
using System.Collections.Generic;

namespace MythHunter.Events
{
    /// <summary>
    /// Базова реалізація IEventBus
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            var type = typeof(TEvent);
            if (!_handlers.ContainsKey(type))
                _handlers[type] = new List<Delegate>();

            _handlers[type].Add(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            var type = typeof(TEvent);
            if (_handlers.TryGetValue(type, out var list))
                list.Remove(handler);
        }

        public void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            var type = typeof(TEvent);
            if (_handlers.TryGetValue(type, out var list))
            {
                foreach (var handler in list)
                    ((Action<TEvent>)handler)?.Invoke(eventData);
            }
        }

        public void Clear()
        {
            _handlers.Clear();
        }
    }
}";
        WriteFile(eventBusPath, eventBusContent);

        // GameBootstrapper.cs
        string bootstrapperPath = $"{CODE_PATH}/Core/Game/GameBootstrapper.cs";
        string bootstrapperContent =
    @"using UnityEngine;
using MythHunter.Core.DI;
using MythHunter.Events;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Початковий ініціалізатор гри
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            var container = new DIContainer();
            container.Register<IEventBus, EventBus>();

            InstallerRegistry.RegisterInstallers(container);

            Debug.Log(""✅ GameBootstrapper: DI container initialized"");
        }
    }
}";
        WriteFile(bootstrapperPath, bootstrapperContent);

        // SystemGroup.cs
        string systemGroupPath = $"{CODE_PATH}/Systems/Core/SystemGroup.cs";
        string systemGroupContent =
    @"using System.Collections.Generic;
using MythHunter.Core.ECS;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Група систем, що виконується послідовно
    /// </summary>
    public class SystemGroup : ISystem
    {
        private readonly List<ISystem> _systems = new();

        public void AddSystem(ISystem system)
        {
            _systems.Add(system);
        }

        public void Initialize()
        {
            foreach (var system in _systems)
                system.Initialize();
        }

        public void Update(float deltaTime)
        {
            foreach (var system in _systems)
                system.Update(deltaTime);
        }

        public void Dispose()
        {
            foreach (var system in _systems)
                system.Dispose();
        }
    }
}";
        WriteFile(systemGroupPath, systemGroupContent);

        // BaseAuthoring.cs
        string baseAuthoringPath = $"{CODE_PATH}/Authoring/BaseAuthoring.cs";
        string baseAuthoringContent =
    @"using UnityEngine;

namespace MythHunter.Authoring
{
    /// <summary>
    /// Базовий Authoring компонент для ентіті
    /// </summary>
    public class BaseAuthoring : MonoBehaviour
    {
        public virtual void ApplyData(int entityId)
        {
            // Override to inject components to the entity
        }
    }
}";
        WriteFile(baseAuthoringPath, baseAuthoringContent);
        // InstallerRegistry.cs
        string installerRegistryPath = $"{CODE_PATH}/Core/InstallerRegistry.cs";
        string installerRegistryContent =
        @"using MythHunter.Core.DI;

namespace MythHunter.Core
{
    /// <summary>
    /// Централізований реєстратор інсталерів для DI
    /// </summary>
    public static class InstallerRegistry
    {
        public static void RegisterInstallers(IDIContainer container)
        {
            // TODO: Wizard буде автоматично додавати сюди інсталери
            // container.Register<MyPanelInstaller>();
        }
    }
}";
        WriteFile(installerRegistryPath, installerRegistryContent);

        // SystemBase.cs
        string systemBasePath = $"{CODE_PATH}/Core/ECS/SystemBase.cs";
        string systemBaseContent =
        @"namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Базовий клас для систем
    /// </summary>
    public abstract class SystemBase : ISystem
    {
        public virtual void Initialize() { }
        public virtual void Update(float deltaTime) { }
        public virtual void Dispose() { }
    }
}";
        WriteFile(systemBasePath, systemBaseContent);
        // InjectAttribute.cs
        string injectAttributePath = $"{CODE_PATH}/Core/DI/InjectAttribute.cs";
        string injectAttributeContent =
        @"using System;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Атрибут для позначення полів для інжекції
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Constructor)]
    public class InjectAttribute : Attribute
    {
    }
}";
        WriteFile(injectAttributePath, injectAttributeContent);
        // Entity.cs
        string entityPath = $"{CODE_PATH}/Core/ECS/Entity.cs";
        string entityContent =
        @"namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Базовий клас для Entity
    /// </summary>
    public class Entity
    {
        public int Id { get; }

        public Entity(int id)
        {
            Id = id;
        }
    }
}";
        WriteFile(entityPath, entityContent);


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
