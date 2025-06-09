// Шлях: Assets/_MythHunter/Code/Core/DI/DIFactories.cs
using System;
using MythHunter.Core.DI;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Інтерфейс для фабрики, яка створює об'єкти через DI
    /// </summary>
    public interface IDIFactory<T> where T : class
    {
        /// <summary>
        /// Створює новий екземпляр об'єкта
        /// </summary>
        T Create();

        /// <summary>
        /// Створює новий екземпляр об'єкта з параметрами
        /// </summary>
        T Create(params object[] parameters);
    }

    /// <summary>
    /// Базова реалізація фабрики через DI
    /// </summary>
    public class DIFactory<T> : IDIFactory<T> where T : class
    {
        private readonly IDIContainer _container;
        private readonly DIScope _scope;

        public DIFactory(IDIContainer container, DIScope scope = null)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _scope = scope;
        }

        /// <summary>
        /// Створює новий екземпляр об'єкта
        /// </summary>
        public T Create()
        {
            // Запам'ятовуємо поточний скоуп
            var currentScope = _container.GetCurrentScope();

            try
            {
                // Встановлюємо скоуп фабрики, якщо він вказаний
                if (_scope != null)
                {
                    _container.SetCurrentScope(_scope);
                }

                // Створюємо об'єкт
                return _container.Resolve<T>();
            }
            finally
            {
                // Відновлюємо попередній скоуп
                if (_scope != null)
                {
                    _container.SetCurrentScope(currentScope);
                }
            }
        }

        /// <summary>
        /// Створює новий екземпляр об'єкта з параметрами
        /// </summary>
        public T Create(params object[] parameters)
        {
            // Реалізація з параметрами вимагає більш складної логіки
            // та рефлексії для виклику конструктора з параметрами
            // Поки що повертаємо звичайний екземпляр
            return Create();
        }
    }

    /// <summary>
    /// Фабрика, яка створює об'єкти через активатор типів
    /// </summary>
    public class ActivatorFactory<T> : IDIFactory<T> where T : class, new()
    {
        private readonly IDIContainer _container;

        public ActivatorFactory(IDIContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        /// <summary>
        /// Створює новий екземпляр об'єкта
        /// </summary>
        public T Create()
        {
            // Створюємо об'єкт
            T instance = new T();

            // Ін'єктуємо залежності
            _container.InjectDependencies(instance);

            return instance;
        }

        /// <summary>
        /// Створює новий екземпляр об'єкта з параметрами
        /// </summary>
        public T Create(params object[] parameters)
        {
            // Активатор не підтримує параметри
            return Create();
        }
    }
}
