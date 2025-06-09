using System;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Атрибут для позначення полів і конструкторів для ін'єкції
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Constructor | AttributeTargets.Method)]
    public class InjectAttribute : Attribute
    {
    }
}