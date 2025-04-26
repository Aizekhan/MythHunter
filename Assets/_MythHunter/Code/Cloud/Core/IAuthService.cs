using System.Threading.Tasks;

namespace MythHunter.Cloud.Core
{
    /// <summary>
    /// Інтерфейс сервісу авторизації
    /// </summary>
    public interface IAuthService : ICloudService
    {
        Task<bool> SignInAsync(string username, string password);
        Task<bool> SignUpAsync(string username, string password, string email);
        Task<bool> SignOutAsync();
        Task<bool> DeleteAccountAsync();
        bool IsSignedIn { get; }
        string CurrentUserId { get; }
    }
}