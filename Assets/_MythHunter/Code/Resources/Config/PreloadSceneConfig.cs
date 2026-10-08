// Assets/_MythHunter/Code/Resources/Config/PreloadSceneConfig.cs
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MythHunter.UI.Core; // ✅ ДОДАНО для ViewId

namespace MythHunter.Resources.Config
{
    /// <summary>
    /// ScriptableObject конфігурація preload для сцени з інтеграцією ViewConfig системи
    /// </summary>
    [CreateAssetMenu(menuName = "MythHunter/Preload/Scene Config", fileName = "PreloadSceneConfig")]
    public class PreloadSceneConfig : ScriptableObject
    {
        [Header("Налаштування сцени")]
        public string sceneName;

        [Header("Ресурси для завантаження")]
        public List<PreloadResourceEntry> resources = new List<PreloadResourceEntry>();

        [Header("Налаштування")]
        public bool autoRegisterOnAwake = true;
        public bool enableDebugLogging = false;

        [System.Serializable]
        public struct PreloadResourceEntry
        {
            [Header("🎯 UI Integration (Пріоритет)")]
            [Tooltip("ViewId з ViewConfig - має найвищий пріоритет для визначення ключа пулу")]
            public ViewId relatedViewId;     // ✅ НОВЕ ПОЛЕ

            [Header("📁 Ресурс (Fallback)")]
            [Tooltip("Використовується тільки якщо relatedViewId = None")]
            public string resourceKey;       // Стає fallback опцією
            public ResourceType resourceType;

            [Header("⚙️ Завантаження")]
            public LoadingMode loadingMode;

            [Header("🎲 Пріоритет")]
            [Range(0, 100)]
            public int priority;

            [Header("🏊 Пул об'єктів")]
            public bool createPool;
            [Range(1, 100)]
            public int poolSize;

            [Header("🎛️ Умови")]
            public bool onlyInEditor;
            public bool onlyInBuild;

            [Header("📝 Опис")]
            [TextArea(2, 3)]
            public string description;

            /// <summary>
            /// ✅ АРХІТЕКТУРНО ПРАВИЛЬНА валідація ресурсу
            /// </summary>
            public bool IsValid()
            {
                // Має бути або ViewId, або resourceKey
                return relatedViewId != ViewId.None || !string.IsNullOrEmpty(resourceKey);
            }

            /// <summary>
            /// Отримує зрозумілий опис ресурсу для логування
            /// </summary>
            public string GetDisplayName()
            {
                if (relatedViewId != ViewId.None)
                    return $"ViewId:{relatedViewId}";

                if (!string.IsNullOrEmpty(resourceKey))
                    return $"Resource:{resourceKey}";

                return "INVALID";
            }
        }

        public enum ResourceType
        {
            GameObject,
            Sprite,
            AudioClip,
            Material,
            Texture2D,
            ScriptableObject,
            HeroArchetypeSO,
            Custom
        }

        public enum LoadingMode
        {
            SingleResource,     // Один конкретний ресурс
            AllFromFolder,      // Всі ресурси з папки
            AllOfTypeFromFolder // Всі ресурси певного типу з папки
        }

        /// <summary>
        /// Конвертує ResourceType в System.Type
        /// </summary>
        public System.Type GetSystemType(ResourceType resourceType)
        {
            return resourceType switch
            {
                ResourceType.GameObject => typeof(GameObject),
                ResourceType.Sprite => typeof(Sprite),
                ResourceType.AudioClip => typeof(AudioClip),
                ResourceType.Material => typeof(Material),
                ResourceType.Texture2D => typeof(Texture2D),
                ResourceType.ScriptableObject => typeof(ScriptableObject),
                ResourceType.HeroArchetypeSO => typeof(MythHunter.Entities.Archetypes.HeroArchetypeSO),
                _ => typeof(UnityEngine.Object)
            };
        }

        /// <summary>
        /// ✅ РОЗШИРЕНА валідація конфігурації з підтримкою ViewId
        /// </summary>
        public List<string> ValidateConfig()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(sceneName))
                errors.Add("❌ Не вказано назву сцени");

            for (int i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];

                // ✅ НОВА ВАЛІДАЦІЯ для ViewId + resourceKey
                if (!resource.IsValid())
                    errors.Add($"❌ Ресурс #{i}: не вказано ні ViewId, ні resourceKey");

                // Валідація пулу
                if (resource.createPool && resource.poolSize <= 0)
                    errors.Add($"❌ Ресурс #{i} ({resource.GetDisplayName()}): розмір пулу повинен бути > 0");

                // Валідація дублікатів
                for (int j = i + 1; j < resources.Count; j++)
                {
                    var otherResource = resources[j];

                    // Перевірка дублікатів ViewId
                    if (resource.relatedViewId != ViewId.None &&
                        resource.relatedViewId == otherResource.relatedViewId)
                    {
                        errors.Add($"❌ Дублікат ViewId: {resource.relatedViewId} у ресурсах #{i} та #{j}");
                    }

                    // Перевірка дублікатів resourceKey
                    if (!string.IsNullOrEmpty(resource.resourceKey) &&
                        resource.resourceKey == otherResource.resourceKey)
                    {
                        errors.Add($"❌ Дублікат resourceKey: {resource.resourceKey} у ресурсах #{i} та #{j}");
                    }
                }

                // Валідація умов
                if (resource.onlyInEditor && resource.onlyInBuild)
                    errors.Add($"⚠️ Ресурс #{i} ({resource.GetDisplayName()}): onlyInEditor та onlyInBuild не можуть бути true одночасно");
            }

            return errors;
        }

        /// <summary>
        /// ✅ СТАТИСТИКА конфігурації для налагодження
        /// </summary>
        public ConfigStatistics GetStatistics()
        {
            return new ConfigStatistics
            {
                TotalResources = resources.Count,
                ViewIdResources = resources.Count(r => r.relatedViewId != ViewId.None),
                FallbackResources = resources.Count(r => r.relatedViewId == ViewId.None && !string.IsNullOrEmpty(r.resourceKey)),
                PooledResources = resources.Count(r => r.createPool),
                EditorOnlyResources = resources.Count(r => r.onlyInEditor),
                BuildOnlyResources = resources.Count(r => r.onlyInBuild)
            };
        }

        /// <summary>
        /// Отримує ресурси за типом ViewId
        /// </summary>
        public List<PreloadResourceEntry> GetResourcesByViewId(ViewId viewId)
        {
            return resources.Where(r => r.relatedViewId == viewId).ToList();
        }

        /// <summary>
        /// Отримує всі унікальні ViewId у конфігурації
        /// </summary>
        public ViewId[] GetAllViewIds()
        {
            return resources
                .Where(r => r.relatedViewId != ViewId.None)
                .Select(r => r.relatedViewId)
                .Distinct()
                .ToArray();
        }

        public struct ConfigStatistics
        {
            public int TotalResources;
            public int ViewIdResources;
            public int FallbackResources;
            public int PooledResources;
            public int EditorOnlyResources;
            public int BuildOnlyResources;
        }

        // ✅ ВАЛІДАЦІЯ В UNITY EDITOR
#if UNITY_EDITOR
        private void OnValidate()
        {
            var errors = ValidateConfig();
            if (errors.Any())
            {
                UnityEngine.Debug.LogWarning($"⚠️ PreloadSceneConfig '{name}' має помилки валідації:\n" + string.Join("\n", errors));
            }
        }
#endif
    }
}
