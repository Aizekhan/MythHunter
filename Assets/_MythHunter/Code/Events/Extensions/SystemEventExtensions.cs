// Файл: Assets/_MythHunter/Code/Events/Extensions/SystemEventExtensions.cs
using System;
using Cysharp.Threading.Tasks;
using MythHunter.Core.ECS;
using MythHunter.Utils.Logging;

namespace MythHunter.Events.Extensions
{
    public static class SystemEventExtensions
    {
        public static void Subscribe<TEvent>(this ISystem system, IEventBus eventBus,
            Action<TEvent> handler, EventPriority priority = EventPriority.Normal)
            where TEvent : struct, IEvent
        {
            eventBus.Subscribe(handler, priority);
        }

        public static void SubscribeAsync<TEvent>(this ISystem system, IEventBus eventBus,
            Func<TEvent, UniTask> handler, EventPriority priority = EventPriority.Normal)
            where TEvent : struct, IEvent
        {
            eventBus.SubscribeAsync(handler, priority);
        }

        public static void Unsubscribe<TEvent>(this ISystem system, IEventBus eventBus,
            Action<TEvent> handler)
            where TEvent : struct, IEvent
        {
            eventBus.Unsubscribe(handler);
        }

        public static void UnsubscribeAsync<TEvent>(this ISystem system, IEventBus eventBus,
            Func<TEvent, UniTask> handler)
            where TEvent : struct, IEvent
        {
            eventBus.UnsubscribeAsync(handler);
        }
    }
}
