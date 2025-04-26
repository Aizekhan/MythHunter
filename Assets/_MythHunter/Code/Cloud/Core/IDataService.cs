using System.Threading.Tasks;
using System.Collections.Generic;

namespace MythHunter.Cloud.Core
{
    /// <summary>
    /// Інтерфейс сервісу даних
    /// </summary>
    public interface IDataService : ICloudService
    {
        Task<T> LoadDataAsync<T>(string key) where T : class;
        Task SaveDataAsync<T>(string key, T data) where T : class;
        Task<bool> DeleteDataAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<List<string>> GetKeysAsync(string prefix);
    }
}