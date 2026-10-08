// Шлях: Assets/_MythHunter/Code/Core/Validation/DependencyValidator.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MythHunter.Core.DI;
using MythHunter.Utils.Logging;
using UnityEngine;

namespace MythHunter.Core.Validation
{
    /// <summary>
    /// Валідатор для перевірки залежностей у проекті
    /// </summary>
    public class DependencyValidator
    {
        private readonly IDIContainer _container;
        private readonly IMythLogger _logger;
        private readonly List<ValidationIssue> _issues = new List<ValidationIssue>();

        /// <summary>
        /// Проблема валідації залежностей
        /// </summary>
        public class ValidationIssue
        {
            public Type ServiceType
            {
                get; set;
            }
            public Type DependencyType
            {
                get; set;
            }
            public string Message
            {
                get; set;
            }
            public ValidationSeverity Severity
            {
                get; set;
            }

            public override string ToString()
            {
                return $"[{Severity}] {ServiceType?.Name} -> {DependencyType?.Name}: {Message}";
            }
        }

        /// <summary>
        /// Рівень важливості проблеми
        /// </summary>
        public enum ValidationSeverity
        {
            Info,
            Warning,
            Error,
            Critical
        }

        public DependencyValidator(IDIContainer container, IMythLogger logger)
        {
            _container = container;
            _logger = logger;
        }

        /// <summary>
        /// Перевіряє всі зареєстровані залежності
        /// </summary>
        public List<ValidationIssue> ValidateAllDependencies()
        {
            _issues.Clear();

            _logger.LogInfo("Starting dependency validation...", "Validation");

            // Знаходимо всі типи, що реалізують інтерфейси з нашої сборки
            var allTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name.Contains("MythHunter"))
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && t.GetInterfaces().Length > 0)
                .ToList();

            // Перевіряємо наявність атрибутів [Inject]
            foreach (var type in allTypes)
            {
                CheckInjectAttributes(type);
            }

            // Перевіряємо всі типи, що мають атрибут [Inject]
            var injectTypes = allTypes.Where(t =>
                t.GetConstructors().Any(c => c.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0) ||
                t.GetFields().Any(f => f.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0) ||
                t.GetProperties().Any(p => p.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0) ||
                t.GetMethods().Any(m => m.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0)
            ).ToList();

            // Перевіряємо кожен тип з ін'єкцією
            foreach (var type in injectTypes)
            {
                ValidateTypeInjections(type);
            }

            // Перевіряємо MonoBehaviour з атрибутами [Inject]
            ValidateMonoBehaviours();

            // Виводимо результати
            _logger.LogInfo($"Dependency validation completed with {_issues.Count} issues", "Validation");
            foreach (var issue in _issues.OrderByDescending(i => i.Severity))
            {
                switch (issue.Severity)
                {
                    case ValidationSeverity.Critical:
                    case ValidationSeverity.Error:
                        _logger.LogError(issue.ToString(), "Validation");
                        break;
                    case ValidationSeverity.Warning:
                        _logger.LogWarning(issue.ToString(), "Validation");
                        break;
                    case ValidationSeverity.Info:
                        _logger.LogInfo(issue.ToString(), "Validation");
                        break;
                }
            }

