using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;


/// <summary>
/// Повний візард для створення структури проекту MythHunter.
/// </summary>
public class MythHunterStructureWizard : EditorWindow
{
    private string rootFolderName = "_MythHunter";
    private string ROOT_PATH => $"Assets/{rootFolderName}";
    private string CODE_PATH => $"{ROOT_PATH}/Code";

    private bool createTestFolders = true;
    private bool createResourceFolders = true;
    private bool createEditorFolders = true;
    private bool createBaseFiles = true;

    private List<string> createdPaths = new List<string>();
    private bool showCreatedFolders = false;

    private Vector2 scrollPosition;

    

    [MenuItem("MythHunter Tools/Project Structure Wizard")]
    public static void ShowWindow()
    {
        GetWindow<MythHunterStructureWizard>("MythHunter Structure Wizard");
    }

    private void OnGUI()
    {

        GUILayout.Label("Створення структури проекту MythHunter", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Цей візард створить повну структуру папок і базові файли для проекту MythHunter.", MessageType.Info);
        
        EditorGUILayout.Space(10);

        // Додати поле для назви кореневої папки
        rootFolderName = EditorGUILayout.TextField("Назва кореневої папки:", rootFolderName);

        EditorGUILayout.LabelField("Опції створення:", EditorStyles.boldLabel);
        createTestFolders = EditorGUILayout.Toggle("Створити папки для тестів", createTestFolders);
        createResourceFolders = EditorGUILayout.Toggle("Створити папки для ресурсів", createResourceFolders);
        createEditorFolders = EditorGUILayout.Toggle("Створити папки для редактора", createEditorFolders);
        createBaseFiles = EditorGUILayout.Toggle("Створити базові файли", createBaseFiles);

        EditorGUILayout.Space(10);

        GUI.enabled = !string.IsNullOrEmpty(rootFolderName);
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
        // Створення всіх папок
        CreateDirectories();

        // Створення базових файлів
        if (createBaseFiles)
        {
            // Створення основних інтерфейсів
            CreateCoreInterfaces();

            // Створення додаткових інтерфейсів
            CreateEcsInterfaces();
            CreateDiInterfaces();
            CreateEventInterfaces();
            CreateUiInterfaces();
            CreateNetworkingInterfaces();
            CreateResourceInterfaces();
            CreateCloudInterfaces();
            CreateUtilInterfaces();

            // Створення імплементацій
            CreateCoreImplementations();
            CreateEcsImplementations();
            CreateEventImplementations();
            CreateSystemImplementations();
            CreateUtilImplementations();
        }
    }

    private void CreateDirectories()
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
                $"{ROOT_PATH}/Tests/Editor/Entities",
                $"{ROOT_PATH}/Tests/Editor/Networking",
                $"{ROOT_PATH}/Tests/Editor/UI",
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
    }

    private void CreateCoreInterfaces()
    {
        // IComponent - базовий інтерфейс для компонентів ECS
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

        // ISystem - базовий інтерфейс для систем ECS
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

        // IEvent - базовий інтерфейс для подій
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

        // README.md - базовий опис проекту
        string readmeFile = $"{ROOT_PATH}/README.md";
        string readmeContent =
    @"# MythHunter Project

## Архітектура проекту

Проект MythHunter використовує компонентно-орієнтовану ECS архітектуру з подійною моделлю комунікації між системами.

### Основні принципи:
- **ECS (Entity-Component-System)** - розділення даних (компоненти) та логіки (системи)
- **Dependency Injection** - явна ін'єкція залежностей через конструктори
- **Events-driven** - комунікація через події, а не прямі виклики методів
- **UniTask** - використання Cysharp.Threading.Tasks замість стандартних System.Threading.Tasks
- **Generic StateMachine** - використання дженериків і enum для типобезпечних машин станів
- **SOLID** - дотримання принципів SOLID
- **Testability** - можливість тестування компонентів окремо
- **Serializability** - серіалізація даних для мережевої передачі

## Структура проекту

- `/Code/Core` - ядро системи (DI, ECS, StateMachine)
- `/Code/Components` - компоненти ECS (дані)
- `/Code/Systems` - системи ECS (логіка)
- `/Code/Events` - події та шина подій
- `/Code/Entities` - фабрики сутностей
- `/Code/Networking` - мережева частина
- `/Code/UI` - система UI на основі MVP
- `/Code/Resources` - система ресурсів
- `/Code/Data` - дані та налаштування
- `/Code/Utils` - утиліти

## Розробка

Для розробки нових компонентів та систем рекомендується використовувати інструмент:
- MythHunter Tools > Component Wizard

## Правила кодування:
- Інтерфейси починаються з I
- Один файл - один клас/інтерфейс
- Компоненти тільки для даних
- Системи тільки для логіки
- Комунікація тільки через події";
        WriteFile(readmeFile, readmeContent);

        // Додаємо файл з імпортами UniTask
        string uniTaskPath = $"{CODE_PATH}/Core/UniTaskImports.cs";
        string uniTaskContent =
    @"// Цей файл містить допоміжні методи для роботи з UniTask
using System;
using System.Collections.Generic;

namespace MythHunter.Core
{
    /// <summary>
    /// Допоміжний клас для роботи з UniTask
    /// </summary>
    public static class UniTaskHelper
    {
        // ВАЖЛИВО: Для роботи цього проекту необхідно встановити пакет UniTask від Cysharp
        // Встановіть його через Package Manager з GitHub URL: https://github.com/Cysharp/UniTask.git
        // Або через меню Window > Package Manager > + > Add package from git URL...
        
        // Приклади використання асинхронних методів можна знайти на сторінці:
        // https://github.com/Cysharp/UniTask/blob/master/README.md
    }
}";
        WriteFile(uniTaskPath, uniTaskContent);
    }

    private void CreateEcsInterfaces()
    {
        // IEcsWorld - інтерфейс світу ECS
        string iEcsWorldPath = $"{CODE_PATH}/Core/ECS/IEcsWorld.cs";
        string iEcsWorldContent =
    @"namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Інтерфейс світу ECS
    /// </summary>
    public interface IEcsWorld
    {
        IEntityManager EntityManager { get; }
        void Initialize();
        void Update(float deltaTime);
        void Dispose();
    }
}";
        WriteFile(iEcsWorldPath, iEcsWorldContent);

        // IEntityManager - інтерфейс менеджера сутностей
        string iEntityManagerPath = $"{CODE_PATH}/Core/ECS/IEntityManager.cs";
        string iEntityManagerContent =
    @"namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Інтерфейс менеджера сутностей
    /// </summary>
    public interface IEntityManager
    {
        int CreateEntity();
        void DestroyEntity(int entityId);
        void AddComponent<TComponent>(int entityId, TComponent component) where TComponent : IComponent;
        bool HasComponent<TComponent>(int entityId) where TComponent : IComponent;
        TComponent GetComponent<TComponent>(int entityId) where TComponent : IComponent;
        void RemoveComponent<TComponent>(int entityId) where TComponent : IComponent;
        int[] GetAllEntities();
        int[] GetEntitiesWith<TComponent>() where TComponent : IComponent;
    }
}";
        WriteFile(iEntityManagerPath, iEntityManagerContent);

        // IFixedUpdateSystem - інтерфейс для систем з фіксованим оновленням
        string iFixedUpdateSystemPath = $"{CODE_PATH}/Systems/Core/IFixedUpdateSystem.cs";
        string iFixedUpdateSystemContent =
    @"using MythHunter.Core.ECS;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Інтерфейс для систем з фіксованим оновленням
    /// </summary>
    public interface IFixedUpdateSystem : ISystem
    {
        void FixedUpdate(float fixedDeltaTime);
    }
}";
        WriteFile(iFixedUpdateSystemPath, iFixedUpdateSystemContent);

        // ILateUpdateSystem - інтерфейс для систем з пізнім оновленням
        string iLateUpdateSystemPath = $"{CODE_PATH}/Systems/Core/ILateUpdateSystem.cs";
        string iLateUpdateSystemContent =
    @"using MythHunter.Core.ECS;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Інтерфейс для систем з пізнім оновленням
    /// </summary>
    public interface ILateUpdateSystem : ISystem
    {
        void LateUpdate(float deltaTime);
    }
}";
        WriteFile(iLateUpdateSystemPath, iLateUpdateSystemContent);

        // Додаємо інтерфейси для типобезпечної машини станів
        string iStatePath = $"{CODE_PATH}/Core/StateMachine/IState.cs";
        string iStateContent =
    @"using System;

namespace MythHunter.Core.StateMachine
{
    /// <summary>
    /// Інтерфейс стану для машини станів
    /// </summary>
    public interface IState<TStateEnum> where TStateEnum : Enum
    {
        void Enter();
        void Update();
        void Exit();
        TStateEnum StateId { get; }
    }
}";
        WriteFile(iStatePath, iStateContent);

        string iStateMachinePath = $"{CODE_PATH}/Core/StateMachine/IStateMachine.cs";
        string iStateMachineContent =
    @"using System;

namespace MythHunter.Core.StateMachine
{
    /// <summary>
    /// Інтерфейс машини станів з підтримкою enum
    /// </summary>
    public interface IStateMachine<TStateEnum> where TStateEnum : Enum
    {
        void RegisterState(TStateEnum stateId, IState<TStateEnum> state);
        void UnregisterState(TStateEnum stateId);
        bool SetState(TStateEnum stateId);
        void Update();
        TStateEnum CurrentState { get; }
        void AddTransition(TStateEnum fromStateId, TStateEnum toStateId);
        bool CanTransition(TStateEnum fromStateId, TStateEnum toStateId);
    }
}";
        WriteFile(iStateMachinePath, iStateMachineContent);
    }

    private void CreateDiInterfaces()
    {
        // IDIContainer - інтерфейс контейнера залежностей
        string iDIContainerPath = $"{CODE_PATH}/Core/DI/IDIContainer.cs";
        string iDIContainerContent =
@"namespace MythHunter.Core.DI
{
    /// <summary>
    /// Інтерфейс контейнера залежностей
    /// </summary>
    public interface IDIContainer
    {
        void Register<TService, TImplementation>() where TImplementation : TService;
        void RegisterSingleton<TService, TImplementation>() where TImplementation : TService;
        void RegisterInstance<TService>(TService instance);
        TService Resolve<TService>();
        bool IsRegistered<TService>();
        void AnalyzeDependencies();
    }
}";
        WriteFile(iDIContainerPath, iDIContainerContent);

        // InjectAttribute - атрибут для ін'єкції залежностей
        string injectAttributePath = $"{CODE_PATH}/Core/DI/InjectAttribute.cs";
        string injectAttributeContent =
@"using System;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Атрибут для позначення полів і конструкторів для ін'єкції
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Method)]
    public class InjectAttribute : Attribute
    {
    }
}";
        WriteFile(injectAttributePath, injectAttributeContent);

        // DIInstaller - інтерфейс для інсталяторів залежностей
        string diInstallerPath = $"{CODE_PATH}/Core/DI/IDIInstaller.cs";
        string diInstallerContent =
