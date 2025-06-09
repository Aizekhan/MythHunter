// Шлях: Assets/Editor/MythHunterCodeGenerator.cs

using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MythHunter.Editor
{
    public class MythHunterCodeGenerator : EditorWindow
    {
        private enum CodeType
        {
            Component,
            SerializableComponent,
            System,
            Event,
            Entity,
            EntityArchetype,
            Service,
            Factory,
            Manager,
            Installer,
            ModelView,
            ScriptableObject
        }

        private CodeType _selectedCodeType = CodeType.Component;
        private string _name = "";
        private string _namespace = "MythHunter";
        private string _subNamespace = "";
        private bool _implementsInterface = true;
        private bool _createInterface = true;
        private bool _useUniTask = false;
        private bool _isNetworkSynchronized = false;
        private bool _usePooling = false;
        private Vector2 _scrollPosition;

        [MenuItem("MythHunter/Code Generator")]
        public static void ShowWindow()
        {
            GetWindow<MythHunterCodeGenerator>("MythHunter Code Generator");
        }

        private void OnGUI()
        {
            GUILayout.Label("MythHunter Code Generator", EditorStyles.boldLabel);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            EditorGUILayout.Space(10);

            // Базові налаштування
            _selectedCodeType = (CodeType)EditorGUILayout.EnumPopup("Тип коду:", _selectedCodeType);
            _name = EditorGUILayout.TextField("Ім'я:", _name);
            _namespace = EditorGUILayout.TextField("Базовий namespace:", _namespace);

            // Підпростір імен залежно від типу
            _subNamespace = GetDefaultSubNamespace(_selectedCodeType);
            _subNamespace = EditorGUILayout.TextField("Підпростір імен:", _subNamespace);

            EditorGUILayout.Space(10);

            // Додаткові опції залежно від типу
            switch (_selectedCodeType)
            {
                case CodeType.Component:
                    _implementsInterface = EditorGUILayout.Toggle("Реалізує IComponent", _implementsInterface);
                    break;
                case CodeType.SerializableComponent:
                    _implementsInterface = EditorGUILayout.Toggle("Реалізує ISerializableComponent", _implementsInterface);
                    break;
                case CodeType.System:
                    _createInterface = EditorGUILayout.Toggle("Створити інтерфейс", _createInterface);
                    _useUniTask = EditorGUILayout.Toggle("Використовувати UniTask", _useUniTask);
                    break;
                case CodeType.Event:
                    _isNetworkSynchronized = EditorGUILayout.Toggle("Мережева подія", _isNetworkSynchronized);
                    _usePooling = EditorGUILayout.Toggle("Використовувати пулінг", _usePooling);
                    break;
                case CodeType.Service:
                    _createInterface = EditorGUILayout.Toggle("Створити інтерфейс", _createInterface);
                    _useUniTask = EditorGUILayout.Toggle("Використовувати UniTask", _useUniTask);
                    break;
                case CodeType.Manager:
                    _createInterface = EditorGUILayout.Toggle("Створити інтерфейс", _createInterface);
                    _useUniTask = EditorGUILayout.Toggle("Використовувати UniTask", _useUniTask);
                    break;
                case CodeType.Factory:
                    _createInterface = EditorGUILayout.Toggle("Створити інтерфейс", _createInterface);
                    break;
                case CodeType.Installer:
                    break;
            }

            EditorGUILayout.Space(20);

            // Кнопка генерації
            GUI.enabled = !string.IsNullOrEmpty(_name);
            if (GUILayout.Button("Згенерувати"))
            {
                GenerateCode();
            }
            GUI.enabled = true;

            EditorGUILayout.EndScrollView();
        }

        private string GetDefaultSubNamespace(CodeType codeType)
        {
            switch (codeType)
            {
                case CodeType.Component:
                    return "Components";
                case CodeType.SerializableComponent:
                    return "Components";
                case CodeType.System:
                    return "Systems";
                case CodeType.Event:
                    return "Events.Domain";
                case CodeType.Entity:
                    return "Entities";
                case CodeType.EntityArchetype:
                    return "Entities.Archetypes";
                case CodeType.Service:
                    return "Services";
                case CodeType.Factory:
                    return "Factories";
                case CodeType.Manager:
                    return "Managers";
                case CodeType.Installer:
                    return "Core.Installers";
                case CodeType.ModelView:
                    return "UI.Models";
                case CodeType.ScriptableObject:
                    return "Data.ScriptableObjects";
                default:
                    return "";
            }
        }

        private string GetFullNamespace()
        {
            if (string.IsNullOrEmpty(_subNamespace))
                return _namespace;
            return $"{_namespace}.{_subNamespace}";
        }

        private void GenerateCode()
        {
            string code = "";
            string folderPath = "";
            string fileName = "";

            // Генерація коду залежно від типу
            switch (_selectedCodeType)
            {
                case CodeType.Component:
                    code = GenerateComponentCode();
                    folderPath = $"Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}";
                    fileName = $"{_name}Component.cs";
                    break;
                case CodeType.SerializableComponent:
                    code = GenerateSerializableComponentCode();
                    folderPath = $"Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}";
                    fileName = $"{_name}Component.cs";
                    break;
                case CodeType.System:
                    code = GenerateSystemCode();
                    folderPath = $"Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}";
                    fileName = $"{_name}System.cs";

                    // Якщо створюємо інтерфейс, генеруємо також файл інтерфейсу
                    if (_createInterface)
                    {
                        string interfaceCode = GenerateSystemInterfaceCode();
                        string interfaceFileName = $"I{_name}System.cs";
                        SaveCodeToFile(interfaceCode, folderPath, interfaceFileName);
                    }
                    break;
                case CodeType.Event:
                    code = GenerateEventCode();
                    folderPath = $"Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}";
                    fileName = $"{_name}Event.cs";
                    break;
                case CodeType.Entity:
                    code = GenerateEntityCode();
                    folderPath = $"Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}";
                    fileName = $"{_name}.cs";
                    break;
                case CodeType.EntityArchetype:
                    code = GenerateEntityArchetypeCode();
                    folderPath = $"Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}";
                    fileName = $"{_name}Archetype.cs";
                    break;
                case CodeType.Service:
                    code = GenerateServiceCode();
                    folderPath = $"Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}";
                    fileName = $"{_name}Service.cs";

                    // Якщо створюємо інтерфейс, генеруємо також файл інтерфейсу
                    if (_createInterface)
                    {
                        string interfaceCode = GenerateServiceInterfaceCode();
                        string interfaceFileName = $"I{_name}Service.cs";
                        SaveCodeToFile(interfaceCode, folderPath, interfaceFileName);
                    }
                    break;
                case CodeType.Factory:
                    code = GenerateFactoryCode();
                    folderPath = $"Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}";
                    fileName = $"{_name}Factory.cs";

                    // Якщо створюємо інтерфейс, генеруємо також файл інтерфейсу
                    if (_createInterface)
                    {
                        string interfaceCode = GenerateFactoryInterfaceCode();
                        string interfaceFileName = $"I{_name}Factory.cs";
                        SaveCodeToFile(interfaceCode, folderPath, interfaceFileName);
                    }
                    break;
                case CodeType.Manager:
                    code = GenerateManagerCode();
                    folderPath = $"Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}";
                    fileName = $"{_name}Manager.cs";

                    // Якщо створюємо інтерфейс, генеруємо також файл інтерфейсу
                    if (_createInterface)
                    {
                        string interfaceCode = GenerateManagerInterfaceCode();
                        string interfaceFileName = $"I{_name}Manager.cs";
                        SaveCodeToFile(interfaceCode, folderPath, interfaceFileName);
                    }
                    break;
                case CodeType.Installer:
                    code = GenerateInstallerCode();
                    folderPath = $"Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}";
                    fileName = $"{_name}Installer.cs";
                    break;
                case CodeType.ModelView:
                    code = GenerateModelViewCode();
                    folderPath = $"Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}";
                    fileName = $"{_name}Model.cs";

                    // Також генеруємо інтерфейс та View
                    string interfaceModelCode = GenerateModelViewInterfaceCode();
                    string interfaceModelFileName = $"I{_name}Model.cs";
                    SaveCodeToFile(interfaceModelCode, folderPath, interfaceModelFileName);

                    string viewCode = GenerateViewCode();
                    string viewFileName = $"{_name}View.cs";
                    SaveCodeToFile(viewCode, folderPath.Replace("Models", "Views"), viewFileName);

                    string presenterCode = GeneratePresenterCode();
                    string presenterFileName = $"{_name}Presenter.cs";
                    SaveCodeToFile(presenterCode, folderPath.Replace("Models", "Presenters"), presenterFileName);

                    string presenterInterfaceCode = GeneratePresenterInterfaceCode();
                    string presenterInterfaceFileName = $"I{_name}Presenter.cs";
                    SaveCodeToFile(presenterInterfaceCode, folderPath.Replace("Models", "Presenters"), presenterInterfaceFileName);
                    break;
                case CodeType.ScriptableObject:
                    code = GenerateScriptableObjectCode();
                    folderPath = $"Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}";
                    fileName = $"{_name}SO.cs";
                    break;
            }

            SaveCodeToFile(code, folderPath, fileName);
        }

        private void SaveCodeToFile(string code, string folderPath, string fileName)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, fileName);
            File.WriteAllText(filePath, code);
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"Файл створено: {filePath}");
        }

        #region Component Generation

        private string GenerateComponentCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/{_name}Component.cs");
            sb.AppendLine("using MythHunter.Core.ECS;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Компонент {_name}");
            sb.AppendLine($"    /// </summary>");

            if (_implementsInterface)
            {
                sb.AppendLine($"    public struct {_name}Component : IComponent");
            }
            else
            {
                sb.AppendLine($"    public struct {_name}Component");
            }

            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: Додайте поля компонента");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateSerializableComponentCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/{_name}Component.cs");
            sb.AppendLine("using MythHunter.Core.ECS;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Серіалізований компонент {_name}");
            sb.AppendLine($"    /// </summary>");

            if (_implementsInterface)
            {
                sb.AppendLine($"    public struct {_name}Component : ISerializableComponent");
            }
            else
            {
                sb.AppendLine($"    public struct {_name}Component");
            }

            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: Додайте поля компонента");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        #endregion

        #region System Generation

        private string GenerateSystemInterfaceCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/I{_name}System.cs");
            sb.AppendLine("using MythHunter.Core.ECS;");

            if (_useUniTask)
            {
                sb.AppendLine("using Cysharp.Threading.Tasks;");
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    public interface I{_name}System : ISystem");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: Додайте публічні методи системи");

            if (_useUniTask)
            {
                sb.AppendLine("        UniTask InitializeAsync();");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateSystemCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/{_name}System.cs");
            sb.AppendLine("using MythHunter.Core.DI;");
            sb.AppendLine("using MythHunter.Core.ECS;");
            sb.AppendLine("using MythHunter.Events;");
            sb.AppendLine("using MythHunter.Utils.Logging;");

            if (_useUniTask)
            {
                sb.AppendLine("using Cysharp.Threading.Tasks;");
                sb.AppendLine("using System;");
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Система {_name}");
            sb.AppendLine($"    /// </summary>");

            if (_createInterface)
            {
                sb.AppendLine($"    public class {_name}System : SystemBase, I{_name}System");
            }
            else
            {
                sb.AppendLine($"    public class {_name}System : SystemBase");
            }

            sb.AppendLine("    {");
            sb.AppendLine("        [Inject]");
            sb.AppendLine($"        public {_name}System(IMythLogger logger, IEventBus eventBus) : base(logger, eventBus)");
            sb.AppendLine("        {");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void Initialize()");
            sb.AppendLine("        {");
            sb.AppendLine("            SubscribeToEvents();");
            sb.AppendLine("            _logger.LogInfo($\"{GetType().Name} initialized\");");
            sb.AppendLine("        }");

            if (_useUniTask)
            {
                sb.AppendLine();
                sb.AppendLine("        public async UniTask InitializeAsync()");
                sb.AppendLine("        {");
                sb.AppendLine("            try");
                sb.AppendLine("            {");
                sb.AppendLine("                // TODO: Асинхронна ініціалізація системи");
                sb.AppendLine("                await UniTask.DelayFrame(1);");
                sb.AppendLine("                _logger.LogInfo($\"{GetType().Name} async initialization completed\");");
                sb.AppendLine("            }");
                sb.AppendLine("            catch (Exception ex)");
                sb.AppendLine("            {");
                sb.AppendLine("                _logger.LogError($\"Error in async initialization: {ex.Message}\", GetType().Name, ex);");
                sb.AppendLine("            }");
                sb.AppendLine("        }");
            }

            sb.AppendLine();
            sb.AppendLine("        public override void Update(float deltaTime)");
            sb.AppendLine("        {");
            sb.AppendLine("            // TODO: Логіка оновлення системи");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        protected override void OnSubscribeToEvents()");
            sb.AppendLine("        {");
            sb.AppendLine("            // TODO: Підписка на події");
            sb.AppendLine("            // Приклад: Subscribe<SomeEvent>(OnSomeEvent);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        protected override void OnUnsubscribeFromEvents()");
            sb.AppendLine("        {");
            sb.AppendLine("            // TODO: Відписка від подій");
            sb.AppendLine("            // Приклад: Unsubscribe<SomeEvent>(OnSomeEvent);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public override void Dispose()");
            sb.AppendLine("        {");
            sb.AppendLine("            UnsubscribeFromEvents();");
            sb.AppendLine("            _logger.LogInfo($\"{GetType().Name} disposed\");");
            sb.AppendLine("        }");
            sb.AppendLine("        ");
            sb.AppendLine("        // Приклад обробника події");
            sb.AppendLine("        /*");
            sb.AppendLine("        private void OnSomeEvent(SomeEvent evt)");
            sb.AppendLine("        {");
            sb.AppendLine("            // Обробка події");
            sb.AppendLine("        }");
            sb.AppendLine("        */");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        #endregion

        #region Event Generation

        private string GenerateEventCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/{_name}Event.cs");
            sb.AppendLine("using System;");
            sb.AppendLine("using MythHunter.Events;");

            if (_isNetworkSynchronized)
            {
                sb.AppendLine("using MythHunter.Events.Network;");
                sb.AppendLine("using MythHunter.Data.Serialization;");
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Подія {_name}");
            sb.AppendLine($"    /// </summary>");

            if (_isNetworkSynchronized)
            {
                sb.AppendLine("    [NetworkEvent(NetworkEventAuthority.Both, NetworkEventPriority.Normal)]");
                sb.AppendLine($"    public struct {_name}Event : INetworkEvent");
            }
            else
            {
                sb.AppendLine($"    public struct {_name}Event : IEvent");
            }

            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: Додайте поля події");
            sb.AppendLine("        public DateTime Timestamp;");
            sb.AppendLine();
            sb.AppendLine("        public string GetEventId() => $\"{GetType().Name}_{Guid.NewGuid()}\";");
            sb.AppendLine();
            sb.AppendLine("        public EventPriority GetPriority() => EventPriority.Normal;");

            if (_isNetworkSynchronized)
            {
                sb.AppendLine();
                sb.AppendLine("        public string GetNetworkEventId() => GetEventId();");
                sb.AppendLine();
                sb.AppendLine("        public bool IsReliable() => true;");
                sb.AppendLine();
                sb.AppendLine("        public NetworkEventPriority GetNetworkPriority() => NetworkEventPriority.Normal;");
                sb.AppendLine();
                sb.AppendLine("        public byte[] Serialize()");
                sb.AppendLine("        {");
                sb.AppendLine("            // TODO: Реалізуйте серіалізацію події");
                sb.AppendLine("            return new byte[0];");
                sb.AppendLine("        }");
                sb.AppendLine();
                sb.AppendLine("        public void Deserialize(byte[] data)");
                sb.AppendLine("        {");
                sb.AppendLine("            // TODO: Реалізуйте десеріалізацію події");
                sb.AppendLine("        }");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        #endregion

        #region Entity Generation

        private string GenerateEntityCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/{_name}.cs");
            sb.AppendLine("using MythHunter.Core.ECS;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Сутність {_name}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public class {_name} : Entity");
            sb.AppendLine("    {");
            sb.AppendLine("        public override string EntityType => \"" + _name + "\";");
            sb.AppendLine();
            sb.AppendLine($"        public {_name}(int id) : base(id)");
            sb.AppendLine("        {");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        // TODO: Додаткові методи для роботи з сутністю");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateEntityArchetypeCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/{_name}Archetype.cs");
            sb.AppendLine("using MythHunter.Core.ECS;");
            sb.AppendLine("using MythHunter.Entities;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Архетип {_name}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public class {_name}Archetype : EntityArchetypeBase");
            sb.AppendLine("    {");
            sb.AppendLine($"        public override string ArchetypeId => \"{_name}\";");
            sb.AppendLine();
            sb.AppendLine("        protected override void DefineRequiredComponents()");
            sb.AppendLine("        {");
            sb.AppendLine("            // TODO: Додайте необхідні компоненти для архетипу");
            sb.AppendLine("            // AddRequiredComponent<SomeComponent>();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        protected override void ApplyBaseComponents(int entityId, IEntityManager entityManager)");
            sb.AppendLine("        {");
            sb.AppendLine("            // TODO: Ініціалізуйте базові компоненти");
            sb.AppendLine("            // entityManager.AddComponent(entityId, new SomeComponent { ... });");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        #endregion

        #region Service Generation

        private string GenerateServiceInterfaceCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/I{_name}Service.cs");

            if (_useUniTask)
            {
                sb.AppendLine("using Cysharp.Threading.Tasks;");
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Інтерфейс сервісу {_name}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public interface I{_name}Service");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: Додайте методи сервісу");

            if (_useUniTask)
            {
                sb.AppendLine("        UniTask InitializeAsync();");
            }
            else
            {
                sb.AppendLine("        void Initialize();");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateServiceCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/{_name}Service.cs");
            sb.AppendLine("using MythHunter.Core.DI;");
            sb.AppendLine("using MythHunter.Utils.Logging;");

            if (_useUniTask)
            {
                sb.AppendLine("using Cysharp.Threading.Tasks;");
                sb.AppendLine("using System;");
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Сервіс {_name}");
            sb.AppendLine($"    /// </summary>");

            if (_createInterface)
            {
                sb.AppendLine($"    public class {_name}Service : I{_name}Service");
            }
            else
            {
                sb.AppendLine($"    public class {_name}Service");
            }

            sb.AppendLine("    {");
            sb.AppendLine("        private readonly IMythLogger _logger;");
            sb.AppendLine();
            sb.AppendLine("        [Inject]");
            sb.AppendLine($"        public {_name}Service(IMythLogger logger)");
            sb.AppendLine("        {");
            sb.AppendLine("            _logger = logger;");
            sb.AppendLine("        }");
            sb.AppendLine();

            if (_useUniTask)
            {
                sb.AppendLine("        public async UniTask InitializeAsync()");
                sb.AppendLine("        {");
                sb.AppendLine("            try");
                sb.AppendLine("            {");
                sb.AppendLine("                // TODO: Асинхронна ініціалізація сервісу");
                sb.AppendLine("                await UniTask.DelayFrame(1);");
                sb.AppendLine("                _logger.LogInfo($\"{GetType().Name} async initialization completed\");");
                sb.AppendLine("            }");
                sb.AppendLine("            catch (Exception ex)");
                sb.AppendLine("            {");
                sb.AppendLine("                _logger.LogError($\"Error in async initialization: {ex.Message}\", GetType().Name, ex);");
                sb.AppendLine("            }");
                sb.AppendLine("        }");
            }
            else
            {
                sb.AppendLine("        public void Initialize()");
                sb.AppendLine("        {");
                sb.AppendLine("            // TODO: Ініціалізація сервісу");
                sb.AppendLine("            _logger.LogInfo($\"{GetType().Name} initialized\");");
                sb.AppendLine("        }");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        #endregion

        #region Factory Generation

        private string GenerateFactoryInterfaceCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/I{_name}Factory.cs");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Інтерфейс фабрики {_name}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public interface I{_name}Factory");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: Додайте методи створення об'єктів");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateFactoryCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/{_name}Factory.cs");
            sb.AppendLine("using MythHunter.Core.DI;");
            sb.AppendLine("using MythHunter.Utils.Logging;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Фабрика {_name}");
            sb.AppendLine($"    /// </summary>");

            if (_createInterface)
            {
                sb.AppendLine($"    public class {_name}Factory : I{_name}Factory");
            }
            else
            {
                sb.AppendLine($"    public class {_name}Factory");
            }

            sb.AppendLine("    {");
            sb.AppendLine("        private readonly IMythLogger _logger;");
            sb.AppendLine();
            sb.AppendLine("        [Inject]");
            sb.AppendLine($"        public {_name}Factory(IMythLogger logger)");
            sb.AppendLine("        {");
            sb.AppendLine("            _logger = logger;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        // TODO: Реалізуйте методи створення об'єктів");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        #endregion

        #region Manager Generation

        private string GenerateManagerInterfaceCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/I{_name}Manager.cs");

            if (_useUniTask)
            {
                sb.AppendLine("using Cysharp.Threading.Tasks;");
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Інтерфейс менеджера {_name}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public interface I{_name}Manager");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: Додайте методи менеджера");

            if (_useUniTask)
            {
                sb.AppendLine("        UniTask InitializeAsync();");
            }
            else
            {
                sb.AppendLine("        void Initialize();");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateManagerCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/{_name}Manager.cs");
            sb.AppendLine("using MythHunter.Core.DI;");
            sb.AppendLine("using MythHunter.Utils.Logging;");
            sb.AppendLine("using MythHunter.Events;");

            if (_useUniTask)
            {
                sb.AppendLine("using Cysharp.Threading.Tasks;");
                sb.AppendLine("using System;");
            }

            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Менеджер {_name}");
            sb.AppendLine($"    /// </summary>");

            if (_createInterface)
            {
                sb.AppendLine($"    public class {_name}Manager : I{_name}Manager");
            }
            else
            {
                sb.AppendLine($"    public class {_name}Manager");
            }

            sb.AppendLine("    {");
            sb.AppendLine("        private readonly IMythLogger _logger;");
            sb.AppendLine("        private readonly IEventBus _eventBus;");
            sb.AppendLine();
            sb.AppendLine("        [Inject]");
            sb.AppendLine($"        public {_name}Manager(IMythLogger logger, IEventBus eventBus)");
            sb.AppendLine("        {");
            sb.AppendLine("            _logger = logger;");
            sb.AppendLine("            _eventBus = eventBus;");
            sb.AppendLine("        }");
            sb.AppendLine();

            if (_useUniTask)
            {
                sb.AppendLine("        public async UniTask InitializeAsync()");
                sb.AppendLine("        {");
                sb.AppendLine("            try");
                sb.AppendLine("            {");
                sb.AppendLine("                // TODO: Асинхронна ініціалізація менеджера");
                sb.AppendLine("                await UniTask.DelayFrame(1);");
                sb.AppendLine("                _logger.LogInfo($\"{GetType().Name} async initialization completed\");");
                sb.AppendLine("            }");
                sb.AppendLine("            catch (Exception ex)");
                sb.AppendLine("            {");
                sb.AppendLine("                _logger.LogError($\"Error in async initialization: {ex.Message}\", GetType().Name, ex);");
                sb.AppendLine("            }");
                sb.AppendLine("        }");
            }
            else
            {
                sb.AppendLine("        public void Initialize()");
                sb.AppendLine("        {");
                sb.AppendLine("            // TODO: Ініціалізація менеджера");
                sb.AppendLine("            _logger.LogInfo($\"{GetType().Name} initialized\");");
                sb.AppendLine("        }");
            }

            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        #endregion

        #region Installer Generation

        private string GenerateInstallerCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/{_name}Installer.cs");
            sb.AppendLine("using MythHunter.Core.DI;");
            sb.AppendLine("using MythHunter.Utils.Logging;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Інсталятор {_name}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public class {_name}Installer : DIInstaller");
            sb.AppendLine("    {");
            sb.AppendLine("        public override void InstallBindings(IDIContainer container)");
            sb.AppendLine("        {");
            sb.AppendLine("            var logger = container.Resolve<IMythLogger>();");
            sb.AppendLine($"            logger.LogInfo(\"Встановлення залежностей {_name}...\", \"Installer\");");
            sb.AppendLine();
            sb.AppendLine("            // TODO: Додайте реєстрацію залежностей");
            sb.AppendLine("            // Приклад: BindSingleton<IService, ServiceImplementation>(container);");
            sb.AppendLine();
            sb.AppendLine($"            logger.LogInfo(\"Встановлення залежностей {_name} завершено\", \"Installer\");");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        #endregion

        #region Model-View Generation

        private string GenerateModelViewInterfaceCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/I{_name}Model.cs");
            sb.AppendLine("using System;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Інтерфейс моделі {_name}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public interface I{_name}Model");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: Додайте властивості та методи моделі");
            sb.AppendLine("        event Action DataChanged;");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateModelViewCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/{_name}Model.cs");
            sb.AppendLine("using System;");
            sb.AppendLine("using MythHunter.Core.DI;");
            sb.AppendLine("using MythHunter.Events;");
            sb.AppendLine("using MythHunter.Utils.Logging;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Модель {_name}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public class {_name}Model : I{_name}Model");
            sb.AppendLine("    {");
            sb.AppendLine("        private readonly IMythLogger _logger;");
            sb.AppendLine("        private readonly IEventBus _eventBus;");
            sb.AppendLine();
            sb.AppendLine("        public event Action DataChanged;");
            sb.AppendLine();
            sb.AppendLine("        [Inject]");
            sb.AppendLine($"        public {_name}Model(IMythLogger logger, IEventBus eventBus)");
            sb.AppendLine("        {");
            sb.AppendLine("            _logger = logger;");
            sb.AppendLine("            _eventBus = eventBus;");
            sb.AppendLine("            SubscribeToEvents();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        // TODO: Додайте властивості та методи моделі");
            sb.AppendLine();
            sb.AppendLine("        private void SubscribeToEvents()");
            sb.AppendLine("        {");
            sb.AppendLine("            // TODO: Підписка на події");
            sb.AppendLine("            // Приклад: _eventBus.Subscribe<SomeEvent>(OnSomeEvent);");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        protected virtual void OnDataChanged()");
            sb.AppendLine("        {");
            sb.AppendLine("            DataChanged?.Invoke();");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GenerateViewCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace("Models", "Views")}/{_name}View.cs");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using UnityEngine.UI;");
            sb.AppendLine("using TMPro;");
            sb.AppendLine("using MythHunter.Core.MonoBehaviours;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace().Replace("Models", "Views")}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// View для {_name}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public class {_name}View : LazyMonoBehaviour");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: Додайте посилання на UI елементи");
            sb.AppendLine("        // [SerializeField] private Button _someButton;");
            sb.AppendLine("        // [SerializeField] private TextMeshProUGUI _someText;");
            sb.AppendLine();
            sb.AppendLine("        public void Initialize()");
            sb.AppendLine("        {");
            sb.AppendLine("            // TODO: Ініціалізація View");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public void Refresh()");
            sb.AppendLine("        {");
            sb.AppendLine("            // TODO: Оновлення View");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        // TODO: Додайте методи для оновлення UI елементів");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GeneratePresenterInterfaceCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace("Models", "Presenters")}/I{_name}Presenter.cs");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace().Replace("Models", "Presenters")}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Інтерфейс презентера {_name}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public interface I{_name}Presenter");
            sb.AppendLine("    {");
            sb.AppendLine("        void Initialize();");
            sb.AppendLine("        void Dispose();");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private string GeneratePresenterCode()
        {
            StringBuilder sb = new StringBuilder();

            string modelNamespace = GetFullNamespace();
            string viewNamespace = GetFullNamespace().Replace("Models", "Views");
            string presenterNamespace = GetFullNamespace().Replace("Models", "Presenters");

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace("Models", "Presenters")}/{_name}Presenter.cs");
            sb.AppendLine("using MythHunter.Core.DI;");
            sb.AppendLine("using MythHunter.Utils.Logging;");
            sb.AppendLine($"using {modelNamespace};");
            sb.AppendLine($"using {viewNamespace};");
            sb.AppendLine();
            sb.AppendLine($"namespace {presenterNamespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// Презентер {_name}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    public class {_name}Presenter : I{_name}Presenter");
            sb.AppendLine("    {");
            sb.AppendLine($"        private readonly I{_name}Model _model;");
            sb.AppendLine($"        private readonly {_name}View _view;");
            sb.AppendLine("        private readonly IMythLogger _logger;");
            sb.AppendLine();
            sb.AppendLine("        [Inject]");
            sb.AppendLine($"        public {_name}Presenter(I{_name}Model model, {_name}View view, IMythLogger logger)");
            sb.AppendLine("        {");
            sb.AppendLine("            _model = model;");
            sb.AppendLine("            _view = view;");
            sb.AppendLine("            _logger = logger;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public void Initialize()");
            sb.AppendLine("        {");
            sb.AppendLine("            _view.Initialize();");
            sb.AppendLine("            _model.DataChanged += OnModelDataChanged;");
            sb.AppendLine("            _view.Refresh();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        private void OnModelDataChanged()");
            sb.AppendLine("        {");
            sb.AppendLine("            _view.Refresh();");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        public void Dispose()");
            sb.AppendLine("        {");
            sb.AppendLine("            _model.DataChanged -= OnModelDataChanged;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        #endregion

        #region ScriptableObject Generation

        private string GenerateScriptableObjectCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"// Шлях: Assets/_MythHunter/Code/{_subNamespace.Replace(".", "/")}/{_name}SO.cs");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GetFullNamespace()}");
            sb.AppendLine("{");
            sb.AppendLine($"    /// <summary>");
            sb.AppendLine($"    /// ScriptableObject для {_name}");
            sb.AppendLine($"    /// </summary>");
            sb.AppendLine($"    [CreateAssetMenu(fileName = \"{_name}\", menuName = \"MythHunter/{_name}\")]");
            sb.AppendLine($"    public class {_name}SO : ScriptableObject");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: Додайте серіалізовані поля");
            sb.AppendLine("        // [SerializeField] private string _someString;");
            sb.AppendLine("        // [SerializeField] private int _someInt;");
            sb.AppendLine();
            sb.AppendLine("        // TODO: Додайте публічні властивості для доступу до полів");
            sb.AppendLine("        // public string SomeString => _someString;");
            sb.AppendLine("        // public int SomeInt => _someInt;");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return sb.ToString();
        }

        #endregion
    }
}
