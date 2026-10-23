// Шлях: Assets/_MythHunter/Code/UI/Navigation/NavigationParameters.cs

using System;
using System.Collections.Generic;
using MythHunter.UI.Core;

namespace MythHunter.UI.Navigation
{
    /// <summary>
    /// Клас для передачі параметрів між екранами під час навігації
    /// </summary>
    public class NavigationParameters
    {
        private readonly Dictionary<string, object> _parameters = new Dictionary<string, object>();

        /// <summary>
        /// Створює порожній об'єкт навігаційних параметрів
        /// </summary>
        public NavigationParameters()
        {
        }

        /// <summary>
        /// Створює об'єкт навігаційних параметрів з параметром TargetViewId
        /// </summary>
        /// <param name="targetViewId">Цільовий ViewId для навігації</param>
        public NavigationParameters(ViewId targetViewId) : this()
        {
            Add("TargetViewId", targetViewId);
        }

        /// <summary>
        /// Додати параметр
        /// </summary>
        public void Add<T>(string key, T value)
        {
            _parameters[key] = value;
        }

        /// <summary>
        /// Додати ViewId як параметр
        /// </summary>
        public void AddViewId(string key, ViewId viewId)
        {
            _parameters[key] = viewId;
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
        /// Отримати ViewId з параметрів
        /// </summary>
        public ViewId GetViewId(string key, ViewId defaultValue = ViewId.None)
        {
            return GetValue(key, defaultValue);
        }

        /// <summary>
        /// Отримати цільовий ViewId з параметрів
        /// </summary>
        public ViewId GetTargetViewId(ViewId defaultValue = ViewId.None)
        {
            return GetViewId("TargetViewId", defaultValue);
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

        /// <summary>
        /// Видаляє параметр за ключем
        /// </summary>
        public void Remove(string key)
        {
            if (Contains(key))
            {
                _parameters.Remove(key);
            }
        }

        /// <summary>
        /// Очищає всі параметри
        /// </summary>
        public void Clear()
        {
            _parameters.Clear();
        }
    }
}
