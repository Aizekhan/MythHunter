// Шлях: Assets/_MythHunter/Code/Systems/Core/SystemCategoryAttribute.cs
using System;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Атрибут для визначення категорії ініціалізації системи
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class SystemCategoryAttribute : Attribute
    {
        public SystemInitializationCategory Category
        {
            get;
        }

        public SystemCategoryAttribute(SystemInitializationCategory category)
        {
            Category = category;
        }
    }
}
