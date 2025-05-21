using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MythHunter.UI.ViewConfigs;

namespace MythHunter.UI.Core
{
    public class ViewConfigRegistry : IViewConfigRegistry
    {
        private readonly Dictionary<ViewId, ViewConfig> _configsByEnum = new Dictionary<ViewId, ViewConfig>();
        private readonly Dictionary<Type, ViewConfig> _configsByType = new Dictionary<Type, ViewConfig>();

        public ViewConfigRegistry()
        {
            LoadAllConfigs();
        }

        private void LoadAllConfigs()
        {
            var configs = UnityEngine.Resources.LoadAll<ViewConfig>("UI/ViewConfigs");
            foreach (var config in configs)
            {
                if (!_configsByEnum.ContainsKey(config.viewId))
                {
                    _configsByEnum.Add(config.viewId, config);

                    // Реєструємо також за типом, якщо можливо
                    Type viewType = Type.GetType(config.viewTypeName);
                    if (viewType != null && !_configsByType.ContainsKey(viewType))
                    {
                        _configsByType.Add(viewType, config);
                    }
                }
            }
        }

        public ViewConfig Get(ViewId viewId)
        {
            _configsByEnum.TryGetValue(viewId, out var config);
            return config;
        }

        public ViewConfig GetByType<T>() where T : UnityEngine.Component, IView
        {
            _configsByType.TryGetValue(typeof(T), out var config);
            return config;
        }

        public IReadOnlyList<ViewConfig> GetAll() => _configsByEnum.Values.ToList();
    }
}
