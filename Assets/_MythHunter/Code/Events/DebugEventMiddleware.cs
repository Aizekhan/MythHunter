// Файл: Assets/_MythHunter/Code/Events/DebugEventMiddleware.cs
using System;
using System.Collections.Generic;

namespace MythHunter.Events
{
    /// <summary>
    /// Проміжний обробник для дебагу подій
    /// </summary>
    public static class DebugEventMiddleware
    {
        private static readonly List<Action<IEvent, Type>> _globalHandlers = new List<Action<IEvent, Type>>();

        public static void Subscribe(Action<IEvent, Type> handler)
        {
            _globalHandlers.Add(handler);
        }

        public static void Unsubscribe(Action<IEvent, Type> handler)
        {
            _globalHandlers.Remove(handler);
        }

        public static void NotifyHandlers<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            foreach (var handler in _globalHandlers)
            {
                handler(eventData, typeof(TEvent));
            }
        }
    }
}
