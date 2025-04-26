using Cysharp.Threading.Tasks;

namespace MythHunter.Cloud.Core
{
    /// <summary>
    /// Інтерфейс сервісу авторизації
    /// </summary>
    public interface IAuthService : ICloudService
    {
        UniTask<bool> SignInAsync(string username, string password);
        UniTask<bool> SignUpAsync(string username, string password, string email);
        UniTask<bool> SignOutAsync();
        UniTask<bool> DeleteAccountAsync();
        bool IsSignedIn { get; }
        string CurrentUserId { get; }
    }
}