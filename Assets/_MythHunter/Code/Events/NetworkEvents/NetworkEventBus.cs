// Шлях: Assets/_MythHunter/Code/Events/NetworkEvents/NetworkEventBus.cs
using System;
using MythHunter.Core.DI;
using MythHunter.Networking.Core;
using MythHunter.Utils.Logging;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MythHunter.Networking.Messages;

namespace MythHunter.Events.Network
{
    /// <summary>
    /// Розширена шина подій з підтримкою мережевої синхронізації
    /// </summary>
    public class NetworkEventBus : EventBus, INetworkEventBus
    {
        private readonly INetworkSystem _networkSystem;
        private readonly Dictionary<Type, NetworkEventMetadata> _networkEventTypes = new Dictionary<Type, NetworkEventMetadata>();

        // Метадані мережевої події
        private class NetworkEventMetadata
        {
            public NetworkEventAuthority Authority
            {
                get; set;
            }
            public NetworkEventPriority Priority
            {
                get; set;
            }
            public bool IsRegistered
            {
                get; set;
            }
        }

        [Inject]
        public NetworkEventBus(IEventPool eventPool,
                              IMythLogger logger,
                              INetworkSystem networkSystem) : base(eventPool, logger)
        {
            _networkSystem = networkSystem;

            // Ініціалізація: сканування всіх типів подій з атрибутом NetworkEvent
            ScanAndRegisterNetworkEvents();

            // Підписка на мережеві повідомлення
            if (_networkSystem != null)
            {
                _networkSystem.OnMessageReceived += HandleNetworkMessage;
            }
        }

        /// <summary>
        /// Сканує та реєструє всі типи подій з атрибутом NetworkEvent
        /// </summary>
        private void ScanAndRegisterNetworkEvents()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                if (!assembly.FullName.Contains("MythHunter"))
                    continue;

                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => t.IsValueType &&
                                typeof(IEvent).IsAssignableFrom(t) &&
                                t.GetCustomAttributes(typeof(NetworkEventAttribute), false).Length > 0);

                    foreach (var type in types)
                    {
                        var attr = (NetworkEventAttribute)type.GetCustomAttributes(typeof(NetworkEventAttribute), false)[0];

                        _networkEventTypes[type] = new NetworkEventMetadata
                        {
                            Authority = attr.Authority,
                            Priority = attr.Priority,
                            IsRegistered = true
                        };
                    }
                }
                catch (Exception ex)
                {
                    // Ігноруємо помилки для стійкості системи
                }
            }
        }

        /// <summary>
        /// Публікує подію і синхронізує її через мережу, якщо потрібно
        /// </summary>
        public override void Publish<TEvent>(TEvent eventData)
        {
            base.Publish(eventData);

            // Перевіряємо, чи подія потребує мережевої синхронізації
            Type eventType = typeof(TEvent);

            if (_networkEventTypes.TryGetValue(eventType, out var metadata))
            {
                SynchronizeEventOverNetwork(eventData, metadata);
            }
        }

        /// <summary>
        /// Синхронізує подію через мережу
        /// </summary>
        private void SynchronizeEventOverNetwork<TEvent>(TEvent eventData, NetworkEventMetadata metadata)
            where TEvent : struct, IEvent
        {
            // Отримуємо тип всередині методу
            Type eventType = typeof(TEvent);

            // Перевіряємо авторитетність
            bool canSendOverNetwork = false;

            switch (metadata.Authority)
            {
                case NetworkEventAuthority.Server:
                    canSendOverNetwork = _networkSystem is IServerNetworkSystem;
                    break;
                case NetworkEventAuthority.Client:
                    canSendOverNetwork = _networkSystem is IClientNetworkSystem;
                    break;
                case NetworkEventAuthority.Both:
                    canSendOverNetwork = true;
                    break;
            }

            if (!canSendOverNetwork)
                return;

            // Створюємо мережеве повідомлення з події
            if (eventData is INetworkEvent networkEvent)
            {
                // Якщо подія реалізує INetworkEvent, використовуємо її безпосередньо
                var message = new NetworkEventMessage<TEvent>
                {
                    Event = eventData,
                    EventType = eventType.AssemblyQualifiedName,
                    IsReliable = networkEvent.IsReliable(),
                    Priority = networkEvent.GetNetworkPriority()
                };

                _networkSystem.SendMessage(message);
            }
            else
            {
                // Якщо подія не реалізує INetworkEvent, обгортаємо її
                var message = new NetworkEventMessage<TEvent>
                {
                    Event = eventData,
                    EventType = eventType.AssemblyQualifiedName,
                    IsReliable = metadata.Priority >= NetworkEventPriority.High,
                    Priority = metadata.Priority
                };

                _networkSystem.SendMessage(message);
            }
        }

        /// <summary>
        /// Обробляє мережеве повідомлення та публікує його як подію
        /// </summary>
        private void HandleNetworkMessage(INetworkMessage message)
        {
            if (message is NetworkEventMessage netEventMsg)
            {
                Type eventType = Type.GetType(netEventMsg.EventType);

                if (eventType != null)
                {
                    // Публікуємо подію локально
                    object eventObj = netEventMsg.DeserializeEvent(eventType);

                    if (eventObj != null)
                    {
                        // Використовуємо рефлексію для виклику PublishEvent з правильним типом
                        var method = typeof(NetworkEventBus).GetMethod("PublishNetworkEvent",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                            .MakeGenericMethod(eventType);

                        method.Invoke(this, new[] { eventObj });
                    }
                }
            }
        }

        /// <summary>
        /// Публікує отриману мережеву подію
        /// </summary>
        private void PublishNetworkEvent<TEvent>(TEvent eventData) where TEvent : struct, IEvent
        {
            // Публікуємо подію, але обходимо мережеву синхронізацію щоб уникнути циклічної передачі
            base.Publish(eventData);
        }

        /// <summary>
        /// Реєструє тип події як мережеву подію
        /// </summary>
        public void RegisterNetworkEvent<TEvent>(NetworkEventAuthority authority, NetworkEventPriority priority)
            where TEvent : struct, IEvent
        {
            _networkEventTypes[typeof(TEvent)] = new NetworkEventMetadata
            {
                Authority = authority,
                Priority = priority,
                IsRegistered = true
            };
        }
    }
}
