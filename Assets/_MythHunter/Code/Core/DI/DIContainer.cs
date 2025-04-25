using System;
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
}