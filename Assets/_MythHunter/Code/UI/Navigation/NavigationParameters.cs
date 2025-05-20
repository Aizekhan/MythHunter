// Шлях: Assets/_MythHunter/Code/UI/Navigation/NavigationParameters.cs

using System;
using System.Collections.Generic;

namespace MythHunter.UI.Navigation
{
    /// <summary>
    /// Клас для передачі параметрів між екранами під час навігації
    /// </summary>
    public class NavigationParameters
    {
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

        /// <summary>
        /// Додати параметр
        /// </summary>
        public void Add<T>(string key, T value)
        {
            _parameters[key] = value;
        }

        /// <summary>
        /// Отримати параметр за ключем
        /// </summary>
        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (_parameters.TryGetValue(key, out var value) && value is T typedValue)
                return typedValue;

            return defaultValue;
        }

        /// <summary>
        /// Перевірити наявність параметра
        /// </summary>
        public bool Contains(string key)
        {
            return _parameters.ContainsKey(key);
        }

        /// <summary>
        /// Отримати всі ключі параметрів
        /// </summary>
        public IEnumerable<string> Keys => _parameters.Keys;

        /// <summary>
        /// Створити новий об'єкт параметрів на основі поточного
        /// </summary>
        public NavigationParameters Clone()
        {
            var clone = new NavigationParameters();
            foreach (var pair in _parameters)
            {
                clone._parameters[pair.Key] = pair.Value;
            }
            return clone;
        }
    }
}
