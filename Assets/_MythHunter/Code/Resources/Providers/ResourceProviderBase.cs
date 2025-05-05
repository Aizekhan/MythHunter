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
                Log($"Resource unloaded: {key}");
            }
        }

        public virtual void UnloadAll()
        {
            _loadedResources.Clear();
            Log("All resources unloaded");
        }

        public virtual T GetFromPool<T>(string key) where T : Object
        {
            // Базова реалізація повертає null - провайдери можуть перевизначити
            return null;
        }

        public virtual void ReturnToPool<T>(string key, T instance) where T : Object
        {
            // Базова реалізація нічого не робить - провайдери можуть перевизначити
        }

        protected void Log(string message, LogLevel level = LogLevel.Debug)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    _logger.LogDebug(message, "Resource");
                    break;
                case LogLevel.Info:
                    _logger.LogInfo(message, "Resource");
                    break;
                case LogLevel.Warning:
                    _logger.LogWarning(message, "Resource");
                    break;
                case LogLevel.Error:
                    _logger.LogError(message, "Resource");
                    break;
                default:
                    _logger.LogInfo(message, "Resource");
                    break;
            }
        }
    }
}
