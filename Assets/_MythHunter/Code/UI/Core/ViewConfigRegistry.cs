// Шлях: Assets/_MythHunter/Code/UI/Core/ViewConfigRegistry.cs
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MythHunter.UI.Core
{
    public class ViewConfigRegistry : IViewConfigRegistry
    {
        private readonly Dictionary<string, ViewConfig> _configs = new();

        public ViewConfigRegistry()
        {
            var configs = UnityEngine.Resources.LoadAll<ViewConfig>("UI/ViewConfigs");
            foreach (var config in configs)
            {
                if (!_configs.ContainsKey(config.ViewId))
                    _configs.Add(config.ViewId, config);
            }
        }

        public ViewConfig Get(string viewId)
        {
            _configs.TryGetValue(viewId, out var config);
            return config;
        }

        public IReadOnlyList<ViewConfig> GetAll() => _configs.Values.ToList();
    }
}