@"namespace MythHunter.Core.DI
{
    /// <summary>
    /// Інтерфейс для інсталяторів залежностей
    /// </summary>
    public interface IDIInstaller
    {
        void InstallBindings(IDIContainer container);
    }
}";
        WriteFile(diInstallerPath, diInstallerContent);
    }

    private void CreateEventInterfaces()
    {
        // IEventBus - інтерфейс шини подій
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

        // IEventSubscriber - інтерфейс для підписників на події
        string iEventSubscriberPath = $"{CODE_PATH}/Events/IEventSubscriber.cs";
        string iEventSubscriberContent =
@"namespace MythHunter.Events
{
    /// <summary>
    /// Інтерфейс для підписників на події
    /// </summary>
    public interface IEventSubscriber
    {
        void SubscribeToEvents();
        void UnsubscribeFromEvents();
    }
}";
        WriteFile(iEventSubscriberPath, iEventSubscriberContent);

        // Базові події (Core Events)
        string gameCoreEventsPath = $"{CODE_PATH}/Events/Domain/GameEvents.cs";
        string gameCoreEventsContent =
@"using System;

namespace MythHunter.Events.Domain
{
    /// <summary>
    /// Подія запуску гри
    /// </summary>
    public struct GameStartedEvent : IEvent
    {
        public DateTime Timestamp;
        
        public string GetEventId() => $""{GetType().Name}_{Guid.NewGuid()}"";
    }
    
    /// <summary>
    /// Подія паузи гри
    /// </summary>
    public struct GamePausedEvent : IEvent
    {
        public bool IsPaused;
        public DateTime Timestamp;
        
        public string GetEventId() => $""{GetType().Name}_{Guid.NewGuid()}"";
    }
    
    /// <summary>
    /// Подія завершення гри
    /// </summary>
    public struct GameEndedEvent : IEvent
    {
        public bool IsVictory;
        public DateTime Timestamp;
        
        public string GetEventId() => $""{GetType().Name}_{Guid.NewGuid()}"";
    }
}";
        WriteFile(gameCoreEventsPath, gameCoreEventsContent);

        // Створення файлу з подіями фаз
        string phaseEventsPath = $"{CODE_PATH}/Events/Domain/PhaseEvents.cs";
        string phaseEventsContent =
@"using System;

namespace MythHunter.Events.Domain
{
    /// <summary>
    /// Фази гри
    /// </summary>
    public enum GamePhase
    {
        None = 0,
        Rune,
        Planning,
        Movement,
        Combat,
        Freeze
    }
    
    /// <summary>
    /// Подія зміни фази
    /// </summary>
    public struct PhaseChangedEvent : IEvent
    {
        public GamePhase PreviousPhase;
        public GamePhase CurrentPhase;
        public DateTime Timestamp;
        
        public string GetEventId() => $""{GetType().Name}_{Guid.NewGuid()}"";
    }
    
    /// <summary>
    /// Подія початку фази
    /// </summary>
    public struct PhaseStartedEvent : IEvent
    {
        public GamePhase Phase;
        public float Duration;
        public DateTime Timestamp;
        
        public string GetEventId() => $""{GetType().Name}_{Guid.NewGuid()}"";
    }
    
    /// <summary>
    /// Подія завершення фази
    /// </summary>
    public struct PhaseEndedEvent : IEvent
    {
        public GamePhase Phase;
        public DateTime Timestamp;
        
        public string GetEventId() => $""{GetType().Name}_{Guid.NewGuid()}"";
    }
}";
        WriteFile(phaseEventsPath, phaseEventsContent);
    }

    private void CreateUiInterfaces()
    {
        // IView - інтерфейс для View (MVP)
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

        // IPresenter - інтерфейс для Presenter (MVP)
        string iPresenterPath = $"{CODE_PATH}/UI/Core/IPresenter.cs";
        string iPresenterContent =
    @"using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Інтерфейс базового Presenter для MVP
    /// </summary>
    public interface IPresenter
    {
        UniTask InitializeAsync();
        void Dispose();
    }
}";
        WriteFile(iPresenterPath, iPresenterContent);

        // IModel - інтерфейс для Model (MVP)
        string iModelPath = $"{CODE_PATH}/UI/Core/IModel.cs";
        string iModelContent =
    @"namespace MythHunter.UI.Core
{
    /// <summary>
    /// Інтерфейс базової Model для MVP
    /// </summary>
    public interface IModel
    {
    }
}";
        WriteFile(iModelPath, iModelContent);

        // IUISystem - інтерфейс для UI системи
        string iUISystemPath = $"{CODE_PATH}/UI/Core/IUISystem.cs";
        string iUISystemContent =
    @"using UnityEngine;
using Cysharp.Threading.Tasks;

