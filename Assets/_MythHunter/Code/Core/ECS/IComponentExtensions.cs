using System;

namespace MythHunter.Core.ECS
{
    /// <summary>
    /// Розширення для роботи з IComponent
    /// </summary>
    public static class IComponentExtensions
    {
        /// <summary>
        /// Перевіряє, чи компонент належить до вказаного типу
        /// </summary>
        public static bool IsOfType<T>(this IComponent component) where T : struct, IComponent
        {
            return component is T;
        }

        /// <summary>
        /// Приводить компонент до вказаного типу, якщо можливо
        /// </summary>
        public static T As<T>(this IComponent component) where T : struct, IComponent
        {
            if (component is T typedComponent)
                return typedComponent;

            throw new InvalidCastException($"Cannot cast component of type {component.GetType().Name} to {typeof(T).Name}");
        }
    }
}
