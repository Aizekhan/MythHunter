using System;
using System.Collections.Generic;
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
            _eventBus?.Subscribe<IEvent>(OnEventReceived);
        }
        
        public void UnsubscribeFromEvents()
        {
            _eventBus?.Unsubscribe<IEvent>(OnEventReceived);
        }
        
        private void OnEventReceived(IEvent evt)
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