namespace MythHunter.UI.Core
{
    /// <summary>
    /// Інтерфейс системи UI
    /// </summary>
    public interface IUISystem
    {
        void ShowView<TView>() where TView : Component, IView;
        void HideView<TView>() where TView : Component, IView;
        UniTask<TView> ShowViewAsync<TView>(string prefabPath) where TView : Component, IView;
        void RegisterView<TView>(TView view) where TView : Component, IView;
        void UnregisterView<TView>(TView view) where TView : Component, IView;
        TView GetView<TView>() where TView : Component, IView;
        bool IsViewActive<TView>() where TView : Component, IView;
    }
}";
        WriteFile(iUISystemPath, iUISystemContent);
    }

    private void CreateNetworkingInterfaces()
    {
        // INetworkSystem - інтерфейс мережевої системи
        string iNetworkSystemPath = $"{CODE_PATH}/Networking/Core/INetworkSystem.cs";
        string iNetworkSystemContent =
    @"using System;
using Cysharp.Threading.Tasks;
using MythHunter.Networking.Messages;

namespace MythHunter.Networking.Core
{
    /// <summary>
    /// Інтерфейс мережевої системи
    /// </summary>
    public interface INetworkSystem
    {
        void StartServer(ushort port);
        UniTask<bool> ConnectToServerAsync(string address, ushort port);
        UniTask DisconnectAsync();
        void SendMessage<T>(T message) where T : INetworkMessage;
        event Action<INetworkMessage> OnMessageReceived;
        event Action<NetworkClientInfo, bool> OnClientConnectionChanged;
        bool IsServer { get; }
        bool IsClient { get; }
        bool IsConnected { get; }
    }
    
    /// <summary>
    /// Інформація про клієнта
    /// </summary>
    public struct NetworkClientInfo
    {
        public int ClientId;
        public string Address;
    }
}";
        WriteFile(iNetworkSystemPath, iNetworkSystemContent);

        // INetworkMessage - інтерфейс мережевого повідомлення
        string iNetworkMessagePath = $"{CODE_PATH}/Networking/Messages/INetworkMessage.cs";
        string iNetworkMessageContent =
    @"using MythHunter.Data.Serialization;

namespace MythHunter.Networking.Messages
{
    /// <summary>
    /// Інтерфейс мережевого повідомлення
    /// </summary>
    public interface INetworkMessage : ISerializable
    {
        string GetMessageId();
    }
}";
        WriteFile(iNetworkMessagePath, iNetworkMessageContent);

        // INetworkSerializer - інтерфейс мережевого серіалізатора
        string iNetworkSerializerPath = $"{CODE_PATH}/Networking/Serialization/INetworkSerializer.cs";
        string iNetworkSerializerContent =
    @"namespace MythHunter.Networking.Serialization
{
    /// <summary>
    /// Інтерфейс мережевого серіалізатора
    /// </summary>
    public interface INetworkSerializer
    {
        byte[] Serialize<T>(T message) where T : INetworkMessage;
        T Deserialize<T>(byte[] data) where T : INetworkMessage, new();
        INetworkMessage Deserialize(byte[] data, System.Type messageType);
    }
}";
        WriteFile(iNetworkSerializerPath, iNetworkSerializerContent);

        // Базові мережеві інтерфейси клієнта і сервера
        string iNetworkClientPath = $"{CODE_PATH}/Networking/Client/INetworkClient.cs";
        string iNetworkClientContent =
    @"using System;
using Cysharp.Threading.Tasks;
using MythHunter.Networking.Messages;

namespace MythHunter.Networking.Client
{
    /// <summary>
    /// Інтерфейс мережевого клієнта
    /// </summary>
    public interface INetworkClient
    {
        UniTask<bool> ConnectAsync(string address, ushort port);
        UniTask DisconnectAsync();
        void SendMessage<T>(T message) where T : INetworkMessage;
        event Action<INetworkMessage> OnMessageReceived;
        event Action OnConnected;
        event Action OnDisconnected;
        bool IsConnected { get; }
        bool IsActive { get; }
    }
}";
        WriteFile(iNetworkClientPath, iNetworkClientContent);

        string iNetworkServerPath = $"{CODE_PATH}/Networking/Server/INetworkServer.cs";
        string iNetworkServerContent =
    @"using System;
using Cysharp.Threading.Tasks;
using MythHunter.Networking.Messages;

namespace MythHunter.Networking.Server
{
    /// <summary>
    /// Інтерфейс мережевого сервера
    /// </summary>
    public interface INetworkServer
    {
        void Start(ushort port);
        UniTask StopAsync();
        void SendMessage<T>(T message, int clientId) where T : INetworkMessage;
        void BroadcastMessage<T>(T message) where T : INetworkMessage;
        event Action<int, INetworkMessage> OnMessageReceived;
        event Action<int> OnClientConnected;
        event Action<int> OnClientDisconnected;
        bool IsRunning { get; }
        bool IsActive { get; }
        int[] GetConnectedClients();
    }
}";
        WriteFile(iNetworkServerPath, iNetworkServerContent);
    }

    private void CreateResourceInterfaces()
    {
        // IResourceProvider - інтерфейс провайдера ресурсів
        string iResourceProviderPath = $"{CODE_PATH}/Resources/Core/IResourceProvider.cs";
        string iResourceProviderContent =
    @"using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MythHunter.Resources.Core
{
    /// <summary>
    /// Інтерфейс провайдера ресурсів
    /// </summary>
    public interface IResourceProvider
    {
        UniTask<T> LoadAsync<T>(string key) where T : UnityEngine.Object;
        UniTask<IReadOnlyList<T>> LoadAllAsync<T>(string pattern) where T : UnityEngine.Object;
        void Unload(string key);
        void UnloadAll();
        T GetFromPool<T>(string key) where T : UnityEngine.Object;
        void ReturnToPool<T>(string key, T instance) where T : UnityEngine.Object;
    }
}";
        WriteFile(iResourceProviderPath, iResourceProviderContent);

        // IObjectPool - інтерфейс пулу об'єктів
        string iObjectPoolPath = $"{CODE_PATH}/Resources/Pool/IObjectPool.cs";
        string iObjectPoolContent =
    @"namespace MythHunter.Resources.Pool
{
    /// <summary>
    /// Інтерфейс пулу об'єктів
    /// </summary>
    public interface IObjectPool<T> where T : class
    {
        T Get();
        void Return(T instance);
        void Clear();
        int CountActive { get; }
        int CountInactive { get; }
    }
}";
        WriteFile(iObjectPoolPath, iObjectPoolContent);

        // ISceneLoader - інтерфейс завантажувача сцен
        string iSceneLoaderPath = $"{CODE_PATH}/Resources/SceneManagement/ISceneLoader.cs";
        string iSceneLoaderContent =
    @"using Cysharp.Threading.Tasks;

namespace MythHunter.Resources.SceneManagement
{
    /// <summary>
    /// Інтерфейс завантажувача сцен
    /// </summary>
    public interface ISceneLoader
    {
        UniTask LoadSceneAsync(string sceneName, bool showLoadingScreen = true);
        UniTask LoadSceneAdditiveAsync(string sceneName);
        UniTask UnloadSceneAsync(string sceneName);
        string GetActiveScene();
        bool IsSceneLoaded(string sceneName);
    }
}";
        WriteFile(iSceneLoaderPath, iSceneLoaderContent);

        // ResourceRequest - клас запиту на ресурс
        string resourceRequestPath = $"{CODE_PATH}/Resources/Core/ResourceRequest.cs";
        string resourceRequestContent =
    @"using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MythHunter.Resources.Core
{
    /// <summary>
    /// Клас запиту на ресурс
    /// </summary>
    public class ResourceRequest<T> where T : UnityEngine.Object
    {
        public string Key { get; }
        public UniTask<T> Task { get; }
        public bool IsCompleted => Task.Status.IsCompleted();
        
        public ResourceRequest(string key, UniTask<T> task)
        {
            Key = key;
            Task = task;
        }
    }
}";
        WriteFile(resourceRequestPath, resourceRequestContent);

        // IAddressablesProvider - інтерфейс провайдера Addressables
        string iAddressablesProviderPath = $"{CODE_PATH}/Resources/Providers/IAddressablesProvider.cs";
        string iAddressablesProviderContent =
    @"using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MythHunter.Resources.Providers
{
    /// <summary>
    /// Інтерфейс провайдера Addressables
    /// </summary>
    public interface IAddressablesProvider
    {
        UniTask<T> LoadAssetAsync<T>(string key) where T : UnityEngine.Object;
        UniTask<IList<T>> LoadAssetsAsync<T>(IEnumerable<string> keys) where T : UnityEngine.Object;
        UniTask<IList<T>> LoadAssetsAsync<T>(string label) where T : UnityEngine.Object;
        void ReleaseAsset<T>(T asset) where T : UnityEngine.Object;
        void ReleaseAssets<T>(IList<T> assets) where T : UnityEngine.Object;
        UniTask<GameObject> InstantiateAsync(string key, Transform parent = null);
        void ReleaseInstance(GameObject instance);
    }
}";
        WriteFile(iAddressablesProviderPath, iAddressablesProviderContent);

        // SceneReference - клас для посилання на сцену
        string sceneReferencePath = $"{CODE_PATH}/Resources/SceneManagement/SceneReference.cs";
        string sceneReferenceContent =
    @"using UnityEngine;

namespace MythHunter.Resources.SceneManagement
{
    /// <summary>
    /// Клас для посилання на сцену
    /// </summary>
    [System.Serializable]
    public class SceneReference
    {
        [SerializeField] private string scenePath;
        [SerializeField] private string sceneName;
        
        public string ScenePath => scenePath;
        public string SceneName => sceneName;
        
        public SceneReference(string path)
        {
            scenePath = path;
            sceneName = System.IO.Path.GetFileNameWithoutExtension(path);
        }
        
        public override string ToString()
        {
            return sceneName;
        }
    }
}";
        WriteFile(sceneReferencePath, sceneReferenceContent);
    }

    private void CreateCloudInterfaces()
    {
        // ICloudService - інтерфейс хмарного сервісу
        string iCloudServicePath = $"{CODE_PATH}/Cloud/Core/ICloudService.cs";
        string iCloudServiceContent =
    @"using Cysharp.Threading.Tasks;

namespace MythHunter.Cloud.Core
{
    /// <summary>
    /// Базовий інтерфейс хмарного сервісу
    /// </summary>
    public interface ICloudService
    {
        UniTask<bool> InitializeAsync();
        bool IsInitialized { get; }
        string GetServiceId();
    }
}";
        WriteFile(iCloudServicePath, iCloudServiceContent);

        // IDataService - інтерфейс сервісу даних
        string iDataServicePath = $"{CODE_PATH}/Cloud/Core/IDataService.cs";
        string iDataServiceContent =
    @"using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace MythHunter.Cloud.Core
{
    /// <summary>
    /// Інтерфейс сервісу даних
    /// </summary>
    public interface IDataService : ICloudService
    {
        UniTask<T> LoadDataAsync<T>(string key) where T : class;
        UniTask SaveDataAsync<T>(string key, T data) where T : class;
        UniTask<bool> DeleteDataAsync(string key);
        UniTask<bool> ExistsAsync(string key);
        UniTask<List<string>> GetKeysAsync(string prefix);
    }
}";
        WriteFile(iDataServicePath, iDataServiceContent);

        // IAuthService - інтерфейс сервісу авторизації
        string iAuthServicePath = $"{CODE_PATH}/Cloud/Core/IAuthService.cs";
        string iAuthServiceContent =
    @"using Cysharp.Threading.Tasks;

namespace MythHunter.Cloud.Core
{
    /// <summary>
    /// Інтерфейс сервісу авторизації
    /// </summary>
    public interface IAuthService : ICloudService
    {
        UniTask<bool> SignInAsync(string username, string password);
        UniTask<bool> SignUpAsync(string username, string password, string email);
        UniTask<bool> SignOutAsync();
        UniTask<bool> DeleteAccountAsync();
        bool IsSignedIn { get; }
        string CurrentUserId { get; }
    }
}";
        WriteFile(iAuthServicePath, iAuthServiceContent);

        // IAnalyticsService - інтерфейс сервісу аналітики
        string iAnalyticsServicePath = $"{CODE_PATH}/Cloud/Analytics/IAnalyticsService.cs";
        string iAnalyticsServiceContent =
    @"using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace MythHunter.Cloud.Analytics
{
    /// <summary>
    /// Інтерфейс сервісу аналітики
    /// </summary>
    public interface IAnalyticsService : ICloudService
    {
        void TrackEvent(string eventName);
        void TrackEvent(string eventName, Dictionary<string, object> parameters);
        UniTask<bool> FlushAsync();
        void SetUserId(string userId);
        void SetUserProperty(string name, string value);
    }
}";
        WriteFile(iAnalyticsServicePath, iAnalyticsServiceContent);

        // AnalyticsEvent - базовий клас події аналітики
        string analyticsEventPath = $"{CODE_PATH}/Cloud/Analytics/AnalyticsEvent.cs";
        string analyticsEventContent =
    @"using System;
using System.Collections.Generic;

namespace MythHunter.Cloud.Analytics
{
    /// <summary>
    /// Базовий клас події аналітики
    /// </summary>
    public abstract class AnalyticsEvent
    {
        public DateTime Timestamp { get; private set; }
        public string EventName { get; protected set; }
        
        protected readonly Dictionary<string, object> Parameters = new Dictionary<string, object>();
        
        protected AnalyticsEvent()
        {
            Timestamp = DateTime.UtcNow;
        }
        
        public Dictionary<string, object> GetParameters()
        {
            Dictionary<string, object> result = new Dictionary<string, object>(Parameters)
            {
                { ""timestamp"", Timestamp.ToString(""o"") }
            };
            
            return result;
        }
    }
}";
        WriteFile(analyticsEventPath, analyticsEventContent);
    }
    private void CreateUtilInterfaces()
    {
        // Створюємо тільки IMythLogger інтерфейс
        string iLoggerPath = $"{CODE_PATH}/Utils/Logging/IMythLogger.cs";
        string iLoggerContent =
    @"using System;

namespace MythHunter.Utils.Logging
{
    /// <summary>
    /// Рівні логування, від найменш до найбільш важливого
    /// </summary>
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Fatal = 5,
        Off = 6 // Вимкнути логування
    }

    /// <summary>
    /// Інтерфейс для системи логування проекту MythHunter.
    /// Забезпечує методи для логування повідомлень різних рівнів важливості.
    /// </summary>
    public interface IMythLogger
    {
        void LogInfo(string message, string category = null);
        void LogWarning(string message, string category = null);
        void LogError(string message, string category = null, Exception exception = null);
        void LogFatal(string message, string category = null);
        void LogDebug(string message, string category = null);
        void LogTrace(string message, string category = null);
        void WithContext(string key, object value);
        void ClearContext();
        void SetDefaultCategory(string category);
        void SetMinLogLevel(LogLevel level);
        void EnableFileLogging(bool enable);
    }
}"
        ;
        WriteFile(iLoggerPath, iLoggerContent);
    }
    private void CreateCoreImplementations()
    {
        // DIContainer - реалізація DI контейнера
        string diContainerPath = $"{CODE_PATH}/Core/DI/DIContainer.cs";
        string diContainerContent =
    @"using System;
using System.Collections.Generic;
using System.Reflection;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Реалізація DI контейнера
    /// </summary>
    public class DIContainer : IDIContainer
    {
        private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        
        public void Register<TService, TImplementation>() where TImplementation : TService
        {
            _factories[typeof(TService)] = () => Activator.CreateInstance(typeof(TImplementation));
        }
        
        public void RegisterSingleton<TService, TImplementation>() where TImplementation : TService
        {
            var serviceType = typeof(TService);
            
            if (!_instances.ContainsKey(serviceType))
            {
                _instances[serviceType] = Activator.CreateInstance(typeof(TImplementation));
            }
        }
        
        public void RegisterInstance<TService>(TService instance)
        {
            _instances[typeof(TService)] = instance;
        }
        
        public TService Resolve<TService>()
        {
            return (TService)Resolve(typeof(TService));
        }
        
        private object Resolve(Type serviceType)
        {
            // Перевірка наявності синглтону
            if (_instances.TryGetValue(serviceType, out var instance))
            {
                return instance;
            }
            
            // Перевірка наявності фабрики
            if (_factories.TryGetValue(serviceType, out var factory))
            {
                return factory();
            }
            
            throw new Exception($""Type {serviceType.Name} is not registered"");
        }
        
        public bool IsRegistered<TService>()
        {
            var serviceType = typeof(TService);
            return _instances.ContainsKey(serviceType) || _factories.ContainsKey(serviceType);
        }
        
        public void AnalyzeDependencies()
        {
            Console.WriteLine(""Analyzing dependencies..."");
            
            foreach (var registration in _factories)
            {
                Console.WriteLine($""Service: {registration.Key.Name}"");
            }
            
            foreach (var instance in _instances)
            {
                Console.WriteLine($""Singleton: {instance.Key.Name}"");
            }
        }
    }
}";
        WriteFile(diContainerPath, diContainerContent);

        // DIInstaller - базовий клас для інсталяторів залежностей
        string diInstallerPath = $"{CODE_PATH}/Core/DI/DIInstaller.cs";
        string diInstallerContent =
    @"namespace MythHunter.Core.DI
{
    /// <summary>
    /// Базовий клас для інсталяторів залежностей
    /// </summary>
    public abstract class DIInstaller : IDIInstaller
    {
        public abstract void InstallBindings(IDIContainer container);
        
        protected void BindSingleton<TService, TImplementation>(IDIContainer container) 
            where TImplementation : TService
        {
            container.RegisterSingleton<TService, TImplementation>();
        }
        
        protected void Bind<TService, TImplementation>(IDIContainer container) 
            where TImplementation : TService
        {
            container.Register<TService, TImplementation>();
        }
        
        protected void BindInstance<TService>(IDIContainer container, TService instance)
        {
            container.RegisterInstance<TService>(instance);
        }
    }
}";
        WriteFile(diInstallerPath, diInstallerContent);

        // InstallerRegistry - реєстр інсталяторів DI
        string installerRegistryPath = $"{CODE_PATH}/Core/InstallerRegistry.cs";
        string installerRegistryContent =
    @"using MythHunter.Core.DI;

