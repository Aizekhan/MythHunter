using System;

namespace MythHunter.Utils.Validation
{
    /// <summary>
    /// Утиліта для перевірки умов
    /// </summary>
    public static class Ensure
    {
        public static void NotNull<T>(T value, string paramName) where T : class
        {
            if (value == null)
                throw new ArgumentNullException(paramName, $"{paramName} cannot be null");
        }
        
        public static void NotNullOrEmpty(string value, string paramName)
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException($"{paramName} cannot be null or empty", paramName);
        }
        
        public static void IsTrue(bool condition, string message)
        {
            if (!condition)
                throw new ArgumentException(message);
        }
        
        public static void IsInRange(int value, int min, int max, string paramName)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} must be between {min} and {max}");
        }
        
        public static void IsInRange(float value, float min, float max, string paramName)
        {
            if (value < min || value > max)
                throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} must be between {min} and {max}");
        }
        
        public static void IsPositive(int value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} must be positive");
        }
        
        public static void IsPositive(float value, string paramName)
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} must be positive");
        }
        
        public static void IsNotNegative(int value, string paramName)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} cannot be negative");
        }
        
        public static void IsNotNegative(float value, string paramName)
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(paramName, value, $"{paramName} cannot be negative");
        }
    }
}