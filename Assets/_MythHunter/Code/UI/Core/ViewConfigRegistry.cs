using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MythHunter.UI.ViewConfigs;

namespace MythHunter.UI.Core
{
    public class ViewConfigRegistry : IViewConfigRegistry
    {
        private readonly Dictionary<ViewId, ViewConfig> _configsById = new();
        private readonly Dictionary<string, ViewConfig> _configsByName = new(); // Якщо потрібно доступ через nameof(ViewId)

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
        public ViewConfig GetByType<T>() where T : Component, IView
        {
            foreach (var config in _configsById.Values)
            {
                if (config.prefabPath.ToLower().Contains(typeof(T).Name.ToLower()))
                    return config;
            }

            return null;
        }
        public IReadOnlyList<ViewConfig> GetAll() => _configsById.Values.ToList();

        
    }
}
