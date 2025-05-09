// Шлях: Assets/_MythHunter/Code/Core/DI/LazyDependency.cs
using System;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Обгортка для лінивої ініціалізації важких залежностей
    /// </summary>
    /// <typeparam name="T">Тип залежності</typeparam>
    public class LazyDependency<T> where T : class
    {
        private readonly Func<T> _factory;
        private T _value;
        private readonly object _lock = new object();
        private bool _isValueCreated;

        /// <summary>
        /// Повертає значення, ініціалізуючи його при першому зверненні
        /// </summary>
        public T Value
        {
            get
            {
                if (!_isValueCreated)
                {
                    lock (_lock)
                    {
                        if (!_isValueCreated)
                        {
                            _value = _factory();
                            _isValueCreated = true;
                        }
                    }
                }
                return _value;
            }
        }

        /// <summary>
        /// Вказує, чи було створено значення
        /// </summary>
        public bool IsValueCreated => _isValueCreated;

        /// <summary>
        /// Створює новий екземпляр LazyDependency
        /// </summary>
        /// <param name="factory">Фабрична функція для створення залежності</param>
        public LazyDependency(Func<T> factory)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        /// <summary>
        /// Явно створює екземпляр залежності, якщо вона ще не створена
        /// </summary>
        public T ForceInitialize()
        {
            return Value;
        }

        /// <summary>
        /// Скидає екземпляр залежності, щоб його було перестворено при наступному зверненні
        /// </summary>
        public void Reset()
        {
            lock (_lock)
            {
                _value = null;
                _isValueCreated = false;
            }
        }

        /// <summary>
        /// Перетворює LazyDependency у тип залежності
        /// </summary>
        public static implicit operator T(LazyDependency<T> lazy)
        {
            return lazy.Value;
        }
    }
}
