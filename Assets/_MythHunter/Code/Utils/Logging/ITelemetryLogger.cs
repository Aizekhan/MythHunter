using System.Collections.Generic;

namespace MythHunter.Utils.Logging
{
    /// <summary>
    /// Інтерфейс телеметричного логера
    /// </summary>
    public interface ITelemetryLogger : IMythLogger
    {
        void TrackMetric(string name, float value);
        void TrackEvent(string name, Dictionary<string, string> properties = null);
        void TrackException(System.Exception exception, Dictionary<string, string> properties = null);
    }
}