// Шлях: Assets/_MythHunter/Code/Core/DI/DIContainer.cs
// Оновлена реалізація з новими можливостями

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Реалізація DI контейнера з підтримкою розширених можливостей
    /// </summary>
    public class DIContainer : IDIContainer
    {
        // Зберігає фабрики для створення екземплярів
        private readonly Dictionary<Type, Registration> _registrations = new Dictionary<Type, Registration>();

        // Зберігає екземпляри синглтонів
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();

        // Зберігає об'єкти LazyDependency для лінивої ініціалізації
        private readonly Dictionary<Type, object> _lazyDependencies = new Dictionary<Type, object>();

        // Логер
        private readonly IMythLogger _logger;

        // Поточний скоуп
        private DIScope _currentScope;
        private readonly ThreadLocal<DIScope> _asyncLocalScope = new ThreadLocal<DIScope>();

        /// <summary>
        /// Клас для зберігання даних про реєстрацію типу
        /// </summary>
        private class Registration
        {
            public Type ServiceType
            {
                get;
            }
            public Type ImplementationType
            {
                get;
            }
            public DILifetime Lifetime
            {
                get;
            }
            public Func<object> Factory
            {
                get; set;
            }

            public Registration(Type serviceType, Type implementationType, DILifetime lifetime)
            {
                ServiceType = serviceType;
                ImplementationType = implementationType;
                Lifetime = lifetime;
            }
        }

        /// <summary>
        /// Створює новий контейнер залежностей
        /// </summary>
        public DIContainer(IMythLogger logger = null)
        {
            _logger = logger;

            // Реєструємо сам контейнер як сінглтон
            RegisterInstance<IDIContainer>(this);
        }

        #region Базова реєстрація

        /// <summary>
        /// Реєструє залежність (транзієнтну за замовчуванням)
        /// </summary>
        public void Register<TService, TImplementation>() where TImplementation : TService
        {
            RegisterWithLifetime<TService, TImplementation>(DILifetime.Transient);
        }

        /// <summary>
        /// Реєструє транзієнтну залежність
        /// </summary>
        public void RegisterTransient<TService, TImplementation>() where TImplementation : TService
        {
            RegisterWithLifetime<TService, TImplementation>(DILifetime.Transient);
        }

        /// <summary>
        /// Реєструє скоупну залежність
        /// </summary>
        public void RegisterScoped<TService, TImplementation>() where TImplementation : TService
        {
            RegisterWithLifetime<TService, TImplementation>(DILifetime.Scoped);
        }

        /// <summary>
        /// Реєструє сінглтон залежність
        /// </summary>
        public void RegisterSingleton<TService, TImplementation>() where TImplementation : TService
        {
            RegisterWithLifetime<TService, TImplementation>(DILifetime.Singleton);
        }

        /// <summary>
        /// Реєструє залежність з вказаним типом часу життя
        /// </summary>
        private void RegisterWithLifetime<TService, TImplementation>(DILifetime lifetime)
            where TImplementation : TService
        {
            var serviceType = typeof(TService);
            var implementationType = typeof(TImplementation);

            var registration = new Registration(serviceType, implementationType, lifetime);
            _registrations[serviceType] = registration;

            // Створюємо фабрику для типу
            registration.Factory = GetOrCreateFactory(implementationType);

            _logger?.LogDebug($"Registered {lifetime} {implementationType.Name} as {serviceType.Name}", "DI");
        }

        /// <summary>
        /// Реєструє екземпляр як сінглтон
        /// </summary>
        public void RegisterInstance<TService>(TService instance)
        {
            var serviceType = typeof(TService);
            _singletons[serviceType] = instance;

            _logger?.LogDebug($"Registered instance of {serviceType.Name}", "DI");
        }

        /// <summary>
        /// Реєструє лінивий сінглтон, який буде створено при першому зверненні
        /// </summary>
        public void RegisterLazySingleton<TService, TImplementation>()

            where TImplementation : TService
            where TService : class
        {
            var serviceType = typeof(TService);
            var implementationType = typeof(TImplementation);

            // Створюємо фабрику для типу
            var factory = GetOrCreateFactory(implementationType);

            // Створюємо LazyDependency
            var lazyDependency = new LazyDependency<TService>(() => (TService)factory());

            // Зберігаємо LazyDependency
            _lazyDependencies[serviceType] = lazyDependency;

            _logger?.LogDebug($"Registered lazy singleton {implementationType.Name} as {serviceType.Name}", "DI");
        }

        #endregion

        #region Резолвінг залежностей

        /// <summary>
        /// Резолвить залежність
        /// </summary>
        public TService Resolve<TService>()
        {
            var serviceType = typeof(TService);
            return (TService)ResolveByType(serviceType);
        }

        /// <summary>
        /// Резолвить лінивий екземпляр залежності
        /// </summary>
        public LazyDependency<TService> ResolveLazy<TService>() where TService : class
        {
            var serviceType = typeof(TService);

            // Перевіряємо, чи існує LazyDependency
            if (_lazyDependencies.TryGetValue(serviceType, out var lazyDep))
            {
                return (LazyDependency<TService>)lazyDep;
            }

            // Якщо не існує, створюємо новий
            var newLazyDep = new LazyDependency<TService>(() => Resolve<TService>());
            _lazyDependencies[serviceType] = newLazyDep;

            return newLazyDep;
        }

        /// <summary>
        /// Резолвить залежність за типом
        /// </summary>
        private object ResolveByType(Type serviceType)
        {
            // Перевіряємо, чи є екземпляр у сінглтонах
            if (_singletons.TryGetValue(serviceType, out var instance))
            {
                return instance;
            }

            // Перевіряємо, чи є лінива залежність
            if (_lazyDependencies.TryGetValue(serviceType, out var lazyDep))
            {
                // Використовуємо рефлексію для виклику Value
                var valueProperty = lazyDep.GetType().GetProperty("Value");
                return valueProperty.GetValue(lazyDep);
            }

            // Перевіряємо, чи тип зареєстрований
            if (!_registrations.TryGetValue(serviceType, out var registration))
            {
                throw new KeyNotFoundException($"Type {serviceType.Name} is not registered");
            }

            // Залежно від типу життєвого циклу
            switch (registration.Lifetime)
            {
                case DILifetime.Singleton:
                    // Створюємо сінглтон, якщо його ще немає
                    if (!_singletons.TryGetValue(serviceType, out var singleton))
                    {
                        singleton = registration.Factory();
                        _singletons[serviceType] = singleton;

                        // Ін'єктуємо залежності
                        InjectDependencies(singleton);
                    }
                    return singleton;

                case DILifetime.Scoped:
                    // Отримуємо поточний скоуп
                    var currentScope = GetCurrentScope();

                    // Отримуємо екземпляр зі скоупу або створюємо новий
                    var scopedInstance = currentScope.GetOrCreateInstance<object>(); // Тип об'єкта буде замінено генеричною реалізацією

                    // Ін'єктуємо залежності
                    InjectDependencies(scopedInstance);

                    return scopedInstance;

                case DILifetime.Transient:
                default:
                    // Створюємо новий екземпляр
                    var transientInstance = registration.Factory();

                    // Ін'єктуємо залежності
                    InjectDependencies(transientInstance);

                    return transientInstance;
            }
        }

        /// <summary>
        /// Перевіряє, чи тип зареєстрований
        /// </summary>
        public bool IsRegistered<TService>()
        {
            var serviceType = typeof(TService);
            return _singletons.ContainsKey(serviceType) ||
                   _lazyDependencies.ContainsKey(serviceType) ||
                   _registrations.ContainsKey(serviceType);
        }

        #endregion

        #region Фабрики і ін'єкція

        /// <summary>
        /// Отримує або створює фабрику для типу
        /// </summary>
        private Func<object> GetOrCreateFactory(Type type)
        {
            // Отримуємо конструктор з атрибутом [Inject] або перший конструктор
            var constructors = type.GetConstructors();
            var constructor = constructors.FirstOrDefault(c =>
                c.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0) ??
                (constructors.Length > 0 ? constructors[0] : null);

            if (constructor == null)
            {
                throw new Exception($"No suitable constructor found for {type.Name}");
            }

            // Отримуємо параметри конструктора
            var parameters = constructor.GetParameters();

            if (parameters.Length == 0)
            {
                // Простий випадок - немає параметрів, просто створюємо об'єкт
                return () => Activator.CreateInstance(type);
            }

            // Створюємо параметри для лямбда-функції
            var paramExpressions = new Expression[parameters.Length];

            // Створюємо вирази для отримання залежностей
            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;

                // Створюємо вираз для виклику ResolveByType методу для отримання залежності
                var resolveMethod = typeof(DIContainer).GetMethod("ResolveByType",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                paramExpressions[i] = Expression.Call(
                    Expression.Constant(this),
                    resolveMethod,
                    Expression.Constant(paramType));

                // Додаємо приведення типу, якщо потрібно
                if (paramType != typeof(object))
                {
                    paramExpressions[i] = Expression.Convert(paramExpressions[i], paramType);
                }
            }

            // Створюємо вираз для виклику конструктора
            var newExpression = Expression.New(constructor, paramExpressions);

            // Перетворюємо на тип object для зберігання в словнику
            var convertExpression = Expression.Convert(newExpression, typeof(object));

            // Компілюємо вираз у функцію
            return Expression.Lambda<Func<object>>(convertExpression).Compile();
        }

        /// <summary>
        /// Ін'єктує залежності в об'єкт
        /// </summary>
        public void InjectDependencies(object target)
        {
            if (target == null)
                return;

            Type targetType = target.GetType();

            // Ін'єкція в поля
            InjectFields(target, targetType);

            // Ін'єкція в властивості
            InjectProperties(target, targetType);

            // Ін'єкція в методи
            InjectMethods(target, targetType);
        }

        /// <summary>
        /// Ін'єктує залежності в поля об'єкта
        /// </summary>
        private void InjectFields(object target, Type targetType)
        {
            var fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0);

            foreach (var field in fields)
            {
                try
                {
                    var fieldType = field.FieldType;
                    var dependency = ResolveByType(fieldType);
                    field.SetValue(target, dependency);
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Error injecting field {field.Name} in {targetType.Name}: {ex.Message}", "DI", ex);
                }
            }
        }

        /// <summary>
        /// Ін'єктує залежності в властивості об'єкта
        /// </summary>
        private void InjectProperties(object target, Type targetType)
        {
            var properties = targetType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0 && p.CanWrite);

            foreach (var property in properties)
            {
                try
                {
                    var propertyType = property.PropertyType;
                    var dependency = ResolveByType(propertyType);
                    property.SetValue(target, dependency);
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Error injecting property {property.Name} in {targetType.Name}: {ex.Message}", "DI", ex);
                }
            }
        }

        /// <summary>
        /// Ін'єктує залежності в методи об'єкта
        /// </summary>
        private void InjectMethods(object target, Type targetType)
        {
            var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0);

            foreach (var method in methods)
            {
                try
                {
                    var parameters = method.GetParameters();
                    var args = new object[parameters.Length];

                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var paramType = parameters[i].ParameterType;
                        args[i] = ResolveByType(paramType);
                    }

                    method.Invoke(target, args);
                }
                catch (Exception ex)
                {
                    _logger?.LogError($"Error injecting method {method.Name} in {targetType.Name}: {ex.Message}", "DI", ex);
                }
            }
        }

        #endregion

        #region Управління скоупами

        /// <summary>
        /// Створює новий скоуп для залежностей
        /// </summary>
        public DIScope CreateScope(string scopeId = null)
        {
            return new DIScope(this, _currentScope, scopeId);
        }

        /// <summary>
        /// Отримує поточний скоуп або створює новий, якщо немає
        /// </summary>
       
        public DIScope GetCurrentScope()
        {
            // Спершу перевіряємо потокозалежний скоуп
            var threadScope = _asyncLocalScope?.Value;
            if (threadScope != null)
                return threadScope;

            // Якщо немає скоупу, створюємо новий
            if (_currentScope == null)
            {
                _currentScope = CreateScope("default");
                _logger?.LogInfo("Created default DI scope", "DI");
            }

            return _currentScope;
        }

        /// <summary>
        /// Встановлює поточний скоуп
        /// </summary>
        public void SetCurrentScope(DIScope scope)
        {
            if (scope == null)
            {
                _logger?.LogWarning("Attempted to set null scope", "DI");
                return;
            }

            if (Thread.CurrentThread.ManagedThreadId == Thread.CurrentThread.ManagedThreadId)
            {
                // Головний потік
                _currentScope = scope;
            }
            else
            {
                // Інший потік - використовуємо ThreadLocal
                _asyncLocalScope.Value = scope;
            }

            _logger?.LogDebug($"Set current DI scope to {scope.ScopeId}", "DI");
        }

        #endregion

        #region Аналіз і діагностика

        /// <summary>
        /// Аналізує залежності між типами
        /// </summary>
        public void AnalyzeDependencies()
        {
            if (_logger == null)
                return;

            _logger.LogInfo("Analyzing dependencies...", "DI");

            // Список всіх зареєстрованих типів
            var allTypes = new HashSet<Type>();

            // Додаємо сінглтони
            foreach (var singleton in _singletons.Keys)
                allTypes.Add(singleton);

            // Додаємо типи з реєстрацій
            foreach (var registration in _registrations.Values)
            {
                allTypes.Add(registration.ServiceType);
                allTypes.Add(registration.ImplementationType);
            }

            // Аналізуємо залежності для кожного типу
            foreach (var type in allTypes)
            {
                var ctor = GetPreferredConstructor(type);
                if (ctor == null)
                    continue;

                var parameters = ctor.GetParameters();
                if (parameters.Length == 0)
                    continue;

                var dependencyList = new List<string>();
                foreach (var param in parameters)
                {
                    var paramType = param.ParameterType;
                    var isRegistered = _singletons.ContainsKey(paramType) || _registrations.ContainsKey(paramType);
                    dependencyList.Add($"{paramType.Name} (Registered: {isRegistered})");
                }

                _logger.LogInfo($"Type {type.Name} depends on: {string.Join(", ", dependencyList)}", "DI");
            }
        }

        /// <summary>
        /// Знаходить конструктор для ін'єкції
        /// </summary>
        private ConstructorInfo GetPreferredConstructor(Type type)
        {
            // Спочатку шукаємо конструктор з атрибутом [Inject]
            var constructors = type.GetConstructors();
            var constructor = constructors.FirstOrDefault(c =>
                c.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0);

            // Якщо такого немає, беремо конструктор з найбільшою кількістю параметрів
            if (constructor == null && constructors.Length > 0)
            {
                constructor = constructors.OrderByDescending(c => c.GetParameters().Length).First();
            }

            return constructor;
        }

        /// <summary>
        /// Перевіряє, чи відсутня залежність
        /// </summary>
        public bool ValidateMissingDependency<TService>()
        {
            var serviceType = typeof(TService);
            return _singletons.ContainsKey(serviceType) || _registrations.ContainsKey(serviceType);
        }

        /// <summary>
        /// Валідує всі залежності для типу
        /// </summary>
        public void ValidateDependencies(Type type)
        {
            var ctor = GetPreferredConstructor(type);
            if (ctor == null)
                return;

            var parameters = ctor.GetParameters();
            foreach (var param in parameters)
            {
                var paramType = param.ParameterType;
                if (!_singletons.ContainsKey(paramType) && !_registrations.ContainsKey(paramType))
                {
                    _logger?.LogWarning($"Missing dependency: {paramType.Name} for {type.Name}", "DI");
                }
            }
        }

        /// <summary>
        /// Валідує всі зареєстровані залежності
        /// </summary>
        public void ValidateAllDependencies()
        {
            if (_logger == null)
                return;

            _logger.LogInfo("Validating all dependencies...", "DI");

            // Список всіх зареєстрованих типів реалізацій
            var allTypes = new HashSet<Type>();
            foreach (var registration in _registrations.Values)
            {
                allTypes.Add(registration.ImplementationType);
            }

            // Валідуємо кожен тип
            foreach (var type in allTypes)
            {
                ValidateDependencies(type);
            }

            _logger.LogInfo("Dependency validation completed", "DI");
        }

        #endregion
    }
}
