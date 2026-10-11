// Шлях: Assets/_MythHunter/Code/Core/DI/DIScope.cs
using System;
using System.Collections.Generic;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Тип скоупу життєвого циклу залежності
    /// </summary>
    public enum DILifetime
    {
        /// <summary>
        /// Транзієнтні залежності створюються щоразу при запиті
        /// </summary>
        Transient,

        /// <summary>
        /// Скоупні залежності існують на час життя скоупу
        /// </summary>
        Scoped,

        /// <summary>
        /// Сінглтони існують на весь час роботи програми
        /// </summary>
        Singleton
    }

    /// <summary>
    /// Скоуп для управління часом життя залежностей
    /// </summary>
    public class DIScope : IDisposable
    {
        private readonly IDIContainer _container;
        private readonly Dictionary<Type, object> _scopedInstances = new Dictionary<Type, object>();
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        private readonly DIScope _parentScope;
        private readonly object _syncLock = new object();
        private bool _isDisposed;

        /// <summary>
        /// Ідентифікатор скоупу
        /// </summary>
        public string ScopeId
        {
            get;
        }

        /// <summary>
        /// Батьківський скоуп
        /// </summary>
        public DIScope ParentScope => _parentScope;

        /// <summary>
        /// Створює новий скоуп
        /// </summary>
        /// <param name="container">Контейнер DI</param>
        /// <param name="parentScope">Батьківський скоуп, якщо є</param>
        /// <param name="scopeId">Ідентифікатор скоупу</param>
        public DIScope(IDIContainer container, DIScope parentScope = null, string scopeId = null)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _parentScope = parentScope;
            ScopeId = scopeId ?? Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Отримує або створює екземпляр залежності в поточному скоупі
        /// </summary>
        public TService GetOrCreateInstance<TService>() where TService : class
        {
            return (TService)GetOrCreateInstance(typeof(TService), () => _container.Resolve<TService>());
        }

        // The container supplies the factory so scoped resolution can be cached under
        // the actual requested service type without recursively resolving object.
        internal object GetOrCreateInstance(Type serviceType, Func<object> factory)
        {
            if (serviceType == null)
                throw new ArgumentNullException(nameof(serviceType));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            lock (_syncLock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(ScopeId);

                if (_scopedInstances.TryGetValue(serviceType, out var instance))
                    return instance;

                var newInstance = factory();

                if (newInstance is IDisposable disposable)
                    _disposables.Add(disposable);

                _scopedInstances[serviceType] = newInstance;
                return newInstance;
            }
        }

        /// <summary>
        /// Перевіряє, чи екземпляр залежності вже створено в цьому скоупі
        /// </summary>
        public bool HasInstance<TService>()
        {
            lock (_syncLock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(ScopeId);

                return _scopedInstances.ContainsKey(typeof(TService));
            }
        }

        /// <summary>
        /// Звільняє ресурси скоупу
        /// </summary>
        public void Dispose()
        {
            lock (_syncLock)
            {
                if (_isDisposed)
                    return;

                // Dispose in reverse creation order (stack semantics). Continue even if
                // one resource throws, so all other scope-owned resources are released.
                for (var i = _disposables.Count - 1; i >= 0; i--)
                {
                    try
                    {
                        _disposables[i].Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error disposing instance: {ex.Message}");
                    }
                }

                _disposables.Clear();
                _scopedInstances.Clear();
                _isDisposed = true;
            }
        }

        /// <summary>
        /// Створює дочірній скоуп
        /// </summary>
        public DIScope CreateChildScope(string scopeId = null)
        {
            lock (_syncLock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(ScopeId);

                return new DIScope(_container, this, scopeId);
            }
        }
    }
}