namespace MythHunter.Core
{
    /// <summary>
    /// Реєстр інсталяторів для DI
    /// </summary>
    public static class InstallerRegistry
    {
        public static void RegisterInstallers(IDIContainer container)
        {
            // Core installers
            // TODO: Wizard will automatically add installers here
        }
    }
}";
        WriteFile(installerRegistryPath, installerRegistryContent);



        // GameBootstrapper - точка входу в гру
        string gameBootstrapperPath = $"{CODE_PATH}/Core/Game/GameBootstrapper.cs";
        string gameBootstrapperContent =
    @"using UnityEngine;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Events;
using MythHunter.Utils.Logging;
using MythHunter.Core.ECS;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Точка входу в гру
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        private IDIContainer _container;
        private IEventBus _eventBus;
        private IMythLogger _logger;
        private IEcsWorld _ecsWorld;
        private GameStateMachine _stateMachine;
        
        private async void Awake()
        {
            InitializeDependencyInjection();
            InitializeLogging();
            InitializeEcs();
            InitializeStateMachine();
            
            DontDestroyOnLoad(gameObject);
            
            _logger.LogInfo(""GameBootstrapper initialized successfully"");
            
            // Асинхронна ініціалізація сервісів
            await InitializeServicesAsync();
        }
        
        private void InitializeDependencyInjection()
        {
            _container = new DIContainer();
            
            // Реєстрація базових сервісів
            _container.RegisterSingleton<IEventBus, EventBus>();
            _container.RegisterSingleton<IMythLogger, MythLogger>();
            
            // Реєстрація всіх інсталяторів
            InstallerRegistry.RegisterInstallers(_container);
        }
        
        private void InitializeLogging()
        {
            _logger = _container.Resolve<IMythLogger>();
            _logger.LogInfo(""Logging system initialized"");
        }
        
        private void InitializeEcs()
        {
            _eventBus = _container.Resolve<IEventBus>();
            
            // Створення ECS світу
            var entityManager = new EntityManager();
            var systemRegistry = new Systems.Core.SystemRegistry();
            
            _ecsWorld = new EcsWorld(entityManager, systemRegistry);
            _container.RegisterInstance<IEntityManager>(entityManager);
            _container.RegisterInstance<IEcsWorld>(_ecsWorld);
            
            _logger.LogInfo(""ECS world initialized"");
        }
        
        private void InitializeStateMachine()
        {
            _stateMachine = new GameStateMachine(_container);
            _stateMachine.Initialize();
            
            _logger.LogInfo(""Game state machine initialized"");
        }
        
        private async UniTask InitializeServicesAsync()
        {
            _logger.LogInfo(""Starting async services initialization"");
            
            // Тут можна ініціалізувати сервіси, які потребують асинхронності
            // Наприклад, завантаження конфігурацій, підключення до серверів тощо
            
            // Імітація асинхронної операції
            await UniTask.Delay(100);
            
            _logger.LogInfo(""Async services initialization completed"");
        }
        
        private void Update()
        {
            _ecsWorld?.Update(Time.deltaTime);
            _stateMachine?.Update();
        }
        
        private void OnDestroy()
        {
            _ecsWorld?.Dispose();
            _logger?.LogInfo(""GameBootstrapper destroyed"");
        }
    }
}";
        WriteFile(gameBootstrapperPath, gameBootstrapperContent);

        // GameStateMachine - машина станів гри
        string gameStateTypePath = $"{CODE_PATH}/Core/Game/GameStateType.cs";
        string gameStateTypeContent =
    @"namespace MythHunter.Core.Game
{
    /// <summary>
    /// Типи станів гри
    /// </summary>
    public enum GameStateType
    {
        None = 0,
        Boot,
        MainMenu,
        Loading,
        Game,
        Pause,
        GameOver
    }
}";
        WriteFile(gameStateTypePath, gameStateTypeContent);

        string gameStateMachinePath = $"{CODE_PATH}/Core/Game/GameStateMachine.cs";
        string gameStateMachineContent =
    @"using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Машина станів гри
    /// </summary>
    public class GameStateMachine
    {
        private readonly IStateMachine<GameStateType> _stateMachine;
        private readonly IMythLogger _logger;
        private readonly IDIContainer _container;
        
        public GameStateMachine(IDIContainer container)
        {
            _container = container;
            _logger = container.Resolve<IMythLogger>();
            _stateMachine = new StateMachine<GameStateType>();
        }
        
        public void Initialize()
        {
            // Реєстрація станів
            _stateMachine.RegisterState(GameStateType.Boot, new BootState(_container));
            _stateMachine.RegisterState(GameStateType.MainMenu, new MainMenuState(_container));
            _stateMachine.RegisterState(GameStateType.Loading, new LoadingState(_container));
            _stateMachine.RegisterState(GameStateType.Game, new GameplayState(_container));
            
            // Налаштування переходів
            _stateMachine.AddTransition(GameStateType.Boot, GameStateType.MainMenu);
            _stateMachine.AddTransition(GameStateType.MainMenu, GameStateType.Loading);
            _stateMachine.AddTransition(GameStateType.Loading, GameStateType.Game);
            _stateMachine.AddTransition(GameStateType.Game, GameStateType.MainMenu);
            
            // Перехід до початкового стану
            _stateMachine.SetState(GameStateType.Boot);
            
            _logger.LogInfo($""Initialized GameStateMachine with initial state: {GameStateType.Boot}"");
        }
        
        public void Update()
        {
            _stateMachine.Update();
        }
        
        public void ChangeState(GameStateType newState)
        {
            _stateMachine.SetState(newState);
        }
        
        public GameStateType CurrentState => _stateMachine.CurrentState;
    }
}";
        WriteFile(gameStateMachinePath, gameStateMachineContent);
    
}
    private void CreateEcsImplementations()
    {
        // Entity - базовий клас для сутностей
        string entityPath = $"{CODE_PATH}/Core/ECS/Entity.cs";
        string entityContent =
    @"namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Базовий клас для сутності
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

        // EntityManager - реалізація менеджера сутностей
        string entityManagerPath = $"{CODE_PATH}/Core/ECS/EntityManager.cs";
        string entityManagerContent =
    @"using System;
using System.Collections.Generic;
using System.Linq;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Реалізація менеджера сутностей
    /// </summary>
    public class EntityManager : IEntityManager
    {
        private int _nextEntityId = 1;
        private readonly Dictionary<int, Dictionary<Type, IComponent>> _components = new Dictionary<int, Dictionary<Type, IComponent>>();
        private readonly Dictionary<Type, HashSet<int>> _entitiesByComponent = new Dictionary<Type, HashSet<int>>();
        
        public int CreateEntity()
        {
            int entityId = _nextEntityId++;
            _components[entityId] = new Dictionary<Type, IComponent>();
            return entityId;
        }
        
        public void DestroyEntity(int entityId)
        {
            if (!_components.ContainsKey(entityId))
                return;
                
            // Видалення всіх компонентів
            foreach (var componentType in _components[entityId].Keys.ToList())
            {
                if (_entitiesByComponent.ContainsKey(componentType))
                {
                    _entitiesByComponent[componentType].Remove(entityId);
                }
            }
            
            _components.Remove(entityId);
        }
        
        public void AddComponent<TComponent>(int entityId, TComponent component) where TComponent : IComponent
        {
            if (!_components.ContainsKey(entityId))
                _components[entityId] = new Dictionary<Type, IComponent>();
                
            Type componentType = typeof(TComponent);
            _components[entityId][componentType] = component;
            
            // Оновлення кешу для швидкого пошуку
            if (!_entitiesByComponent.ContainsKey(componentType))
                _entitiesByComponent[componentType] = new HashSet<int>();
                
            _entitiesByComponent[componentType].Add(entityId);
        }
        
        public bool HasComponent<TComponent>(int entityId) where TComponent : IComponent
        {
            return _components.ContainsKey(entityId) && _components[entityId].ContainsKey(typeof(TComponent));
        }
        
        public TComponent GetComponent<TComponent>(int entityId) where TComponent : IComponent
        {
            if (!HasComponent<TComponent>(entityId))
                return default;
                
            return (TComponent)_components[entityId][typeof(TComponent)];
        }
        
        public void RemoveComponent<TComponent>(int entityId) where TComponent : IComponent
        {
            if (!HasComponent<TComponent>(entityId))
                return;
                
            Type componentType = typeof(TComponent);
            _components[entityId].Remove(componentType);
            
            if (_entitiesByComponent.ContainsKey(componentType))
            {
                _entitiesByComponent[componentType].Remove(entityId);
            }
        }
        
        public int[] GetAllEntities()
        {
            return _components.Keys.ToArray();
        }
        
        public int[] GetEntitiesWith<TComponent>() where TComponent : IComponent
        {
            Type componentType = typeof(TComponent);
            
            if (!_entitiesByComponent.ContainsKey(componentType))
                return Array.Empty<int>();
                
            return _entitiesByComponent[componentType].ToArray();
        }
    }
}";
        WriteFile(entityManagerPath, entityManagerContent);

        // EcsWorld - реалізація світу ECS
        string ecsWorldPath = $"{CODE_PATH}/Core/ECS/EcsWorld.cs";
        string ecsWorldContent =
    @"using MythHunter.Systems.Core;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Реалізація світу ECS
    /// </summary>
    public class EcsWorld : IEcsWorld
    {
        private readonly IEntityManager _entityManager;
        private readonly SystemRegistry _systemRegistry;
        
        public IEntityManager EntityManager => _entityManager;
        
        public EcsWorld(IEntityManager entityManager, SystemRegistry systemRegistry)
        {
            _entityManager = entityManager;
            _systemRegistry = systemRegistry;
        }
        
        public void Initialize()
        {
            _systemRegistry.InitializeAll();
        }
        
        public void Update(float deltaTime)
        {
            _systemRegistry.UpdateAll(deltaTime);
        }
        
        public void Dispose()
        {
            _systemRegistry.DisposeAll();
        }
    }
}";
        WriteFile(ecsWorldPath, ecsWorldContent);

        // SystemBase - базовий клас для систем
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

        // BaseEntityFactory - базова фабрика сутностей
        string baseEntityFactoryPath = $"{CODE_PATH}/Entities/EntityFactory.cs";
        string baseEntityFactoryContent =
    @"using MythHunter.Core.ECS;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;

