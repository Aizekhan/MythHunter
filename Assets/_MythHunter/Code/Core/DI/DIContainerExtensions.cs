using System;
using System.Reflection;
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

            try
            {
                var componentType = component.GetType();

                // Шукаємо метод з атрибутом [Inject]
                var methods = componentType.GetMethods(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                foreach (var method in methods)
                {
                    var injectAttribute = method.GetCustomAttributes(typeof(InjectAttribute), true);
                    if (injectAttribute.Length > 0)
                    {
                        // Отримуємо параметри методу
                        var parameters = method.GetParameters();
                        var args = new object[parameters.Length];

                        // Резолвимо кожен параметр через DI
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var parameterType = parameters[i].ParameterType;
                            args[i] = container.Resolve(parameterType);
                        }

                        // Викликаємо метод з резолвленими залежностями
                        method.Invoke(component, args);

                        if (logger != null)
                        {
                            logger.LogDebug($"Dependencies injected into {componentType.Name}", "DI");
                        }
                    }
                }

                // Також перевіряємо поля з атрибутом [Inject]
                var fields = componentType.GetFields(
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);

                foreach (var field in fields)
                {
                    var injectAttribute = field.GetCustomAttributes(typeof(InjectAttribute), true);
                    if (injectAttribute.Length > 0)
                    {
                        var fieldType = field.FieldType;
                        var dependency = container.Resolve(fieldType);

                        // Встановлюємо значення поля
                        field.SetValue(component, dependency);

                        if (logger != null)
                        {
                            logger.LogDebug($"Field {field.Name} injected into {componentType.Name}", "DI");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (logger != null)
                {
                    logger.LogError($"Error injecting dependencies into {component.GetType().Name}: {ex.Message}", "DI", ex);
                }
                else
                {
                    logger.LogError($"Error injecting dependencies into {component.GetType().Name}: {ex.Message}");
                }
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
