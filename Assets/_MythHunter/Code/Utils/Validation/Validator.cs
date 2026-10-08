using System;
using System.Collections.Generic;

namespace MythHunter.Utils.Validation
{
    /// <summary>
    /// Клас для валідації об'єктів
    /// </summary>
    public class Validator<T>
    {
        private readonly List<Func<T, ValidationResult>> _validationRules = new List<Func<T, ValidationResult>>();
        
        /// <summary>
        /// Додає правило валідації
        /// </summary>
        public Validator<T> AddRule(Func<T, ValidationResult> rule)
        {
            _validationRules.Add(rule);
            return this;
        }
        
        /// <summary>
        /// Додає умову, яка має виконуватись
        /// </summary>
        public Validator<T> AddCondition(Func<T, bool> condition, string errorMessage)
        {
            _validationRules.Add(obj => condition(obj) 
                ? ValidationResult.Success() 
                : ValidationResult.Error(errorMessage));
                
            return this;
        }
        
        /// <summary>
        /// Перевіряє об'єкт на відповідність усім правилам
        /// </summary>
        public ValidationResult Validate(T obj)
        {
            List<string> errors = new List<string>();
            
            foreach (var rule in _validationRules)
            {
                var result = rule(obj);
                if (!result.IsValid)
                {
                    errors.AddRange(result.Errors);
                    
                    // Якщо помилка критична - відразу повертаємо результат
                    if (result.IsCritical)
                    {
                        return ValidationResult.Critical(errors);
                    }
                }
            }
            
            return errors.Count > 0 ? ValidationResult.Error(errors) : ValidationResult.Success();
        }
    }
    
    /// <summary>
    /// Результат валідації
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public bool IsCritical { get; private set; }
        public List<string> Errors { get; } = new List<string>();
        
        public static ValidationResult Success()
        {
            return new ValidationResult();
        }
        
        public static ValidationResult Error(string error)
        {
            var result = new ValidationResult();
            result.Errors.Add(error);
            return result;
        }
        
        public static ValidationResult Error(List<string> errors)
        {
            var result = new ValidationResult();
            result.Errors.AddRange(errors);
            return result;
        }
        
        public static ValidationResult Critical(string error)
        {
            var result = new ValidationResult { IsCritical = true };
            result.Errors.Add(error);
            return result;
        }
        
        public static ValidationResult Critical(List<string> errors)
        {
            var result = new ValidationResult { IsCritical = true };
            result.Errors.AddRange(errors);
            return result;
        }
    }
}