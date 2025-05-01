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

        public void InjectDependencies(object target)
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
                if (!IsRegistered(serviceType))
                {
                    if (_logger != null)
                    {
                        _logger.LogWarning($"Cannot inject field {field.Name} of type {serviceType.Name}: service not registered", "DI");
                    }
                    continue;
                }

                try
                {
                    object service = Resolve(serviceType);
                    field.SetValue(target, service);
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
                if (!IsRegistered(serviceType))
                {
                    if (_logger != null)
                    {
                        _logger.LogWarning($"Cannot inject property {property.Name} of type {serviceType.Name}: service not registered", "DI");
                    }
                    continue;
                }

                try
                {
                    object service = Resolve(serviceType);
                    property.SetValue(target, service);
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
                    if (!IsRegistered(serviceType))
                    {
                        if (_logger != null)
                        {
                            _logger.LogWarning($"Cannot inject parameter {parameters[i].Name} of type {serviceType.Name}: service not registered", "DI");
                        }
                        allResolved = false;
                        break;
                    }

                    try
                    {
                        args[i] = Resolve(serviceType);
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

        // Допоміжний метод для запиту з певним типом
        private bool IsRegistered(Type serviceType)
        {
            return _instances.ContainsKey(serviceType) || _factories.ContainsKey(serviceType);
        }

        // Допоміжний метод для запиту з певним типом
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

            throw new Exception($"Type {serviceType.Name} is not registered");
        }
    }
}
