using System.Threading.Tasks;

namespace MythHunter.Cloud.Core
{
    /// <summary>
    /// Базовий інтерфейс хмарного сервісу
    /// </summary>
    public interface ICloudService
    {
        Task<bool> Initialize();
        bool IsInitialized { get; }
        string GetServiceId();
    }
}