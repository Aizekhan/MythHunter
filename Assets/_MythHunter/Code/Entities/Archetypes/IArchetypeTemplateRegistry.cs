// Файл: Assets/_MythHunter/Code/Entities/Archetypes/IArchetypeTemplateRegistry.cs
using System;
using System.Collections.Generic;
using MythHunter.Core.ECS; // Додайте цей рядок

namespace MythHunter.Entities.Archetypes
{
    public interface IArchetypeTemplateRegistry
    {
        IEnumerable<string> GetAllTemplateIds();
        bool MatchesTemplate(int entityId, string templateId);
        int CreateEntityFromTemplate(string templateId, Dictionary<Type, object> overrides = null);

        // Додайте обмеження where T : struct, IComponent
        ArchetypeTemplateBuilder RegisterArchetypeTemplate(string archetypeId);
        void AddComponentToTemplate<T>(string archetypeId, T component) where T : struct, IComponent;
        void AddComponentChecker<T>(string archetypeId, Func<T, bool> predicate) where T : struct, IComponent;
    }
}
