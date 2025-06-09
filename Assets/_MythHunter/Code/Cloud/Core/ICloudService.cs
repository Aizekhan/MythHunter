using Cysharp.Threading.Tasks;

namespace MythHunter.Cloud.Core
{
    /// <summary>
    /// Базовий інтерфейс хмарного сервісу
    /// </summary>
    public interface ICloudService
    {
        UniTask<bool> InitializeAsync();
        bool IsInitialized { get; }
        string GetServiceId();
    }
}