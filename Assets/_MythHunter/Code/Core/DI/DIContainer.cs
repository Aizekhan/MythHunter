using System;
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
            
            throw new Exception($"Type {serviceType.Name} is not registered");
        }
        
        public bool IsRegistered<TService>()
        {
            var serviceType = typeof(TService);
            return _instances.ContainsKey(serviceType) || _factories.ContainsKey(serviceType);
        }
        
        public void AnalyzeDependencies()
        {
            Console.WriteLine("Analyzing dependencies...");
            
            foreach (var registration in _factories)
            {
                Console.WriteLine($"Service: {registration.Key.Name}");
            }
            
            foreach (var instance in _instances)
            {
                Console.WriteLine($"Singleton: {instance.Key.Name}");
            }
        }
    }
}