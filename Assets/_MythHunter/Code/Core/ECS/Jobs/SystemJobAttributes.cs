// Шлях: Assets/_MythHunter/Code/Core/ECS/Jobs/SystemJobAttributes.cs
using System;

namespace MythHunter.Core.ECS.Jobs
{
    /// <summary>
    /// Вказує, що система може виконуватись паралельно з іншими
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ParallelizableSystemAttribute : Attribute
    {
        // Ідентифікатор групи систем, які можуть виконуватись паралельно
        // Системи з однаковою групою не виконуються паралельно між собою
        public string GroupId
        {
            get;
        }

        // Пріоритет виконання (вищий пріоритет = раніше виконання)
        public int Priority
        {
            get;
        }

        public ParallelizableSystemAttribute(string groupId = "", int priority = 0)
        {
            GroupId = groupId;
            Priority = priority;
        }
    }

    /// <summary>
    /// Вказує, що система має виконуватись послідовно та блокує паралельне виконання
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SequentialSystemAttribute : Attribute
    {
        // Пріоритет виконання (вищий пріоритет = раніше виконання)
        public int Priority
        {
            get;
        }

        public SequentialSystemAttribute(int priority = 0)
        {
            Priority = priority;
        }
    }

    /// <summary>
    /// Вказує, що система має залежності від інших систем
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SystemDependencyAttribute : Attribute
    {
        // Тип системи, від якої є залежність
        public Type DependsOnSystemType
        {
            get;
        }

        public SystemDependencyAttribute(Type dependsOnSystemType)
        {
            DependsOnSystemType = dependsOnSystemType;
        }
    }
}
