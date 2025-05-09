// Шлях: Assets/_MythHunter/Code/Core/DI/DIContainerExtensions.cs
using System;
using UnityEngine;
using MythHunter.Utils.Logging;

namespace MythHunter.Core.DI
{
    /// <summary>
    /// Розширення для DI контейнера
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

            // Використовуємо публічний метод InjectDependencies
            container.InjectDependencies(component);

            // Логування, якщо потрібно
            logger?.LogDebug($"Injected dependencies into {component.GetType().Name}", "DI");
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

        public static void BindSingleton<TService>(this IDIContainer container, TService instance)
        {
            container.RegisterInstance<TService>(instance);
        }

        public static void BindSingleton<TService, TImplementation>(this IDIContainer container)
           where TImplementation : TService, new()
        {
            container.RegisterSingleton<TService, TImplementation>();
        }

        /// <summary>
        /// Знаходить або створює MonoBehaviour компонент та реєструє його як сінглтон
        /// </summary>
        public static T BindMonoBehaviour<T, TImpl>(this IDIContainer container, string gameObjectName = null)
            where TImpl : MonoBehaviour, T
            where T : class
        {
            // Шукаємо існуючий компонент у сцені
            var component = UnityEngine.Object.FindFirstObjectByType<TImpl>();

            if (component == null)
            {
                // Створюємо новий GameObject з компонентом, якщо не знайдено
                var gameObject = new GameObject(gameObjectName ?? typeof(TImpl).Name);
                component = gameObject.AddComponent<TImpl>();
                UnityEngine.Object.DontDestroyOnLoad(gameObject);
            }

            // Реєструємо компонент у контейнері
            container.RegisterInstance<T>(component);
            return component;
        }
    }
}
