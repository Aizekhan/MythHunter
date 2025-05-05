// Шлях: Assets/_MythHunter/Code/Resources/Providers/ResourceProviderBase.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Resources.Core;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Resources.Providers
{
    /// <summary>
    /// Базовий клас для всіх провайдерів ресурсів
    /// </summary>
    public abstract class ResourceProviderBase : IResourceProvider, IPrioritizable
    {
        protected readonly IMythLogger _logger;
        protected readonly Dictionary<string, Object> _loadedResources = new Dictionary<string, Object>();
        protected readonly int _priority;

        public int Priority => _priority;

        protected ResourceProviderBase(IMythLogger logger, int priority = 0)
        {
            _logger = logger;
            _priority = priority;
        }

        public abstract UniTask<T> LoadAsync<T>(string key) where T : Object;

        public abstract UniTask<IReadOnlyList<T>> LoadAllAsync<T>(string pattern) where T : Object;

        public virtual void Unload(string key)
        {
            if (_loadedResources.TryGetValue(key, out var resource))
            {
                _loadedResources.Remove(key);
                LogInfo($"Resource unloaded: {key}");
            }
        }

        public virtual void UnloadAll()
        {
            _loadedResources.Clear();
            LogInfo("All resources unloaded");
        }

        protected void LogDebug(string message)
        {
            _logger?.LogDebug(message, "Resource");
        }

        protected void LogInfo(string message)
        {
            _logger?.LogInfo(message, "Resource");
        }

        protected void LogWarning(string message)
        {
            _logger?.LogWarning(message, "Resource");
        }

        protected void LogError(string message)
        {
            _logger?.LogError(message, "Resource");
        }
    }
}
