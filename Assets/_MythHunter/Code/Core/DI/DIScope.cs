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
        private readonly HashSet<IDisposable> _disposables = new HashSet<IDisposable>();
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
            Type serviceType = typeof(TService);
            lock (_syncLock)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(ScopeId);

                // Якщо екземпляр вже існує в цьому скоупі, повертаємо його
                if (_scopedInstances.TryGetValue(serviceType, out var instance))
                    return (TService)instance;

                // Інакше створюємо новий екземпляр
                var newInstance = _container.Resolve<TService>();

                // Запам'ятовуємо екземпляр, якщо він потребує звільнення ресурсів
                if (newInstance is IDisposable disposable)
                    _disposables.Add(disposable);

                // Зберігаємо екземпляр у скоупі
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

                // Звільняємо ресурси всіх IDisposable екземплярів
                foreach (var disposable in _disposables)
                {
                    try
                    {
                        disposable.Dispose();
                    }
                    catch (Exception ex)
                    {
                        // Логуємо помилку, але продовжуємо звільнення ресурсів
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
