using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace MythHunter.Cloud.Core
{
    /// <summary>
    /// Інтерфейс сервісу даних
    /// </summary>
    public interface IDataService : ICloudService
    {
        UniTask<T> LoadDataAsync<T>(string key) where T : class;
        UniTask SaveDataAsync<T>(string key, T data) where T : class;
        UniTask<bool> DeleteDataAsync(string key);
        UniTask<bool> ExistsAsync(string key);
        UniTask<List<string>> GetKeysAsync(string prefix);
    }
}