            return _issues;
        }

        /// <summary>
        /// Перевіряє MonoBehaviour на наявність атрибутів [Inject]
        /// </summary>
        private void ValidateMonoBehaviours()
        {
            // Знаходимо всі типи, що успадковуються від MonoBehaviour
            var monoBehaviourTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name.Contains("MythHunter"))
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && typeof(MonoBehaviour).IsAssignableFrom(t))
                .ToList();

            foreach (var type in monoBehaviourTypes)
            {
                // Перевіряємо наявність атрибутів [Inject]
                bool hasInject = type.GetFields().Any(f => f.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0) ||
                    type.GetProperties().Any(p => p.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0) ||
                    type.GetMethods().Any(m => m.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0);

                if (hasInject)
                {
                    // Перевіряємо наявність компонента DependencyInjector в проекті
                    bool hasInjector = false;
                    try
                    {
                        var injectorType = AppDomain.CurrentDomain.GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .FirstOrDefault(t => t.Name == "DependencyInjector" && typeof(MonoBehaviour).IsAssignableFrom(t));

                        hasInjector = injectorType != null;
                    }
                    catch { /* Ігноруємо помилки */ }

                    if (!hasInjector)
                    {
                        AddIssue(type, null, "MonoBehaviour has [Inject] attributes but DependencyInjector is not present in the scene", ValidationSeverity.Warning);
                    }

                    // Валідуємо ін'єкції
                    ValidateTypeInjections(type);
                }
            }
        }

        /// <summary>
        /// Перевіряє атрибути [Inject] для типу
        /// </summary>
        private void CheckInjectAttributes(Type type)
        {
            // Перевіряємо конструктори
            var constructors = type.GetConstructors()
                .Where(c => c.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0)
                .ToList();

            if (constructors.Count > 1)
            {
                AddIssue(type, null, $"Type has multiple constructors with [Inject] attribute", ValidationSeverity.Error);
            }

            // Перевіряємо методи
            var methods = type.GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0)
                .ToList();

            foreach (var method in methods)
            {
                if (method.ReturnType != typeof(void))
                {
                    AddIssue(type, null, $"Method {method.Name} has [Inject] attribute but doesn't return void", ValidationSeverity.Error);
                }
            }
        }

        /// <summary>
        /// Валідує ін'єкції для типу
        /// </summary>
        private void ValidateTypeInjections(Type type)
        {
            // Перевіряємо конструктори
            var constructor = type.GetConstructors()
                .FirstOrDefault(c => c.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0);

            if (constructor == null)
            {
                constructor = type.GetConstructors().FirstOrDefault();
            }

            if (constructor != null)
            {
                foreach (var param in constructor.GetParameters())
                {
                    if (!IsRegistered(param.ParameterType))
                    {
                        AddIssue(type, param.ParameterType, $"Constructor parameter {param.Name} of type {param.ParameterType.Name} is not registered", ValidationSeverity.Error);
                    }
                }
            }

            // Перевіряємо поля
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(f => f.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0))
            {
                if (!IsRegistered(field.FieldType))
                {
                    AddIssue(type, field.FieldType, $"Field {field.Name} of type {field.FieldType.Name} is not registered", ValidationSeverity.Error);
                }
            }

            // Перевіряємо властивості
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(p => p.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0))
            {
                if (!IsRegistered(property.PropertyType))
                {
                    AddIssue(type, property.PropertyType, $"Property {property.Name} of type {property.PropertyType.Name} is not registered", ValidationSeverity.Error);
                }
            }

            // Перевіряємо методи
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.GetCustomAttributes(typeof(InjectAttribute), true).Length > 0))
            {
                foreach (var param in method.GetParameters())
                {
                    if (!IsRegistered(param.ParameterType))
                    {
                        AddIssue(type, param.ParameterType, $"Method {method.Name} parameter {param.Name} of type {param.ParameterType.Name} is not registered", ValidationSeverity.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Перевіряє, чи зареєстрований тип в контейнері
        /// </summary>
        private bool IsRegistered(Type type)
        {
            try
            {
                // Використовуємо рефлексію для виклику методу IsRegistered
                var methodInfo = typeof(IDIContainer).GetMethod("IsRegistered").MakeGenericMethod(type);
                return (bool)methodInfo.Invoke(_container, null);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Додає проблему до списку
        /// </summary>
        private void AddIssue(Type serviceType, Type dependencyType, string message, ValidationSeverity severity)
        {
            _issues.Add(new ValidationIssue
            {
                ServiceType = serviceType,
                DependencyType = dependencyType,
                Message = message,
                Severity = severity
            });
        }
    }
}
