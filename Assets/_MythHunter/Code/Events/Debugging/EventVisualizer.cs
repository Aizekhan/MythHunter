using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using MythHunter.Core.DI;

namespace MythHunter.Events.Debugging
{
    /// <summary>
    /// Візуалізатор подій для відлагодження
    /// </summary>
    public class EventVisualizer : MonoBehaviour, IEventSubscriber
    {
        [Inject] private IEventBus _eventBus;
        
        private readonly List<EventRecord> _eventHistory = new List<EventRecord>();
        private readonly int _maxEvents = 100;
        private bool _isVisible = false;
        private Vector2 _scrollPosition;
        private readonly Dictionary<Type, Delegate> _handlers = new Dictionary<Type, Delegate>();
        
        private void OnEnable()
        {
            SubscribeToEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }
        
        public void SubscribeToEvents()
        {
            if (_eventBus == null) return;
            
            try
            {
                // Отримуємо всі завантажені збірки
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        // Знаходимо всі структури, які реалізують IEvent
                       var allTypes = assembly.GetTypes();
                         var eventTypes = new List<Type>();
                            foreach (var type in allTypes)
                                    {
                                       if (type.IsValueType && typeof(IEvent).IsAssignableFrom(type))
                                                     {
                                                          eventTypes.Add(type);
                                                     }
                                    }
                        
                        foreach (var eventType in eventTypes)
                        {
                            SubscribeToEventType(eventType);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Failed to scan assembly {assembly.FullName}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error subscribing to events: {ex.Message}");
            }
        }
        
        private void SubscribeToEventType(Type eventType)
        {
            try
            {
                // Отримуємо метод Subscribe з правильним типом
                var methodInfo = typeof(IEventBus).GetMethod("Subscribe").MakeGenericMethod(eventType);
                
                // Створюємо типізований делегат для обробки події
                var handlerType = typeof(Action<>).MakeGenericType(eventType);
                var handler = Delegate.CreateDelegate(handlerType, this, 
                    GetType().GetMethod("OnEventReceived", BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(eventType));
                
                // Зберігаємо делегат для подальшої відписки
                _handlers[eventType] = handler;
                
                // Викликаємо метод Subscribe
                methodInfo.Invoke(_eventBus, new object[] { handler });
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to subscribe to event type {eventType.Name}: {ex.Message}");
            }
        }
        
        public void UnsubscribeFromEvents()
        {
            if (_eventBus == null) return;
            
            try
            {
                foreach (var pair in _handlers)
                {
                    var eventType = pair.Key;
                    var handler = pair.Value;
                    
                    // Отримуємо метод Unsubscribe з правильним типом
                    var methodInfo = typeof(IEventBus).GetMethod("Unsubscribe").MakeGenericMethod(eventType);
                    
                    // Викликаємо метод Unsubscribe
                    methodInfo.Invoke(_eventBus, new object[] { handler });
                }
                
                _handlers.Clear();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error unsubscribing from events: {ex.Message}");
            }
        }
        
        private void OnEventReceived<T>(T evt) where T : struct, IEvent
        {
            _eventHistory.Add(new EventRecord
            {
                EventType = evt.GetType().Name,
                EventId = evt.GetEventId(),
                Timestamp = DateTime.Now
            });
            
            if (_eventHistory.Count > _maxEvents)
                _eventHistory.RemoveAt(0);
        }
        
        private void OnGUI()
        {
            if (!_isVisible) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 400, 500));
            GUILayout.BeginVertical("box");
            
            GUILayout.Label("Event Visualizer", GUI.skin.box);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear", GUILayout.Width(80)))
            {
                _eventHistory.Clear();
            }
            if (GUILayout.Button("Close", GUILayout.Width(80)))
            {
                _isVisible = false;
            }
            GUILayout.EndHorizontal();
            
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);
            
            foreach (var record in _eventHistory)
            {
                GUILayout.BeginHorizontal("box");
                GUILayout.Label($"{record.Timestamp.ToString("HH:mm:ss.fff")}", GUILayout.Width(100));
                GUILayout.Label(record.EventType);
                GUILayout.EndHorizontal();
            }
            
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F8))
            {
                _isVisible = !_isVisible;
            }
        }
        
        private struct EventRecord
        {
            public string EventType;
            public string EventId;
            public DateTime Timestamp;
        }
    }
}