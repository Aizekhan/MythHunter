// Шлях: Assets/_MythHunter/Code/Systems/Core/SystemPriorities.cs

using System;

namespace MythHunter.Systems.Core
{
    /// <summary>
    /// Статичні константи пріоритетів для систем
    /// </summary>
    public static class SystemPriorities
    {
        // Ключові системи (запускаються першими)
        public const int Core = 1000;
        // Фазова система
        public const int Phase = 900;
        // Системи вводу
        public const int Input = 800;
        // Системи планування
        public const int Planning = 700;
        // Системи руху
        public const int Movement = 600;
        // Системи бою
        public const int Combat = 500;
        // Системи штучного інтелекту
        public const int AI = 400;
        // Системи фізики
        public const int Physics = 300;
        // Ігрові системи
        public const int Gameplay = 200;
        // Системи UI
        public const int UI = 100;
        // Системи аналітики та діагностики (останні)
        public const int Analytics = 0;

        /// <summary>
        /// Повертає назву пріоритету за його значенням
        /// </summary>
        public static string GetPriorityName(int priority)
        {
            if (priority >= Core)
                return "Core";
            if (priority >= Phase)
                return "Phase";
            if (priority >= Input)
                return "Input";
            if (priority >= Planning)
                return "Planning";
            if (priority >= Movement)
                return "Movement";
            if (priority >= Combat)
                return "Combat";
            if (priority >= AI)
                return "AI";
            if (priority >= Physics)
                return "Physics";
            if (priority >= Gameplay)
                return "Gameplay";
            if (priority >= UI)
                return "UI";
            return "Analytics";
        }
    }
}
