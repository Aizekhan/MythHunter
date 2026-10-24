using System;
using Cysharp.Threading.Tasks;
using RPGFramework.DI;
using MythHunter.Resources.Core;
using MythHunter.Utils.Logging;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif
public interface IPrefabProvider
{
    string GetPrefabPathByArchetypeId(string archetypeId);
    UniTask<GameObject> LoadPrefabByArchetypeAsync(string archetypeId);
    bool ValidateArchetypePrefab(string archetypeId);
}