namespace MythHunter.Entities
{
    /// <summary>
    /// Базова фабрика сутностей
    /// </summary>
    public abstract class EntityFactory
    {
        protected readonly IEntityManager EntityManager;
        protected readonly IMythLogger Logger;
        
        [Inject]
        public EntityFactory(IEntityManager entityManager, IMythLogger logger)
        {
            EntityManager = entityManager;
            Logger = logger;
        }
        
        protected int CreateBaseEntity()
        {
            int entityId = EntityManager.CreateEntity();
            EntityManager.AddComponent(entityId, new NameComponent { Name = ""Entity_"" + entityId });
            EntityManager.AddComponent(entityId, new IdComponent { Id = entityId });
            
            Logger.LogDebug($""Created entity with ID {entityId}"");
            
            return entityId;
        }
    }
}";
        WriteFile(baseEntityFactoryPath, baseEntityFactoryContent);

        // NameComponent - базовий компонент імені
        string nameComponentPath = $"{CODE_PATH}/Components/Core/NameComponent.cs";
        string nameComponentContent =
    @"using System.IO;
using MythHunter.Core.ECS;
using MythHunter.Data.Serialization;

namespace MythHunter.Components.Core
{
    /// <summary>
    /// Компонент імені
    /// </summary>
    public struct NameComponent : IComponent, ISerializable
    {
        public string Name;
        
        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Name ?? string.Empty);
                return stream.ToArray();
            }
        }
        
        public void Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Name = reader.ReadString();
            }
        }
    }
}";
        WriteFile(nameComponentPath, nameComponentContent);

        // IdComponent - базовий компонент ідентифікатора
        string idComponentPath = $"{CODE_PATH}/Components/Core/IdComponent.cs";
        string idComponentContent =
    @"using System.IO;
using MythHunter.Core.ECS;
using MythHunter.Data.Serialization;

namespace MythHunter.Components.Core
{
    /// <summary>
    /// Компонент ідентифікатора
    /// </summary>
    public struct IdComponent : IComponent, ISerializable
    {
        public int Id;
        
        public byte[] Serialize()
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(Id);
                return stream.ToArray();
            }
        }
        
        public void Deserialize(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            using (BinaryReader reader = new BinaryReader(stream))
            {
                Id = reader.ReadInt32();
            }
        }
    }
}";
        WriteFile(idComponentPath, idComponentContent);

        // StateMachine - реалізація машини станів з дженериками
        string stateMachinePath = $"{CODE_PATH}/Core/StateMachine/StateMachine.cs";
        string stateMachineContent =
    @"using System;
using System.Collections.Generic;

namespace MythHunter.Core.StateMachine
{
    /// <summary>
    /// Реалізація машини станів з підтримкою enum
    /// </summary>
    public class StateMachine<TStateEnum> : IStateMachine<TStateEnum> where TStateEnum : Enum
    {
        private readonly Dictionary<TStateEnum, IState<TStateEnum>> _states = new Dictionary<TStateEnum, IState<TStateEnum>>();
        private readonly HashSet<(TStateEnum from, TStateEnum to)> _allowedTransitions = new HashSet<(TStateEnum from, TStateEnum to)>();
        
        private IState<TStateEnum> _currentState;
        
        public TStateEnum CurrentState => _currentState != null ? _currentState.StateId : default;
        
        public void RegisterState(TStateEnum stateId, IState<TStateEnum> state)
        {
            _states[stateId] = state;
        }
        
        public void UnregisterState(TStateEnum stateId)
        {
            if (_states.ContainsKey(stateId))
            {
                if (_currentState != null && EqualityComparer<TStateEnum>.Default.Equals(_currentState.StateId, stateId))
                {
                    _currentState.Exit();
                    _currentState = null;
                }
                
                _states.Remove(stateId);
            }
        }
        
        public bool SetState(TStateEnum stateId)
        {
            if (!_states.ContainsKey(stateId))
                return false;
                
            if (_currentState != null)
            {
                if (EqualityComparer<TStateEnum>.Default.Equals(_currentState.StateId, stateId))
                    return true;
                    
                if (!CanTransition(_currentState.StateId, stateId))
                    return false;
                    
                _currentState.Exit();
            }
            
            _currentState = _states[stateId];
            _currentState.Enter();
            
            return true;
        }
        
        public void Update()
        {
            _currentState?.Update();
        }
        
        public void AddTransition(TStateEnum fromStateId, TStateEnum toStateId)
        {
            _allowedTransitions.Add((fromStateId, toStateId));
        }
        
        public bool CanTransition(TStateEnum fromStateId, TStateEnum toStateId)
        {
            return _allowedTransitions.Contains((fromStateId, toStateId));
        }
    }
}";
        WriteFile(stateMachinePath, stateMachineContent);

        // BaseState - базовий клас для станів
        string baseStatePath = $"{CODE_PATH}/Core/StateMachine/BaseState.cs";
        string baseStateContent =
    @"using System;
using MythHunter.Core.DI;

namespace MythHunter.Core.StateMachine
{
    /// <summary>
    /// Базовий клас для станів з підтримкою enum
    /// </summary>
    public abstract class BaseState<TStateEnum> : IState<TStateEnum> where TStateEnum : Enum
    {
        protected readonly IDIContainer Container;
        
        public abstract TStateEnum StateId { get; }
        
        protected BaseState(IDIContainer container)
        {
            Container = container;
        }
        
        public virtual void Enter() { }
        
        public virtual void Update() { }
        
        public virtual void Exit() { }
    }
}";
        WriteFile(baseStatePath, baseStateContent);
    }

    private void CreateEventImplementations()
    {
        // EventBus - реалізація шини подій
        string eventBusPath = $"{CODE_PATH}/Events/EventBus.cs";
        string eventBusContent =
    @"using System;
using System.Collections.Generic;

namespace MythHunter.Events
{
    /// <summary>
    /// Реалізація шини подій
    /// </summary>
    public class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();
        
        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);
            
            if (!_handlers.ContainsKey(eventType))
                _handlers[eventType] = new List<Delegate>();
                
            _handlers[eventType].Add(handler);
        }
        
        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);
            
            if (!_handlers.ContainsKey(eventType))
                return;
                
            _handlers[eventType].Remove(handler);
            
            if (_handlers[eventType].Count == 0)
                _handlers.Remove(eventType);
        }
        
        public void Publish<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            Type eventType = typeof(TEvent);
            
            if (!_handlers.ContainsKey(eventType))
                return;
                
            foreach (var handler in _handlers[eventType])
            {
                try
                {
                    ((Action<TEvent>)handler).Invoke(eventData);
                }
                catch (Exception ex)
                {
                    // Логування виключень при обробці подій
                    Console.Error.WriteLine($""Error handling event {eventType.Name}: {ex.Message}"");
                }
            }
        }
        
        public void Clear()
        {
            _handlers.Clear();
        }
    }
}";
        WriteFile(eventBusPath, eventBusContent);

        // EventLogger - логер подій для дебагу
        string eventLoggerPath = $"{CODE_PATH}/Events/Debugging/EventLogger.cs";
        string eventLoggerContent =
    @"using System;
using MythHunter.Utils.Logging;
using MythHunter.Core.DI;

namespace MythHunter.Events.Debugging
{
    /// <summary>
    /// Логер подій для відлагодження
    /// </summary>
    public class EventLogger : IEventSubscriber
    {
        private readonly IEventBus _eventBus;
        private readonly IMythLogger _logger;
        private bool _isEnabled = false;
        
        [Inject]
        public EventLogger(IEventBus eventBus, IMythLogger logger)
        {
            _eventBus = eventBus;
            _logger = logger;
        }
        
        public void Enable()
        {
            if (!_isEnabled)
            {
                SubscribeToEvents();
                _isEnabled = true;
                _logger.LogInfo(""Event logger enabled"");
            }
        }
        
        public void Disable()
        {
            if (_isEnabled)
            {
                UnsubscribeFromEvents();
                _isEnabled = false;
                _logger.LogInfo(""Event logger disabled"");
            }
        }
        
        public void SubscribeToEvents()
        {
            // Підписка на всі події (можна замінити на конкретний список)
            _eventBus.Subscribe<IEvent>(OnAnyEvent);
        }
        
