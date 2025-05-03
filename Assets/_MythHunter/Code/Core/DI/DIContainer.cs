// Шлях: Assets/_MythHunter/Code/Core/DI/DIContainer.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Реалізація DI контейнера
    /// </summary>
    public class DIContainer : IDIContainer
    {
        private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        private readonly IMythLogger _logger;

        public DIContainer(IMythLogger logger = null)
        {
            _logger = logger;
        }

        public void Register<TService, TImplementation>() where TImplementation : TService
        {
            _factories[typeof(TService)] = () => Activator.CreateInstance(typeof(TImplementation));
        }

        // Шлях: Assets/_MythHunter/Code/Core/DI/DIContainer.cs
        public void RegisterSingleton<TService, TImplementation>() where TImplementation : TService
        {
            var serviceType = typeof(TService);

            if (!_instances.ContainsKey(serviceType))
            {
                // Отримуємо інформацію про конструктори
                var constructors = typeof(TImplementation).GetConstructors();
                if (constructors.Length == 0)
                {
                    // Якщо немає явних конструкторів, спробуємо використати конструктор за замовчуванням
                    _instances[serviceType] = Activator.CreateInstance(typeof(TImplementation));
                }
                else
                {
                    // Беремо перший конструктор (бажано з атрибутом [Inject])
                    var constructor = constructors.FirstOrDefault(c =>
                        c.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0) ?? constructors[0];

                    // Отримуємо параметри конструктора
                    var parameters = constructor.GetParameters();
                    var args = new object[parameters.Length];

                    // Розв'язуємо залежності для кожного параметра
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var paramType = parameters[i].ParameterType;
                        args[i] = ResolveType(paramType);
                    }

                    // Створюємо екземпляр з передачею залежностей
                    _instances[serviceType] = constructor.Invoke(args);
                }
            }
        }

        // Допоміжний метод для розв'язання типів
        private object ResolveType(Type serviceType)
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

            if (_logger != null)
            {
                _logger.LogError($"Type {serviceType.Name} is not registered", "DI");
            }

            throw new Exception($"Type {serviceType.Name} is not registered");
        }

        public void RegisterInstance<TService>(TService instance)
        {
            _instances[typeof(TService)] = instance;
        }

        public TService Resolve<TService>()
        {
            var serviceType = typeof(TService);

            // Перевірка наявності синглтону
            if (_instances.TryGetValue(serviceType, out var instance))
            {
                return (TService)instance;
            }

            // Перевірка наявності фабрики
            if (_factories.TryGetValue(serviceType, out var factory))
            {
                return (TService)factory();
            }

            if (_logger != null)
            {
                _logger.LogError($"Type {serviceType.Name} is not registered", "DI");
            }

            throw new Exception($"Type {serviceType.Name} is not registered");
        }

        public bool IsRegistered<TService>()
        {
            var serviceType = typeof(TService);
            return _instances.ContainsKey(serviceType) || _factories.ContainsKey(serviceType);
        }

        public void AnalyzeDependencies()
        {
            if (_logger != null)
            {
                _logger.LogInfo("Analyzing dependencies...", "DI");

                foreach (var registration in _factories)
                {
                    _logger.LogInfo($"Service: {registration.Key.Name}", "DI");
                }

                foreach (var instance in _instances)
                {
                    _logger.LogInfo($"Singleton: {instance.Key.Name}", "DI");
                }
            }
        }

        // Базовий метод ін'єкції - для внутрішнього використання
        internal void InjectDependenciesInternal(object target, bool logInjections)
        {
            if (target == null)
                return;

            Type targetType = target.GetType();

            // Пошук полів з атрибутом [Inject]
            var fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0);

            foreach (var field in fields)
            {
                Type serviceType = field.FieldType;
                if (!IsRegisteredType(serviceType))
                {
                    if (_logger != null && logInjections)
                    {
                        _logger.LogWarning($"Cannot inject field {field.Name} of type {serviceType.Name}: service not registered", "DI");
                    }
                    continue;
                }

                try
                {
                    object service = ResolveType(serviceType);
                    field.SetValue(target, service);

                    if (_logger != null && logInjections)
                    {
                        _logger.LogDebug($"Injected field {field.Name} of type {serviceType.Name}", "DI");
                    }
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                    {
                        _logger.LogError($"Error injecting field {field.Name}: {ex.Message}", "DI", ex);
                    }
                }
            }

            // Пошук властивостей з атрибутом [Inject]
            var properties = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0);

            foreach (var property in properties)
            {
                Type serviceType = property.PropertyType;
                if (!IsRegisteredType(serviceType))
                {
                    if (_logger != null && logInjections)
                    {
                        _logger.LogWarning($"Cannot inject property {property.Name} of type {serviceType.Name}: service not registered", "DI");
                    }
                    continue;
                }

                try
                {
                    object service = ResolveType(serviceType);
                    property.SetValue(target, service);

                    if (_logger != null && logInjections)
                    {
                        _logger.LogDebug($"Injected property {property.Name} of type {serviceType.Name}", "DI");
                    }
                }
                catch (Exception ex)
                {
                    if (_logger != null)
                    {
                        _logger.LogError($"Error injecting property {property.Name}: {ex.Message}", "DI", ex);
                    }
                }
            }

            // Пошук методів з атрибутом [Inject]
            var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0);

            foreach (var method in methods)
            {
                var parameters = method.GetParameters();
                var args = new object[parameters.Length];

                bool allResolved = true;

                for (int i = 0; i < parameters.Length; i++)
                {
                    Type serviceType = parameters[i].ParameterType;
                    if (!IsRegisteredType(serviceType))
                    {
                        if (_logger != null && logInjections)
                        {
                            _logger.LogWarning($"Cannot inject parameter {parameters[i].Name} of type {serviceType.Name}: service not registered", "DI");
                        }
                        allResolved = false;
                        break;
                    }

                    try
                    {
                        args[i] = ResolveType(serviceType);
                    }
                    catch (Exception ex)
                    {
                        if (_logger != null)
                        {
                            _logger.LogError($"Error resolving parameter {parameters[i].Name}: {ex.Message}", "DI", ex);
                        }
                        allResolved = false;
                        break;
                    }
                }

                if (allResolved)
                {
                    try
                    {
                        method.Invoke(target, args);

                        if (_logger != null && logInjections)
                        {
                            _logger.LogDebug($"Invoked method {method.Name} with {args.Length} parameters", "DI");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (_logger != null)
                        {
                            _logger.LogError($"Error invoking method {method.Name}: {ex.Message}", "DI", ex);
                        }
                    }
                }
            }
        }

        // Публічний інтерфейс для ін'єкції
        public void InjectDependencies(object target)
        {
            InjectDependenciesInternal(target, true);
        }

        // Допоміжні методи для роботи з типами
        private bool IsRegisteredType(Type serviceType)
        {
            return _instances.ContainsKey(serviceType) || _factories.ContainsKey(serviceType);
        }

       
    }
}
