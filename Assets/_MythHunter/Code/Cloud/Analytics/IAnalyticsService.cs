using System.Collections.Generic;
using System.Threading.Tasks;

namespace MythHunter.Cloud.Analytics
{
    /// <summary>
    /// Інтерфейс сервісу аналітики
    /// </summary>
    public interface IAnalyticsService : ICloudService
    {
        void TrackEvent(string eventName);
        void TrackEvent(string eventName, Dictionary<string, object> parameters);
        Task<bool> FlushAsync();
        void SetUserId(string userId);
        void SetUserProperty(string name, string value);
    }
}