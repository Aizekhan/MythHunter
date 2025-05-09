using System.Collections.Generic;

namespace MythHunter.Entities.Archetypes
{
    public interface IArchetypeRegistry
    {
        void RegisterEntityArchetype(int entityId, string archetypeId);
        void UnregisterEntityArchetype(int entityId);
        string GetEntityArchetype(int entityId);
        void Clear();
        bool IsEntityOfArchetype(int entityId, string archetypeId);
        List<int> GetEntitiesByArchetype(string archetypeId);

    }
}