        public void UnsubscribeFromEvents()
        {
            _eventBus.Unsubscribe<IEvent>(OnAnyEvent);
        }
        
        private void OnAnyEvent(IEvent evt)
        {
            _logger.LogDebug($""Event: {evt.GetType().Name}, ID: {evt.GetEventId()}"");
        }
    }
}";
        WriteFile(eventLoggerPath, eventLoggerContent);

        // EventVisualizer - для візуалізації подій в редакторі
        string eventVisualizerPath = $"{CODE_PATH}/Events/Debugging/EventVisualizer.cs";
        string eventVisualizerContent =
    @"using System;
using System.Collections.Generic;
using UnityEngine;
using MythHunter.Core.DI;

namespace MythHunter.Events.Debugging
{
    /// <summary>
    /// Візуалізатор подій для відлагодження
    /// </summary>
    public class EventVisualizer : MonoBehaviour, IEventSubscriber
    {
        [Inject] private IEventBus _eventBus;
        
        private readonly List<EventRecord> _eventHistory = new List<EventRecord>();
        private readonly int _maxEvents = 100;
        private bool _isVisible = false;
        private Vector2 _scrollPosition;
        
        private void OnEnable()
        {
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        public void SubscribeToEvents()
        {
            _eventBus?.Subscribe<IEvent>(OnEventReceived);
        }
        
        public void UnsubscribeFromEvents()
        {
            _eventBus?.Unsubscribe<IEvent>(OnEventReceived);
        }
        
        private void OnEventReceived(IEvent evt)
        {
            _eventHistory.Add(new EventRecord
            {
                EventType = evt.GetType().Name,
                EventId = evt.GetEventId(),
                Timestamp = DateTime.Now
            });
            
            if (_eventHistory.Count > _maxEvents)
                _eventHistory.RemoveAt(0);
        }
        
        private void OnGUI()
        {
            if (!_isVisible) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 400, 500));
            GUILayout.BeginVertical(""box"");
            
            GUILayout.Label(""Event Visualizer"", GUI.skin.box);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(""Clear"", GUILayout.Width(80)))
            {
                _eventHistory.Clear();
            }
            if (GUILayout.Button(""Close"", GUILayout.Width(80)))
            {
                _isVisible = false;
            }
            GUILayout.EndHorizontal();
            
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
            foreach (var record in _eventHistory)
            {
                GUILayout.BeginHorizontal(""box"");
                GUILayout.Label($""{record.Timestamp.ToString(""HH:mm:ss.fff"")}"", GUILayout.Width(100));
                GUILayout.Label(record.EventType);
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                _isVisible = !_isVisible;
            }
        }
        
        private struct EventRecord
        {
            public string EventType;
            public string EventId;
            public DateTime Timestamp;
        }
    }
}";
        WriteFile(eventVisualizerPath, eventVisualizerContent);
    }
    private void CreateSystemImplementations()
    {
        // SystemRegistry - реєстр систем
        string systemRegistryPath = $"{CODE_PATH}/Systems/Core/SystemRegistry.cs";
        string systemRegistryContent =
    @"using System.Collections.Generic;
using MythHunter.Core.ECS;
using MythHunter.Systems.Core;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Реєстр систем для керування їх життєвим циклом
    /// </summary>
    public class SystemRegistry
    {
        private readonly List<ISystem> _updateSystems = new List<ISystem>();
        private readonly List<IFixedUpdateSystem> _fixedUpdateSystems = new List<IFixedUpdateSystem>();
        private readonly List<ILateUpdateSystem> _lateUpdateSystems = new List<ILateUpdateSystem>();
        
        public void RegisterSystem(ISystem system)
        {
            _updateSystems.Add(system);
            
            if (system is IFixedUpdateSystem fixedUpdateSystem)
                _fixedUpdateSystems.Add(fixedUpdateSystem);
                
            if (system is ILateUpdateSystem lateUpdateSystem)
                _lateUpdateSystems.Add(lateUpdateSystem);
        }
        
        public void InitializeAll()
        {
            foreach (var system in _updateSystems)
            {
                system.Initialize();
            }
        }
        
        public void UpdateAll(float deltaTime)
        {
            foreach (var system in _updateSystems)
            {
                system.Update(deltaTime);
            }
        }
        
        public void FixedUpdateAll(float fixedDeltaTime)
        {
            foreach (var system in _fixedUpdateSystems)
            {
                system.FixedUpdate(fixedDeltaTime);
            }
        }
        
        public void LateUpdateAll(float deltaTime)
        {
            foreach (var system in _lateUpdateSystems)
            {
                system.LateUpdate(deltaTime);
            }
        }
        
        public void DisposeAll()
        {
            foreach (var system in _updateSystems)
            {
                system.Dispose();
            }
            
            _updateSystems.Clear();
            _fixedUpdateSystems.Clear();
            _lateUpdateSystems.Clear();
        }
    }
}";
        WriteFile(systemRegistryPath, systemRegistryContent);

        // SystemGroup - група систем
        string systemGroupPath = $"{CODE_PATH}/Systems/Groups/SystemGroup.cs";
        string systemGroupContent =
    @"using System.Collections.Generic;
using MythHunter.Core.ECS;

namespace MythHunter.Systems.Groups
{
    /// <summary>
    /// Група систем для послідовного виконання
    /// </summary>
    public class SystemGroup : SystemBase
    {
        private readonly List<ISystem> _systems = new List<ISystem>();
        
        public void AddSystem(ISystem system)
        {
            _systems.Add(system);
        }
        
        public override void Initialize()
        {
            foreach (var system in _systems)
            {
                system.Initialize();
            }
        }
        
        public override void Update(float deltaTime)
        {
            foreach (var system in _systems)
            {
                system.Update(deltaTime);
            }
        }
        
        public override void Dispose()
        {
            foreach (var system in _systems)
            {
                system.Dispose();
            }
            
            _systems.Clear();
        }
    }
}";
        WriteFile(systemGroupPath, systemGroupContent);


        // GameplayState - оновлюємо для використання типізованої StateMachine
        string gameplayStatePath = $"{CODE_PATH}/Core/Game/GameplayState.cs";
        string gameplayStateContent =
    @"using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;
using MythHunter.Events;
using MythHunter.Events.Domain;
using Cysharp.Threading.Tasks;
using System;

namespace MythHunter.Core.Game
{
    /// <summary>
    /// Стан ігрового процесу
    /// </summary>
    public class GameplayState : BaseState<GameStateType>
    {
        private readonly IMythLogger _logger;
        private readonly IEventBus _eventBus;
        
        public override GameStateType StateId => GameStateType.Game;
        
        public GameplayState(IDIContainer container) : base(container)
        {
            _logger = container.Resolve<IMythLogger>();
            _eventBus = container.Resolve<IEventBus>();
        }
        
        public override void Enter()
        {
            _logger.LogInfo(""Entering gameplay state"");
            
            // Публікуємо подію старту гри
            _eventBus.Publish(new GameStartedEvent
            {
                Timestamp = DateTime.UtcNow
            });
            
            // Асинхронна ініціалізація
            InitializeAsync().Forget();
        }
        
        private async UniTaskVoid InitializeAsync()
        {
            _logger.LogInfo(""Starting async gameplay initialization"");
            
            // Приклад асинхронної ініціалізації
            await UniTask.Delay(100);
            
            _logger.LogInfo(""Async gameplay initialization completed"");
        }
        
        public override void Update()
        {
            // Логіка оновлення геймплею
        }
        
