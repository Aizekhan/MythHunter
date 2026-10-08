using System.Collections.Generic;
using System;
using MythHunter.Core.ECS;

namespace MythHunter.Entities.Archetypes
{
    public interface IArchetypeSystem : ISystem
    {
        int CreateEntityFromArchetype(string archetypeId, Dictionary<Type, object> overrides = null);
        string GetEntityArchetype(int entityId);
        void RegisterEntityArchetype(int entityId, string archetypeId);
    }
}
