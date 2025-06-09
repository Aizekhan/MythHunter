// Файл: Assets/_MythHunter/Code/Entities/Archetypes/ArchetypeTemplateBuilder.cs
using System;
using MythHunter.Core.ECS;

namespace MythHunter.Entities.Archetypes
{
    /// <summary>
    /// Будівельник для шаблону архетипу
    /// </summary>
    public class ArchetypeTemplateBuilder : IArchetypeTemplateBuilder
    {
        private readonly ArchetypeTemplateRegistry.ArchetypeTemplate _template;
        private readonly ArchetypeTemplateRegistry _registry;

        public ArchetypeTemplateBuilder(ArchetypeTemplateRegistry.ArchetypeTemplate template, ArchetypeTemplateRegistry registry)
        {
            _template = template;
            _registry = registry;
        }

        /// <summary>
        /// Додає компонент до шаблону архетипу
        /// </summary>
        public IArchetypeTemplateBuilder WithComponent<T>(T component) where T : struct, IComponent
        {
            _template.DefaultComponents[typeof(T)] = component;
            _registry.AddComponentToTemplate(_template.ArchetypeId, component);
            return this;
        }

        /// <summary>
        /// Додає функцію перевірки компонента
        /// </summary>
        public IArchetypeTemplateBuilder WithComponentCheck<T>(Func<T, bool> predicate) where T : struct, IComponent
        {
            _registry.AddComponentChecker(_template.ArchetypeId, predicate);
            return this;
        }

        /// <summary>
        /// Завершує створення шаблону архетипу
        /// </summary>
        public IArchetypeTemplateRegistry Build()
        {
            return _registry;
        }
    }
}
