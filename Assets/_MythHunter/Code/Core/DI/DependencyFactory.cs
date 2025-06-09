using System;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Інтерфейс для фабрики залежностей
    /// </summary>
    public interface IDependencyFactory<T>
    {
        T Create();
    }

    /// <summary>
    /// Фабрика, яка створює об'єкти через контейнер залежностей
    /// </summary>
    public class DependencyFactory<T> : IDependencyFactory<T> where T : class
    {
        private readonly Func<T> _factoryFunc;

        public DependencyFactory(Func<T> factoryFunc)
        {
            _factoryFunc = factoryFunc ?? throw new ArgumentNullException(nameof(factoryFunc));
        }

        public T Create()
        {
            return _factoryFunc();
        }
    }

    /// <summary>
    /// Розширення для реєстрації фабрик
    /// </summary>
    public static class DIContainerFactoryExtensions
    {
        /// <summary>
        /// Реєструє фабрику, яка створює об'єкти за допомогою контейнера
        /// </summary>
        public static void RegisterFactory<T>(this IDIContainer container, Func<IDIContainer, T> factoryFunc)
            where T : class
        {
            // Створюємо фабрику, яка використовує контейнер
            var factory = new DependencyFactory<T>(() => factoryFunc(container));
            container.RegisterInstance<IDependencyFactory<T>>(factory);
        }

        /// <summary>
        /// Реєструє фабрику, яка створює об'єкти типу TImplementation за інтерфейсом TService
        /// </summary>
        public static void RegisterFactory<TService, TImplementation>(this IDIContainer container)
            where TImplementation : class, TService
            where TService : class
        {
            // Реєструємо фабрику, яка створює об'єкти через контейнер
            container.RegisterFactory<TService>(c => (TService)Activator.CreateInstance(typeof(TImplementation)));
        }
    }
}
