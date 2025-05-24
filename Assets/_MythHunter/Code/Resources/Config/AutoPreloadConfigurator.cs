// Assets/_MythHunter/Code/Resources/Config/AutoPreloadConfigurator.cs
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Resources.Config;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Resources
{
    /// <summary>
    /// Автоматично реєструє preload конфігурації зі ScriptableObject-ів
    /// </summary>
    public class AutoPreloadConfigurator
    {
        private readonly IPreloadManager _preloadManager;
        private readonly IMythLogger _logger;
        private readonly List<PreloadSceneConfig> _loadedConfigs = new();

        [Inject]
        public AutoPreloadConfigurator(IPreloadManager preloadManager, IMythLogger logger)
        {
            _preloadManager = preloadManager;
            _logger = logger;
        }

        /// <summary>
        /// Завантажує та реєструє всі preload конфігурації
        /// </summary>
        public async UniTask LoadAndRegisterAllConfigsAsync()
        {
            _logger.LogInfo("🔧 Завантаження preload конфігурацій зі ScriptableObject-ів", "PreloadConfig");

            try
            {
                // Завантажуємо всі конфігурації з Resources
                var configs = UnityEngine.Resources.LoadAll<PreloadSceneConfig>("PreloadConfigs");

                if (configs.Length == 0)
                {
                    _logger.LogWarning("⚠️ Не знайдено жодної preload конфігурації в Resources/PreloadConfigs", "PreloadConfig");
                    return;
                }

                _logger.LogInfo($"📋 Знайдено {configs.Length} preload конфігурацій", "PreloadConfig");

                foreach (var config in configs)
                {
                    await RegisterConfigAsync(config);
                }

                _logger.LogInfo($"✅ Зареєстровано {_loadedConfigs.Count} preload конфігурацій", "PreloadConfig");
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"❌ Помилка завантаження preload конфігурацій: {ex.Message}", "PreloadConfig", ex);
            }
        }

        /// <summary>
        /// Реєструє конкретну конфігурацію
        /// </summary>
        private async UniTask RegisterConfigAsync(PreloadSceneConfig config)
        {
            if (config == null)
            {
                _logger.LogWarning("⚠️ Пропущено null конфігурацію", "PreloadConfig");
                return;
            }

            // Валідуємо конфігурацію
            var errors = config.ValidateConfig();
            if (errors.Any())
            {
                _logger.LogError($"❌ Помилки в конфігурації {config.name}:", "PreloadConfig");
                foreach (var error in errors)
                {
                    _logger.LogError($"   • {error}", "PreloadConfig");
                }
                return;
            }

            _logger.LogInfo($"📝 Реєстрація preload конфігурації для сцени: {config.sceneName}", "PreloadConfig");

            int registeredCount = 0;
            foreach (var resource in config.resources)
            {
                // Перевіряємо умови
                if (resource.onlyInEditor && !Application.isEditor)
                    continue;
                if (resource.onlyInBuild && Application.isEditor)
                    continue;

                try
                {
                    // ✅ ДОДАТИ ПЕРЕВІРКУ ІСНУВАННЯ РЕСУРСУ
                    var systemType = config.GetSystemType(resource.resourceType);
                    var testResource = UnityEngine.Resources.Load(resource.resourceKey, systemType);

                    if (testResource == null)
                    {
                        _logger.LogWarning($"⚠️ Ресурс не знайдено: {resource.resourceKey}, пропускаємо", "PreloadConfig");
                        continue;
                    }

                    // Реєструємо ресурс
                    _preloadManager.RegisterScenePreload(
                        config.sceneName,
                        resource.resourceKey,
                        systemType,
                        resource.priority,
                        resource.createPool,
                        resource.poolSize
                    );

                    registeredCount++;
                }
                catch (System.Exception ex)
                {
                    _logger.LogError($"❌ Помилка реєстрації ресурсу {resource.resourceKey}: {ex.Message}", "PreloadConfig", ex);
                }
            }

            _loadedConfigs.Add(config);
            _logger.LogInfo($"✅ Зареєстровано {registeredCount}/{config.resources.Count} ресурсів для сцени {config.sceneName}", "PreloadConfig");

            await UniTask.Yield(); // Даємо можливість Unity обробити інші задачі
        }

        /// <summary>
        /// Отримує статистику завантажених конфігурацій
        /// </summary>
        public PreloadConfigStatistics GetStatistics()
        {
            return new PreloadConfigStatistics
            {
                TotalConfigs = _loadedConfigs.Count,
                TotalResources = _loadedConfigs.Sum(c => c.resources.Count),
                ScenesWithConfigs = _loadedConfigs.Select(c => c.sceneName).ToArray(),
                ConfigNames = _loadedConfigs.Select(c => c.name).ToArray()
            };
        }

        public struct PreloadConfigStatistics
        {
            public int TotalConfigs;
            public int TotalResources;
            public string[] ScenesWithConfigs;
            public string[] ConfigNames;
        }
    }
}
