using System;
using Cysharp.Threading.Tasks;
using MythHunter.Core.DI;
using MythHunter.Resources.Core;
using MythHunter.Utils.Logging;
using UnityEngine;

public interface IPrefabProvider
{
    string GetPrefabPathByArchetypeId(string archetypeId);
    UniTask<GameObject> LoadPrefabByArchetypeAsync(string archetypeId);
    bool ValidateArchetypePrefab(string archetypeId);
}
