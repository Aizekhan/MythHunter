using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Resources.Core;
using MythHunter.Utils.Logging;
using System;
using UnityEngine;

public class PrefabProvider : IPrefabProvider
{
    private const string HEROES_PREFAB_ROOT = "Prefabs/Heroes/";
    private readonly IResourceManager _resourceManager;
    private readonly IMythLogger _logger;

    [Inject]
    public PrefabProvider(IResourceManager resourceManager, IMythLogger logger)
    {
        _resourceManager = resourceManager;
        _logger = logger;
    }

    public string GetPrefabPathByArchetypeId(string archetypeId)
    {
        if (string.IsNullOrEmpty(archetypeId))
        {
            _logger.LogWarning("ArchetypeId is null or empty", "PrefabProvider");
            return string.Empty;
        }

        return $"{HEROES_PREFAB_ROOT}{archetypeId}";
    }

    public async UniTask<GameObject> LoadPrefabByArchetypeAsync(string archetypeId)
    {
        string prefabPath = GetPrefabPathByArchetypeId(archetypeId);

        try
        {
            var prefab = await _resourceManager.LoadAsync<GameObject>(prefabPath);
            if (prefab == null)
            {
                _logger.LogWarning($"Prefab not found for archetype: {archetypeId} at path: {prefabPath}", "PrefabProvider");
            }
            return prefab;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to load prefab for archetype {archetypeId}: {ex.Message}", "PrefabProvider", ex);
            return null;
        }
    }

    public bool ValidateArchetypePrefab(string archetypeId)
    {
        string prefabPath = GetPrefabPathByArchetypeId(archetypeId);

        // В Editor можна перевірити існування файлу
#if UNITY_EDITOR
        string fullPath = $"Assets/Resources/{prefabPath}.prefab";
        return UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(fullPath) != null;
#else
        // В рантаймі просто повертаємо true, перевірка буде при завантаженні
        return true;
#endif
    }
}
