// Assets/_MythHunter/Editor/Tools/ListRequiredPrefabs.cs

using UnityEditor;
using UnityEngine;
using System.Linq;
using MythHunter.Entities.Archetypes; // або твій namespace для HeroArchetypeSO

public static class ListRequiredPrefabs
{
    [MenuItem("MythHunter/Debug/List Required Prefabs")]
    public static void ListRequiredPrefabsMethod()
    {
        var heroArchetypes = Resources.LoadAll<HeroArchetypeSO>("ScriptableObjects/Heroes");

        Debug.Log("=== ПОТРІБНІ ПРЕФАБИ ===");
        foreach (var archetype in heroArchetypes)
        {
            if (!string.IsNullOrEmpty(archetype.ArchetypeId))
            {
                string prefabPath = $"Assets/Resources/Prefabs/Heroes/{archetype.ArchetypeId}.prefab";
                bool exists = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null;

                Debug.Log($"{(exists ? "✅" : "❌")} {archetype.ArchetypeId} -> {prefabPath}");
            }
        }
    }
}
