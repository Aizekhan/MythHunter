// Шлях: Assets/_MythHunter/Code/Core/DI/DIContainer.cs
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Реалізація DI контейнера з оптимізованим створенням об'єктів через Expression Trees
    /// </summary>
    public class DIContainer : IDIContainer
    {
        private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, object> _instances = new Dictionary<Type, object>();
        private readonly Dictionary<Type, Func<object>> _compiledFactories = new Dictionary<Type, Func<object>>();
        private readonly IMythLogger _logger;

        // Кеш для методів ін'єкції в поля/властивості/методи
        private readonly Dictionary<Type, List<Action<object>>> _injectors = new Dictionary<Type, List<Action<object>>>();

        public DIContainer(IMythLogger logger = null)
        {
            _logger = logger;
        }

        public void Register<TService, TImplementation>() where TImplementation : TService
        {
            var serviceType = typeof(TService);
            var implType = typeof(TImplementation);

            // Створюємо та кешуємо фабрику для створення об'єктів
            _factories[serviceType] = GetOrCreateFactory(implType);
        }

        public void RegisterSingleton<TService, TImplementation>() where TImplementation : TService
        {
            var serviceType = typeof(TService);

            if (!_instances.ContainsKey(serviceType))
            {
                var implType = typeof(TImplementation);

                // Отримуємо фабрику або створюємо нову
                var factory = GetOrCreateFactory(implType);

                // Створюємо екземпляр відразу і зберігаємо
                _instances[serviceType] = factory();
                _logger?.LogInfo($"Registered singleton {implType.Name} as {serviceType.Name}", "DI");
            }
        }

        public void RegisterInstance<TService>(TService instance)
        {
            _instances[typeof(TService)] = instance;
        }

        public TService Resolve<TService>()
        {
            var serviceType = typeof(TService);

            // Перевірка, чи є сінглтон
            if (_instances.TryGetValue(serviceType, out var instance))
            {
                return (TService)instance;
            }

            // Перевірка, чи є фабрика
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

            // Отримуємо або створюємо ін'єктори для даного типу
            var targetInjectors = GetOrCreateInjectors(targetType);

            // Виконуємо всі ін'єктори для цього об'єкта
            foreach (var injector in targetInjectors)
            {
                injector(target);
            }
        }

        // Отримує кешовану або створює нову фабрику для типу
        private Func<object> GetOrCreateFactory(Type type)
        {
            // Перевірка в кеші
            if (_compiledFactories.TryGetValue(type, out var factory))
            {
                return factory;
            }

            // Створюємо нову фабрику через Expression Trees
            factory = BuildFactory(type);
            _compiledFactories[type] = factory;

            return factory;
        }

        // Будує фабрику для створення об'єктів через Expression Trees
        private Func<object> BuildFactory(Type type)
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

                // Створюємо вираз для виклику Resolve методу для отримання залежності
                var resolveMethod = typeof(DIContainer).GetMethod("Resolve").MakeGenericMethod(paramType);
                paramExpressions[i] = Expression.Call(Expression.Constant(this), resolveMethod);
            }

            // Створюємо вираз для виклику конструктора
            var newExpression = Expression.New(constructor, paramExpressions);

            // Перетворюємо на тип object для зберігання в словнику
            var convertExpression = Expression.Convert(newExpression, typeof(object));

            // Компілюємо вираз у функцію
            return Expression.Lambda<Func<object>>(convertExpression).Compile();
        }

        // Отримує або створює список ін'єкторів для типу
        private List<Action<object>> GetOrCreateInjectors(Type type)
        {
            if (_injectors.TryGetValue(type, out var injectors))
            {
                return injectors;
            }

            injectors = new List<Action<object>>();

            // Додаємо ін'єктори для полів
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0))
            {
                var fieldType = field.FieldType;

                // Створюємо вираз для ін'єкції в поле
                var targetParam = Expression.Parameter(typeof(object), "target");
                var targetCast = Expression.Convert(targetParam, type);

                // Створюємо вираз для отримання залежності
                var resolveMethod = typeof(DIContainer).GetMethod("Resolve").MakeGenericMethod(fieldType);
                var resolveCall = Expression.Call(Expression.Constant(this), resolveMethod);

                // Створюємо вираз для присвоєння поля
                var fieldAccess = Expression.Field(targetCast, field);
                var assignExpr = Expression.Assign(fieldAccess, resolveCall);

                // Компілюємо в делегат
                var injector = Expression.Lambda<Action<object>>(assignExpr, targetParam).Compile();
                injectors.Add(injector);
            }

            // Додаємо ін'єктори для властивостей
            foreach (var prop in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0 && p.CanWrite))
            {
                var propType = prop.PropertyType;

                // Створюємо вираз для ін'єкції у властивість
                var targetParam = Expression.Parameter(typeof(object), "target");
                var targetCast = Expression.Convert(targetParam, type);

                // Створюємо вираз для отримання залежності
                var resolveMethod = typeof(DIContainer).GetMethod("Resolve").MakeGenericMethod(propType);
                var resolveCall = Expression.Call(Expression.Constant(this), resolveMethod);

                // Створюємо вираз для присвоєння властивості
                var propAccess = Expression.Property(targetCast, prop);
                var assignExpr = Expression.Assign(propAccess, resolveCall);

                // Компілюємо в делегат
                var injector = Expression.Lambda<Action<object>>(assignExpr, targetParam).Compile();
                injectors.Add(injector);
            }

            // Додаємо ін'єктори для методів (складніше, тому використовуємо окремий метод)
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
     .Where(m => m.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0))
            {
                // Створюємо оптимізований ін'єктор для методу
                var methodInjector = CreateMethodInjector(type, method);
                injectors.Add(methodInjector);
            }

            _injectors[type] = injectors;
            return injectors;
        }

        // Додаємо у клас DIContainer новий метод для оптимізації ін'єкції в методи
        private Action<object> CreateMethodInjector(Type targetType, MethodInfo method)
        {
            var parameters = method.GetParameters();

            // Створюємо вираз для параметра target
            var targetParam = Expression.Parameter(typeof(object), "target");
            var targetCast = Expression.Convert(targetParam, targetType);

            // Створюємо вирази для отримання параметрів методу
            var paramExprs = new Expression[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var paramType = parameters[i].ParameterType;
                var resolveMethod = typeof(DIContainer).GetMethod("Resolve").MakeGenericMethod(paramType);
                paramExprs[i] = Expression.Call(Expression.Constant(this), resolveMethod);
            }

            // Створюємо вираз для виклику методу з параметрами
            var methodCallExpr = Expression.Call(targetCast, method, paramExprs);

            // Компілюємо вираз у делегат
            return Expression.Lambda<Action<object>>(methodCallExpr, targetParam).Compile();
        }
        // Допоміжний метод для ін'єкції в метод (поки що через рефлексію)
        private void InjectIntoMethod(object target, MethodInfo method)
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

        // Допоміжний метод для отримання залежності за типом
        private object ResolveByType(Type type)
        {
            // Перевірка, чи є синглтон
            if (_instances.TryGetValue(type, out var instance))
            {
                return instance;
            }

            // Перевірка, чи є фабрика
            if (_factories.TryGetValue(type, out var factory))
            {
                return factory();
            }

            throw new Exception($"Type {type.Name} is not registered");
        }
    }
}
