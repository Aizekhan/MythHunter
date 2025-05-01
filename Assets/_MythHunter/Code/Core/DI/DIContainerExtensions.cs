// Шлях: Assets/_MythHunter/Code/Core/DI/DIContainerExtensions.cs
using System;
using UnityEngine;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Розширення для DI контейнера для підтримки MonoBehaviour
    /// </summary>
    public static class DIContainerExtensions
    {
        /// <summary>
        /// Виконує ін'єкцію залежностей у MonoBehaviour компонент
        /// </summary>
        public static void InjectInto(this IDIContainer container, MonoBehaviour component, IMythLogger logger = null)
        {
            if (component == null)
                return;

            if (container is DIContainer diContainer)
            {
                // Використовуємо внутрішній метод з логуванням
                diContainer.InjectDependenciesInternal(component, logger != null);
            }
            else
            {
                // Запасний варіант для інших реалізацій
                container.InjectDependencies(component);
            }
        }

        /// <summary>
        /// Резолвить залежність вказаного типу
        /// </summary>
        public static object Resolve(this IDIContainer container, Type type)
        {
            // Використовуємо рефлексію для виклику методу Resolve<T>
            var method = typeof(IDIContainer).GetMethod("Resolve").MakeGenericMethod(type);
            return method.Invoke(container, null);
        }
    }
}