        public override void Exit()
        {
            _logger.LogInfo(""Exiting gameplay state"");
            
            // Публікуємо подію завершення гри
            _eventBus.Publish(new GameEndedEvent
            {
                IsVictory = false,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}";
        WriteFile(gameplayStatePath, gameplayStateContent);

        // Заготовки інших станів
        CreateBasicGameState("Boot", "Boot");
        CreateBasicGameState("MainMenu", "MainMenu");
        CreateBasicGameState("Loading", "Loading");
    }
    private void CreateBasicGameState(string stateName, string stateTypeName)
    {
        string statePath = $"{CODE_PATH}/Core/Game/{stateName}State.cs";
        string stateContent =
    $@"using MythHunter.Core.DI;
using MythHunter.Core.StateMachine;
using MythHunter.Utils.Logging;
using Cysharp.Threading.Tasks;
using System;

namespace MythHunter.Core.Game
{{
    /// <summary>
    /// Стан {stateName}
    /// </summary>
    public class {stateName}State : BaseState<GameStateType>
    {{
        private readonly IMythLogger _logger;
        
        public override GameStateType StateId => GameStateType.{stateTypeName};
        
        public {stateName}State(IDIContainer container) : base(container)
        {{
            _logger = container.Resolve<IMythLogger>();
        }}
        
        public override void Enter()
        {{
            _logger.LogInfo(""Entering {stateName} state"", ""GameState"");
            
            // Асинхронна ініціалізація
            InitializeAsync().Forget();
        }}
        
        private async UniTaskVoid InitializeAsync()
        {{
            try
            {{
                // Приклад асинхронної ініціалізації
                await UniTask.Delay(100);
                
                _logger.LogInfo(""{stateName} state initialized asynchronously"", ""GameState"");
            }}
            catch (Exception ex)
            {{
                _logger.LogError($""Error in {stateName} initialization: {{ex.Message}}"", ""GameState"", ex);
            }}
        }}
        
        public override void Update()
        {{
            // Логіка оновлення {stateName} стану
        }}
        
        public override void Exit()
        {{
            _logger.LogInfo(""Exiting {stateName} state"", ""GameState"");
        }}
    }}
}}";
        WriteFile(statePath, stateContent);
    }
    /// <summary>
    /// //////////////////////////////////////CreateUtilImplementations() ////////////////////////////////
    /// </summary>
    private void CreateUtilImplementations()
    {
        CreateUnityLogger();
       
        CreateEnsureHelper();
        CreateValidator();
    }
    private void CreateUnityLogger()
    {
        // Створюємо тільки MythLogger реалізацію
        string mythLoggerPath = $"{CODE_PATH}/Utils/Logging/MythLogger.cs";
        string mythLoggerContent =
    @"using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MythHunter.Utils.Logging
{
    /// <summary>
    /// Рівні логування, від найменш до найбільш важливого
    /// </summary>
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warning = 3,
        Error = 4,
        Fatal = 5,
        Off = 6 // Вимкнути логування
    }

    /// <summary>
    /// Сигнатура для методів, що будуть вставлятися в логи для додаткової обробки
    /// </summary>
    public delegate string LogEnricher(Dictionary<string, object> properties);

    /// <summary>
    /// Розширена реалізація логера для проекту MythHunter з підтримкою кольорів, категорій та контексту.
    /// </summary>
    public class MythLogger : IMythLogger
    {
        #region Fields and Properties

        private const string DEFAULT_CATEGORY = ""General"";
        private const string LOG_FILE_PREFIX = ""mythgame_log_"";
        private const string LOG_FILE_EXT = "".log"";
        private const int MAX_LOG_FILES = 5;
        private const float MB = 1024 * 1024;
        private const float MAX_LOG_SIZE_MB = 10;

        // Статичні значки для різних типів логів
        private static readonly Dictionary<LogLevel, string> LogIcons = new Dictionary<LogLevel, string>
        {
            { LogLevel.Trace, ""🔍"" },
            { LogLevel.Debug, ""🐞"" },
            { LogLevel.Info, ""ℹ️"" },
            { LogLevel.Warning, ""⚠️"" },
            { LogLevel.Error, ""❌"" },
            { LogLevel.Fatal, ""☠️"" }
        };

        // Статичні кольори для різних типів логів (кольори у форматі для Unity Console)
        private static readonly Dictionary<LogLevel, string> LogColors = new Dictionary<LogLevel, string>
        {
            { LogLevel.Trace, ""#AAAAAA"" },  // Світло-сірий
            { LogLevel.Debug, ""#DDDDDD"" },  // Сірий
            { LogLevel.Info, ""#FFFFFF"" },   // Білий
            { LogLevel.Warning, ""#FFCC00"" },// Жовтий
            { LogLevel.Error, ""#FF6666"" },  // Червоний
            { LogLevel.Fatal, ""#FF0000"" }   // Яскраво-червоний
        };

        // Колекція категорій логів з їхніми назвами та ярликами
        private static readonly Dictionary<string, string> LogCategories = new Dictionary<string, string>
        {
            { ""General"", ""🌐"" },
            { ""Network"", ""🌍"" },
            { ""Combat"", ""⚔️"" },
            { ""Movement"", ""🏃"" },
            { ""AI"", ""🧠"" },
            { ""UI"", ""🖥️"" },
            { ""Performance"", ""⚡"" },
            { ""Physics"", ""🔄"" },
            { ""Audio"", ""🔊"" },
            { ""Input"", ""🎮"" },
            { ""Resource"", ""📦"" },
            { ""Database"", ""💾"" },
            { ""Replay"", ""📼"" },
            { ""Analytics"", ""📊"" },
            { ""Phase"", ""⏱️"" },
            { ""Rune"", ""🔮"" },
            { ""Item"", ""🎒"" },
            { ""Character"", ""👤"" },
            { ""Startup"", ""🚀"" },
            { ""Cloud"", ""☁️"" }
        };

        // Мінімальний рівень логування
        private LogLevel _minLogLevel;

        // Флаг для файлового логування
        private bool _logToFile;

        // Шлях до файлу логу
        private string _logFilePath;

        // Збагачувачі логу
        private List<LogEnricher> _enrichers = new List<LogEnricher>();

        // Контекст логування
        private Dictionary<string, object> _context = new Dictionary<string, object>();

        // Категорія за замовчуванням
        private string _defaultCategory = DEFAULT_CATEGORY;

        // Об'єкт блокування для потокобезпечного запису у файл
        private readonly object _fileLock = new object();

        #endregion

        #region Constructors

        /// <summary>
        /// Створює новий екземпляр MythLogger
        /// </summary>
        /// <param name=""minLogLevel"">Мінімальний рівень логування</param>
        /// <param name=""logToFile"">Чи потрібно писати логи у файл</param>
        /// <param name=""defaultCategory"">Категорія за замовчуванням</param>
        public MythLogger(LogLevel minLogLevel = LogLevel.Info, bool logToFile = false, string defaultCategory = DEFAULT_CATEGORY)
        {
            _minLogLevel = minLogLevel;
            _logToFile = logToFile;
            _defaultCategory = defaultCategory;

            if (logToFile)
            {
                InitializeFileLogging();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Логує інформаційне повідомлення
        /// </summary>
        public void LogInfo(string message, string category = null)
        {
            Log(LogLevel.Info, message, category ?? _defaultCategory);
        }

        /// <summary>
        /// Логує попередження
        /// </summary>
        public void LogWarning(string message, string category = null)
        {
            Log(LogLevel.Warning, message, category ?? _defaultCategory);
        }

        /// <summary>
        /// Логує помилку
        /// </summary>
        public void LogError(string message, string category = null)
        {
            Log(LogLevel.Error, message, category ?? _defaultCategory);
        }

        /// <summary>
        /// Логує фатальну помилку
        /// </summary>
        public void LogFatal(string message, string category = null)
        {
            Log(LogLevel.Fatal, message, category ?? _defaultCategory);
        }

        /// <summary>
        /// Логує відлагоджувальне повідомлення
        /// </summary>
        public void LogDebug(string message, string category = null)
        {
            Log(LogLevel.Debug, message, category ?? _defaultCategory);
        }

        /// <summary>
        /// Логує трасувальне повідомлення
        /// </summary>
        public void LogTrace(string message, string category = null)
        {
            Log(LogLevel.Trace, message, category ?? _defaultCategory);
        }

        /// <summary>
        /// Асоціює контекстні дані з логером
        /// </summary>
        public void WithContext(string key, object value)
        {
            if (_context.ContainsKey(key))
            {
                _context[key] = value;
            }
            else
            {
                _context.Add(key, value);
            }
        }

        /// <summary>
        /// Очищує всі контекстні дані
        /// </summary>
        public void ClearContext()
        {
            _context.Clear();
        }

        /// <summary>
        /// Додає збагачувач до логера
        /// </summary>
        public void AddEnricher(LogEnricher enricher)
        {
            if (enricher != null && !_enrichers.Contains(enricher))
            {
                _enrichers.Add(enricher);
            }
        }

        /// <summary>
        /// Видаляє збагачувач з логера
        /// </summary>
        public void RemoveEnricher(LogEnricher enricher)
        {
            if (enricher != null)
            {
                _enrichers.Remove(enricher);
            }
        }

        /// <summary>
        /// Встановлює категорію за замовчуванням
        /// </summary>
        public void SetDefaultCategory(string category)
        {
            _defaultCategory = !string.IsNullOrEmpty(category) ? category : DEFAULT_CATEGORY;
        }

        /// <summary>
        /// Змінює мінімальний рівень логування
        /// </summary>
        public void SetMinLogLevel(LogLevel level)
        {
            _minLogLevel = level;
        }

        /// <summary>
        /// Включає або виключає файлове логування
        /// </summary>
        public void EnableFileLogging(bool enable)
        {
            if (enable && !_logToFile)
            {
                _logToFile = true;
                InitializeFileLogging();
            }
            else if (!enable && _logToFile)
            {
                _logToFile = false;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Основний метод логування
        /// </summary>
        private void Log(
            LogLevel level,
            string message,
            string category,
            [CallerMemberName] string callerMember = """",
            [CallerFilePath] string callerFilePath = """",
            [CallerLineNumber] int callerLineNumber = 0
        )
        {
            if (level < _minLogLevel)
                return;

            string callerFile = Path.GetFileName(callerFilePath);

            // Підготовка контексту для збагачувачів
            var properties = new Dictionary<string, object>(_context)
            {
                { ""level"", level },
                { ""message"", message },
                { ""category"", category },
                { ""timestamp"", DateTime.Now },
                { ""caller"", $""{callerFile}:{callerMember}:{callerLineNumber}"" }
            };

            // Застосування збагачувачів
            foreach (var enricher in _enrichers)
            {
                try
                {
                    string enrichment = enricher(properties);
                    if (!string.IsNullOrEmpty(enrichment))
                    {
                        message += "" "" + enrichment;
                    }
                }
                catch (Exception ex)
                {
                    string errorMsg = $""Error in log enricher: {ex.Message}"";
                    UnityEngine.Debug.LogError(errorMsg);
                }
            }

            // Отримання значка для категорії
            string categoryIcon = GetCategoryIcon(category);

            // Отримання значка для рівня логування
            string levelIcon = LogIcons.ContainsKey(level) ? LogIcons[level] : """";

            // Форматування повідомлення з часом, категорією, рівнем та значками
            string timeStr = DateTime.Now.ToString(""HH:mm:ss.fff"");
            string colorTag = GetColorTag(level);

            // Форматування повного повідомлення для логу
            string fullMessage = $""{timeStr} {categoryIcon} {levelIcon} <b>[{category}]</b> {message}"";

            // Якщо є детальний контекст, додати його
            if (properties.Count > 5) // Базовий контекст містить 5 елементів
            {
                StringBuilder contextStr = new StringBuilder("" {"");
                bool first = true;

                foreach (var kv in properties.Where(p =>
                   p.Key != ""level"" &&
                   p.Key != ""message"" &&
                   p.Key != ""category"" &&
                   p.Key != ""timestamp"" &&
                   p.Key != ""caller""))
                {
                    if (!first)
                        contextStr.Append("", "");
                    first = false;

                    contextStr.Append($""{kv.Key}={FormatContextValue(kv.Value)}"");
                }

                contextStr.Append(""}"");
                fullMessage += contextStr.ToString();
            }

            // Додавання інформації про виклик для рівнів Debug та Trace
            if (level <= LogLevel.Debug)
            {
                fullMessage += $"" [{callerFile}:{callerMember}():{callerLineNumber}]"";
            }

            // Виведення у консоль Unity
            LogToUnityConsole(level, $""{colorTag}{fullMessage}</color>"");

            // Виведення у файл, якщо увімкнено
            if (_logToFile)
            {
                LogToFile(level, $""{timeStr} [{level}] [{category}] {message}"");
            }
        }

        /// <summary>
        /// Отримує відповідний значок для категорії логу
        /// </summary>
        private string GetCategoryIcon(string category)
        {
            return LogCategories.ContainsKey(category) ? LogCategories[category] : ""📝"";
        }

        /// <summary>
        /// Повертає тег кольору для рівня логування
        /// </summary>
        private string GetColorTag(LogLevel level)
        {
            return LogColors.ContainsKey(level) ? $""<color={LogColors[level]}>"" : ""<color=white>"";
        }

        /// <summary>
        /// Виводить повідомлення у консоль Unity з відповідним рівнем логування
        /// </summary>
        private void LogToUnityConsole(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Error:
                case LogLevel.Fatal:
                    UnityEngine.Debug.LogError(message);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(message);
                    break;
                default:
                    UnityEngine.Debug.Log(message);
                    break;
            }
        }

        /// <summary>
        /// Записує повідомлення у файл логу
        /// </summary>
        private void LogToFile(LogLevel level, string message)
        {
            if (string.IsNullOrEmpty(_logFilePath))
                return;

            try
            {
                lock (_fileLock)
                {
                    File.AppendAllText(_logFilePath, message + Environment.NewLine);

                    // Перевірка розміру файлу
                    FileInfo logFile = new FileInfo(_logFilePath);
                    if (logFile.Length > MAX_LOG_SIZE_MB * MB)
                    {
                        RotateLogFiles();
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($""Помилка при записі в файл логу: {ex.Message}"");
                _logToFile = false;
            }
        }

        /// <summary>
        /// Ініціалізує файлове логування
        /// </summary>
        private void InitializeFileLogging()
        {
            try
            {
                string logsDir = Path.Combine(Application.persistentDataPath, ""Logs"");

                if (!Directory.Exists(logsDir))
                {
                    Directory.CreateDirectory(logsDir);
                }

                // Створення нового файлу логу з поточною датою та часом
                string timestamp = DateTime.Now.ToString(""yyyyMMdd_HHmmss"");
                _logFilePath = Path.Combine(logsDir, $""{LOG_FILE_PREFIX}{timestamp}{LOG_FILE_EXT}"");

                // Запис заголовка логу
                File.WriteAllText(_logFilePath, $""=== MythHunter Log Started at {DateTime.Now} ===\n"" +
                    $""Application Version: {Application.version}\n"" +
                    $""Unity Version: {Application.unityVersion}\n"" +
                    $""Platform: {Application.platform}\n"" +
                    $""System Language: {Application.systemLanguage}\n"" +
                    $""Device Model: {SystemInfo.deviceModel}\n"" +
                    $""Device Name: {SystemInfo.deviceName}\n"" +
                    $""Operating System: {SystemInfo.operatingSystem}\n"" +
                    $""Processor: {SystemInfo.processorType}\n"" +
                    $""Memory: {SystemInfo.systemMemorySize} MB\n"" +
                    $""Graphics Device: {SystemInfo.graphicsDeviceName}\n"" +
                    $""Graphics Memory: {SystemInfo.graphicsMemorySize} MB\n"" +
                    $""=== Log Entries ===\n\n"");

                // Очищення старих логів
                CleanupOldLogs(logsDir);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($""Не вдалося ініціалізувати файлове логування: {ex.Message}"");
                _logToFile = false;
            }
        }

        /// <summary>
        /// Ротація файлів логу при досягненні максимального розміру
        /// </summary>
        private void RotateLogFiles()
        {
            try
            {
                string directory = Path.GetDirectoryName(_logFilePath);
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(_logFilePath);
                string extension = Path.GetExtension(_logFilePath);

                // Створення нового файлу з номером
                string timestamp = DateTime.Now.ToString(""yyyyMMdd_HHmmss"");
                string newLogPath = Path.Combine(directory, $""{fileNameWithoutExt}_{timestamp}{extension}"");

                // Закриття поточного файлу і створення нового
                _logFilePath = newLogPath;

                // Запис заголовка у новий файл
                File.WriteAllText(_logFilePath, $""=== MythHunter Log Continued at {DateTime.Now} ===\n\n"");

                // Очищення старих логів
                CleanupOldLogs(directory);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($""Помилка при ротації файлів логу: {ex.Message}"");
            }
        }

        /// <summary>
        /// Видаляє старі файли логів, залишаючи тільки останні
        /// </summary>
        private void CleanupOldLogs(string directory)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(directory);
                FileInfo[] logFiles = di.GetFiles($""{LOG_FILE_PREFIX}*{LOG_FILE_EXT}"")
                                      .OrderByDescending(f => f.LastWriteTime)
                                      .ToArray();

                // Залишаємо тільки останні MAX_LOG_FILES файлів
                for (int i = MAX_LOG_FILES; i < logFiles.Length; i++)
                {
                    logFiles[i].Delete();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($""Помилка при очищенні старих логів: {ex.Message}"");
            }
        }

        /// <summary>
        /// Форматує значення для контексту логу
        /// </summary>
        private string FormatContextValue(object value)
        {
            if (value == null)
                return ""null"";

            if (value is string)
                return $""\""{value}\"""";

            if (value is DateTime dt)
                return dt.ToString(""yyyy-MM-dd HH:mm:ss.fff"");

            return value.ToString();
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Створює стандартний логер з конфігурацією за замовчуванням
        /// </summary>
        public static MythLogger CreateDefaultLogger()
        {
            // В режимі редактора використовуємо розширений режим з файловим логуванням
            if (Application.isEditor)
            {
                return new MythLogger(LogLevel.Debug, true, ""General"");
            }

            // В релізній збірці використовуємо більш обмежений режим
            bool isDevelopmentBuild = UnityEngine.Debug.isDebugBuild;
            LogLevel level = isDevelopmentBuild ? LogLevel.Info : LogLevel.Warning;
            bool logToFile = isDevelopmentBuild;

            return new MythLogger(level, logToFile, ""General"");
        }

        #endregion
    }

    /// <summary>
    /// Фабрика для створення логерів з різними налаштуваннями
    /// </summary>
    public static class MythLoggerFactory
    {
        private static IMythLogger _defaultLogger;
        private static Dictionary<string, IMythLogger> _loggers = new Dictionary<string, IMythLogger>();

        /// <summary>
        /// Створює або повертає логер за замовчуванням
        /// </summary>
        public static IMythLogger GetDefaultLogger()
        {
            if (_defaultLogger == null)
            {
                _defaultLogger = MythLogger.CreateDefaultLogger();
            }

            return _defaultLogger;
        }

        /// <summary>
        /// Створює або повертає логер для конкретної підсистеми
        /// </summary>
        public static IMythLogger GetLogger(string subsystem)
        {
            if (string.IsNullOrEmpty(subsystem))
            {
                return GetDefaultLogger();
            }

            if (!_loggers.TryGetValue(subsystem, out var logger))
            {
                logger = new MythLogger(defaultCategory: subsystem);
                _loggers[subsystem] = logger;
            }

            return logger;
        }

        /// <summary>
        /// Створює спеціалізований логер з конкретними параметрами
        /// </summary>
        public static IMythLogger CreateCustomLogger(LogLevel level, bool logToFile, string category)
        {
            return new MythLogger(level, logToFile, category);
        }
    }
}
"
    ;
        WriteFile(mythLoggerPath, mythLoggerContent);
    }


    private void CreateEnsureHelper()
    {
        string ensurePath = $"{CODE_PATH}/Utils/Validation/Ensure.cs";
        string ensureContent =
    @"using System;

namespace MythHunter.Utils.Validation
{
    /// <summary>
    /// Утиліта для перевірки умов
    /// </summary>
    public static class Ensure
    {
        public static void NotNull<T>(T value, string paramName) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(paramName, $""{paramName} cannot be null"");
        }
        
        public static void NotNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($""{paramName} cannot be null or empty"", paramName);
        }
        
        public static void IsTrue(bool condition, string message)
        {
            if (!condition)
                throw new ArgumentException(message);
        }
        
        public static void IsInRange(int value, int min, int max, string paramName)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(paramName, value, $""{paramName} must be between {min} and {max}"");
        }
        
        public static void IsInRange(float value, float min, float max, string paramName)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(paramName, value, $""{paramName} must be between {min} and {max}"");
        }
        
        public static void IsPositive(int value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(paramName, value, $""{paramName} must be positive"");
        }
        
        public static void IsPositive(float value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(paramName, value, $""{paramName} must be positive"");
        }
        
        public static void IsNotNegative(int value, string paramName)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(paramName, value, $""{paramName} cannot be negative"");
        }
        
        public static void IsNotNegative(float value, string paramName)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(paramName, value, $""{paramName} cannot be negative"");
        }
    }
}";
        WriteFile(ensurePath, ensureContent);
    }
    private void CreateValidator()
    {
        string validatorPath = $"{CODE_PATH}/Utils/Validation/Validator.cs";
        string validatorContent =
    @"using System;
using System.Collections.Generic;

namespace MythHunter.Utils.Validation
{
    /// <summary>
    /// Клас для валідації об'єктів
    /// </summary>
    public class Validator<T>
    {
        private readonly List<Func<T, ValidationResult>> _validationRules = new List<Func<T, ValidationResult>>();
        
        /// <summary>
        /// Додає правило валідації
        /// </summary>
        public Validator<T> AddRule(Func<T, ValidationResult> rule)
        {
            _validationRules.Add(rule);
            return this;
        }
        
        /// <summary>
        /// Додає умову, яка має виконуватись
        /// </summary>
        public Validator<T> AddCondition(Func<T, bool> condition, string errorMessage)
        {
            _validationRules.Add(obj => condition(obj) 
                ? ValidationResult.Success() 
                : ValidationResult.Error(errorMessage));
                
            return this;
        }
        
        /// <summary>
        /// Перевіряє об'єкт на відповідність усім правилам
        /// </summary>
        public ValidationResult Validate(T obj)
        {
            List<string> errors = new List<string>();
            
            foreach (var rule in _validationRules)
            {
                var result = rule(obj);
                if (!result.IsValid)
                {
                    errors.AddRange(result.Errors);
                    
                    // Якщо помилка критична - відразу повертаємо результат
                    if (result.IsCritical)
                    {
                        return ValidationResult.Critical(errors);
                    }
                }
            }
            
            return errors.Count > 0 ? ValidationResult.Error(errors) : ValidationResult.Success();
        }
    }
    
    /// <summary>
    /// Результат валідації
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public bool IsCritical { get; private set; }
        public List<string> Errors { get; } = new List<string>();
        
        public static ValidationResult Success()
        {
            return new ValidationResult();
        }
        
        public static ValidationResult Error(string error)
        {
            var result = new ValidationResult();
            result.Errors.Add(error);
            return result;
        }
        
        public static ValidationResult Error(List<string> errors)
        {
            var result = new ValidationResult();
            result.Errors.AddRange(errors);
            return result;
        }
        
        public static ValidationResult Critical(string error)
        {
            var result = new ValidationResult { IsCritical = true };
            result.Errors.Add(error);
            return result;
        }
        
        public static ValidationResult Critical(List<string> errors)
        {
            var result = new ValidationResult { IsCritical = true };
            result.Errors.AddRange(errors);
            return result;
        }
    }
}";
        WriteFile(validatorPath, validatorContent);
    }
    /// //////////////////////////////////////CreateUtilImplementations() ////////////////////////////////
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


