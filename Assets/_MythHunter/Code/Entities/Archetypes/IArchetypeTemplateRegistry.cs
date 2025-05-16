using System.Collections.Generic;
using System;
using static MythHunter.Entities.Archetypes.ArchetypeTemplateRegistry;
namespace MythHunter.Entities.Archetypes
{
    public interface IArchetypeTemplateRegistry
    {
        IEnumerable<string> GetAllTemplateIds();
        bool MatchesTemplate(int entityId, string templateId);
        int CreateEntityFromTemplate(string templateId, Dictionary<Type, object> overrides = null);
      
    }
}
