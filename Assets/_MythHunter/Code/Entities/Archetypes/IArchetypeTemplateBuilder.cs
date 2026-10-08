// Файл: Assets/_MythHunter/Code/Entities/Archetypes/IArchetypeTemplateBuilder.cs
using System;
using MythHunter.Core.ECS;

namespace MythHunter.Entities.Archetypes
{
    /// <summary>
    /// Інтерфейс будівельника для шаблону архетипу
    /// </summary>
    public interface IArchetypeTemplateBuilder
    {
        /// <summary>
        /// Додає компонент до шаблону архетипу
        /// </summary>
        IArchetypeTemplateBuilder WithComponent<T>(T component) where T : struct, IComponent;

        /// <summary>
        /// Додає функцію перевірки компонента
        /// </summary>
        IArchetypeTemplateBuilder WithComponentCheck<T>(Func<T, bool> predicate) where T : struct, IComponent;

        /// <summary>
        /// Завершує створення шаблону архетипу
        /// </summary>
        IArchetypeTemplateRegistry Build();
    }
}
