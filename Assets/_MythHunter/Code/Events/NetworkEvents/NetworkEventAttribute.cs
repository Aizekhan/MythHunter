// Шлях: Assets/_MythHunter/Code/Events/NetworkEvents/NetworkEventAttribute.cs
using System;

namespace MythHunter.Events.Network
{
    /// <summary>
    /// Атрибут для позначення подій, що потребують мережевої синхронізації
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct)]
    public class NetworkEventAttribute : Attribute
    {
        /// <summary>
        /// Тип авторитету для події
        /// </summary>
        public NetworkEventAuthority Authority
        {
            get;
        }

        /// <summary>
        /// Пріоритет мережевої передачі
        /// </summary>
        public NetworkEventPriority Priority
        {
            get;
        }

        public NetworkEventAttribute(NetworkEventAuthority authority = NetworkEventAuthority.Server,
                                    NetworkEventPriority priority = NetworkEventPriority.Normal)
        {
            Authority = authority;
            Priority = priority;
        }
    }

    /// <summary>
    /// Визначає, хто має право генерувати подію
    /// </summary>
    public enum NetworkEventAuthority
    {
        /// <summary>
        /// Лише сервер може генерувати цю подію
        /// </summary>
        Server,

        /// <summary>
        /// Лише клієнт може генерувати цю подію
        /// </summary>
        Client,

        /// <summary>
        /// І сервер, і клієнт можуть генерувати цю подію
        /// </summary>
        Both
    }

    /// <summary>
    /// Пріоритет передачі мережевої події
    /// </summary>
    public enum NetworkEventPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Critical = 3
    }
}
