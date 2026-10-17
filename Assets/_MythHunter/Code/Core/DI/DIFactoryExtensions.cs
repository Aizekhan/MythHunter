// Шлях: Assets/_MythHunter/Code/Core/DI/DIFactoryExtensions.cs
using System;
using MythHunter.Core.DI;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Розширення для роботи з фабриками
    /// </summary>
    public static class DIFactoryExtensions
    {
        /// <summary>
        /// Реєструє фабрику для створення об'єктів типу T
        /// </summary>
        public static void RegisterFactory<T>(this IDIContainer container) where T : class, new()
        {
            var factory = new ActivatorFactory<T>(container);
            container.RegisterInstance<IDIFactory<T>>(factory);
        }

        /// <summary>
        /// Реєструє фабрику для створення об'єктів типу TImpl за інтерфейсом TService
        /// </summary>
        public static void RegisterFactory<TService, TImpl>(this IDIContainer container)
            where TImpl : class, TService, new()
            where TService : class
        {
            var factory = new DIFactory<TService>(container);
            container.RegisterInstance<IDIFactory<TService>>(factory);
        }

        /// <summary>
        /// Реєструє фабрику зі спеціальним скоупом
        /// </summary>
        public static void RegisterFactoryWithScope<T>(this IDIContainer container, DIScope scope) where T : class
        {
            var factory = new DIFactory<T>(container, scope);
            container.RegisterInstance<IDIFactory<T>>(factory);
        }

        /// <summary>
        /// Реєструє фабрику з функцією для створення об'єктів
        /// </summary>
        public static void RegisterFactoryFunc<T>(this IDIContainer container, Func<T> factoryFunc) where T : class
        {
            var factory = new FuncFactory<T>(factoryFunc);
            container.RegisterInstance<IDIFactory<T>>(factory);
        }
    }

    /// <summary>
    /// Фабрика, яка створює об'єкти через функцію
    /// </summary>
    public class FuncFactory<T> : IDIFactory<T> where T : class
    {
        private readonly Func<T> _factoryFunc;

        public FuncFactory(Func<T> factoryFunc)
        {
            _factoryFunc = factoryFunc ?? throw new ArgumentNullException(nameof(factoryFunc));
        }

        public T Create()
        {
            return _factoryFunc();
        }

        public T Create(params object[] parameters)
        {
            // Ігноруємо параметри
            return _factoryFunc();
        }
    }
}
