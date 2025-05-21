// Шлях: Assets/_MythHunter/Code/UI/Core/ViewConfigRegistry.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MythHunter.UI.ViewConfigs;

namespace MythHunter.UI.Core
{
    public class ViewConfigRegistry : IViewConfigRegistry
    {
        private readonly Dictionary<ViewId, ViewConfig> _configsById = new();
        private readonly Dictionary<string, ViewConfig> _configsByName = new();
        private readonly Dictionary<Type, ViewConfig> _configsByType = new(); // Додано для кешування відповідностей

        public ViewConfigRegistry()
        {
            LoadAllConfigs();
        }

        private void LoadAllConfigs()
        {
            var configs = UnityEngine.Resources.LoadAll<ViewConfig>("UI/ViewConfigs");

            foreach (var config in configs)
            {
                if (!_configsById.ContainsKey(config.viewId))
                {
                    _configsById.Add(config.viewId, config);
                    _configsByName.Add(config.viewId.ToString(), config);
                }
            }
        }

        public ViewConfig Get(ViewId viewId)
        {
            _configsById.TryGetValue(viewId, out var config);
            return config;
        }

        /// <summary>
        /// Знаходить конфігурацію за ім'ям типу представлення.
        /// Використовується тільки для внутрішньої логіки пошуку ViewId.
        /// </summary>
        /// <param name="viewTypeName">Ім'я типу представлення</param>
        /// <returns>Знайдена конфігурація або null</returns>
        public ViewConfig GetByTypeName(string viewTypeName)
        {
            if (string.IsNullOrEmpty(viewTypeName))
                return null;

            // Спочатку спробуємо знайти за ім'ям
            if (_configsByName.TryGetValue(viewTypeName, out var config))
                return config;

            // Потім - за шляхом префабу
            foreach (var existingConfig in _configsById.Values)
            {
                if (existingConfig.prefabPath.ToLower().Contains(viewTypeName.ToLower()))
                    return existingConfig;
            }

            return null;
        }

        // Цей метод залишається для зворотної сумісності, але буде позначений як застарілий
        [System.Obsolete("Використовуйте GetByTypeName замість GetByType для відповідності принципам архітектури")]
        public ViewConfig GetByType<T>() where T : Component, IView
        {
            return GetByTypeName(typeof(T).Name);
        }

        public IReadOnlyList<ViewConfig> GetAll() => _configsById.Values.ToList();
    }
}
