// Assets/_MythHunter/Code/Resources/Config/PreloadSceneConfig.cs
using Mono.Cecil;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace MythHunter.Resources.Config
{
    /// <summary>
    /// ScriptableObject конфігурація preload для сцени
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
            [Header("Ресурс")]
            public string resourceKey;
            public ResourceType resourceType;

            [Header("Завантаження")]
            public LoadingMode loadingMode; // ✅ НОВИЙ ЕНУМ

            [Header("Пріоритет")]
            [Range(0, 100)]
            public int priority;

            [Header("Пул об'єктів")]
            public bool createPool;
            [Range(1, 100)]
            public int poolSize;

            [Header("Умови")]
            public bool onlyInEditor;
            public bool onlyInBuild;

            [Header("Опис")]
            [TextArea(2, 3)]
            public string description;
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
        /// Валідує конфігурацію на наявність помилок
        /// </summary>
        public List<string> ValidateConfig()
        {
            var errors = new List<string>();

            if (string.IsNullOrEmpty(sceneName))
                errors.Add("Не вказано назву сцени");

            for (int i = 0; i < resources.Count; i++)
            {
                var resource = resources[i];

                if (string.IsNullOrEmpty(resource.resourceKey))
                    errors.Add($"Ресурс #{i}: не вказано ключ ресурсу");

                if (resource.createPool && resource.poolSize <= 0)
                    errors.Add($"Ресурс #{i} ({resource.resourceKey}): розмір пулу повинен бути > 0");

                // Перевірка на дублікати
                for (int j = i + 1; j < resources.Count; j++)
                {
                    if (resources[j].resourceKey == resource.resourceKey)
                        errors.Add($"Дублікат ресурсу: {resource.resourceKey}");
                }
            }

            return errors;
        }
    }
}
