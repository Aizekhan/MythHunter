using System;
using System.Collections.Generic;
using MythHunter.Cloud.Core;
namespace MythHunter.Cloud.Analytics
{
    /// <summary>
    /// Базовий клас події аналітики
    /// </summary>
    public abstract class AnalyticsEvent
    {
        public DateTime Timestamp { get; private set; }
        public string EventName { get; protected set; }
        
        protected readonly Dictionary<string, object> Parameters = new Dictionary<string, object>();
        
        protected AnalyticsEvent()
        {
            Timestamp = DateTime.UtcNow;
        }
        
        public Dictionary<string, object> GetParameters()
        {
            Dictionary<string, object> result = new Dictionary<string, object>(Parameters)
            {
                { "timestamp", Timestamp.ToString("o") }
            };
            
            return result;
        }
    }
}