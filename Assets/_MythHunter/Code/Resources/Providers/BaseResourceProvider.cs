// Шлях: Assets/_MythHunter/Code/Resources/Providers/BaseResourceProvider.cs
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Resources.Providers
{
    /// <summary>
    /// Базовий абстрактний клас для ресурсних провайдерів
    /// </summary>
    public abstract class BaseResourceProvider
    {
        protected readonly IMythLogger _logger;
        protected readonly Dictionary<string, Object> _loadedResources = new Dictionary<string, Object>();

        protected BaseResourceProvider(IMythLogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Логування з вказаною категорією
        /// </summary>
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

        /// <summary>
        /// Перевірка, чи завантажений ресурс
        /// </summary>
        protected bool IsResourceLoaded(string key)
        {
            return _loadedResources.ContainsKey(key);
        }

        /// <summary>
        /// Очищення кешу ресурсів
        /// </summary>
        protected void ClearResourceCache()
        {
            _loadedResources.Clear();
            MythHunter.Resources.Core.MythResourceUtils.UnloadUnusedAssets();
        }
    }
}
